using System;
using System.Collections.Generic;
using System.Text;

namespace PluginUtils
{
    public struct ResultInfo
    {
        public string EventName;
        public string MSG;
        public string AIJSON;
        public string Filename;
        public string Tags;
        public ResultInfo(string eventName, string msg = "", string tags = "", string aijson = "", string filename = "")
        {
            EventName = eventName;
            MSG = msg;
            AIJSON = aijson;
            Filename = filename;
            Tags = tags;
        }
    }
}
