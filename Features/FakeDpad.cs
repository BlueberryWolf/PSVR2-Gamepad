namespace PSVR2Gamepad.Features
{
    /// <summary>
    /// Fake D-Pad computation from analog stick input.
    /// - Uses magnitude threshold from <see cref="FakeDpadConfig"/>.
    /// - Prefers a dominant axis to avoid unwanted diagonals; allows diagonals near 45°.
    /// - Input convention: y > 0 is up.
    /// </summary>
    public static class FakeDpad
    {
        // D-Pad tuning (higher deadzone reduces noise; higher dominance prefers single-axis over diagonals)
        private const float AxisDeadzone = 0.25f;
        private const float DominanceBias = 0.10f;

        /// <summary>
        /// Compute D-Pad booleans from stick using dominant-axis.
        /// </summary>
        public static void ComputeDpadFromStick(float x, float y, out bool up, out bool down, out bool left, out bool right)
        {
            up = down = left = right = false;

            if (Math.Sqrt((x * x) + (y * y)) < FakeDpadConfig.Threshold)
                return;

            float ax = Math.Abs(x);
            float ay = Math.Abs(y);

            bool horizDominant = (ax - ay) > DominanceBias;
            bool vertDominant = (ay - ax) > DominanceBias;

            // Horizontal movement (dominant or diagonal)
            if (ax > AxisDeadzone)
            {
                if (!vertDominant) // Not vertically dominant, so allow horizontal
                {
                    left = x < 0;
                    right = x > 0;
                }
            }

            // Vertical movement (dominant or diagonal)
            if (ay > AxisDeadzone)
            {
                if (!horizDominant) // Not horizontally dominant, so allow vertical
                {
                    // note: y>0 is up
                    up = y > 0;
                    down = y < 0;
                }
            }
        }
    }
}