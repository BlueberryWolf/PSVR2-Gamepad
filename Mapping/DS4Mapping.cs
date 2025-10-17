using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.DualShock4;
using PSVR2Gamepad.Features;
using PSVR2Gamepad.Models;

namespace PSVR2Gamepad.Mapping
{
    public static class DS4Mapping
    {
        public static void Map(PSVR2Report left, PSVR2Report right, IDualShock4Controller controller, bool fakeDpad)
        {
            // Sticks
            controller.SetAxisValue(DualShock4Axis.LeftThumbX, ToByteAxis(left.Stick.X));
            controller.SetAxisValue(DualShock4Axis.LeftThumbY, ToByteAxis(-left.Stick.Y)); // Inverted Y
            controller.SetAxisValue(DualShock4Axis.RightThumbX, ToByteAxis(right.Stick.X));
            controller.SetAxisValue(DualShock4Axis.RightThumbY, ToByteAxis(-right.Stick.Y)); // Inverted Y

            // Triggers
            controller.SetSliderValue(DualShock4Slider.LeftTrigger, ToByteTrigger(left.Trigger.PullPercent));
            controller.SetSliderValue(DualShock4Slider.RightTrigger, ToByteTrigger(right.Trigger.PullPercent));

            // Bumpers (Grips)
            controller.SetButtonState(DualShock4Button.ShoulderLeft, left.Grip.Click);
            controller.SetButtonState(DualShock4Button.ShoulderRight, right.Grip.Click);

            // Face Buttons
            controller.SetButtonState(DualShock4Button.Cross, right.Cross.Click);
            controller.SetButtonState(DualShock4Button.Circle, right.Circle.Click);
            controller.SetButtonState(DualShock4Button.Square, left.Square.Click);
            controller.SetButtonState(DualShock4Button.Triangle, left.Triangle.Click);

            // Stick Clicks
            controller.SetButtonState(DualShock4Button.ThumbLeft, left.Stick.Click);
            controller.SetButtonState(DualShock4Button.ThumbRight, right.Stick.Click);

            // Special Buttons
            controller.SetButtonState(DualShock4SpecialButton.Share, left.Option.Click);
            controller.SetButtonState(DualShock4Button.Options, right.Option.Click);
            controller.SetButtonState(DualShock4SpecialButton.Ps, right.Menu.Click);

            // D-Pad
            if (fakeDpad)
            {
                FakeDpad.ComputeDpadFromStick(left.Stick.X, left.Stick.Y, out bool dpadUp, out bool dpadDown, out bool dpadLeft, out bool dpadRight);
                var dpad = DualShock4DPadDirection.None;
                if (dpadUp) dpad = dpadLeft ? DualShock4DPadDirection.Northwest : (dpadRight ? DualShock4DPadDirection.Northeast : DualShock4DPadDirection.North);
                else if (dpadDown) dpad = dpadLeft ? DualShock4DPadDirection.Southwest : (dpadRight ? DualShock4DPadDirection.Southeast : DualShock4DPadDirection.South);
                else if (dpadLeft) dpad = DualShock4DPadDirection.West;
                else if (dpadRight) dpad = DualShock4DPadDirection.East;
                controller.SetDPadDirection(dpad);
            }
            else
            {
                controller.SetDPadDirection(DualShock4DPadDirection.None);
            }

            controller.SubmitReport();
        }

        private static byte ToByteAxis(float value)
        {
            value = Math.Clamp(value, -1f, 1f);
            return (byte)Math.Round((value + 1) * 127.5);
        }

        private static byte ToByteTrigger(float value)
        {
            value = Math.Clamp(value, 0f, 1f);
            return (byte)Math.Round(value * 255f);
        }
    }
}