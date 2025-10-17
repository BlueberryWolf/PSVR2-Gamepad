using Nefarius.ViGEm.Client;
using PSVR2Gamepad.Models;
using PSVR2Gamepad.Hardware;
using PSVR2Gamepad.Features;

namespace PSVR2Gamepad.Bridge
{
    public sealed class ViGEmBridge : IDisposable
    {
        private readonly ViGEmClient _client;
        private readonly IVirtualGamepad _virtualGamepad;
        private readonly object _lock = new object();

        private PSVR2Report? _left;
        private PSVR2Report? _right;
        private PSVR2Controller? _leftCtlRef;
        private PSVR2Controller? _rightCtlRef;
        private bool _prevLeftMenuDown = false;

        public ViGEmBridge(BridgeConfig config)
        {
            _client = new ViGEmClient();

            if (config.ControllerType == VirtualControllerType.DS4)
            {
                _virtualGamepad = new DS4VirtualGamepad(_client, OnFeedbackReceived);
            }
            else
            {
                _virtualGamepad = new Xbox360VirtualGamepad(_client, OnFeedbackReceived);
            }

            Console.WriteLine($"Creating virtual {config.ControllerType} controller...");
            _virtualGamepad.Connect();
        }

        private void OnFeedbackReceived(byte largeMotor, byte smallMotor)
        {
            try
            {
                DistributeRumble(largeMotor, smallMotor);
            }
            catch { /* ignore transient write errors */ }
        }

        private void DistributeRumble(byte largeMotor, byte smallMotor)
        {
            if (_leftCtlRef != null && _rightCtlRef != null)
            {
                _leftCtlRef.SendRumble(largeMotor);
                _rightCtlRef.SendRumble(smallMotor);
            }
            else if (_leftCtlRef != null)
            {
                byte combined = (byte)Math.Max(largeMotor, smallMotor);
                _leftCtlRef.SendRumble(combined);
            }
            else if (_rightCtlRef != null)
            {
                byte combined = (byte)Math.Max(largeMotor, smallMotor);
                _rightCtlRef.SendRumble(combined);
            }
        }

        public void AttachLeftController(PSVR2Controller controller) => _leftCtlRef = controller;
        public void AttachRightController(PSVR2Controller controller) => _rightCtlRef = controller;
        public void DetachLeftController() => _leftCtlRef = null;
        public void DetachRightController() => _rightCtlRef = null;

        public void UpdateLeft(PSVR2Report report)
        {
            lock (_lock)
            {
                bool leftMenuDown = report.Menu.Click;
                if (leftMenuDown && !_prevLeftMenuDown)
                {
                    FakeDpadConfig.Toggle();
                }
                _prevLeftMenuDown = leftMenuDown;

                _left = report;
                _virtualGamepad.Update(_left, _right, FakeDpadConfig.Enabled);
            }
        }

        public void UpdateRight(PSVR2Report report)
        {
            lock (_lock)
            {
                _right = report;
                _virtualGamepad.Update(_left, _right, FakeDpadConfig.Enabled);
            }
        }

        public void Dispose()
        {
            try { _virtualGamepad.Dispose(); } catch { }
            try { _client.Dispose(); } catch { }
        }
    }
}