using Emgu.TF;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static Plugins.Main;

namespace Plugins
{
    

    internal interface ITensorProcessor
    {
        event EventHandlers.ProcessorEventHandler ProcessorReady;
        event EventHandlers.ProcessorResultHandler ResultGenerated;
        bool Ready { get; }
        Task Init();
        void Recognise(Tensor t);
        bool IsAudio { get; }
        Size SizeRequired { get; }
        void Close();
        Session Session { get; }

    }

}
