using System;
using System.Collections.Generic;
using System.Text;

namespace Plugins
{
    class AudioFeatureBuffer
    {
        public const int InputSamplingRate = 16000;

        private readonly MelSpectrogram _processor;
        private readonly int _stftHopLength;
        private readonly int _stftWindowLength;
        private readonly int _nMelBands;

        private readonly float[] _waveformBuffer;
        private int _waveformCount;
        private readonly float[] _outputBuffer;
        private int _outputCount;

        public AudioFeatureBuffer(int stftHopLength = 160, int stftWindowLength = 400, int nMelBands = 64)
        {
            _processor = new MelSpectrogram();
            _stftHopLength = stftHopLength;
            _stftWindowLength = stftWindowLength;
            _nMelBands = nMelBands;

            _waveformBuffer = new float[2 * _stftHopLength + _stftWindowLength];
            _waveformCount = 0;
            _outputBuffer = new float[_nMelBands * (_stftWindowLength + _stftHopLength)];
            _outputCount = 0;
        }

        public int OutputCount { get { return _outputCount; } }
        public float[] OutputBuffer { get { return _outputBuffer; } }

        public float[] Resample(float[] waveform, int sampleRate)
        {
            if (sampleRate == InputSamplingRate)
            {
                return waveform;
            }
            else
            {
                int toLen = (int)(waveform.Length * ((double)InputSamplingRate / sampleRate));
                float stepRate = ((float)sampleRate) / InputSamplingRate;
                float[] toWaveform = new float[toLen];
                for (int toIndex = 0; toIndex < toWaveform.Length; toIndex++)
                {
                    int fromIndex = (int)(toIndex * stepRate);
                    if (fromIndex < waveform.Length)
                    {
                        toWaveform[toIndex] = waveform[fromIndex];
                    }
                }
                return toWaveform;
            }
        }

        public int Write(float[] waveform, int offset, int count)
        {
            int written = 0;

            if (_waveformCount > 0)
            {
                int needed = ((_waveformCount - 1) / _stftHopLength) * _stftHopLength + _stftWindowLength - _waveformCount;
                written = Math.Min(needed, count);

                Array.Copy(waveform, offset, _waveformBuffer, _waveformCount, written);
                _waveformCount += written;

                int wavebufferOffset = 0;
                while (wavebufferOffset + _stftWindowLength < _waveformCount)
                {
                    _processor.Transform(_waveformBuffer, wavebufferOffset, _outputBuffer, _outputCount);
                    _outputCount += _nMelBands;
                    wavebufferOffset += _stftHopLength;
                }

                if (written < needed)
                {
                    Array.Copy(_waveformBuffer, wavebufferOffset, _waveformBuffer, 0, _waveformCount - wavebufferOffset);
                    _waveformCount -= wavebufferOffset;
                    return written;
                }

                _waveformCount = 0;
                written -= _stftWindowLength - _stftHopLength;
            }

            while (written + _stftWindowLength < count)
            {
                if (_outputCount + _nMelBands >= _outputBuffer.Length)
                {
                    return written;
                }
                _processor.Transform(waveform, offset + written, _outputBuffer, _outputCount);
                _outputCount += _nMelBands;
                written += _stftHopLength;
            }

            Array.Copy(waveform, offset + written, _waveformBuffer, 0, count - written);
            _waveformCount = count - written;
            written = count;
            return written;
        }

        public void ConsumeOutput(int count)
        {
            Array.Copy(_outputBuffer, count, _outputBuffer, 0, _outputCount - count);
            _outputCount -= count;
        }
    }
}
