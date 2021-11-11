using System;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing;
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

namespace Plugins
{
    public class Main : PluginBase, ICamera, IMicrophone
    {
        private Font _drawFont;
        private bool _needUpdate = false;
        private ITensorProcessor _processor;
        
        public Main() : base()
        {
            //get cross platform font family
            string[] fontfams = new[] { "Verdana", "Arial", "Helvetica", "Geneva", "FreeMono", "DejaVu Sans" };
            FontFamily fam = null;
            foreach (var fontfam in fontfams)
            {
                if (SystemFonts.Collection.TryFind(fontfam, out fam))
                    break;
            }
            if (fam == null)
            {
                fam = SystemFonts.Collection.Families.First();
            }

            _drawFont = SystemFonts.CreateFont(fam.Name, 20, FontStyle.Regular);
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

        public byte[] ProcessAudioFrame(byte[] rawData, int bytesRecorded)
        {
            //22050, one channel
            if (_processor?.Ready ?? false)
            {
                if (_processor.IsAudio)
                {
                    
                }
                
            }

            return rawData;
        }

        private Task _processorTask;
        public void ProcessVideoFrame(IntPtr frame, System.Drawing.Size sz, int channels, int stride)
        {
            if (_needUpdate)
            {
                Initialize();
            }

            var _area = Rectangle.Empty;
            if (!string.IsNullOrEmpty(ConfigObject.Area))
            {
                var i = Array.ConvertAll(ConfigObject.Area.Split(','), int.Parse);
                var r = Utils.ScalePercentageRectangle(new System.Drawing.Rectangle(i[0], i[1], i[2], i[3]), sz);
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
                        var targetSize = _processor.SizeRequired == System.Drawing.Size.Empty ? sz : _processor.SizeRequired;
                        unsafe
                        {
                            var buffer = new ReadOnlySpan<byte>((void*)frame, stride * sz.Height);
                            using (var image = Image.Load<Bgr24>(buffer, out _))
                            {
                                ResizeOptions options = new ResizeOptions
                                {
                                    TargetRectangle = _area,
                                    Compand = true,
                                    Mode = ResizeMode.Max,
                                    Size = new Size(targetSize.Width, targetSize.Height)
                                };
                                image.Mutate(x => x.Resize(options));

                                if (image.TryGetSinglePixelSpan(out var span))
                                {
                                    byte[] rgbBytes = MemoryMarshal.AsBytes(span).ToArray();
                                    t = new Tensor(DataType.Uint8, new int[] { 1, _processor.SizeRequired.Width, _processor.SizeRequired.Height, 3 });
                                    Marshal.Copy(rgbBytes, 0, t.DataPointer, rgbBytes.Length);
                                }
                                else
                                    return;

                            }
                            
                        }
                        _processorTask = Task.Run(()=>_processor.Recognise(t));
                    }
                }
            }

            if (ConfigObject.Overlay) {
                var lres = lockedResults.ToList();
                if (lres.Count == 0)
                    return;
                unsafe
                {
                    using (var image = Image.WrapMemory<Bgr24>(frame.ToPointer(), sz.Width, sz.Height))
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
