using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MoveCursorByHand.App_Code
{
    [StructLayout(LayoutKind.Sequential)]
    struct LPPoint
    {
        public int X;
        public int Y;
    }
}
