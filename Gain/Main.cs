using System;
using System.Collections.Generic;
using NAudio.Dsp;
using Newtonsoft.Json;
using PluginUtils;

namespace Plugins
{
    public class Main : PluginBase, IMicrophone
    {
        
        private EqualizerBand[] _bands;
        private BiQuadFilter[,] _filters = null;
        
        public Main(): base()
        {
            CreateFilters();
        }

        public string Supports
        {
            get
            {
                return "audio";
            }
        }

        public byte[] ProcessAudioFrame(byte[] rawData, int bytesRecorded)
        {
            byte[] truncArray = new byte[bytesRecorded];

            Array.Copy(rawData, truncArray, truncArray.Length);
            if (ConfigObject.enabled)
            {
                List<float> samples = new List<float>();
                for (int n = 0; n < bytesRecorded; n += 2)
                {
                    float sampleValue = BitConverter.ToInt16(truncArray, n) / 32768f;
                    for (int band = 0; band < _bands.Length; band++)
                    {
                        sampleValue = _filters[0, band].Transform(sampleValue);
                    }

                    samples.Add(sampleValue);
                }

                truncArray = GetSamplesWaveData(samples.ToArray(), samples.Count);

            }
            return truncArray;
        }

        #region audio processing
        private static byte[] GetSamplesWaveData(float[] samples, int samplesCount)
        {
            var pcm = new byte[samplesCount * 2];
            int sampleIndex = 0,
                pcmIndex = 0;

            while (sampleIndex < samplesCount)
            {
                var outsample = (short)(samples[sampleIndex] * short.MaxValue);
                pcm[pcmIndex] = (byte)(outsample & 0xff);
                pcm[pcmIndex + 1] = (byte)((outsample >> 8) & 0xff);

                sampleIndex++;
                pcmIndex += 2;
            }

            return pcm;
        }        

        private void CreateFilters()
        {
            _bands = new[]
                    {
                        new EqualizerBand {Bandwidth = 0.8f, Frequency = 100, Gain = ConfigObject.band1},
                        new EqualizerBand {Bandwidth = 0.8f, Frequency = 200, Gain = ConfigObject.band2},
                        new EqualizerBand {Bandwidth = 0.8f, Frequency = 400, Gain = ConfigObject.band3},
                        new EqualizerBand {Bandwidth = 0.8f, Frequency = 800, Gain = ConfigObject.band4},
                        new EqualizerBand {Bandwidth = 0.8f, Frequency = 1200, Gain = ConfigObject.band5},
                        new EqualizerBand {Bandwidth = 0.8f, Frequency = 2400, Gain = ConfigObject.band6},
                        new EqualizerBand {Bandwidth = 0.8f, Frequency = 4800, Gain = ConfigObject.band7},
                        new EqualizerBand {Bandwidth = 0.8f, Frequency = 9600, Gain = ConfigObject.band8}
                    };

            if (_filters == null)
                _filters = new BiQuadFilter[1, _bands.Length];

            for (int bandIndex = 0; bandIndex < _bands.Length; bandIndex++)
            {
                var band = _bands[bandIndex];
                for (int n = 0; n < 1; n++)
                {
                    if (_filters[n, bandIndex] == null)
                        _filters[n, bandIndex] = BiQuadFilter.PeakingEQ(16000, band.Frequency, band.Bandwidth, band.Gain);
                    else
                        _filters[n, bandIndex].SetPeakingEq(16000, band.Frequency, band.Bandwidth, band.Gain);
                }
            }
        }

        class EqualizerBand
        {
            public float Frequency { get; set; }
            public float Gain { get; set; }
            public float Bandwidth { get; set; }
        }
        #endregion

        ~Main()
        {
            Dispose(false);
        }

    }
}
