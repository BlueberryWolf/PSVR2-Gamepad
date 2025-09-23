using HidSharp;
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
                Thread.Sleep(100);
            }

            // Cleanup
            try { leftController?.Stop(); } catch { }
            try { rightController?.Stop(); } catch { }
            try { leftController?.Dispose(); } catch { }
            try { rightController?.Dispose(); } catch { }

            // Local helpers capture local variables
            void AttachIfPresent()
            {
                var leftDevice = deviceList.GetHidDeviceOrNull(PSVR2Constants.VidSony, PSVR2Constants.PidLeft);
                var rightDevice = deviceList.GetHidDeviceOrNull(PSVR2Constants.VidSony, PSVR2Constants.PidRight);

                // Attach left if newly found
                if (leftDevice != null && leftController == null)
                {
                    leftController = new PSVR2Controller(ReportParser.Side.Left, leftDevice);
                    InitializeController(leftController, "L", display, r => bridge.UpdateLeft(r));
                    bridge.AttachLeftController(leftController);
                }

                // Attach right if newly found
                if (rightDevice != null && rightController == null)
                {
                    rightController = new PSVR2Controller(ReportParser.Side.Right, rightDevice);
                    InitializeController(rightController, "R", display, r => bridge.UpdateRight(r));
                    bridge.AttachRightController(rightController);
                }

                // Detach left if missing now
                if (leftDevice == null && leftController != null)
                {
                    display.UpdateLine("L", "L controller disconnected.");
                    try { leftController.Stop(); } catch { }
                    try { leftController.Dispose(); } catch { }
                    leftController = null;
                    bridge.DetachLeftController();
                }

                // Detach right if missing now
                if (rightDevice == null && rightController != null)
                {
                    display.UpdateLine("R", "R controller disconnected.");
                    try { rightController.Stop(); } catch { }
                    try { rightController.Dispose(); } catch { }
                    rightController = null;
                    bridge.DetachRightController();
                }

                // If both are missing and none were connected before, provide guidance
                if (leftDevice == null && rightDevice == null && leftController == null && rightController == null)
                {
                    display.UpdateLine("L", "No controllers found. Pair via Bluetooth, leave app running.");
                }
            }
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
    }
}