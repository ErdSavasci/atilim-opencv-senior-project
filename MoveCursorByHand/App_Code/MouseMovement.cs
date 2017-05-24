using Emgu.CV;
using System;
using System.Drawing;

namespace MoveCursorByHand.App_Code
{
    class MouseMovement
    {
        public MouseMovement()
        {

        }

        public void MoveMouse(int x1, int x2, int y1, int y2, ref Mat frame)
        {
            LPPoint initialCursorPosition;
            Native.GetCursorPos(out initialCursorPosition);
            ScreenProperties screenProperties = new ScreenProperties();

            double mouseSensitivity = 0.25; //0 < mouseSensitivity < 1
            double resolutionDiffWidth = screenProperties.getWidth() / (double)frame.Width;
            double resolutionDiffHeight = screenProperties.getHeight() / (double)frame.Height;

            Console.WriteLine("Old Mouse X: " + initialCursorPosition.X + "/ Old Mouse Y: " + initialCursorPosition.Y);

            if (Math.Abs(x2 - x1) > 5 || Math.Abs(y2 - y1) > 5)
            {
                initialCursorPosition.X = Math.Min(Math.Max(0, initialCursorPosition.X + (int)(((x2 - x1) * resolutionDiffWidth) * (mouseSensitivity * 2))), screenProperties.getWidth());
                initialCursorPosition.Y = Math.Min(Math.Max(0, initialCursorPosition.Y + (int)(((y2 - y1) * resolutionDiffHeight) * (mouseSensitivity * 2))), screenProperties.getHeight());
            }

            Console.WriteLine("New Mouse X: " + initialCursorPosition.X + "/ New Mouse Y: " + initialCursorPosition.Y);

            Native.SetCursorPos(initialCursorPosition.X, initialCursorPosition.Y);
        }
    }
}
