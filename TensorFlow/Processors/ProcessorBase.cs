using Emgu.TF;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Text;
using Tensorflow;

namespace Plugins.Processors
{
    internal class ProcessorBase
    {
        
        public bool CloseWhenReady = false;


        public bool Ready { get; internal set; }

        public bool IsAudio => false;

        internal RectangleF GenRectangle(float[] r)
        {
            float x1 = r[0];
            float y1 = r[1];
            float x2 = r[2];
            float y2 = r[3];
            return new RectangleF(y1, x1, y2 - y1, x2 - x1);
        }

        private SessionOptions _so = null;
        public SessionOptions SessionOptions
        {
            get
            {
                if (_so != null)
                    return _so;
                _so = new SessionOptions();
                ConfigProto config = new ConfigProto
                {
                    LogDevicePlacement = true
                };
                if (TfInvoke.IsGoogleCudaEnabled)
                {
                    config.GpuOptions = new GPUOptions
                    {
                        AllowGrowth = true
                    };
                }
                _so.SetConfig(config.ToProtobuf());
                return _so;
            }
        }
    }
}
