using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Text;

namespace Plugins
{
    internal class AudioHelperStream : WaveStream
    {
        private readonly WaveFormat format;
        private long position = 0;
        private readonly long length;
        private readonly byte[] _buffer;

        public AudioHelperStream(byte[] src, long length, WaveFormat format)
        {
            this.format = format;
            this.length = length;
            _buffer = src;
        }

        public override WaveFormat WaveFormat
        {
            get { return format; }
        }

        public override long Length
        {
            get { return length; }
        }

        public override long Position
        {
            get
            {
                return position;
            }
            set
            {
                position = value;
            }
        }

        public override int Read(byte[] dest, int offset, int count)
        {
            if (position >= length)
            {
                return 0;
            }
            count = (int)Math.Min(count, length - position);

            Buffer.BlockCopy(_buffer, (int)position, dest, offset, count);
            position += count;
            return count;
        }
    }
}
