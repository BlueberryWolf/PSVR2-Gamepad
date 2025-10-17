namespace PSVR2Gamepad.Bridge
{
    public enum VirtualControllerType
    {
        Xbox360,
        DS4
    }

    public class BridgeConfig
    {
        public VirtualControllerType ControllerType { get; set; }

        public BridgeConfig()
        {
            // Default to DS4. This can be overridden by JSON config.
            ControllerType = VirtualControllerType.DS4;
        }
    }
}
