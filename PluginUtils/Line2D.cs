using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace PluginUtils
{
    public class Line2D
    {
        public Point InitialPoint;
        public Point TerminalPoint;
        public Point MidPoint;
        public DateTime LastTripped;
        public int LeftUpTripCount;
        public int RightDownTripCount;
        public int Angle;
        private Point _p1, _p2;

        public Line2D(Size frameSize, Point p1, Point p2)
        {
            _p1 = p1;
            _p2 = p2;
            Update(frameSize);
        }

        internal Point ScaleUpPercentToFrame(Size sz, Point p)
        {
            var x = (p.X / 100d) * sz.Width;
            var y = (p.Y / 100d) * sz.Height;

            return new Point(Convert.ToInt16(x), Convert.ToInt16(y));
        }

        public void Update(Size frameSize)
        {
            InitialPoint = ScaleUpPercentToFrame(frameSize, _p1);
            TerminalPoint = ScaleUpPercentToFrame(frameSize, _p2);
            LastTripped = DateTime.MinValue;
            MidPoint = new Point(InitialPoint.X + (TerminalPoint.X - InitialPoint.X) / 2, InitialPoint.Y + (TerminalPoint.Y - InitialPoint.Y) / 2);

            Angle = 0;
            if (InitialPoint != TerminalPoint)
            {
                float xDiff = InitialPoint.X - TerminalPoint.X;
                float yDiff = InitialPoint.Y - TerminalPoint.Y;

                Angle = Convert.ToInt32(Math.Atan2(yDiff, xDiff) * 180.0 / Math.PI);
                Angle = (Angle + 360) % 360;
            }
        }
    }
}
