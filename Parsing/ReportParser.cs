using System;
using System.Buffers.Binary;
using PSVR2Gamepad.Models;

namespace PSVR2Gamepad.Parsing
{
    public static class ReportParser
    {
        // Button bit flags
        private const uint BTN_OPTION_L = 0x000100;
        private const uint BTN_OPTION_R = 0x000200;
        private const uint BTN_MENU = 0x001000;
        private const uint BTN_TRIG_L = 0x000040;
        private const uint BTN_TRIG_R = 0x000080;
        private const uint BTN_TRIG_T = 0x008000;
        private const uint BTN_UPPER_L = 0x000008;
        private const uint BTN_UPPER_R = 0x000004;
        private const uint BTN_UPPER_T = 0x010000;
        private const uint BTN_LOWER_L = 0x000001;
        private const uint BTN_LOWER_R = 0x000002;
        private const uint BTN_LOWER_T = 0x020000;
        private const uint BTN_STICK_L = 0x000400;
        private const uint BTN_STICK_R = 0x000800;
        private const uint BTN_STICK_T = 0x040000;
        private const uint BTN_GRIP_L = 0x000010;
        private const uint BTN_GRIP_R = 0x000020;
        private const uint BTN_GRIP_T = 0x080000;

        public enum Side { Left, Right }

        public static PSVR2Report ParseReport(byte[] buffer, int length, Side controllerSide)
        {
            var report = new PSVR2Report { ReportId = buffer[0] };
            const int offset = 1; // Report ID offset

            // Parse analog values
            report.Stick = ParseStick(buffer, offset);
            report.Trigger = ParseTrigger(buffer, offset);
            report.Grip = ParseGrip(buffer, offset);

            // Parse digital buttons
            uint buttons = BinaryPrimitives.ReadUInt32LittleEndian(new ReadOnlySpan<byte>(buffer, offset + 8, 4));
            ParseButtons(report, buttons, controllerSide);

            // Parse sensor data
            report.Gyro = ParseGyro(buffer, offset);
            report.Accel = ParseAccel(buffer, offset);

            // Parse counters and power
            report.Counters = ParseCounters(buffer, offset);
            report.Power = ParsePower(buffer, offset);

            report.RawHex = BitConverter.ToString(buffer, 0, length).Replace("-", " ").ToLowerInvariant();
            return report;
        }

        private static (float x, float y) ParseStick(byte[] buffer, int offset)
        {
            return (
                x: ((buffer[offset + 1] * 10f) - 1275f) / 1275f,
                y: ((buffer[offset + 2] * 10f) - 1275f) / 1275f
            );
        }

        private static (float pullPercent, float capPercent, bool click, bool touch) ParseTrigger(byte[] buffer, int offset)
        {
            return (
                pullPercent: (buffer[offset + 3] * 100f) / 255f,
                capPercent: (buffer[offset + 4] * 100f) / 255f,
                click: false, // Updated later in ParseButtons
                touch: false  // Updated later in ParseButtons
            );
        }

        private static (float capPercent, bool click, bool touch) ParseGrip(byte[] buffer, int offset)
        {
            return (
                capPercent: (buffer[offset + 5] * 100f) / 255f,
                click: false, // Updated later in ParseButtons
                touch: false  // Updated later in ParseButtons
            );
        }

        private static void ParseButtons(PSVR2Report report, uint buttons, Side controllerSide)
        {
            bool isLeft = controllerSide == Side.Left;

            // Option and Menu buttons
            report.Option = isLeft ? (buttons & BTN_OPTION_L) != 0 : (buttons & BTN_OPTION_R) != 0;
            report.Menu = (buttons & BTN_MENU) != 0;

            // Update trigger with click/touch states
            bool trigClick = isLeft ? (buttons & BTN_TRIG_L) != 0 : (buttons & BTN_TRIG_R) != 0;
            report.Trigger = (report.Trigger.pullPercent, report.Trigger.capPercent, trigClick, (buttons & BTN_TRIG_T) != 0);

            // Update grip with click/touch states
            bool gripClick = isLeft ? (buttons & BTN_GRIP_L) != 0 : (buttons & BTN_GRIP_R) != 0;
            report.Grip = (report.Grip.capPercent, gripClick, (buttons & BTN_GRIP_T) != 0);

            // Face buttons
            bool upperClick = isLeft ? (buttons & BTN_UPPER_L) != 0 : (buttons & BTN_UPPER_R) != 0;
            bool lowerClick = isLeft ? (buttons & BTN_LOWER_L) != 0 : (buttons & BTN_LOWER_R) != 0;

            if (isLeft)
            {
                report.Triangle = (upperClick, (buttons & BTN_UPPER_T) != 0);
                report.Square = (lowerClick, (buttons & BTN_LOWER_T) != 0);
            }
            else
            {
                report.Circle = (upperClick, (buttons & BTN_UPPER_T) != 0);
                report.Cross = (lowerClick, (buttons & BTN_LOWER_T) != 0);
            }

            // Stick button
            bool stickClick = isLeft ? (buttons & BTN_STICK_L) != 0 : (buttons & BTN_STICK_R) != 0;
            report.StickBtn = (stickClick, (buttons & BTN_STICK_T) != 0);
        }

        private static (short x, short y, short z) ParseGyro(byte[] buffer, int offset)
        {
            return (
                x: (short)BinaryPrimitives.ReadInt16LittleEndian(new ReadOnlySpan<byte>(buffer, offset + 16, 2)),
                y: (short)BinaryPrimitives.ReadInt16LittleEndian(new ReadOnlySpan<byte>(buffer, offset + 18, 2)),
                z: (short)BinaryPrimitives.ReadInt16LittleEndian(new ReadOnlySpan<byte>(buffer, offset + 20, 2))
            );
        }

        private static (short x, short y, short z) ParseAccel(byte[] buffer, int offset)
        {
            return (
                x: (short)BinaryPrimitives.ReadInt16LittleEndian(new ReadOnlySpan<byte>(buffer, offset + 22, 2)),
                y: (short)BinaryPrimitives.ReadInt16LittleEndian(new ReadOnlySpan<byte>(buffer, offset + 24, 2)),
                z: (short)BinaryPrimitives.ReadInt16LittleEndian(new ReadOnlySpan<byte>(buffer, offset + 26, 2))
            );
        }

        private static (uint powerOn, uint timestamp1, uint timestamp2) ParseCounters(byte[] buffer, int offset)
        {
            return (
                powerOn: BinaryPrimitives.ReadUInt32LittleEndian(new ReadOnlySpan<byte>(buffer, offset + 12, 4)),
                timestamp1: BinaryPrimitives.ReadUInt32LittleEndian(new ReadOnlySpan<byte>(buffer, offset + 28, 4)),
                timestamp2: BinaryPrimitives.ReadUInt32LittleEndian(new ReadOnlySpan<byte>(buffer, offset + 48, 4))
            );
        }

        private static (bool pluggedIn, bool charging, bool charged, int batteryPercent) ParsePower(byte[] buffer, int offset)
        {
            byte pwrFlags = buffer[offset + 42];
            byte pwrConn = buffer[offset + 43];
            return (
                pluggedIn: (pwrConn & 0x10) != 0,
                charging: (pwrFlags & 0x10) != 0,
                charged: (pwrFlags & 0x20) != 0,
                batteryPercent: ((pwrFlags & 0x0F) * 11) + 1
            );
        }
    }
}