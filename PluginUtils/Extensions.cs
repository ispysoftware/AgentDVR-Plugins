using System;
using System.Drawing;

namespace PluginUtils
{
    public static class Extensions
    {
        public static string ToRGBString(this Color color)
        {
            return color.R + "," + color.G + "," + color.B;
        }

        public static Point Adjust(this Point point, int x, int y)
        {
            return new Point(point.X + x, point.Y + y);
        }
    }
}
