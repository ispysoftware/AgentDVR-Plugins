using Newtonsoft.Json;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Plugins
{
    internal static class EventHandlers
    {
        public delegate void ProcessorEventHandler(ITensorProcessor sender, EventArgs e);
        public delegate void ProcessorResultHandler(ITensorProcessor sender, ResultEventArgs e);

        public class ResultEntry
        {
            public string Label;
            public int Probability;
            public RectangleF Region = Rectangle.Empty;

        }
        public class ResultEventArgs : EventArgs
        {
            public List<ResultEntry> Results;
            public ResultEventArgs(List<ResultEntry> results)
            {
                Results = results.OrderByDescending(p => p.Probability).ToList();
            }

            public override string ToString()
            {
                var sb = new StringBuilder("{\"results\":[");
                foreach (var result in Results)
                    sb.Append($"{{\"label\":{JsonConvert.ToString(result.Label)},\"probability\":{result.Probability}}}, \"region\":{{\"x\":{result.Region.X.ToString(CultureInfo.InvariantCulture)},\"y\":{result.Region.Y.ToString(CultureInfo.InvariantCulture)},\"w\":{result.Region.Width.ToString(CultureInfo.InvariantCulture)},\"h\":{result.Region.Height.ToString(CultureInfo.InvariantCulture)}}}}},");
                return sb.ToString().Trim(',')+"]}";
            }
        }
    }
}
