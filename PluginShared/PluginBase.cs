using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using static PluginShared.Utils;

namespace PluginShared
{
    public class PluginBase
    {
        public string CameraName, MicrophoneName;
        public int CameraID, MicrophoneID, LocalPort;

        public string GetResultJSON()
        {
            var json = JsonConvert.SerializeObject(Results);
            Results.Clear();
            return json;
        }

        public List<ResultInfo> Results = new List<ResultInfo>();

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

        public void SetMicrophoneInfo(string name, int objectID, int localPort)
        {
            MicrophoneName = name;
            MicrophoneID = objectID;
            LocalPort = localPort;
        }
    }
}
