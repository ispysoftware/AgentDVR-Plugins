using System;
using System.Collections.Generic;
using PluginUtils;
using SixLabors.Fonts;
using System.Linq;

namespace Plugins
{
    public class Main : PluginBase, ICamera, IMicrophone
    {
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

            DelayBuffer.DrawFont = SystemFonts.CreateFont(fam.Name, 20, FontStyle.Regular);

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
            DelayBuffer.Clear();
            DelayBuffer.BufferSeconds = ConfigObject.Delay;
            
        }

        public override void ProcessAgentEvent(string ev)
        {
            
        }
        
        public byte[] ProcessAudioFrame(byte[] rawData, int bytesRecorded, int samplerate, int channels)
        {
            return DelayBuffer.GetBuffer(rawData, bytesRecorded);
        }

        public void ProcessVideoFrame(IntPtr frame, System.Drawing.Size sz, int channels, int stride)
        {
            DelayBuffer.GetBuffer(frame, sz, channels, stride);
        }

        ~Main()
        {
            DelayBuffer.Close();
            Dispose(false);
        }
    }
}
