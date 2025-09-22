using System;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;
using PSVR2Gamepad.Models;
using PSVR2Gamepad.Hardware;
using PSVR2Gamepad.Mapping;

namespace PSVR2Gamepad.Bridge
{
    public sealed class ViGEmBridge : IDisposable
    {
        private readonly ViGEmClient _client;
        private readonly IXbox360Controller _x360;
        private readonly object _lock = new object();

        private PSVR2Report _left;
        private PSVR2Report _right;
        private PSVR2Controller _leftCtlRef;
        private PSVR2Controller _rightCtlRef;

        public ViGEmBridge()
        {
            _client = new ViGEmClient();
            _x360 = _client.CreateXbox360Controller();
            _x360.FeedbackReceived += OnFeedbackReceived;
            _x360.Connect();
        }

        private void OnFeedbackReceived(object sender, Xbox360FeedbackReceivedEventArgs e)
        {
            try
            {
                DistributeRumble(e.LargeMotor, e.SmallMotor);
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

        public void UpdateLeft(PSVR2Report report)
        {
            lock (_lock)
            {
                _left = report;
                Xbox360Mapping.ApplyReport(_x360, _left, _right);
            }
        }

        public void UpdateRight(PSVR2Report report)
        {
            lock (_lock)
            {
                _right = report;
                Xbox360Mapping.ApplyReport(_x360, _left, _right);
            }
        }

        public void Dispose()
        {
            try { _x360.Disconnect(); } catch { }
            try { _client.Dispose(); } catch { }
        }
    }
}