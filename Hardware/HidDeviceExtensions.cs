using System.Linq;
using HidSharp;
using PSVR2Gamepad.Constants;

namespace PSVR2Gamepad.Hardware
{
    public static class HidDeviceExtensions
    {
        public static HidDevice GetHidDeviceOrNull(this DeviceList list, int vid, int pid)
        {
            return list.GetHidDevices(vid, pid).FirstOrDefault();
        }
    }
}