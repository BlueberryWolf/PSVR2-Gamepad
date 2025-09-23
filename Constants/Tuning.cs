namespace PSVR2Gamepad.Constants
{
    /// <summary>
    /// Runtime tuning knobs with JSON config
    /// Defaults are chosen for minimal latency and reliability.
    /// </summary>
    public static class Tuning
    {
        // Mutable to allow JSON config to override defaults at startup
        public static int ReadTimeoutMs = 50; // minimal latency default
        public static int WriteTimeoutMs = 25; // faster writes
        public static int WatchdogStaleMs = 1500;
        public static int ReopenBackoffStartMs = 150;
        public static int ReopenBackoffMaxMs = 3000;
        public static int ConsoleMinIntervalMs = 100;

        public static ThreadPriority ReaderThreadPriority = ThreadPriority.AboveNormal;

    }
}