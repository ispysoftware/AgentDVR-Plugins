using System;
using System.Collections.Generic;
using System.Text;

namespace PluginUtils
{
    public class Points
    {
        public float x, y, x2, y2;
    }

    //properties, not fields — System.Text.Json ignores fields unless IncludeFields is set
    public class PolygonPoint
    {
        public float x { get; set; }
        public float y { get; set; }
    }
}
