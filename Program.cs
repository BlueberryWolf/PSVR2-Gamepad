﻿using HidSharp;
using PSVR2Gamepad.Constants;
using PSVR2Gamepad.Hardware;
using PSVR2Gamepad.Bridge;
using PSVR2Gamepad.UI;
using PSVR2Gamepad.Parsing;
using PSVR2Gamepad.Models;

namespace PSVR2Gamepad
{
    public class Program
    {
        private const string LeftSide = "L";
        private const string RightSide = "R";

        static void Main()
        {
            Config.ConfigLoader.ApplyFromJson();

            var display = new ConsoleDisplay();
            display.Initialize();

            using var bridge = new ViGEmBridge();
            var deviceList = DeviceList.Local;

            var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (s, e) => { e.Cancel = true; cts.Cancel(); };

            PSVR2Controller? leftController = null;
            PSVR2Controller? rightController = null;

            // Initial attach if devices are present
            AttachIfPresent();

            // Hotplug support
            deviceList.Changed += (s, e) =>
            {
                try { AttachIfPresent(); } catch { /* ignore transient errors */ }
            };

            // Keep the app alive until Ctrl+C
            while (!cts.IsCancellationRequested)
            {
                Thread.Sleep(Tuning.MainLoopIntervalMs);
            }

            // Cleanup
            DisposeController(leftController);
            DisposeController(rightController);

            // Local helper captures local variables
            async void AttachIfPresent()
            {
                try
                {
                    (leftController, _) = await UpdateControllerConnection(leftController, ReportParser.Side.Left, PSVR2Constants.PidLeft, LeftSide, display,
                        r => bridge.UpdateLeft(r), c => bridge.AttachLeftController(c), () => bridge.DetachLeftController()).ConfigureAwait(false);

                    (rightController, _) = await UpdateControllerConnection(rightController, ReportParser.Side.Right, PSVR2Constants.PidRight, RightSide, display,
                        r => bridge.UpdateRight(r), c => bridge.AttachRightController(c), () => bridge.DetachRightController()).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    display.UpdateLine("SYS", $"Error during device attach: {ex.Message}");
                }

                // If both are still missing, provide guidance.
                if (leftController == null && rightController == null)
                {
                    display.UpdateLine("SYS", "No controllers found. Connect via USB or Bluetooth.");
                    display.UpdateLine("L", "Left controller not found.");
                    display.UpdateLine("R", "Right controller not found.");
                }
            }
        }

        private static async Task<(PSVR2Controller? controller, bool changed)> UpdateControllerConnection(
            PSVR2Controller? controller,
            ReportParser.Side side,
            int productId,
            string sideLabel,
            ConsoleDisplay display,
            Action<PSVR2Report> updateAction,
            Action<PSVR2Controller> attachAction,
            Action detachAction)
        {
            var device = await Task.Run(() => DeviceList.Local.GetHidDeviceOrNull(PSVR2Constants.VidSony, productId)).ConfigureAwait(false);

            // Attach if newly found
            if (device != null && controller == null)
            {
                controller = await Task.Run(() =>
                {
                    var newCtrl = new PSVR2Controller(side, device);
                    InitializeController(newCtrl, sideLabel, display, updateAction);
                    attachAction(newCtrl);
                    return newCtrl;
                });

                return (controller, true);
            }

            if (device == null && controller != null)
            {
                display.UpdateLine(sideLabel, $"{sideLabel} controller disconnected.");
                DisposeController(controller);
                controller = null;
                detachAction();
                return (null, true);
            }

            // Return the controller (either the original or null if detached)
            return (controller, false);
        }

        private static void InitializeController(
            PSVR2Controller controller,
            string side,
            ConsoleDisplay display,
            Action<PSVR2Report> updateAction)
        {
            if (controller == null) return;

            if (controller.Open())
            {
                display.UpdateLine(side, $"{side} controller opened.");
                controller.StartReading(
                    report =>
                    {
                        updateAction(report);
                        display.PrintReport(side, report);
                    },
                    ex => display.UpdateLine(side, $"{side} error: {ex.Message}")
                );
            }
            else
            {
                display.UpdateLine(side, $"Failed to open {side} controller.");
            }
        }

        private static void DisposeController(PSVR2Controller? controller)
        {
            if (controller == null) return;
            try { controller.Stop(); }
            catch { /* ignored */ }

            try { controller.Dispose(); }
            catch { /* ignored */ }
        }
    }
}