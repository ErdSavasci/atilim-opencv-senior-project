using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace MoveCursorByHand.App_Code
{
    public class Native
    {
        [DllImport("User32.dll")]
        internal static extern IntPtr SetForegroundWindow(IntPtr hWnd);

        [DllImport("User32.dll")]
        internal static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("User32.dll")]
        internal static extern bool MoveWindow(IntPtr handle, int x, int y, int width, int height, bool redraw);

        [DllImport("User32.dll")]
        internal static extern long SetCursorPos(int x, int y);

        [DllImport("User32.dll")]
        internal static extern bool ClientToScreen(IntPtr hWnd, ref Point point);

        [DllImport("user32.dll")]
        internal static extern bool GetCursorPos(out Point lpPoint);
    }
}
