using System;

namespace PSVR2Gamepad.Models
{
    public class PSVR2Report
    {
        public byte ReportId { get; set; }
        public (float x, float y) Stick { get; set; }
        public (float pullPercent, float capPercent, bool click, bool touch) Trigger { get; set; }
        public (float capPercent, bool click, bool touch) Grip { get; set; }
        public (bool click, bool touch) Triangle { get; set; }
        public (bool click, bool touch) Square { get; set; }
        public (bool click, bool touch) Circle { get; set; }
        public (bool click, bool touch) Cross { get; set; }
        public (bool click, bool touch) StickBtn { get; set; }
        public bool Option { get; set; }
        public bool Menu { get; set; }
        public (short x, short y, short z) Gyro { get; set; }
        public (short x, short y, short z) Accel { get; set; }
        public (bool pluggedIn, bool charging, bool charged, int batteryPercent) Power { get; set; }
        public (uint powerOn, uint timestamp1, uint timestamp2) Counters { get; set; }
        public string RawHex { get; set; }
    }
}