using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using PluginUtils;

namespace Plugins
{
    public class Main : PluginBase, IAgentPluginCamera, IAgentPluginMicrophone
    {
        private DateTime _lastAlert = DateTime.UtcNow;
        

        public Main()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }
                
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
        

        public override List<string> GetCustomEvents()
        {
            return new List<string>() { "Rectangle Bounce"};
        }       

        public void ProcessVideoFrame(IntPtr frame, Size sz, int channels, int stride)
        {
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
                //draw something on the frame
                using (var img = new Bitmap(sz.Width, sz.Height, stride, System.Drawing.Imaging.PixelFormat.Format24bppRgb, frame))
                {
                    using (Graphics g = Graphics.FromImage(img))
                    {
                        g.FillRectangle(Brushes.Red, new Rectangle(recLoc, new Size(recSize, recSize)));
                        g.DrawString("Hi!", new Font(new FontFamily("Verdana"), 20, FontStyle.Bold), Brushes.White, recLoc.Adjust(5, 20));

                        //draw trip wires if defined
                        if (!string.IsNullOrEmpty(ConfigObject.Example_Trip_Wires))
                        {
                            var lines = Utils.ParseTripWires(sz, ConfigObject.Example_Trip_Wires);
                            foreach(var line in lines)
                            {
                                g.DrawLine(Pens.Red, line.InitialPoint, line.TerminalPoint);
                            }
                        }
                        //draw rectangles if defined
                        if (!string.IsNullOrEmpty(ConfigObject.Example_Area))
                        {
                            var areas = Utils.ParseAreas(sz, ConfigObject.Example_Area);
                            foreach (var area in areas)
                            {
                                g.FillRectangle(Brushes.Aqua, area);
                            }
                        }
                    }
                }
                //bounce rectangle about
                MoveRec(sz.Width,sz.Height);
            }
        }

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
                Results.Add(new ResultInfo("Rectangle Bounce", "bounce detected"));
                XBounce = -XBounce;
                recLoc.X -= XBounce * speed;
            }

            if ((recLoc.Y >= 0) && (recLoc.Y + recSize <= height)) //Within Y Bounds
            {
                recLoc.Y -= YBounce * speed;
            }
            else
            {
                Results.Add(new ResultInfo("Rectangle Bounce", "bounce detected"));
                YBounce = -YBounce;
                recLoc.Y -= YBounce * speed;
            }
        }
        #endregion

        public string Supports
        {
            get
            {
                return "video,audio";
            }
        }

        ~Main()
        {
            Dispose(false);
        }
    }
}
