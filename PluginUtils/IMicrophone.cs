using System;
using System.Collections.Generic;
using System.Text;

namespace PluginUtils
{
    public interface IMicrophone: IPlugin
    {
        /// <summary>
        /// Process incoming audio data from Agent. This will be in a fixed format of 22050Hz, one channel
        /// </summary>
        /// <param name="rawData">byte array of the raw data from the microphone</param>
        /// <param name="bytesRecorded">The number of bytes in the rawData</param>
        /// <param name="format">The audio format</param>
        /// <returns>The modified audio data</returns>
        byte[] ProcessAudioFrame(byte[] rawData, int bytesRecorded, int samplerate, int channels);
        /// <summary>
        /// Set the basic microphone information for use in the plugin
        /// </summary>
        /// <param name="name">The Agent microphone name</param>
        /// <param name="objectID">The Agent object ID</param>
        /// <param name="localPort">The local port of the Server for API calls</param>
        /// <param name="sampleRate">The number of samples in 1 second (Hz)</param>
        /// <param name="channels">The number of audio channels</param>
        void SetMicrophoneInfo(string name, int objectID, int localPort, int sampleRate, int channels);

    }
}
