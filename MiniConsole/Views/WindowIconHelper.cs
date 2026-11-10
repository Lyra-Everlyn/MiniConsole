using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace MiniConsole.Views
{
    public static class WindowIconHelper
    {
        private const int WM_SETICON = 0x0080;
        private const int ICON_SMALL = 0;
        private const int ICON_BIG = 1;

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, IntPtr lParam);

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr ExtractIcon(IntPtr hInst, string lpszExeFileName, int nIconIndex);

        public static bool EnsureStandaloneWithCustomIcon(string[] args)
        {
            string? exePath = Environment.ProcessPath;

            if (string.IsNullOrEmpty(exePath)) return false;
            if (!args.Contains("--standalone"))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "conhost.exe",
                    Arguments = $"\"{exePath}\" --standalone",
                    UseShellExecute = false
                });
                return true;
            }

            IntPtr consoleHandle = GetConsoleWindow();
            if (consoleHandle != IntPtr.Zero)
            {
                IntPtr hIcon = ExtractIcon(IntPtr.Zero, exePath, 0);
                if (hIcon != IntPtr.Zero && hIcon != new IntPtr(1))
                {
                    SendMessage(consoleHandle, WM_SETICON, ICON_BIG, hIcon);
                    SendMessage(consoleHandle, WM_SETICON, ICON_SMALL, hIcon);
                }
            }

            return false;
        }
    }
}
