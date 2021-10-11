using System;
using System.Drawing;

namespace Plugins
{
    public static class Extensions
    {
        public static string ToRGBString(this Color color)
        {
            return color.R + "," + color.G + "," + color.B;
        }
    }
}
