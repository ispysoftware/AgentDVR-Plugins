using System;
using System.Collections.Generic;
using System.Text;

namespace PluginShared
{
    public interface IAgentPlugin: IDisposable
    {
        /// <summary>
        /// Fixed string defining if this plugin supports "video", "audio" or both "video,audio"
        /// </summary>
        string Supports { get; }
        /// <summary>
        /// Update the plugin configuration from JSON
        /// </summary>
        /// <param name="json"></param>
        void SetConfiguration(string json);
        /// <summary>
        /// The JSON representation of the plugin configuration
        /// </summary>
        /// <param name="languageCode">language code - if not found will default to English</param>
        /// <returns></returns>
        string GetConfiguration(string languageCode);
        /// <summary>
        /// The last exception this plugin encountered if any
        /// </summary>
        Exception LastException { get; }
        /// <summary>
        /// The path to where the Agent executable is stored - for example: C:\Program Files\Agent\
        /// </summary>
        string AppPath
        {
            get;
            set;
        }

        /// <summary>
        /// The path to where the Agent data is stored - for example: C:\Program Files\Agent\Media\
        /// </summary>
        string AppDataPath
        {
            get;
            set;
        }
        /// <summary>
        /// Process an incoming event from Agent
        /// </summary>
        /// <param name="ev">ev will be one of: MotionAlert,MotionDetect,ManualAlert,RecordingStart,RecordingStop,AudioAlert,AudioDetect</param>
        /// <returns>A string</returns>
        void ProcessAgentEvent(string ev);
        
        /// <summary>
        /// return a list of events your plugin generates in human readable format, for example "Door opened","Light switched off","Camera tampered with". Do not include the plugin name.
        /// </summary>
        /// <returns>A list of the available custom events</returns>
        List<string> GetCustomEvents();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        string GetResultJSON();
    }
}
