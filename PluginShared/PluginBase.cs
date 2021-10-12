using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using static PluginShared.Utils;

namespace PluginShared
{
    public class PluginBase
    {
        private bool _disposed;
        private readonly List<string> loadedAssemblies = new List<string>();
        public Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
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


        public string CameraName, MicrophoneName;
        public int CameraID, MicrophoneID, LocalPort;

        public virtual string GetResultJSON()
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
