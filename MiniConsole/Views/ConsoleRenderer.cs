using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace MiniConsole.Views
{
    public static class ConsoleRenderer
    {
        private static bool _isProgressBarActive;
        private static readonly Stopwatch UiThrottleTimer = Stopwatch.StartNew();

        public static void PrintBanner(string logPath, bool removeTags, int pollIntervalMs)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            //Console.WriteLine($"[Target Log]   : {logPath}");
            Console.WriteLine($"[Strip Tags]   : {removeTags} | [Poll Interval]: {pollIntervalMs}ms \n");
            Console.ResetColor();
        }

        public static void PrintSystemEvent(string message, ConsoleColor color = ConsoleColor.Magenta)
        {
            ClearProgressBarLine();
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        public static void PrintWarning(string message)
        {
            ClearProgressBarLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[Warning] {message}");
            Console.ResetColor();
        }

        public static void PrintError(string message)
        {
            ClearProgressBarLine();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[Error] {message}");
            Console.ResetColor();
        }

        public static void PrintStandardLog(string rawLine, string displayLine)
        {
            ClearProgressBarLine();

            if (rawLine.Contains("[error]", StringComparison.OrdinalIgnoreCase) ||
                displayLine.Contains("Error", StringComparison.OrdinalIgnoreCase))
            {
                Console.ForegroundColor = ConsoleColor.Red;
            }
            else if (rawLine.Contains("[global]", StringComparison.OrdinalIgnoreCase))
            {
                Console.ForegroundColor = ConsoleColor.White;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Gray;
            }

            Console.WriteLine(displayLine);
            Console.ResetColor();
        }

        public static void RenderProgressBar(long current, long total, string statusInfo, bool forceRender = false)
        {
            if (!forceRender && UiThrottleTimer.ElapsedMilliseconds < 50)
            {
                return;
            }
            UiThrottleTimer.Restart();

            if (total <= 0) total = 1;
            double percent = Math.Min(100.0, (double)current / total * 100.0);

            const int barWidth = 25;
            int filled = (int)Math.Round(percent / 100.0 * barWidth);
            string bar = new string('█', filled) + new string('░', barWidth - filled);

            Console.ForegroundColor = ConsoleColor.Cyan;
            string output = $"\r[{bar}] {percent,6:F2}% | ID: {current:N0}/{total:N0} | {statusInfo}";

            int padWidth = Math.Max(0, Console.WindowWidth - 1);
            if (output.Length < padWidth)
            {
                output = output.PadRight(padWidth);
            }

            Console.Write(output);
            Console.ResetColor();

            _isProgressBarActive = true;
        }

        public static void ClearProgressBarLine()
        {
            if (_isProgressBarActive)
            {
                Console.WriteLine();
                _isProgressBarActive = false;
            }
        }
    }
}
