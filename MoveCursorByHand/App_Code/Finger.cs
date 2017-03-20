using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoveCursorByHand.App_Code
{
    public enum Finger
    {
        THUMB,
        INDEX,
        MIDDLE,
        RING,
        LITTLE,
        UNKNOWN
    }

    public static class FingerFunctions
    {
        public static T getNext<T>(this T src) where T : struct
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException(string.Format("Argument {0} is not an Enum", typeof(T).FullName));

            T[] enumArray = (T[])Enum.GetValues(src.GetType());
            int index = Array.IndexOf(enumArray, src) + 1;
            return (index == enumArray.Length ? enumArray[0] : enumArray[index]);
        }

        public static T getPrevious<T>(this T src) where T : struct
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException(string.Format("Argument {0} is not an Enum", typeof(T).FullName));

            T[] enumArray = (T[])Enum.GetValues(src.GetType());
            int index = Array.IndexOf(enumArray, src) - 1;
            return (index == -1 ? enumArray[enumArray.Length - 1] : enumArray[index]);
        }
    }
}
