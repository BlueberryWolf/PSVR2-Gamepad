using System.Text.Json;
using PSVR2Gamepad.Constants;
using PSVR2Gamepad.Bridge;

namespace PSVR2Gamepad.Config
{
    public static class ConfigLoader
    {
        private const string DefaultFileName = "psvr2-gamepad.config.json";

        public static void ApplyFromJson(BridgeConfig bridgeConfig, string? path = null)
        {
            try
            {
                var file = string.IsNullOrWhiteSpace(path) ? Path.Combine(AppContext.BaseDirectory, DefaultFileName) : path;
                if (!File.Exists(file))
                {
                    // No config file; keep defaults
                    return;
                }

                var json = File.ReadAllText(file);
                var cfg = JsonSerializer.Deserialize<TuningConfig>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                if (cfg == null) return;

                // Apply virtual controller type
                if (!string.IsNullOrWhiteSpace(cfg.VirtualController) &&
                    Enum.TryParse<VirtualControllerType>(cfg.VirtualController, true, out var controllerType))
                {
                    bridgeConfig.ControllerType = controllerType;
                }

                // Apply with null checks. JSON overrides defaults only for specified values
                if (cfg.ReadTimeoutMs.HasValue) Tuning.ReadTimeoutMs = Math.Max(1, cfg.ReadTimeoutMs.Value);
                if (cfg.WriteTimeoutMs.HasValue) Tuning.WriteTimeoutMs = Math.Max(1, cfg.WriteTimeoutMs.Value);
                if (cfg.WatchdogStaleMs.HasValue) Tuning.WatchdogStaleMs = Math.Max(100, cfg.WatchdogStaleMs.Value);
                if (cfg.ReopenBackoffStartMs.HasValue) Tuning.ReopenBackoffStartMs = Math.Max(50, cfg.ReopenBackoffStartMs.Value);
                if (cfg.ReopenBackoffMaxMs.HasValue) Tuning.ReopenBackoffMaxMs = Math.Max(Tuning.ReopenBackoffStartMs, cfg.ReopenBackoffMaxMs.Value);
                if (cfg.ConsoleMinIntervalMs.HasValue) Tuning.ConsoleMinIntervalMs = Math.Max(0, cfg.ConsoleMinIntervalMs.Value);
                if (!string.IsNullOrWhiteSpace(cfg.ReaderThreadPriority))
                {
                    if (Enum.TryParse<ThreadPriority>(cfg.ReaderThreadPriority, true, out var p))
                        Tuning.ReaderThreadPriority = p;
                }

                if (cfg.FakeDpadEnabled.HasValue)
                {
                    Features.FakeDpadConfig.SetEnabled(cfg.FakeDpadEnabled.Value);
                }
                if (cfg.FakeDpadThreshold.HasValue)
                {
                    Features.FakeDpadConfig.SetThreshold(cfg.FakeDpadThreshold.Value);
                }
            }
            catch (Exception ex)
            {
                // Log config errors for troubleshooting, but keep the app running
                System.Diagnostics.Debug.WriteLine($"ConfigLoader error: {ex.Message}");
            }
        }
    }
}