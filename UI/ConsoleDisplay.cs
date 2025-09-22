using System;
using System.Threading;
using PSVR2Gamepad.Models;

namespace PSVR2Gamepad.UI
{
    public class ConsoleDisplay
    {
        private readonly object _consoleLock = new object();
        private readonly TimeSpan _minInterval = TimeSpan.FromMilliseconds(100);

        private int _lineL = -1;
        private int _lineR = -1;
        private int _promptLine = -1;
        private DateTime _lastPrintL = DateTime.MinValue;
        private DateTime _lastPrintR = DateTime.MinValue;

        public void Initialize()
        {
            Console.Clear();
            Console.WriteLine("PSVR2 Gamepad: looking for controllers...");
            _lineL = Console.CursorTop; Console.WriteLine("L: waiting...");
            _lineR = Console.CursorTop; Console.WriteLine("R: waiting...");
            _promptLine = Console.CursorTop; Console.WriteLine("Press Enter to quit...");
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

                var (curLeft, curTop) = Console.GetCursorPosition();

                Console.SetCursorPosition(0, targetLine);
                Console.Write(text);

                Console.SetCursorPosition(0, _promptLine);
                Console.Write("Press Enter to quit...");
                Console.SetCursorPosition(curLeft, curTop);
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
            string upperBtn, lowerBtn;

            if (side == "L")
            {
                upperBtn = $"Triangle(C{Bt(r.Triangle.click)} T{Bt(r.Triangle.touch)})";
                lowerBtn = $"Square(C{Bt(r.Square.click)} T{Bt(r.Square.touch)})";
            }
            else
            {
                upperBtn = $"Circle(C{Bt(r.Circle.click)} T{Bt(r.Circle.touch)})";
                lowerBtn = $"Cross(C{Bt(r.Cross.click)} T{Bt(r.Cross.touch)})";
            }

            return $"{side} | " +
                   $"Stick({r.Stick.x:F2},{r.Stick.y:F2}) " +
                   $"Trig({r.Trigger.pullPercent:F0}% cap {r.Trigger.capPercent:F0}%|C{Bt(r.Trigger.click)} T{Bt(r.Trigger.touch)}) " +
                   $"Grip(cap {r.Grip.capPercent:F0}%|C{Bt(r.Grip.click)} T{Bt(r.Grip.touch)}) " +
                   $"{upperBtn} {lowerBtn} " +
                   $"S(C{Bt(r.StickBtn.click)} T{Bt(r.StickBtn.touch)}) " +
                   $"Opt({Bt(r.Option)}) Menu({Bt(r.Menu)}) " +
                   $"Gyro({r.Gyro.x},{r.Gyro.y},{r.Gyro.z}) " +
                   $"Accel({r.Accel.x},{r.Accel.y},{r.Accel.z}) " +
                   $"Pwr({r.Power.batteryPercent}% Chg:{Bt(r.Power.charging)} Full:{Bt(r.Power.charged)} Plug:{Bt(r.Power.pluggedIn)})";
        }

        private static string Bt(bool value) => value ? "1" : "0";

        public void WaitForExit()
        {
            Console.SetCursorPosition(0, _promptLine);
            Console.ReadLine();
        }
    }
}
