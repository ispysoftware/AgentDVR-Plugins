using System;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using PluginUtils;
using SixLabors.Fonts;
using System.Linq;
using Emgu.TF;
using System.Threading.Tasks;
using Plugins.Processors;
using System.Runtime.InteropServices;
using System.Text;
using System.Diagnostics;
using Newtonsoft.Json;

namespace Plugins
{
    public class Main : PluginBase, ICamera
    {
        private Font _drawFont;
        private bool _needUpdate = false;
        private ITensorProcessor _processor;
        private static readonly string[] fontfams = new[] { "Verdana", "Arial", "Helvetica", "Geneva", "FreeMono", "DejaVu Sans" };

        public Main() : base()
        {
            //get cross platform font family           
            FontFamily fam = SystemFonts.Collection.Families.First();
            foreach (var fontfam in fontfams)
            {
                if (SystemFonts.Collection.TryGet(fontfam, out fam))
                    break;
            }
            
            _drawFont = SystemFonts.CreateFont(fam.Name, 20, FontStyle.Regular);
        }

        public string Supports
        {
            get
            {
                return "video";
            }
        }

        public override List<string> GetCustomEvents()
        {
            return new List<string>() { "Tensor Triggered", "Tensor Result" };
        }

        public override void SetConfiguration(string json)
        {
            base.SetConfiguration(json);
            _needUpdate = true;

        }

        public override void ProcessAgentEvent(string ev)
        {
            switch (ev)
            {
                case "MotionAlert":
                    break;
                case "MotionDetect":
                    break;
                case "ManualAlert":
                    break;
                case "RecordingStart":
                    break;
                case "RecordingStop":
                    break;
                case "AudioAlert":
                    break;
                case "AudioDetect":
                    break;
            }
        }

        private void Initialize()
        {
            if (_processor!=null)
            {
                if (!_processor.Ready) //wait till it's ready before we close it
                    return;
                _processor.ProcessorReady -= _processor_ProcessorReady;
                _processor.Close();
            }

            _processor = null;

            switch (ConfigObject.Model)
            {
                case "Inception":
                    _processor = new InceptionProcessor();
                    break;
                case "MaskRcnnInceptionV2Coco":
                    _processor = new MaskRCNNProcessor();
                    break;
                case "MultiboxGraph":
                    _processor = new MultiBoxGraphProcessor();
                    break;
                case "Resnet":
                    _processor = new ResnetProcessor();
                    break;
            }
            if (_processor != null)
            {
                _processor.ProcessorReady += _processor_ProcessorReady;
                _processor.ResultGenerated += _processor_ResultGenerated;
                Task.Run(() => _processor.Init());
            }
        }

        private void _processor_ResultGenerated(ITensorProcessor sender, EventHandlers.ResultEventArgs e)
        {
            var results = e.Results.Where(p=>p.Probability> ConfigObject.MinConfidence).ToList();
            lockedResults = results;

            
            foreach(var result in results)
                Results.Add(new ResultInfo("Tensor Result", result.Label, result.Label, e.ToString()));
                       
            
        }

        private void _processor_ProcessorReady(ITensorProcessor sender, EventArgs e)
        {
            Session.Device[] devices = GetSessionDevices(sender.Session);
            StringBuilder sb = new StringBuilder();
            foreach (Session.Device d in devices)
            {
                sb.Append($"{d.Type}: {d.Name}{Environment.NewLine}");
            }
            Debug.WriteLine($"Session Devices:{Environment.NewLine}{sb}");
        }

        private static Session.Device[] GetSessionDevices(Session session)
        {
            if (session == null)
                return null;
            return session.ListDevices(null);
        }

