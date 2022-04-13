using System;
using System.Collections.Generic;
using System.Linq;
using NAudio.Dsp;
using Newtonsoft.Json;
using PluginUtils;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using NAudio.Wave;
using System.Globalization;

namespace Plugins
{
    public class Main : PluginBase, IMicrophone
    {
        private const int AudioBufferLengthSec = 10;
        private int audioOffset;
        private AudioFeatureBuffer featureBuffer = new AudioFeatureBuffer();
        private bool _update;

        

        public Main(): base()
        {
            
        }

        public override List<string> GetCustomEvents()
        {
            return new List<string> { "Sound Detected", "Sound Recognized" };
        }

        public string Supports
        {
            get
            {
                return "audio";
            }
        }

        public override void SetConfiguration(string json)
        {
            base.SetConfiguration(json);
            _update = true;
        }
        private static List<YamClass> _classes = null;
        private static List<YamClass> Classes
        {
            get
            {
                if (_classes!=null)
                    return _classes;

                _classes = new List<YamClass>();
                var txt = ResourceLoader.GetResourceText("yamnet_class_map.csv");
                var lines = txt.Split(Environment.NewLine.ToCharArray());
                for(var i=1;i<lines.Length;i++)
                {
                    var line = lines[i];
                    if (string.IsNullOrEmpty(line))
                        continue;
                    var parts = line.Split(',');
                    int id = Convert.ToInt32(parts[0]);
                    string name = parts[2];
                    if (line.IndexOf("\"")>0)
                    {
                        name = line.Substring(line.IndexOf("\"")).Trim('"');
                    }
                    name = name.Replace(",", "-").ToLowerInvariant();
                    _classes.Add(new YamClass() { id = id, name = name });

                }
                _classes = _classes.OrderBy(p => p.name).ToList();
                return _classes;
            }
        }


        private struct YamClass
        {
            public int id;
            public string name;
        }


        private static string ClassesJSON()
        {
            var js = "[";
            foreach (var c in Classes)
            {
                js += "{\"original\":" + JsonConvert.ToString(c.name) + ",\"translated\":" + JsonConvert.ToString(c.name) + "},";
            }
            js = js.Trim(',') + "]";
            return js;
        }

        public override string GetConfiguration(string languageCode)
        {
            //populate json
            var json = ResourceLoader.LoadJson(languageCode);
            json = json.Replace("\"YAMOBJECTS\"", ClassesJSON());
            json = json.Replace("OBJECTS_SELECTED", ConfigObject.listenfor);

            dynamic d = Utils.PopulateResponse(json, ConfigObject);
            return JsonConvert.SerializeObject(d);
        
        }

       
        public byte[] ProcessAudioFrame(byte[] rawData, int bytesRecorded, int samplerate, int channels)
        {
            if (ConfigObject.enabled)
            {
                //convert to float array
                List<float> _fsamples = new List<float>();
                var skip = channels * 2;
                for(var i = 0; i < rawData.Length; i+=skip)
                {
                    Int16 s = BitConverter.ToInt16(rawData, i);
                    float f = s / 32768f;
                    _fsamples.Add(f);
                }

                var waveform = featureBuffer.Resample(_fsamples.ToArray(), samplerate);
                int offset = 0;
                while (offset < waveform.Length)
                {
                    int written = featureBuffer.Write(waveform, offset, waveform.Length - offset);
                    offset += written;
                    while (featureBuffer.OutputCount >= 96 * 64)
                    {
                        try
                        {
                            var features = new float[96 * 64];
                            Array.Copy(featureBuffer.OutputBuffer, 0, features, 0, 96 * 64);
                            OnPatchReceived(features);
                        }
                        finally
                        {
                            featureBuffer.ConsumeOutput(48 * 64);
                        }
                    }
                }
            }
            return rawData;
        }



        private InferenceSession modelSession = null;

        #region audio processing
        private void OnPatchReceived(float[] features)
        {
            if (modelSession == null)
            {
                modelSession = new InferenceSession(ResourceLoader.GetResourceBytes("yamnet.onnx"));   
            }

            var inputMeta = modelSession.InputMetadata;
            var container = new List<NamedOnnxValue>();

            var name = inputMeta.Keys.First();
            
            var tensor = new DenseTensor<float>(features, inputMeta[name].Dimensions);
            container.Add(NamedOnnxValue.CreateFromTensor<float>(name, tensor));
            using (var results = modelSession.Run(container))
            {
                var r = results.First().AsTensor<float>();
                int prediction = MaxProbability(r, out var max);
                var c = Classes.First(p => p.id == prediction).name;
                var aijson = "{\"sound\":" + JsonConvert.ToString(c) + ",\"probability\": " + max.ToString(CultureInfo.InvariantCulture) + "}";
                if (max * 100 >= ConfigObject.confidence)
                {
                    Results.Add(new ResultInfo("Sound Recognized", c, c, aijson));
                    if (ConfigObject.alerts)
                    {
                        if ((","+ConfigObject.listenfor+",").Contains(","+c+","))
                            Results.Add(new ResultInfo("alert", c, c, aijson));

                    }
                }
            }
        }

        static int MaxProbability(Tensor<float> probabilities, out float max)
        {
            max = -9999.9F;
            int maxIndex = -1;
            for (int i = 0; i < probabilities.Length; ++i)
            {
                float prob = probabilities.GetValue(i);
                if (prob > max)
                {
                    max = prob;
                    maxIndex = i;
                }
            }
            return maxIndex;

        }


        #endregion

        ~Main()
        {
            modelSession?.Dispose();
            Dispose(false);
        }

    }
}
