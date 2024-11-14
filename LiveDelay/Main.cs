using System;
using System.Collections.Generic;
using PluginUtils;
using SixLabors.Fonts;
using System.Linq;

namespace Plugins
{
    public class Main : PluginBase, ICamera, IMicrophone
    {
        private DelayBuffer _delayBuffer;
        private Font _messageFont;

        public Main():base()
        {
            
            //get cross platform font family
            string[] fontfams = new[] { "Verdana", "Arial", "Helvetica", "Geneva", "FreeMono", "DejaVu Sans"};
            var ff = false;
            FontFamily fam;
            foreach (var fontfam in fontfams)
            {
                if (SystemFonts.Collection.TryGet(fontfam, out fam))
                {
                    ff = true;
                    break;
                }
            }
            if (!ff)
                fam = SystemFonts.Collection.Families.First();

            _messageFont = SystemFonts.CreateFont(fam.Name, 20, FontStyle.Regular);
            _delayBuffer = new DelayBuffer();
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
            return new List<string>() {  };
        }

        public override void SetConfiguration(string json)
        {
            base.SetConfiguration(json);
            _delayBuffer.Clear();
            _delayBuffer.BufferSeconds = ConfigObject.Delay;
            
        }

        public override void ProcessAgentEvent(string ev)
        {
            
        }
        
        public byte[] ProcessAudioFrame(byte[] rawData, int bytesRecorded, int samplerate, int channels)
        {
            return _delayBuffer.GetBuffer(rawData, bytesRecorded);
        }

        public void ProcessVideoFrame(IntPtr frame, System.Drawing.Size sz, int channels, int stride)
        {
            _delayBuffer.GetBuffer(frame, sz, channels, stride, _messageFont);
        }

        ~Main()
        {
            _delayBuffer?.Close();
            Dispose(false);
        }
    }
}
