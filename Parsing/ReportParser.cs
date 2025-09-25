using PSVR2Gamepad.Models;
using PSVR2Gamepad.Constants;
using static PSVR2Gamepad.Constants.ParsingConstants;
using System.Runtime.InteropServices;

namespace PSVR2Gamepad.Parsing
{
    public static class ReportParser
    {
        public enum Side { Left, Right }

        public static PSVR2Report? ParseReport(byte[] buffer, int length, Side controllerSide)
        {
            // USB reports are 64 bytes (1-byte Report ID + 63 bytes data)
            if (length == PSVR2Constants.UsbInputReportSize)
            {
                // USB Report ID is 0x01, data starts at buffer[1]
                return ParseReportData(buffer, 1, controllerSide);
            }
            // Bluetooth reports are 78 bytes (1-byte Report ID + 77 bytes data)
            else if (length == PSVR2Constants.BtInputReportSize)
            {
                // BT Report ID is 0x31, data starts at buffer[1]
                return ParseReportData(buffer, 1, controllerSide);
            }
            return null; // Or handle other report types if necessary
        }

        private static PSVR2Report ParseReportData(byte[] buffer, int offset, Side controllerSide)
        {
            var report = new PSVR2Report();
            var inputOffsets = (buffer[0] == PSVR2Constants.ReportIdBt) ? (IInputReportOffsets)new PSVR2Constants.BtInputOffsets() : new PSVR2Constants.UsbInputOffsets();

            float stickX = (buffer[inputOffsets.StickX] - StickAxisCenter) / StickAxisMax;
            float stickY = -(buffer[inputOffsets.StickY] - StickAxisCenter) / StickAxisMax;
            float triggerPull = buffer[inputOffsets.TriggerPull] / TriggerMax;
            float triggerCap = buffer[inputOffsets.TriggerCap] / TriggerMax;
            float gripCap = buffer[inputOffsets.GripCap] / TriggerMax;
            uint buttons = MemoryMarshal.Read<uint>(buffer.AsSpan(inputOffsets.Buttons));
            report.Gyro = new Vector3State(X: BitConverter.ToInt16(buffer, inputOffsets.Gyro), Y: BitConverter.ToInt16(buffer, inputOffsets.Gyro + 2), Z: BitConverter.ToInt16(buffer, inputOffsets.Gyro + 4));
            report.Accel = new Vector3State(X: BitConverter.ToInt16(buffer, inputOffsets.Accel), Y: BitConverter.ToInt16(buffer, inputOffsets.Accel + 2), Z: BitConverter.ToInt16(buffer, inputOffsets.Accel + 4));

            if (buffer[0] == PSVR2Constants.ReportIdBt)
            {
                // BT Power info is in a single byte at a different location
                byte powerByte = buffer[inputOffsets.Power];
                int batteryLevel = (powerByte & BatteryLevelMask) * BatteryMaxLevelPercent / 8;
                report.Power = new PowerState(
                    BatteryLevel: (byte)Math.Clamp(batteryLevel, 0, 100),
                    IsCharging: (powerByte & IsChargingMaskBt) != 0,
                    IsBatteryFull: (powerByte & IsBatteryFullMask) != 0);
            }
            else // USB (ReportIdUsb)
            {
                // USB Power info is split across two bytes
                byte battery0 = buffer[inputOffsets.Battery0];
                byte battery1 = buffer[inputOffsets.Battery1];
                int batteryLevel = (battery0 & BatteryLevelMask) * BatteryMaxLevelPercent / 8;
                report.Power = new PowerState(
                    BatteryLevel: (byte)Math.Clamp(batteryLevel, 0, 100),
                    IsCharging: (battery1 & IsChargingMaskUsb) != 0,
                    IsBatteryFull: (battery0 & IsBatteryFullMask) != 0);
            }

            if (controllerSide == Side.Left)
            {
                report.Triangle = report.Triangle with { Click = (buttons & PSVR2Constants.MaskTriangle) != 0 };
                report.Square = report.Square with { Click = (buttons & PSVR2Constants.MaskSquare) != 0 };
                report.Option = report.Option with { Click = (buttons & PSVR2Constants.MaskCreate) != 0 };
                report.Menu = report.Menu with { Click = (buttons & PSVR2Constants.MaskHome) != 0 };
                report.Trigger = report.Trigger with { Click = (buttons & PSVR2Constants.MaskL2) != 0 };
                report.Stick = report.Stick with { Click = (buttons & PSVR2Constants.MaskL3) != 0 };
                report.Grip = report.Grip with { Click = (buttons & PSVR2Constants.MaskL1) != 0 };
                report.Triangle = report.Triangle with { Touch = (buttons & PSVR2Constants.TouchUpper) != 0 };
                report.Square = report.Square with { Touch = (buttons & PSVR2Constants.TouchLower) != 0 };
            }
            else // Right
            {
                report.Circle = report.Circle with { Click = (buttons & PSVR2Constants.MaskCircle) != 0 };
                report.Cross = report.Cross with { Click = (buttons & PSVR2Constants.MaskCross) != 0 };
                report.Option = report.Option with { Click = (buttons & PSVR2Constants.MaskOptions) != 0 };
                report.Trigger = report.Trigger with { Click = (buttons & PSVR2Constants.MaskR2) != 0 };
                report.Stick = report.Stick with { Click = (buttons & PSVR2Constants.MaskR3) != 0 };
                report.Grip = report.Grip with { Click = (buttons & PSVR2Constants.MaskR1) != 0 };
                report.Circle = report.Circle with { Touch = (buttons & PSVR2Constants.TouchUpper) != 0 };
                report.Cross = report.Cross with { Touch = (buttons & PSVR2Constants.TouchLower) != 0 };
            }

            report.Menu = report.Menu with { Click = (buttons & PSVR2Constants.MaskHome) != 0 };
            report.Trigger = report.Trigger with { PullPercent = triggerPull, CapPercent = triggerCap, Touch = (buttons & PSVR2Constants.TouchTrigger) != 0 };
            report.Stick = report.Stick with { X = stickX, Y = stickY, Touch = (buttons & PSVR2Constants.TouchStick) != 0 };
            report.Grip = report.Grip with { CapPercent = gripCap, Touch = (buttons & PSVR2Constants.TouchGrip) != 0 };

            return report;
        }
    }
}