using Emgu.TF;
using Emgu.TF.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using static Plugins.EventHandlers;

namespace Plugins.Processors
{
    internal class MaskRCNNProcessor : ProcessorBase, ITensorProcessor
    {
        private MaskRcnnInceptionV2Coco model;
        public Size SizeRequired => Size.Empty;

        public Session Session => null;

        public event ProcessorEventHandler ProcessorReady;
        public event ProcessorResultHandler ResultGenerated;

        public void Close()
        {
            if (Ready)
                model?.Dispose();
            else
                CloseWhenReady = true;
        }

        public async Task Init()
        {
            model = new MaskRcnnInceptionV2Coco(null, SessionOptions);
            
            await model.Init();
            if (CloseWhenReady)
            {
                model.Dispose();
                return;
            }

            Ready = true;
            ProcessorReady?.Invoke(this, EventArgs.Empty);
        }

        public void Recognise(Tensor t)
        {
            var results = model.Recognize(t);
            List<ResultEntry> reslist = new List<ResultEntry>();
            foreach (var result in results)
            {
                if (!double.IsNaN(result[0].Probability))
                    reslist.Add(new ResultEntry() { Label = result[0].Label, Probability = Convert.ToInt32(result[0].Probability * 100), Region = GenRectangle(result[0].Region)});
            }
            if (reslist.Count > 0)
                ResultGenerated?.Invoke(this, new EventHandlers.ResultEventArgs(reslist));
        }

        
    }
}