        private Task _processorTask;
        public void ProcessVideoFrame(IntPtr frame, System.Drawing.Size sz, int channels, int stride)
        {
            if (_needUpdate)
            {
                _needUpdate = false;
                Initialize();
                return;
            }

            var _area = Rectangle.Empty;
            if (!string.IsNullOrEmpty(ConfigObject.Area))
            {
                dynamic zone = JsonConvert.DeserializeObject(ConfigObject.Area);
                var d = zone[0];
                var x = Convert.ToInt32(d["x"].Value);
                var y = Convert.ToInt32(d["y"].Value);
                var w = Convert.ToInt32(d["w"].Value);
                var h = Convert.ToInt32(d["h"].Value);

                var r = Utils.ScalePercentageRectangle(new System.Drawing.Rectangle(x,y,w,h), sz);
                r.Height = r.Width;
                if (r.Top + r.Height > sz.Height)
                    r.Height = sz.Height - r.Top;
                if (r.Left + r.Width > sz.Width)
                    r.Width= sz.Width - r.Left;
                r.Height = Math.Min(r.Height, r.Width);
                r.Width = Math.Min(r.Width, r.Height);


                _area = new Rectangle(r.X, r.Y, r.Width, r.Height);
            }


            if (_processor?.Ready ?? false)
            {
                if (!_processor.IsAudio)
                {
                    if (!Utils.TaskRunning(_processorTask)) {
                        if (_area == Rectangle.Empty)
                            return;

                        Tensor t;
                        var targetSize = _processor.SizeRequired == Size.Empty ? new Size(_area.Width,_area.Height) : _processor.SizeRequired;
                        unsafe
                        {
                            using (var image = Image.WrapMemory<Bgr24>((void*) frame, stride, sz.Width, sz.Height))
                            {
                                using (Image<Bgr24> copy = image.Clone(x => x.Resize(targetSize.Width, targetSize.Height, KnownResamplers.Bicubic, _area, new Rectangle(0, 0, targetSize.Width, targetSize.Height), true)))
                                {
                                    Memory<Bgr24> data;
                                    if (copy.DangerousTryGetSinglePixelMemory(out data))
                                    {
                                        var bytes = MemoryMarshal.AsBytes(data.Span);
                                        t = new Tensor(DataType.Float, new int[] { 1, targetSize.Height, targetSize.Width, 3 });
                                        var dataPtr = t.DataPointer;
                                        using (var mh = data.Pin())
                                        {
                                            var step = Emgu.TF.Util.Toolbox.Pixel24ToPixelFloat((IntPtr)mh.Pointer, targetSize.Width, targetSize.Height, 0, 1.0f / 255.0f, false, false, dataPtr);
                                            _processorTask = Task.Run(() => _processor.Recognise(t));
                                        }
                                    }
                                }
                            }
                            
                        }
                        //
                    }
                }
            }

            if (ConfigObject.Overlay && _lockedResults!=null) {
                var lres = lockedResults.ToList();
                if (lres.Count == 0)
                    return;
                unsafe
                {
                    using (var image = Image.WrapMemory<Bgr24>((void*)frame, stride, sz.Width, sz.Height))
                    {
                        foreach (var l in lres)
                        {
                            if (l.Region != Rectangle.Empty)
                            {
                                var box = TranslateToOriginalArea(l.Region, _area);
                                image.Mutate(x => x.Draw(Color.Red, 2, box));
                                image.Mutate(x => x.DrawText($"{l.Label} ({l.Probability}%)", _drawFont, Color.White, new PointF(box.X + 10, box.Y + 20)));
                            }
                        }
                    }
                }
            }
            
        }
        private static float[] ConvertByteToFloat(byte[] array)
        {
            float[] floatValues = new float[array.Length / 3];

            int j = 0;
            for (int i = 0; i < array.Length; i+=3)
            {
                int v = Bytes2Int(array[i],array[i+1],array[i+2]);
                floatValues[j] = v;
                j++;
            }
            return floatValues;
        }

        private static int Bytes2Int(byte b1, byte b2, byte b3)
        {
            int r = 0;
            byte b0 = 0xff;

            if ((b1 & 0x80) != 0) r |= b0 << 24;
            r |= b1 << 16;
            r |= b2 << 8;
            r |= b3;
            return r;
        }

        private RectangleF TranslateToOriginalArea(RectangleF from, Rectangle source)
        {
            return new RectangleF(source.Left + from.Width * source.Width,source.Top + from.Height * source.Height, from.Width * source.Width,from.Height * source.Height);
        }

        private List<EventHandlers.ResultEntry> _lockedResults = null;
        private List<EventHandlers.ResultEntry> lockedResults
        {
            get
            {
                lock (this)
                    return _lockedResults;
            }
            set
            {
                lock (this)
                    _lockedResults = value.ToList();
            }
        }

        ~Main()
        {
            Dispose(false);
        }
    }
}
