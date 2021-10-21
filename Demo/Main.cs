using System;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using PluginUtils;
using SixLabors.Fonts;
using System.Linq;

namespace Plugins
{
    public class Main : PluginBase, ICamera, IMicrophone
    {
        private DateTime _lastAlert = DateTime.UtcNow;
        private Font _drawFont;
        private bool _needUpdate = false;
        private IPen _wirepen, _trippedpen;
        private List<Line2D> _tripwires = new List<Line2D>();

        public Main():base()
        {
            
            //get cross platform font family
            string[] fontfams = new[] { "Verdana", "Arial", "Helvetica", "Geneva", "FreeMono", "DejaVu Sans"};
            FontFamily fam = null;
            foreach(var fontfam in fontfams)
            {
                if (SystemFonts.Collection.TryFind(fontfam, out fam))
                    break;
            }
            if (fam==null)
            {
                fam = SystemFonts.Collection.Families.First();
            }

            _drawFont = SystemFonts.CreateFont(fam.Name, 20, FontStyle.Regular);
            _wirepen = Pens.Solid(Color.Green, 3);
            _trippedpen = Pens.Solid(Color.Red, 3);

        }

        public string Supports
        {
            get
            {
                return "video,audio";
            }
        }

        public override List<string> GetCustomEvents()
        {
            return new List<string>() { "Box Bounce", "Box Crossed Tripwire" };
        }

        public override void SetConfiguration(string json)
        {
            base.SetConfiguration(json);
            _needUpdate = true;
            
        }

        public override void ProcessAgentEvent(string ev)
        {
            switch(ev)
            {
                case "MotionAlert":
                    break;
                case "MotionDetect":
                    break;
                case "ManualAlert":
                    break;
                case "RecordingStart":
                    //this will tag new recordings with "Demo plugin attached"
                    Results.Add(new ResultInfo("tag", "", "Demo plugin attached"));
                    break;
                case "RecordingStop":
                    break;
                case "AudioAlert":
                    break;
                case "AudioDetect":
                    break;
            }
        }

        public byte[] ProcessAudioFrame(byte[] rawData, int bytesRecorded)
        {
            //22050, one channel
            CheckAlert();
            if (!ConfigObject.VolumeEnabled)
                return rawData;

            //demo audio effect
            return adjustVolume(rawData, Convert.ToDouble(ConfigObject.Volume) / 100d);
        }

        public void ProcessVideoFrame(IntPtr frame, System.Drawing.Size sz, int channels, int stride)
        {
            if (_needUpdate)
            {
                _tripwires = Utils.ParseTripWires(sz, ConfigObject.Example_Trip_Wires);
                _needUpdate = false;
            }
            //fire off an alert every 10 seconds
            CheckAlert();

            if (ConfigObject.MirrorEnabled)
            {

                //demo mirror effect
                var bWidth = sz.Width / ConfigObject.Size;
                unsafe
                {
                    byte* ptr = (byte*)frame;

                    for (var y = 0; y < sz.Height; y++)
                    {
                        for (var b = 0; b < ConfigObject.Size; b++)
                        {
                            int xStart = b * bWidth, xEnd = Math.Min(sz.Width, (b + 1) * bWidth);
                            int j = 0;
                            for (var x = xStart; x < xEnd; x++)
                            {
                                for (int c = 0; c < channels; c++)
                                    ptr[y * stride + (x * channels) + c] = ptr[y * stride + (xEnd - x) * channels + c];

                            }
                        }
                    }
                }
            }

            if (ConfigObject.GraphicsEnabled)
            {
                //Use SixLabors drawing for cross platform support.
                unsafe
                {
                    using (var image = Image.WrapMemory<Bgr24>(frame.ToPointer(), sz.Width, sz.Height))
                    {
                        var box = new Rectangle(recLoc, new Size(recSize, recSize));
                        image.Mutate(x => x.Fill(Color.Red, box));

                        image.Mutate(x => x.DrawText("DVD", _drawFont, Color.White, new PointF(recLoc.X + 10, recLoc.Y + 20)));

                        //draw trip wires if defined
                        if (_tripwires.Count>0)
                        {
                            foreach (var wire in _tripwires)
                            {
                                var points = new PointF[] { new PointF(wire.InitialPoint.X, wire.InitialPoint.Y), new PointF(wire.TerminalPoint.X, wire.TerminalPoint.Y) };

                                var pen = _wirepen;

                                if (Utils.LineIntersectsRect(wire.InitialPoint, wire.TerminalPoint, new System.Drawing.Rectangle(box.X, box.Y, box.Width, box.Height))) {
                                    pen = _trippedpen;
                                    if (wire.LastTripped < DateTime.UtcNow.AddSeconds(-4))
                                    {
                                        //crossed tripwire, suspend new event from this tripwire for at least 4 seconds
                                        wire.LastTripped = DateTime.UtcNow;
                                        Results.Add(new ResultInfo("Box Crossed Tripwire", "box crossed the tripwire"));
                                    }
                                }

                                image.Mutate(x => x.DrawLines(pen, points));

                            }
                        }
                        //draw rectangles if defined
                        if (!string.IsNullOrEmpty(ConfigObject.Example_Area))
                        {
                            var areas = Utils.ParseAreas(sz, ConfigObject.Example_Area);
                            foreach (var area in areas)
                            {
                                image.Mutate(x => x.Fill(Color.WhiteSmoke, new Rectangle(area.X, area.Y, area.Width, area.Height)));
                            }
                        }
                    }
                }

                //bounce rectangle about
                MoveRec(sz.Width,sz.Height);
            }
        }

        #region adjust volume
        private byte[] adjustVolume(byte[] audioSamples, double volume)
        {
            byte[] array = new byte[audioSamples.Length];
            for (int i = 0; i < array.Length; i += 2)
            {
                // convert byte pair to int
                short buf1 = audioSamples[i + 1];
                short buf2 = audioSamples[i];

                buf1 = (short)((buf1 & 0xff) << 8);
                buf2 = (short)(buf2 & 0xff);

                short res = (short)(buf1 | buf2);
                res = (short)(res * volume);

                // convert back
                array[i] = (byte)res;
                array[i + 1] = (byte)(res >> 8);

            }
            return array;
        }
        #endregion

        #region bouncing rectangle
        private Point recLoc = new Point(100, 100);
        private int recSize = 80;
        private int speed = 5;
        private int XBounce = 1;
        private int YBounce = -1;

        private void MoveRec(int width, int height)
        {
            if ((recLoc.X >= 0) && (recLoc.X + recSize <= width)) //Within X Bounds
            {
                recLoc.X -= XBounce * speed;
            }
            else
            {
                Results.Add(new ResultInfo("Box Bounce", "bounce detected"));
                XBounce = -XBounce;
                recLoc.X -= XBounce * speed;
            }

            if ((recLoc.Y >= 0) && (recLoc.Y + recSize <= height)) //Within Y Bounds
            {
                recLoc.Y -= YBounce * speed;
            }
            else
            {
                Results.Add(new ResultInfo("Box Bounce", "bounce detected"));
                YBounce = -YBounce;
                recLoc.Y -= YBounce * speed;
            }
        }
        #endregion

        private void CheckAlert()
        {
            if (ConfigObject.AlertsEnabled)
            {
                if (_lastAlert < DateTime.UtcNow.AddSeconds(-10))
                {
                    _lastAlert = DateTime.UtcNow;
                    Results.Add(new ResultInfo("alert"));
                }
            }
        }

        ~Main()
        {
            Dispose(false);
        }
    }
}
