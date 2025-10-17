using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using PSVR2Gamepad.Mapping;
using PSVR2Gamepad.Models;

#pragma warning disable CS0618

namespace PSVR2Gamepad.Bridge
{
    public class DS4VirtualGamepad : IVirtualGamepad
    {
        private readonly IDualShock4Controller _controller;
        private readonly Action<byte, byte> _onRumble;

        public DS4VirtualGamepad(ViGEmClient client, Action<byte, byte> onRumble)
        {
            _controller = client.CreateDualShock4Controller();
            _onRumble = onRumble;
            _controller.FeedbackReceived += OnFeedbackReceived;
        }

        private void OnFeedbackReceived(object sender, Nefarius.ViGEm.Client.Targets.DualShock4.DualShock4FeedbackReceivedEventArgs e)
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
            DS4Mapping.Map(l, r, _controller, fakeDpad);
        }

        public void SetRumble(byte largeMotor, byte smallMotor)
        {
            // in case you (repo viewer :3) want to implement in app haptics instead of just receiving XInput haptics
        }

        public void Dispose()
        {
#pragma warning disable CS0618 // 'IDualShock4Controller.FeedbackReceived' is obsolete
            _controller.FeedbackReceived -= OnFeedbackReceived;
#pragma warning restore CS0618
            _controller.Dispose();
        }
    }
}