namespace PSVR2Gamepad.Config
{
    public sealed class TuningConfig
    {
        // Features
        public bool? FakeDpadEnabled { get; set; }
        public float? FakeDpadThreshold { get; set; }

        // I/O timeouts
        public int? ReadTimeoutMs { get; set; }
        public int? WriteTimeoutMs { get; set; }

        // Reliability
        public int? WatchdogStaleMs { get; set; }
        public int? ReopenBackoffStartMs { get; set; }
        public int? ReopenBackoffMaxMs { get; set; }

        // Console output
        public int? ConsoleMinIntervalMs { get; set; }

        // Threading
        public string? ReaderThreadPriority { get; set; }
    }
}