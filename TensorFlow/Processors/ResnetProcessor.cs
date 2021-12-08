using Emgu.TF;
using Emgu.TF.Models;
using System;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using System.Threading.Tasks;
using Tensorflow;
using static Plugins.EventHandlers;

namespace Plugins.Processors
{
    internal class ResnetProcessor : ProcessorBase, ITensorProcessor
    {
        private Resnet model;
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
            model = new Resnet(null, SessionOptions);

            await model.Init();
            if (CloseWhenReady)
            {
                model.Dispose();
                return;
            }

            MetaGraphDef metaGraphDef = MetaGraphDef.Parser.ParseFrom(model.MetaGraphDefBuffer.Data);
            var signatureDef = metaGraphDef.SignatureDef["serving_default"];
            var inputNode = signatureDef.Inputs;
            var outputNode = signatureDef.Outputs;

            HashSet<string> opNames = new HashSet<string>();
            HashSet<string> couldBeInputs = new HashSet<string>();
            HashSet<string> couldBeOutputs = new HashSet<string>();
            foreach (Operation op in model.Graph)
            {

                String name = op.Name;
                opNames.Add(name);

                if (op.NumInputs == 0 && op.OpType.Equals("Placeholder"))
                {
                    couldBeInputs.Add(op.Name);
                    AttrMetadata dtypeMeta = op.GetAttrMetadata("dtype");
                    AttrMetadata shapeMeta = op.GetAttrMetadata("shape");
                    Emgu.TF.DataType type = op.GetAttrType("dtype");
                    Int64[] shape = op.GetAttrShape("shape");
                    Emgu.TF.Buffer valueBuffer = op.GetAttrValueProto("shape");
                    Emgu.TF.Buffer shapeBuffer = op.GetAttrTensorShapeProto("shape");
                    TensorShapeProto shapeProto = TensorShapeProto.Parser.ParseFrom(shapeBuffer.Data);
                }

                if (op.OpType.Equals("Const"))
                {
                    AttrMetadata dtypeMeta = op.GetAttrMetadata("dtype");
                    AttrMetadata valueMeta = op.GetAttrMetadata("value");
                    using (Tensor valueTensor = op.GetAttrTensor("value"))
                    {
                        var dim = valueTensor.Dim;
                    }
                }

                if (op.OpType.Equals("Conv2D"))
                {
                    AttrMetadata stridesMeta = op.GetAttrMetadata("strides");
                    AttrMetadata paddingMeta = op.GetAttrMetadata("padding");
                    AttrMetadata boolMeta = op.GetAttrMetadata("use_cudnn_on_gpu");
                    Int64[] strides = op.GetAttrIntList("strides");
                    bool useCudnn = op.GetAttrBool("use_cudnn_on_gpu");
                    String padding = op.GetAttrString("padding");
                }

                foreach (Output output in op.Outputs)
                {
                    int[] shape = model.Graph.GetTensorShape(output);
                    if (output.NumConsumers == 0)
                    {
                        couldBeOutputs.Add(name);
                    }
                }

                Emgu.TF.Buffer buffer = model.Graph.GetOpDef(op.OpType);
                OpDef opDef = OpDef.Parser.ParseFrom(buffer.Data);
            }

            using (Emgu.TF.Buffer versionDef = model.Graph.Versions())
            {
                int l = versionDef.Length;
            }

            Ready = true;
            ProcessorReady?.Invoke(this, EventArgs.Empty);
        }

        public void Recognise(Tensor t)
        {
            Resnet.RecognitionResult[][] results = model.Recognize(t);
            List<ResultEntry> reslist = new List<ResultEntry>();
            foreach (var result in results)
            {
                if (!double.IsNaN(result[0].Probability))
                    reslist.Add(new ResultEntry() { Label = result[0].Label, Probability = Convert.ToInt32(result[0].Probability * 100)});
            }

            if (reslist.Count > 0)
                ResultGenerated?.Invoke(this, new EventHandlers.ResultEventArgs(reslist));
        }

    }
}
