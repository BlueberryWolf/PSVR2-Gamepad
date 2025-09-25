namespace PSVR2Gamepad.Constants
{
    /// <summary>
    /// Constants related to parsing controller input reports.
    /// </summary>
    public static class ParsingConstants
    {
        // Normalization values for analog inputs
        public const float StickAxisCenter = 127.5f;
        public const float StickAxisMax = 127.5f;
        public const float TriggerMax = 255.0f;

        // Battery level calculation values
        public const int BatteryMaxLevelRaw = 5; // Raw value range is 0-5
        public const int BatteryMaxLevelPercent = 100;

        // Power state bitmasks
        public const byte BatteryLevelMask = 0x0F;
        public const byte IsChargingMaskBt = 0x10;
        public const byte IsBatteryFullMask = 0x20;
        public const byte IsChargingMaskUsb = 0x08;

        // Button bitmasks
        public static class Buttons
        {
            public const uint Triangle = 1 << 0;
            public const uint Square = 1 << 1;
            public const uint Circle = 1 << 2;
            public const uint Cross = 1 << 3;
            public const uint L1 = 1 << 8;
            public const uint L2 = 1 << 9;
            public const uint L3 = 1 << 10;
            public const uint R1 = 1 << 11;
            public const uint R2 = 1 << 12;
            public const uint R3 = 1 << 13;
            public const uint Create = 1 << 14;  // Left controller "Option"
            public const uint Options = 1 << 15; // Right controller "Option"
            public const uint Home = 1 << 16;
        }

        // Touch bitmasks
        public static class Touch
        {
            public const uint Upper = 1 << 17; // Triangle or Circle
            public const uint Lower = 1 << 18; // Square or Cross
            public const uint Stick = 1 << 19;
            public const uint Trigger = 1 << 20;
            public const uint Grip = 1 << 21;
        }
    }
}