using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PluginShared;
using ZXing;
using ZXing.Common;

namespace Plugins
{
    public class Main : PluginBase, IAgentPluginCamera
    {
        private DateTime _lastScan = DateTime.UtcNow;

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
        public string GetConfiguration(string languageCode)
        {
            //populate json
            dynamic d = Utils.PopulateResponse(ResourceLoader.LoadJson(languageCode), ConfigObject);
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

        private Task _processor;
        public void ProcessVideoFrame(IntPtr frame, Size sz, int channels, int stride)
        {
            if (channels!=3)
                return;

            if (Utils.TaskRunning(_processor))
                return;

            if (_lastScan > DateTime.UtcNow.AddMilliseconds(0 - ConfigObject.Interval))
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

            if (ConfigObject.AZTEC)
                fmts.Add(BarcodeFormat.AZTEC);

            if (ConfigObject.CODABAR)
                fmts.Add(BarcodeFormat.CODABAR);

           
            if (ConfigObject.CODE_128)
                fmts.Add(BarcodeFormat.CODE_128);

            if (ConfigObject.CODE_39)
                fmts.Add(BarcodeFormat.CODE_39);

            if (ConfigObject.CODE_93)
                fmts.Add(BarcodeFormat.CODE_93);

            if (ConfigObject.DATA_MATRIX)
                fmts.Add(BarcodeFormat.DATA_MATRIX);

            if (ConfigObject.EAN_13)
                fmts.Add(BarcodeFormat.EAN_13);

            if (ConfigObject.EAN_8)
                fmts.Add(BarcodeFormat.EAN_8);

            if (ConfigObject.IMB)
                fmts.Add(BarcodeFormat.IMB);

            if (ConfigObject.MAXICODE)
                fmts.Add(BarcodeFormat.MAXICODE);

            if (ConfigObject.MSI)
                fmts.Add(BarcodeFormat.MSI);

            if (ConfigObject.PDF_417)
                fmts.Add(BarcodeFormat.PDF_417);

            if (ConfigObject.PHARMACODE)
                fmts.Add(BarcodeFormat.PHARMA_CODE);

            if (ConfigObject.PLESSEY)
                fmts.Add(BarcodeFormat.PLESSEY);

            if (ConfigObject.QR_CODE)
                fmts.Add(BarcodeFormat.QR_CODE);

            if (ConfigObject.RSS_14)
                fmts.Add(BarcodeFormat.RSS_14);

            if (ConfigObject.RSS_Expanded)
                fmts.Add(BarcodeFormat.RSS_EXPANDED);

            if (ConfigObject.UPC_A)
                fmts.Add(BarcodeFormat.UPC_A);

            if (ConfigObject.UPC_E)
                fmts.Add(BarcodeFormat.UPC_E);

            if (ConfigObject.UPC_EAN_Extension)
                fmts.Add(BarcodeFormat.UPC_EAN_EXTENSION);

            if (ConfigObject.Intensive)
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
                    Results.Add(new Utils.ResultInfo("Barcode Recognized", rawResult.Text));
                }
            }
        }

        public override List<string> GetCustomEvents()
        {
            return new List<string> { "Barcode Recognized" };
        }

        public string Supports
        {
            get
            {
                return "video";
            }
        }

        ~Main()
        {
            Dispose(false);
        }
    }
}
