using System;
using System.Drawing;

namespace PluginShared
{
    public static class Extensions
    {
        public static string ToRGBString(this Color color)
        {
            return color.R + "," + color.G + "," + color.B;
        }
    }
}
