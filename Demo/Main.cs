using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace Plugins
{
    public class Main : IDisposable
    {
        private bool _disposed;
        private List<string> loadedAssemblies = new List<string>();
        private DateTime _lastAlert = DateTime.UtcNow;
        private string _cameraName, _microphoneName;
        private int _cameraID, _microphoneID, _localPort;

        public Main()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        private configuration _configObject;
        public configuration ConfigObject
        {
            get
            {
                if (_configObject != null)
                    return _configObject;

                _configObject = new configuration();
                return _configObject;
            }
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            AssemblyName assemblyName = new AssemblyName(args.Name);
            string curAssemblyFolder = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);

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

        private void CheckAlert()
        {
            if (ConfigObject.AlertsEnabled)
            {
                if (_lastAlert < DateTime.UtcNow.AddSeconds(-10))
                {
                    _lastAlert = DateTime.UtcNow;
                    _result = "alert";
                }
            }
        }

        public string ProcessEvent(string ev)
        {
            switch(ev)
            {
                case "MotionAlert":
                    break;
                case "MotionDetect":
                    break;
                case "ManualAlert":
                    break;
                case "RecordingStart":
                    break;
                case "RecordingStop":
                    return "tagged by the demo plugin";
                case "AudioAlert":
                    break;
                case "AudioDetect":
                    break;
            }
            return "";
        }

        public byte[] ProcessAudioFrame(byte[] rawData, int bytesRecorded)
        {
            //22050, one channel
            CheckAlert();
            if (!ConfigObject.AudioEnabled)
                return rawData;

            //demo audio effect
            return adjustVolume(rawData, Convert.ToDouble(ConfigObject.Volume) / 100d);
        }

        private byte[] adjustVolume(byte[] audioSamples, double volume)
        {
            byte[] array = new byte[audioSamples.Length];
            for (int i = 0; i < array.Length; i += 2)
            {
                // convert byte pair to int
                short buf1 = audioSamples[i + 1];
                short buf2 = audioSamples[i];

                buf1 = (short)((buf1 & 0xff) << 8);
                buf2 = (short)(buf2 & 0xff);

                short res = (short)(buf1 | buf2);
                res = (short)(res * volume);

                // convert back
                array[i] = (byte)res;
                array[i + 1] = (byte)(res >> 8);

            }
            return array;
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

        private string _result = "";
        public string Result
        {
            get
            {
                //return "alert" or "detected" or some other text to trigger the pluginEvent action
                string r = _result;
                _result = ""; //reset
                return r;
            }
            set { _result = value; }
        }

        public void SetCameraInfo(string name, int objectID, int localPort)
        {
            _cameraName = name;
            _cameraID = objectID;
            _localPort = localPort;
        }

        public void SetMicrophoneInfo(string name, int objectID, int localPort)
        {
            _microphoneName = name;
            _microphoneID = objectID;
            _localPort = localPort;
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
            var json = ResourceLoader.LoadJson(languageCode);
            dynamic d = Utils.PopulateResponse(json, ConfigObject);
            return JsonConvert.SerializeObject(d);
        }

        public void SetConfiguration(string json)
        {
            //populate configObject with json values
            try
            {
                dynamic d = JsonConvert.DeserializeObject(json);
                Utils.PopulateObject(d, ConfigObject);
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

        public void ProcessVideoFrame(IntPtr frame, Size sz, int channels, int stride)
        {
            CheckAlert();
            //process frame here
            if (!ConfigObject.VideoEnabled)
                return;

            //demo mirror effect
            var bWidth = sz.Width / ConfigObject.Size;
            unsafe {
                byte* ptr = (byte*)frame;
                
                for (var y = 0; y < sz.Height; y++)
                {
                    for (var b = 0; b < ConfigObject.Size; b++)
                    {
                        int xStart = b * bWidth, xEnd = Math.Min(sz.Width,(b + 1) * bWidth);
                        int j = 0;
                        for (var x = xStart; x < xEnd; x++)
                        {
                            for (int c = 0; c < channels; c++)
                                ptr[y * stride + (x * channels) + c] = ptr[y * stride + (xEnd-x) * channels + c];

                        }
                    }
                }
            }
        }

        public string Supports
        {
            get
            {
                var t = "";
                if (ConfigObject.SupportsAudio)
                    t += "audio,";
                if (ConfigObject.SupportsVideo)
                    t += "video";

                return t.Trim(',');
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
