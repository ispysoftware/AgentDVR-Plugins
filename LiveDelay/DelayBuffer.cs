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
        public static int BufferSeconds;
        public static bool BufferFull;
        public byte[] Data;
        public DateTime Created;
        public static Font DrawFont;
        
        public DelayBuffer(byte[] data)
        {
            Created = DateTime.UtcNow;
            Data = new byte[data.Length];
            Buffer.BlockCopy(data,0, Data, 0, data.Length);
        }

        private static Queue<DelayBuffer> _audioQueue = new Queue<DelayBuffer>();
        private static Queue<DelayBuffer> _videoQueue = new Queue<DelayBuffer>();

        public static byte[] GetBuffer(byte[] rawData, int bytesRecorded)
        {
            if (BufferSeconds == 0)
                return rawData;

            _audioQueue.Enqueue(new DelayBuffer(rawData));
            if (!BufferFull)
                BufferFull = _audioQueue.Peek().Created < DateTime.UtcNow.AddSeconds(0 - BufferSeconds);
            if (BufferFull)
            {
                return _audioQueue.Dequeue().Data;
            }
            //return live audio until we have a buffer
            return rawData;
        }

        public static void GetBuffer(IntPtr frame, System.Drawing.Size sz, int channels, int stride)
        {
            if (BufferSeconds == 0)
                return;
            byte[] data = new byte[stride * sz.Height];
            Marshal.Copy(frame,data,0,data.Length);
            _videoQueue.Enqueue(new DelayBuffer(data));

            if (!BufferFull)
                BufferFull = _videoQueue.Peek().Created < DateTime.UtcNow.AddSeconds(0 - BufferSeconds);

            if (BufferFull)
            {
                var d = _videoQueue.Dequeue().Data;
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
                using (var image = Image.WrapMemory<Bgr24>(frame.ToPointer(), sz.Width, sz.Height))
                {
                    const string txt = "DELAYING...";
                    FontRectangle size = TextMeasurer.Measure(txt, new RendererOptions(DrawFont));//, new TextOptions(font));          
                    var box = new Rectangle(sz.Width/2 - (int)size.Width/2 - 5, sz.Height/2 - (int)size.Height/2 - 5 , (int)size.Width+ 10, (int) size.Height+10);
                    image.Mutate(x => x.Fill(Color.Red, box));
                    image.Mutate(x => x.DrawText(txt, DrawFont, Color.White, new PointF(box.X + 5, box.Y + 5)));

                }
            }

        }

        public static void Clear()
        {
            _audioQueue = new Queue<DelayBuffer>();
            _videoQueue = new Queue<DelayBuffer>();
            BufferFull = false;
        }
    }
}
