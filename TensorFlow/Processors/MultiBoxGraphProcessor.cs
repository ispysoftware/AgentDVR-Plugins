using Emgu.TF;
using Emgu.TF.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using Tensorflow;
using static Plugins.EventHandlers;

namespace Plugins.Processors
{
    internal class MultiBoxGraphProcessor : ProcessorBase, ITensorProcessor
    {
        private MultiboxGraph model;
        public Size SizeRequired => new Size(224, 224);

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
            model = new MultiboxGraph(null, SessionOptions);

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
            var results = model.Detect(t);
            List<ResultEntry> reslist = new List<ResultEntry>();
            foreach (var result in results)
            {
                reslist.Add(new ResultEntry() { Label = "Found", Probability = Convert.ToInt32(result.Scores * 100), Region = GenRectangle(result.DecodedLocations) });
            }
            if (reslist.Count > 0)
                ResultGenerated?.Invoke(this, new EventHandlers.ResultEventArgs(reslist));
        }


    }
}
