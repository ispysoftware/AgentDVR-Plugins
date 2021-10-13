using System;
using System.Collections.Generic;
using System.Text;

namespace PluginUtils
{
    public interface IAgentPluginMicrophone: IAgentPlugin
    {
        /// <summary>
        /// Process incoming audio data from Agent. This will be in a fixed format of 22050Hz, one channel
        /// </summary>
        /// <param name="rawData">byte array of the raw data from the microphone</param>
        /// <param name="bytesRecorded">The number of bytes in the rawData</param>
        /// <returns>The modified audio data</returns>
        byte[] ProcessAudioFrame(byte[] rawData, int bytesRecorded);
        /// <summary>
        /// Set the basic microphone information for use in the plugin
        /// </summary>
        /// <param name="name">The Agent microphone name</param>
        /// <param name="objectID">The Agent object ID</param>
        /// <param name="localPort">The local port of the Server for API calls</param>
        void SetMicrophoneInfo(string name, int objectID, int localPort);

    }
}
