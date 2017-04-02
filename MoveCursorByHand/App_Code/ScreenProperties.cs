using System.Drawing;
using System.Windows.Forms;

namespace MoveCursorByHand.App_Code
{
    class ScreenProperties
    {
        private int width = 0, height = 0;

        public ScreenProperties(Control control)
        {
            Screen screen = Screen.FromControl(control);
            Rectangle screenArea = screen.WorkingArea;
            width = screenArea.Width;
            height = screenArea.Height;
        }

        public ScreenProperties()
        {
            Rectangle wholeScreenArea = new Rectangle(0, 0, 0, 0);
            Screen[] screens = Screen.AllScreens;
            foreach(Screen screen in screens)
            {
                wholeScreenArea = Rectangle.Union(wholeScreenArea, screen.Bounds);
            }
            width = wholeScreenArea.Width;
            height = wholeScreenArea.Height;
        }

        public int getWidth()
        {
            return width;
        }

        public int getHeight()
        {
            return height;
        }
    }
}
