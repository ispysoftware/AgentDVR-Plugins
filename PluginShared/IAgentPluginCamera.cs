using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace PluginShared
{
    public interface IAgentPluginCamera: IAgentPlugin
    {
        /// <summary>
        /// Processes incoming video frames from Agent
        /// </summary>
        /// <param name="frame">Pointer to raw frame data</param>
        /// <param name="sz">Size of the frame in pixels</param>
        /// <param name="channels">Number of channels (should be 3)</param>
        /// <param name="stride">Stride (or Step) of the image</param>
        void ProcessVideoFrame(IntPtr frame, Size sz, int channels, int stride);
        
        /// <summary>
        /// Set the basic camera information for use in the plugin
        /// </summary>
        /// <param name="name">The Agent camera name</param>
        /// <param name="objectID">The Agent object ID</param>
        /// <param name="localPort">The local port of the Server for API calls</param>
        void SetCameraInfo(string name, int objectID, int localPort);        
    }
}
