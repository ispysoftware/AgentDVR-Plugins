using System;
using System.Collections.Generic;
using System.Text;

namespace Plugins
{
    class MelSpectrogram
    {
        private readonly double[] window;
        private readonly double[] melBands;
        private readonly double[] temp1;
        private readonly double[] temp2;
        private readonly int _fftLength;
        private readonly int _nMelBands;
        private readonly double _sampleRate;
        private readonly double _logOffset;

        public MelSpectrogram(
            int sampleRate = 16000,
            int stftWindowLength = 400, int stftLength = 512,
            int nMelBands = 64, double melMinHz = 125.0, double melMaxHz = 7500.0,
            double logOffset = 0.001)
        {
            _sampleRate = sampleRate;
            window = MakeHannWindow(stftWindowLength);
            melBands = MakeMelBands(melMinHz, melMaxHz, nMelBands);
            temp1 = new double[stftLength];
            temp2 = new double[stftLength];
            _fftLength = stftLength;
            _nMelBands = nMelBands;
            _logOffset = logOffset;
        }

        static double[] MakeHannWindow(int windowLength)
        {
            double[] window = new double[windowLength];
            for (int i = 0; i < windowLength; i++)
            {
                window[i] = 0.5 * (1 - Math.Cos(2 * Math.PI * i / windowLength));
            }
            return window;
        }

        public void Transform(float[] waveform, int waveformOffset, float[] melspec, int melspecOffset)
        {
            GetFrame(waveform, waveformOffset, temp1);
            CFFT(temp1, temp2, _fftLength);
            ToMagnitude(temp2, temp1, _fftLength);
            ToMelSpec(temp2, melspec, melspecOffset);
        }

        private void ToMelSpec(double[] spec, float[] melspec, int melspecOffset)
        {
            for (int i = 0; i < _nMelBands; i++)
            {
                double startHz = melBands[i];
                double peakHz = melBands[i + 1];
                double endHz = melBands[i + 2];
                double v = 0.0;
                int j = (int)(startHz * _fftLength / _sampleRate) + 1;
                while (true)
                {
                    double hz = j * _sampleRate / _fftLength;
                    if (hz > peakHz)
                        break;
                    double r = (hz - startHz) / (peakHz - startHz);
                    v += spec[j] * r;
                    j++;
                }
                while (true)
                {
                    double hz = j * _sampleRate / _fftLength;
                    if (hz > endHz)
                        break;
                    double r = (endHz - hz) / (endHz - peakHz);
                    v += spec[j] * r;
                    j++;
                }
                melspec[melspecOffset + i] = (float)Math.Log(v + _logOffset);
            }
        }

        void GetFrame(float[] waveform, int start, double[] frame)
        {
            for (int i = 0; i < window.Length; i++)
            {
                frame[i] = waveform[start + i] * window[i];
            }
            for (int i = window.Length; i < frame.Length; i++)
            {
                frame[i] = 0.0;
            }
        }

        static void ToMagnitude(double[] xr, double[] xi, int N)
        {
            for (int n = 0; n < N; n++)
            {
                xr[n] = Math.Sqrt(xr[n] * xr[n] + xi[n] * xi[n]);
            }
        }

        static double HzToMel(double hz)
        {
            return 2595 * Math.Log10(1 + hz / 700);
        }

        static double MelToHz(double mel)
        {
            return (Math.Pow(10, mel / 2595) - 1) * 700;
        }

        static double[] MakeMelBands(double melMinHz, double melMaxHz, int nMelBanks)
        {
            double melMin = HzToMel(melMinHz);
            double melMax = HzToMel(melMaxHz);
            double[] melBanks = new double[nMelBanks + 2];
            for (int i = 0; i < nMelBanks + 2; i++)
            {
                double mel = (melMax - melMin) * i / (nMelBanks + 1) + melMin;
                melBanks[i] = MelToHz(mel);
            }
            return melBanks;
        }

        static int SwapIndex(int i)
        {
            return (i >> 8) & 0x01
                 | (i >> 6) & 0x02
                 | (i >> 4) & 0x04
                 | (i >> 2) & 0x08
                 | (i) & 0x10
                 | (i << 2) & 0x20
                 | (i << 4) & 0x40
                 | (i << 6) & 0x80
                 | (i << 8) & 0x100;
        }

        public static void CFFT(double[] xr, double[] xi, int N)
        {
            double[] t = xi;
            xi = xr;
            xr = t;
            for (int i = 0; i < N; i++)
            {
                xr[i] = xi[SwapIndex(i)];
            }
            for (int i = 0; i < N; i++)
            {
                xi[i] = 0.0;
            }
            for (int n = 1; n < N; n *= 2)
            {
                for (int j = 0; j < N; j += n * 2)
                {
                    for (int k = 0; k < n; k++)
                    {
                        double ar = Math.Cos(-Math.PI * k / n);
                        double ai = Math.Sin(-Math.PI * k / n);
                        double er = xr[j + k];
                        double ei = xi[j + k];
                        double or = xr[j + k + n];
                        double oi = xi[j + k + n];
                        double aor = ar * or - ai * oi;
                        double aoi = ai * or + ar * oi;
                        xr[j + k] = er + aor;
                        xi[j + k] = ei + aoi;
                        xr[j + k + n] = er - aor;
                        xi[j + k + n] = ei - aoi;
                    }
                }
            }
        }
    }
}
