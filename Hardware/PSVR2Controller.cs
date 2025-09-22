using System;
using System.Threading;
using System.Threading.Tasks;
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

        private HidStream _stream;
        private CancellationTokenSource _cts;
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
                if (_stream.WriteTimeout == 0) _stream.WriteTimeout = 50;

                DetectConnectionTypeIfNeeded();

                byte[] packet = _isBluetooth
                    ? RumbleProtocol.CreateBluetoothRumblePacket(strength, ref _outSeq)
                    : RumbleProtocol.CreateUsbRumblePacket(strength);

                _stream.Write(packet);
            }
            catch { /* ignore write failures */ }
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

            _stream.ReadTimeout = Timeout.Infinite;
            _stream.WriteTimeout = 50;
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
                _stream.GetFeature(feat);
            }
            catch { /* Non-fatal */ }
        }

        public void StartReading(Action<PSVR2Report> onReport, Action<Exception> onError = null)
        {
            if (_stream == null) throw new InvalidOperationException("Open the device first.");

            Task.Run(() => ReadLoop(onReport, onError), _cts.Token);
        }

        private void ReadLoop(Action<PSVR2Report> onReport, Action<Exception> onError)
        {
            var buffer = new byte[128];
            while (!_cts.IsCancellationRequested)
            {
                try
                {
                    int length = _stream.Read(buffer, 0, buffer.Length);
                    if (length <= 0 || buffer[0] != PSVR2Constants.ReportId) continue;

                    var report = ReportParser.ParseReport(buffer, length, ControllerSide);
                    onReport?.Invoke(report);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    onError?.Invoke(ex);
                    Thread.Sleep(10);
                }
            }
        }

        public void Stop()
        {
            try { _cts?.Cancel(); } catch { }
        }

        public void Dispose()
        {
            try { Stop(); } catch { }
            try { _stream?.Dispose(); } catch { }
            try { _cts?.Dispose(); } catch { }
        }
    }
}
