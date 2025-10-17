using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using PSVR2Gamepad.Mapping;
using PSVR2Gamepad.Models;

namespace PSVR2Gamepad.Bridge
{
    public class Xbox360VirtualGamepad : IVirtualGamepad
    {
        private readonly IXbox360Controller _controller;
        private readonly Action<byte, byte> _onRumble;

        public Xbox360VirtualGamepad(ViGEmClient client, Action<byte, byte> onRumble)
        {
            _controller = client.CreateXbox360Controller();
            _onRumble = onRumble;
            _controller.FeedbackReceived += OnFeedbackReceived;
        }

        private void OnFeedbackReceived(object sender, Nefarius.ViGEm.Client.Targets.Xbox360.Xbox360FeedbackReceivedEventArgs e)
        {
            _onRumble(e.LargeMotor, e.SmallMotor);
        }

        public void Connect()
        {
            _controller.Connect();
        }

        public void Disconnect()
        {
            _controller.Disconnect();
        }

        public void Update(PSVR2Report? left, PSVR2Report? right, bool fakeDpad)
        {
            var l = left ?? PSVR2Report.Empty;
            var r = right ?? PSVR2Report.Empty;
            Xbox360Mapping.Map(l, r, _controller, fakeDpad);
        }

        public void SetRumble(byte largeMotor, byte smallMotor)
        {
            // in case you (repo viewer :3) want to implement in app haptics instead of just receiving XInput haptics
        }

        public void Dispose()
        {
            _controller.FeedbackReceived -= OnFeedbackReceived;
        }
    }
}