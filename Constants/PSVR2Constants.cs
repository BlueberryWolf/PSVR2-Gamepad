namespace PSVR2Gamepad.Constants
{
    public interface IInputReportOffsets
    {
        int StickX { get; }
        int StickY { get; }
        int TriggerPull { get; }
        int TriggerCap { get; }
        int GripCap { get; }
        int Buttons { get; }
        int Gyro { get; }
        int Accel { get; }
        int Power { get; }
        int Battery0 { get; }
        int Battery1 { get; }
    }

    public static class PSVR2Constants
    {
        // Device Identifiers
        public const int VidSony = 0x054C;
        public const int PidLeft = 0x0E45;
        public const int PidRight = 0x0E46;

        // HID Report IDs
        public const byte ReportIdUsb = 0x01;           // USB Input Report
        public const byte ReportIdBt = 0x31;            // Bluetooth Input Report
        public const byte FeatureReportIdEnable = 0x05; // Feature report to enable 0x31 reports over BT

        // HID Report Sizes (including the 1-byte report ID)
        public const int UsbInputReportSize = 64;
        public const int BtInputReportSize = 78;
        public const int BtOutputReportSize = 78;       // Report ID (1) + Payload (77)

        // Button Masks (from 4-byte button state field)
        public const uint MaskSquare = 0x000001;        // Left Lower
        public const uint MaskCross = 0x000002;         // Right Lower
        public const uint MaskCircle = 0x000004;        // Right Upper
        public const uint MaskTriangle = 0x000008;      // Left Upper
        public const uint MaskL1 = 0x000010;            // Left Grip
        public const uint MaskR1 = 0x000020;            // Right Grip
        public const uint MaskL2 = 0x000040;            // Left Trigger
        public const uint MaskR2 = 0x000080;            // Right Trigger
        public const uint MaskL3 = 0x000400;            // Left Stick Click
        public const uint MaskR3 = 0x000800;            // Right Stick Click
        public const uint MaskCreate = 0x000100;        // Left Option
        public const uint MaskOptions = 0x000200;       // Right Option
        public const uint MaskHome = 0x001000;          // PS Button (shared)

        // Capacitive Touch Masks
        public const uint TouchGrip = 0x080000;         // L1/R1 Grip
        public const uint TouchTrigger = 0x8000;        // L2/R2 Trigger
        public const uint TouchStick = 0x040000;        // L3/R3 Stick
        public const uint TouchUpper = 0x010000;        // Triangle/Circle
        public const uint TouchLower = 0x020000;        // Square/Cross

        // USB Input Report (0x01) Field Offsets (from start of 64-byte report)
        public static class UsbInput
        {
            public const int StickX = 1;
            public const int StickY = 2;
            public const int TriggerPull = 3;
            public const int TriggerCap = 4;
            public const int GripCap = 5;
            public const int Buttons = 8;
            public const int Gyro = 16;
            public const int Accel = 22;
            public const int Battery0 = 53;
            public const int Battery1 = 54;
        }

        // Bluetooth Input Report (0x31) Field Offsets (from start of 78-byte report)
        public static class BtInput
        {
            public const int StickX = 2;
            public const int StickY = 3;
            public const int TriggerPull = 4;
            public const int TriggerCap = 5;
            public const int GripCap = 6;
            public const int Buttons = 9;
            public const int Gyro = 17;
            public const int Accel = 23;
            public const int Power = 43; // Single byte for level, charging, full
        }

        public readonly struct BtInputOffsets : IInputReportOffsets
        {
            public int StickX => BtInput.StickX;
            public int StickY => BtInput.StickY;
            public int TriggerPull => BtInput.TriggerPull;
            public int TriggerCap => BtInput.TriggerCap;
            public int GripCap => BtInput.GripCap;
            public int Buttons => BtInput.Buttons;
            public int Gyro => BtInput.Gyro;
            public int Accel => BtInput.Accel;
            public int Power => BtInput.Power;
            public int Battery0 => -1; // Not used for BT
            public int Battery1 => -1; // Not used for BT
        }

        public readonly struct UsbInputOffsets : IInputReportOffsets
        {
            public int StickX => UsbInput.StickX;
            public int StickY => UsbInput.StickY;
            public int TriggerPull => UsbInput.TriggerPull;
            public int TriggerCap => UsbInput.TriggerCap;
            public int GripCap => UsbInput.GripCap;
            public int Buttons => UsbInput.Buttons;
            public int Gyro => UsbInput.Gyro;
            public int Accel => UsbInput.Accel;
            public int Power => -1; // Not used for USB
            public int Battery0 => UsbInput.Battery0;
            public int Battery1 => UsbInput.Battery1;
        }
    }
}