using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.Fonts;

namespace Plugins
{
    internal class DelayBuffer
    {
        public int BufferSeconds;
        public bool BufferFull;
        public DateTime Created;
        private byte[] _data;
        
        public DelayBuffer(byte[] data)
        {
            Created = DateTime.UtcNow;
            _data = data;
        }

        public DelayBuffer()
        {
            Created = DateTime.UtcNow;
        }

        private Queue<DelayBuffer> _audioQueue = new Queue<DelayBuffer>();
        private Queue<DelayBuffer> _videoQueue = new Queue<DelayBuffer>();

        public byte[] GetBuffer(byte[] rawData, int bytesRecorded)
        {
            if (BufferSeconds == 0)
                return rawData;

            _audioQueue.Enqueue(new DelayBuffer(rawData));
            if (!BufferFull)
                BufferFull = _audioQueue.Peek().Created < DateTime.UtcNow.AddSeconds(0 - BufferSeconds);
            if (BufferFull)
            {
                return _audioQueue.Dequeue()._data;
            }
            //return live audio until we have a buffer
            return rawData;
        }

        public void GetBuffer(IntPtr frame, System.Drawing.Size sz, int channels, int stride, Font font)
        {
            if (BufferSeconds == 0)
                return;
            byte[] data = new byte[stride * sz.Height];
            if (frame == IntPtr.Zero || data.Length <= 0)
                return;
            Marshal.Copy(frame,data,0,data.Length);
            
            _videoQueue.Enqueue(new DelayBuffer(data));

            if (!BufferFull)
                BufferFull = _videoQueue.Peek().Created < DateTime.UtcNow.AddSeconds(0 - BufferSeconds);

            if (BufferFull)
            {
                var d = _videoQueue.Dequeue()._data;
                if (stride * sz.Height == d.Length)
                {
                    Marshal.Copy(d, 0, frame, d.Length);
                    return;
                }
                //resolution changed - need to clear buffer out
            }


            //write buffering notice
            unsafe
            {
                using (var image = Image.WrapMemory<Bgr24>(frame.ToPointer(), stride * sz.Height, sz.Width, sz.Height))
                {
                    const string txt = "DELAYING...";
                    FontRectangle size = TextMeasurer.MeasureAdvance(txt, new TextOptions(font));
                    var box = new Rectangle(sz.Width / 2 - (int)size.Width / 2 - 5, sz.Height / 2 - (int)size.Height / 2 - 5, (int)size.Width + 10, (int)size.Height + 10);
                    image.Mutate(x => x.Fill(Color.Red, box));
                    image.Mutate(x => x.DrawText(txt, font, Color.White, new PointF(box.X + 5, box.Y + 5)));

                }
            }

        }

        public void Clear()
        {
            _audioQueue = new Queue<DelayBuffer>();
            _videoQueue = new Queue<DelayBuffer>();
            BufferFull = false;
        }

        public void Close()
        {
            _audioQueue = null;
            _videoQueue = null;
        }
    }
}
