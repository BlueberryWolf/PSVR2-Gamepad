using PSVR2Gamepad.Models;
using System.Text;

namespace PSVR2Gamepad.UI
{
    public class ConsoleDisplay
    {
        private readonly object _consoleLock = new object();
        private readonly TimeSpan _minInterval = TimeSpan.FromMilliseconds(Constants.Tuning.ConsoleMinIntervalMs);

        private int _lineL = -1;
        private int _lineR = -1;
        private DateTime _lastPrintL = DateTime.MinValue;
        private DateTime _lastPrintR = DateTime.MinValue;

        public void Initialize()
        {
            Console.Clear();
            Console.CursorVisible = false;
            Console.WriteLine("PSVR2 Gamepad: looking for controllers...");
            _lineL = Console.CursorTop; Console.WriteLine("L: waiting...");
            _lineR = Console.CursorTop; Console.WriteLine("R: waiting...");
        }

        public void UpdateLine(string side, string text)
        {
            lock (_consoleLock)
            {
                int targetLine = side == "L" ? _lineL : _lineR;
                if (targetLine < 0)
                {
                    Console.WriteLine(text);
                    return;
                }

                int width = Math.Max(40, Console.BufferWidth - 1);
                if (text.Length >= width) text = text.Substring(0, width - 1);
                text = text.PadRight(width - 1, ' ');

                var originalPos = Console.GetCursorPosition();

                Console.SetCursorPosition(0, targetLine);
                Console.Write(text);
                Console.SetCursorPosition(originalPos.Left, originalPos.Top);
            }
        }

        public void PrintReport(string side, PSVR2Report report)
        {
            var now = DateTime.UtcNow;
            if (side == "L" && (now - _lastPrintL) < _minInterval) return;
            if (side == "R" && (now - _lastPrintR) < _minInterval) return;

            string text = FormatReport(side, report);
            UpdateLine(side, text);

            if (side == "L") _lastPrintL = now;
            else _lastPrintR = now;
        }

        private static string FormatReport(string side, PSVR2Report r)
        {
            var sb = new StringBuilder();
            sb.Append($"{side} | ");
            sb.Append($"Stick({r.Stick.X:+0.00;-0.00; 0.00},{r.Stick.Y:+0.00;-0.00; 0.00}) ");
            sb.Append($"Trig({FmtPct(r.Trigger.PullPercent)} cap {FmtPct(r.Trigger.CapPercent)}|C{Bt(r.Trigger.Click)} T{Bt(r.Trigger.Touch)}) ");
            sb.Append($"Grip(cap {FmtPct(r.Grip.CapPercent)}|C{Bt(r.Grip.Click)} T{Bt(r.Grip.Touch)}) ");

            if (side == "L")
            {
                sb.Append($"Tri(C{Bt(r.Triangle.Click)} T{Bt(r.Triangle.Touch)}) ");
                sb.Append($"Sqr(C{Bt(r.Square.Click)} T{Bt(r.Square.Touch)}) ");
            }
            else
            {
                sb.Append($"Cir(C{Bt(r.Circle.Click)} T{Bt(r.Circle.Touch)}) ");
                sb.Append($"Crs(C{Bt(r.Cross.Click)} T{Bt(r.Cross.Touch)}) ");
            }

            sb.Append($"S(C{Bt(r.Stick.Click)} T{Bt(r.Stick.Touch)}) ");
            sb.Append($"Opt({Bt(r.Option.Click)}) Menu({Bt(r.Menu.Click)}) ");
            sb.Append($"Gyro({r.Gyro.X,6},{r.Gyro.Y,6},{r.Gyro.Z,6}) ");
            sb.Append($"Accel({r.Accel.X,6},{r.Accel.Y,6},{r.Accel.Z,6}) ");
            sb.Append($"Pwr({r.Power.BatteryLevel}% Chg:{Bt(r.Power.IsCharging)} Full:{Bt(r.Power.IsBatteryFull)})");

            return sb.ToString();
        }

        private static string Bt(bool value) => value ? "1" : "0";

        private static string FmtPct(float value) =>
            $"{Math.Round(value * 100),3}%";
    }
}
