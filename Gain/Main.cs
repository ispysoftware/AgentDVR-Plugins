using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NAudio.Dsp;
using Newtonsoft.Json;

namespace Plugins
{
    public class Main : IDisposable
    {
        private bool _disposed;
        private EqualizerBand[] _bands;
        private BiQuadFilter[,] _filters = null;
        private bool _update = true;
        private List<string> loadedAssemblies = new List<string>();

        public Main()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }
        class EqualizerBand
        {
            public float Frequency { get; set; }
            public float Gain { get; set; }
            public float Bandwidth { get; set; }
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            AssemblyName assemblyName = new AssemblyName(args.Name);
            string curAssemblyFolder = System.IO.Path.GetDirectoryName(new System.Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath);

            DirectoryInfo directoryInfo = new DirectoryInfo(curAssemblyFolder);

            foreach (FileInfo fileInfo in directoryInfo.GetFiles())
            {
                string fileNameWithoutExt = fileInfo.Name.Replace(fileInfo.Extension, "");

                if (assemblyName.Name.ToUpperInvariant() == fileNameWithoutExt.ToUpperInvariant())
                {
                    //prevent stack overflow
                    if (!loadedAssemblies.Contains(fileInfo.FullName))
                    {
                        loadedAssemblies.Add(fileInfo.FullName);
                        return Assembly.Load(AssemblyName.GetAssemblyName(fileInfo.FullName));
                    }
                }
            }

            return null;
        }

        private void CreateFilters()
        {
            _bands = new[]
                    {
                        new EqualizerBand {Bandwidth = 0.8f, Frequency = 100, Gain = Utils.ConfigObject.band1},
                        new EqualizerBand {Bandwidth = 0.8f, Frequency = 200, Gain = Utils.ConfigObject.band2},
                        new EqualizerBand {Bandwidth = 0.8f, Frequency = 400, Gain = Utils.ConfigObject.band3},
                        new EqualizerBand {Bandwidth = 0.8f, Frequency = 800, Gain = Utils.ConfigObject.band4},
                        new EqualizerBand {Bandwidth = 0.8f, Frequency = 1200, Gain =Utils.ConfigObject.band5},
                        new EqualizerBand {Bandwidth = 0.8f, Frequency = 2400, Gain =Utils.ConfigObject.band6},
                        new EqualizerBand {Bandwidth = 0.8f, Frequency = 4800, Gain =Utils.ConfigObject.band7},
                        new EqualizerBand {Bandwidth = 0.8f, Frequency = 9600, Gain =Utils.ConfigObject.band8}
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
            _update = false;
        }


        public byte[] ProcessAudioFrame(byte[] rawData, int bytesRecorded)
        {
            //16000, one channel
            byte[] truncArray = new byte[bytesRecorded];

            Array.Copy(rawData, truncArray, truncArray.Length);
            if (Utils.ConfigObject.enabled)
            {
                if (_update)
                {
                    CreateFilters();
                }
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

        public string AppPath
        {
            get;
            set;
        }

        public string AppDataPath
        {
            get;
            set;
        }

        public string ObjectName
        {
            get;
            set;
        }

        public string Result
        {
            get
            {
                //return "alert" or "detected" or some other text to trigger the pluginEvent action
                return "";
            }
        }

        public string Command(string command)
        {
            switch (command)
            {
                case "sayhello":
                    //do stuff here
                    return "{\"type\":\"success\",\"msg\":\"Hello from the Plugin!\"}";
            }

            return "{\"type\":\"error\",\"msg\":\"Command not recognised\"}";
        }

        public Exception LastException
        {
            get
            {
                var ex = Utils.LastException;
                Utils.LastException = null;
                return ex;
            }
        }

        public string GetConfiguration(string languageCode)
        {
            //populate json
            dynamic d = Utils.PopulateResponse(Utils.Json(languageCode), Utils.ConfigObject);
            return JsonConvert.SerializeObject(d);
        }

        public void SetConfiguration(string json)
        {
            //populate configObject with json values
            try
            {
                dynamic d = JsonConvert.DeserializeObject(json);
                Utils.PopulateObject(d, Utils.ConfigObject);
                _update = true;
            }
            catch (Exception ex)
            {
                Utils.LastException = ex;
            }

        }

        //Implement IDisposable.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Free other state (managed objects).
                }
                // Free your own state (unmanaged objects).
                // Set large fields to null.
                _disposed = true;
            }
        }

        public string Supports
        {
            get
            {
                return "audio";
            }
        }

        // Use C# destructor syntax for finalization code.
        ~Main()
        {
            // Simply call Dispose(false).
            Dispose(false);
        }
    }
}
