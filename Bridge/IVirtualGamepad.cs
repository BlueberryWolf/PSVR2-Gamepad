using PSVR2Gamepad.Models;

namespace PSVR2Gamepad.Bridge
{
    public interface IVirtualGamepad : IDisposable
    {
        void Connect();
        void Disconnect();
        void Update(PSVR2Report? left, PSVR2Report? right, bool fakeDpad);
        void SetRumble(byte largeMotor, byte smallMotor);
    }
}