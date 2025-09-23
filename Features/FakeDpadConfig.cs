namespace PSVR2Gamepad.Features
{
    public static class FakeDpadConfig
    {
        // Enable/disable fake D-Pad globally
        public static bool Enabled { get; private set; } = false; // default OFF

        // Stick magnitude threshold to register a D-Pad direction
        public static float Threshold { get; private set; } = 0.5f;

        // Set threshold from config
        public static void SetThreshold(float value)
        {
            if (value > 0f && value <= 1f)
                Threshold = value;
        }

        // Runtime control
        public static void Toggle() => Enabled = !Enabled;
        public static void SetEnabled(bool value) => Enabled = value;
    }
}
