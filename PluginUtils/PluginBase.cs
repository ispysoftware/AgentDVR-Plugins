using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace PluginUtils
{
    public class PluginBase
    {
        private bool _disposed;
        

        public PluginBase()
        {
            
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
        public string GetConfiguration(string languageCode)
        {
            //populate json
            dynamic d = Utils.PopulateResponse(ResourceLoader.LoadJson(languageCode), ConfigObject);
            return JsonConvert.SerializeObject(d);
        }

        public virtual void SetConfiguration(string json)
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

        public string CameraName, MicrophoneName;
        public int CameraID, MicrophoneID, LocalPort, SampleRate, Channels;

        public virtual string GetResultJSON()
        {
            var json = JsonConvert.SerializeObject(Results);
            Results.Clear();
            return json;
        }

        private List<ResultInfo> _results = new List<ResultInfo>();
        private static object ResultsLock = new object();  
        public List<ResultInfo> Results
        {
            get
            {
                lock (ResultsLock)
                    return _results;
            }
            set
            {
                lock(ResultsLock)  
                    _results = value;   
            }
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

        public virtual void ProcessAgentEvent(string ev)
        {
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

        public virtual List<string> GetCustomEvents()
        {
            return null;
        }


        public void SetCameraInfo(string name, int objectID, int localPort)
        {
            CameraName = name;
            CameraID = objectID;
            LocalPort = localPort;
        }

        public void SetMicrophoneInfo(string name, int objectID, int localPort, int sampleRate, int channels)
        {
            MicrophoneName = name;
            MicrophoneID = objectID;
            LocalPort = localPort;
            SampleRate = sampleRate;
            Channels = channels;
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
    }
}
