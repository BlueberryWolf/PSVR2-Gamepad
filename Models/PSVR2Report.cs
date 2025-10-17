namespace PSVR2Gamepad.Models
{
    public record ButtonState(bool Click, bool Touch);
    public record AnalogButtonState(float PullPercent, float CapPercent, bool Click, bool Touch);
    public record StickState(float X, float Y, bool Click, bool Touch);
    public record Vector3State(short X, short Y, short Z);
    public record PowerState(byte BatteryLevel, bool IsCharging, bool IsBatteryFull);

    public class PSVR2Report
    {
        public StickState Stick { get; set; } = new(0, 0, false, false);
        public AnalogButtonState Trigger { get; set; } = new(0, 0, false, false); // L2/R2
        public AnalogButtonState Grip { get; set; } = new(0, 0, false, false); // L1/R1
        public ButtonState Triangle { get; set; } = new(false, false); // Left Controller
        public ButtonState Square { get; set; } = new(false, false);   // Left Controller
        public ButtonState Cross { get; set; } = new(false, false);    // Right Controller
        public ButtonState Circle { get; set; } = new(false, false);   // Right Controller
        public ButtonState Option { get; set; } = new(false, false);
        public ButtonState Menu { get; set; } = new(false, false);
        public Vector3State Gyro { get; set; } = new(0, 0, 0);
        public Vector3State Accel { get; set; } = new(0, 0, 0);
        public PowerState Power { get; set; } = new(0, false, false);

        public static PSVR2Report Empty { get; } = new PSVR2Report();
    }
}