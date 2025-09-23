using HidSharp;
using PSVR2Gamepad.Constants;
using PSVR2Gamepad.Models;
using PSVR2Gamepad.Parsing;
using PSVR2Gamepad.Communication;

namespace PSVR2Gamepad.Hardware
{
    public sealed class PSVR2Controller : IDisposable
    {
        public ReportParser.Side ControllerSide { get; }
        public HidDevice Device { get; }

        private HidStream? _stream;
        private CancellationTokenSource? _cts;
        private bool _isBluetooth;
        private byte _outSeq = 1;

        public PSVR2Controller(ReportParser.Side side, HidDevice device)
        {
            ControllerSide = side;
            Device = device;
        }

        public void SendRumble(byte strength)
        {
            if (_stream?.CanWrite != true) return;

            try
            {
                if (_stream.WriteTimeout == 0) _stream.WriteTimeout = Tuning.WriteTimeoutMs;

                DetectConnectionTypeIfNeeded();

                byte[] packet = _isBluetooth
                    ? RumbleProtocol.CreateBluetoothRumblePacket(strength, ref _outSeq)
                    : RumbleProtocol.CreateUsbRumblePacket(strength);

                _stream.Write(packet);
            }
            catch (Exception ex)
            {
                // Log write failures for troubleshooting
                System.Diagnostics.Debug.WriteLine($"SendRumble error: {ex.Message}");
            }
        }

        private void DetectConnectionTypeIfNeeded()
        {
            if (_isBluetooth) return;

            try
            {
                int maxOut = Device.GetMaxOutputReportLength();
                _isBluetooth = maxOut >= 78;
            }
            catch { _isBluetooth = false; }
        }

        public bool Open()
        {
            if (!Device.TryOpen(out _stream)) return false;

            _stream.ReadTimeout = Tuning.ReadTimeoutMs;
            _stream.WriteTimeout = Tuning.WriteTimeoutMs;
            DetectConnectionTypeIfNeeded();

            // Initialize with feature report
            TryInitializeDevice();

            _cts = new CancellationTokenSource();
            return true;
        }

        private void TryInitializeDevice()
        {
            try
            {
                var feat = new byte[64];
                feat[0] = PSVR2Constants.FeatureReportIdEnable;
                _stream?.GetFeature(feat);
            }
            catch (Exception ex)
            {
                // Log non-fatal feature report errors
                System.Diagnostics.Debug.WriteLine($"TryInitializeDevice error: {ex.Message}");
            }
        }

        public void StartReading(Action<PSVR2Report> onReport, Action<Exception> onError)
        {
            if (_stream == null) throw new InvalidOperationException("Open the device first.");

            var token = _cts?.Token ?? CancellationToken.None;
            Task.Run(() =>
            {
                try { Thread.CurrentThread.Priority = Tuning.ReaderThreadPriority; } catch { }
                ReadLoop(onReport, onError ?? (_ => { }), token);
            }, token);
        }

        private void ReadLoop(Action<PSVR2Report> onReport, Action<Exception> onError, CancellationToken token)
        {
            var buffer = new byte[128];
            var lastDataUtc = DateTime.UtcNow;

            while (!token.IsCancellationRequested)
            {
                try
                {
                    int length = _stream?.Read(buffer, 0, buffer.Length) ?? 0;

                    if (length <= 0)
                    {
                        // Timeout or no data
                        if ((DateTime.UtcNow - lastDataUtc).TotalMilliseconds > Tuning.WatchdogStaleMs)
                        {
                            // Treat as stale link; try reopen
                            if (!TryReopenWithBackoff(token)) break;
                            lastDataUtc = DateTime.UtcNow;
                        }
                        continue;
                    }

                    if (buffer[0] != PSVR2Constants.ReportId)
                    {
                        // Ignore unrelated report IDs
                        continue;
                    }

                    lastDataUtc = DateTime.UtcNow;
                    var report = ReportParser.ParseReport(buffer, length, ControllerSide);
                    onReport?.Invoke(report);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (TimeoutException)
                {
                    // Non-fatal: check watchdog for stale
                    if ((DateTime.UtcNow - lastDataUtc).TotalMilliseconds > Tuning.WatchdogStaleMs)
                    {
                        if (!TryReopenWithBackoff(token)) break;
                        lastDataUtc = DateTime.UtcNow;
                    }
                    continue;
                }
                catch (Exception ex)
                {
                    onError?.Invoke(ex);

                    if (token.IsCancellationRequested) break;
                    if (!TryReopenWithBackoff(token))
                    {
                        // Couldn't reopen (likely canceled); stop loop
                        break;
                    }
                    lastDataUtc = DateTime.UtcNow;
                }
            }
        }

        private bool TryReopenWithBackoff(CancellationToken token)
        {
            try { _stream?.Dispose(); } catch { }

            int delayMs = Tuning.ReopenBackoffStartMs;
            int maxDelayMs = Tuning.ReopenBackoffMaxMs;

            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (Device.TryOpen(out _stream))
                    {
                        _stream.ReadTimeout = 100;
                        _stream.WriteTimeout = 50;
                        DetectConnectionTypeIfNeeded();
                        TryInitializeDevice();
                        return true;
                    }
                }
                catch
                {
                    // Ignore and retry
                }

                Thread.Sleep(delayMs);
                delayMs = Math.Min(delayMs * 2, maxDelayMs);
            }
            return false;
        }

        public void Stop()
        {
            try { _cts?.Cancel(); } catch { }
            try { _stream?.Dispose(); } catch { } // ensure any pending Read() is interrupted
        }

        public void Dispose()
        {
            try { Stop(); } catch { }
            try { _cts?.Dispose(); } catch { }
        }
    }
}