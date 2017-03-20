using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoveCursorByHand.App_Code
{
    public class Resolution
    {
        private int width;
        private int height;

        public int getWidth() { return width; }
        public int getHeight() { return height; }

        public Resolution(int width, int height)
        {
            this.width = width;
            this.height = height;
        }
    }
}
