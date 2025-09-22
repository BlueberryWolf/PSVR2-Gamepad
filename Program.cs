using System;
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
            var display = new ConsoleDisplay();
            display.Initialize();

            using var bridge = new ViGEmBridge();
            var deviceList = DeviceList.Local;

            var leftDevice = deviceList.GetHidDeviceOrNull(PSVR2Constants.VidSony, PSVR2Constants.PidLeft);
            var rightDevice = deviceList.GetHidDeviceOrNull(PSVR2Constants.VidSony, PSVR2Constants.PidRight);

            if (leftDevice == null && rightDevice == null)
            {
                display.UpdateLine("L", "No controllers found. Pair via Bluetooth, then rerun.");
                display.WaitForExit();
                return;
            }

            using var leftController = leftDevice != null ? new PSVR2Controller(ReportParser.Side.Left, leftDevice) : null;
            using var rightController = rightDevice != null ? new PSVR2Controller(ReportParser.Side.Right, rightDevice) : null;

            InitializeController(leftController, "L", display, r => bridge.UpdateLeft(r));
            InitializeController(rightController, "R", display, r => bridge.UpdateRight(r));

            if (leftController != null) bridge.AttachLeftController(leftController);
            if (rightController != null) bridge.AttachRightController(rightController);

            display.WaitForExit();

            leftController?.Stop();
            rightController?.Stop();
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