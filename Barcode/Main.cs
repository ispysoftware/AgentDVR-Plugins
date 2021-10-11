using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ZXing;
using ZXing.Common;

namespace Plugins
{
    public class Main : IDisposable
    {
        private bool _disposed;
        private List<string> loadedAssemblies = new List<string>();
        private DateTime _lastScan = DateTime.UtcNow;

        public Main()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
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
                string r = _result;
                _result = "";
                return r;
            }
            set { _result = value; }
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
        private Task _processor;
        public void ProcessVideoFrame(IntPtr frame, Size sz, int channels, int stride)
        {
            //process frame here
            if (!Utils.ConfigObject.VideoEnabled || channels!=3)
                return;

            if (Utils.TaskRunning(_processor))
                return;

            if (_lastScan > DateTime.UtcNow.AddMilliseconds(0 - Utils.ConfigObject.Interval))
                return;

            _lastScan = DateTime.UtcNow;
            byte[] rgbRaw = new byte[stride * sz.Height];
            Marshal.Copy(frame, rgbRaw, 0, rgbRaw.Length);
            _processor = Task.Run(() => RunScanner(rgbRaw, sz.Width, sz.Height));

        }

        private void RunScanner(byte[] img, int width, int height)
        {
            var mBarcodeReader = new MultiFormatReader();


            var hints = new Dictionary<DecodeHintType, object>();
            var fmts = new List<BarcodeFormat>();

            if (Utils.ConfigObject.AZTEC)
                fmts.Add(BarcodeFormat.AZTEC);

            if (Utils.ConfigObject.CODABAR)
                fmts.Add(BarcodeFormat.CODABAR);

           
            if (Utils.ConfigObject.CODE_128)
                fmts.Add(BarcodeFormat.CODE_128);

            if (Utils.ConfigObject.CODE_39)
                fmts.Add(BarcodeFormat.CODE_39);

            if (Utils.ConfigObject.CODE_93)
                fmts.Add(BarcodeFormat.CODE_93);

            if (Utils.ConfigObject.DATA_MATRIX)
                fmts.Add(BarcodeFormat.DATA_MATRIX);

            if (Utils.ConfigObject.EAN_13)
                fmts.Add(BarcodeFormat.EAN_13);

            if (Utils.ConfigObject.EAN_8)
                fmts.Add(BarcodeFormat.EAN_8);

            if (Utils.ConfigObject.IMB)
                fmts.Add(BarcodeFormat.IMB);

            if (Utils.ConfigObject.MAXICODE)
                fmts.Add(BarcodeFormat.MAXICODE);

            if (Utils.ConfigObject.MSI)
                fmts.Add(BarcodeFormat.MSI);

            if (Utils.ConfigObject.PDF_417)
                fmts.Add(BarcodeFormat.PDF_417);

            if (Utils.ConfigObject.PHARMACODE)
                fmts.Add(BarcodeFormat.PHARMA_CODE);

            if (Utils.ConfigObject.PLESSEY)
                fmts.Add(BarcodeFormat.PLESSEY);

            if (Utils.ConfigObject.QR_CODE)
                fmts.Add(BarcodeFormat.QR_CODE);

            if (Utils.ConfigObject.RSS_14)
                fmts.Add(BarcodeFormat.RSS_14);

            if (Utils.ConfigObject.RSS_Expanded)
                fmts.Add(BarcodeFormat.RSS_EXPANDED);

            if (Utils.ConfigObject.UPC_A)
                fmts.Add(BarcodeFormat.UPC_A);

            if (Utils.ConfigObject.UPC_E)
                fmts.Add(BarcodeFormat.UPC_E);

            if (Utils.ConfigObject.UPC_EAN_Extension)
                fmts.Add(BarcodeFormat.UPC_EAN_EXTENSION);

            if (Utils.ConfigObject.Intensive)
                hints.Add(DecodeHintType.TRY_HARDER, true);

            hints.Add(DecodeHintType.POSSIBLE_FORMATS, fmts);

            Result rawResult = null;
            var r = new RGBLuminanceSource(img, width, height);
            var x = new HybridBinarizer(r);
            var bitmap = new BinaryBitmap(x);

            try
            {
                rawResult = mBarcodeReader.decode(bitmap, hints);
            }
            catch (ReaderException e)
            {

            }
            catch (Exception ex)
            {
                string m = ex.Message;
            }

            if (rawResult != null)
            {
                if (!String.IsNullOrEmpty(rawResult.Text))
                {
                    _result = rawResult.Text;
                }
            }
        }

        public string Supports
        {
            get
            {
                var t = "";
                if (Utils.ConfigObject.SupportsAudio)
                    t += "audio,";
                if (Utils.ConfigObject.SupportsVideo)
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
