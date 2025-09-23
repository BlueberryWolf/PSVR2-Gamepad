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
        private const float DpadAxisDeadzone = 0.25f;
        private const float DpadDominanceBias = 0.10f;

        /// <summary>
        /// Compute D-Pad booleans from stick using dominant-axis with optional diagonals.
        /// </summary>
        public static void ComputeDpadFromStick(float x, float y, out bool up, out bool down, out bool left, out bool right)
        {
            up = down = left = right = false;

            float mag = (float)Math.Sqrt((x * x) + (y * y));
            if (mag < FakeDpadConfig.Threshold)
                return;

            float ax = Math.Abs(x);
            float ay = Math.Abs(y);

            // If both axes are tiny, do nothing
            if (ax < DpadAxisDeadzone && ay < DpadAxisDeadzone)
                return;

            bool horizDominant = (ax - ay) > DpadDominanceBias;
            bool vertDominant = (ay - ax) > DpadDominanceBias;

            if (horizDominant)
            {
                // Horizontal only
                left = x < -DpadAxisDeadzone;
                right = x > DpadAxisDeadzone;
                return;
            }
            if (vertDominant)
            {
                // Vertical only (note: y>0 is up)
                up = y > DpadAxisDeadzone;
                down = y < -DpadAxisDeadzone;
                return;
            }

            // Near the diagonal: allow both if each axis crosses the deadzone
            if (ax >= DpadAxisDeadzone)
            {
                left = x < -DpadAxisDeadzone;
                right = x > DpadAxisDeadzone;
            }
            if (ay >= DpadAxisDeadzone)
            {
                up = y > DpadAxisDeadzone;
                down = y < -DpadAxisDeadzone;
            }
        }
    }
}