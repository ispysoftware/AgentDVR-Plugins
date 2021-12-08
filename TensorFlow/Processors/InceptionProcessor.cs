using Emgu.TF;
using Emgu.TF.Models;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tensorflow;
using static Plugins.EventHandlers;

namespace Plugins.Processors
{
    internal class InceptionProcessor: ProcessorBase, ITensorProcessor
    {
        private Inception model;
        public event EventHandlers.ProcessorEventHandler ProcessorReady;
        public event EventHandlers.ProcessorResultHandler ResultGenerated;

        public Size SizeRequired => new Size(224, 224);//, 299);


        public async Task Init()
        {
            //inceptionGraph = new Inception();

            //inceptionGraph.OnDownloadProgressChanged += InceptionGraph_OnDownloadProgressChanged;
            //inceptionGraph.Init(
            //    new string[] { "tensorflow_inception_graph.pb", "imagenet_comp_graph_label_strings.txt" },
            //    "https://github.com/emgucv/models/blob/master/inception/",
            //    "Placeholder",
            //    "final_result");


            model = new Inception(null, SessionOptions);
            model.OnDownloadProgressChanged += _inceptionGraph_OnDownloadProgressChanged; ;
            
            //use a retrained model to recognize flowers
           
            try
            {
                //await model.Init(
                //  new string[] { "optimized_graph.pb", "output_labels.txt" },
                //  "https://github.com/emgucv/models/raw/master/inception_flower_retrain/",
                //  "Placeholder",
                //  "final_result");


                await model.Init(null);
                    //new string[] { "tensorflow_inception_graph.pb", "imagenet_comp_graph_label_strings.txt" },
                    //"https://github.com/dotnet/machinelearning-samples/blob/main/samples/csharp/getting-started/DeepLearning_ImageClassification_TensorFlow/ImageClassification/assets/inputs/inception/"
                    //,


                    //, 
                    //null);
            }
            catch (Exception ex)
            {
                var m = ex.Message;
            }



            Ready = true;
            if (CloseWhenReady)
            {
                model.Dispose();
                return;
            }

            ProcessorReady?.Invoke(this, EventArgs.Empty);
        }

        public Session Session => model?.Session;

        public void Close()
        {
            if (Ready)
                model?.Dispose();
            else
                CloseWhenReady = true;
        }

        private void _inceptionGraph_OnDownloadProgressChanged(object sender, System.Net.DownloadProgressChangedEventArgs e)
        {
            
        }

        public void Recognise(Tensor t)
        {
            var results = model.Recognize(t);
            List<ResultEntry> reslist = new List<ResultEntry>();
            foreach(var result in results)
            {
                if (!double.IsNaN(result[0].Probability))
                    reslist.Add(new ResultEntry() { Label = result[0].Label, Probability = Convert.ToInt32(result[0].Probability * 100) });
            }
            if (reslist.Count > 0)
                ResultGenerated?.Invoke(this, new ResultEventArgs(reslist));
        }

        private void InceptionGraph_OnDownloadProgressChanged(object sender, System.Net.DownloadProgressChangedEventArgs e)
        {
            if (e.TotalBytesToReceive == 0)
            {

            }
        }
    }
}
