namespace PSVR2Gamepad.Features
{
    public static class FakeDpadConfig
    {
        // Enable/disable fake D-Pad globally
        public static bool Enabled { get; private set; } = false; // default OFF

        // Stick magnitude threshold to register a D-Pad direction
        public static float Threshold { get; private set; } = 0.5f;

        static FakeDpadConfig()
        {
            try
            {
                var thr = System.Environment.GetEnvironmentVariable("PSVR2_FAKE_DPAD_THRESHOLD");
                if (!string.IsNullOrEmpty(thr) && float.TryParse(thr, out var t) && t > 0f && t <= 1f)
                {
                    Threshold = t;
                }
            }
            catch
            {
                Threshold = 0.5f;
            }
        }

        // Runtime control
        public static void Toggle() => Enabled = !Enabled;
        public static void SetEnabled(bool value) => Enabled = value;
    }
}
