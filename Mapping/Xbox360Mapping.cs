using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;
using PSVR2Gamepad.Models;
using PSVR2Gamepad.Features;

namespace PSVR2Gamepad.Mapping
{
    /// <summary>
    /// Maps PSVR2 controller inputs to an emulated Xbox 360 controller.
    /// Includes a toggleable Fake D-Pad that uses the left stick when enabled.
    /// </summary>
    public static class Xbox360Mapping
    {
        private static bool _prevLeftMenuDown;

        public static void ApplyReport(IXbox360Controller controller, PSVR2Report left, PSVR2Report right)
        {
            ApplyAnalogInputs(controller, left, right);
            ApplyButtons(controller, left, right);
            controller.SubmitReport();
        }

        /// <summary>
        /// Maps analog stick and trigger values.
        /// </summary>
        private static void ApplyAnalogInputs(IXbox360Controller controller, PSVR2Report left, PSVR2Report right)
        {
            // Sticks
            if (left != null)
            {
                controller.SetAxisValue(Xbox360Axis.LeftThumbX, ToShortAxis(left.Stick.X));
                controller.SetAxisValue(Xbox360Axis.LeftThumbY, ToShortAxis(left.Stick.Y));
            }
            if (right != null)
            {
                controller.SetAxisValue(Xbox360Axis.RightThumbX, ToShortAxis(right.Stick.X));
                controller.SetAxisValue(Xbox360Axis.RightThumbY, ToShortAxis(right.Stick.Y));
            }

            // Triggers
            controller.SetSliderValue(Xbox360Slider.LeftTrigger, left != null ? ToTrigger(left.Trigger.PullPercent) : (byte)0);
            controller.SetSliderValue(Xbox360Slider.RightTrigger, right != null ? ToTrigger(right.Trigger.PullPercent) : (byte)0);
        }

        /// <summary>
        /// Maps digital buttons and handles Fake D-Pad toggle and output.
        /// </summary>
        private static void ApplyButtons(IXbox360Controller controller, PSVR2Report left, PSVR2Report right)
        {
            // Toggle Fake D-Pad with LEFT menu button (edge-triggered)
            bool leftMenuDown = left?.Menu.Click == true;
            if (leftMenuDown && !_prevLeftMenuDown)
            {
                FakeDpadConfig.Toggle();
            }
            _prevLeftMenuDown = leftMenuDown;

            // Shoulders
            controller.SetButtonState(Xbox360Button.LeftShoulder, left?.Grip.Click == true);
            controller.SetButtonState(Xbox360Button.RightShoulder, right?.Grip.Click == true);

            // ABXY: Cross, Circle, Square, Triangle
            controller.SetButtonState(Xbox360Button.A, right?.Cross.Click == true);
            controller.SetButtonState(Xbox360Button.B, right?.Circle.Click == true);
            controller.SetButtonState(Xbox360Button.X, left?.Square.Click == true);
            controller.SetButtonState(Xbox360Button.Y, left?.Triangle.Click == true);

            // Back / Start 
            controller.SetButtonState(Xbox360Button.Back, left?.Option.Click == true);
            controller.SetButtonState(Xbox360Button.Start, (right?.Option.Click == true));

            // Guide from RIGHT menu
            controller.SetButtonState(Xbox360Button.Guide, (right?.Menu.Click == true));

            // Stick buttons
            controller.SetButtonState(Xbox360Button.LeftThumb, left?.Stick.Click == true);
            controller.SetButtonState(Xbox360Button.RightThumb, right?.Stick.Click == true);

            // Fake D-Pad: map left stick to D-Pad when enabled
            if (FakeDpadConfig.Enabled && left != null)
            {
                bool up, down, leftDir, rightDir;
                FakeDpad.ComputeDpadFromStick(left.Stick.X, left.Stick.Y, out up, out down, out leftDir, out rightDir);

                controller.SetButtonState(Xbox360Button.Up, up);
                controller.SetButtonState(Xbox360Button.Down, down);
                controller.SetButtonState(Xbox360Button.Left, leftDir);
                controller.SetButtonState(Xbox360Button.Right, rightDir);
            }
            else
            {
                // Keep neutral when not engaged
                controller.SetButtonState(Xbox360Button.Up, false);
                controller.SetButtonState(Xbox360Button.Down, false);
                controller.SetButtonState(Xbox360Button.Left, false);
                controller.SetButtonState(Xbox360Button.Right, false);
            }
        }

        private static short ToShortAxis(float value)
        {
            value = Math.Clamp(value, -1f, 1f);
            return (short)Math.Round(value * short.MaxValue);
        }

        private static byte ToTrigger(float value)
        {
            value = Math.Clamp(value, 0f, 1f);
            return (byte)Math.Round(value * 255f);
        }
    }
}