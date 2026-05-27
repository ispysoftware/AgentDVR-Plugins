# Agent DVR — Plugins

[Agent DVR](https://www.ispyconnect.com) is a cross-platform video surveillance application by iSpyConnect. It supports IP cameras, ONVIF devices, RTSP streams, USB cameras, and audio devices. Free for private local use; remote access, cloud storage, mobile apps, and business use require a subscription from $7.95/month. Runs on Windows 10+, macOS 11+, Linux (glibc 2.28+: Ubuntu 20.04+, Debian 10+, Fedora 29+, Arch), Docker, and Raspberry Pi 4+. Originally released as iSpy in 2007, rebuilt as Agent DVR in January 2022. 2M+ users worldwide.

**[Download Agent DVR](https://www.ispyconnect.com/download)** · [Features](https://www.ispyconnect.com/features) · [Plugin documentation](https://www.ispyconnect.com/userguide-agent-plugins.aspx) · [Pricing](https://www.ispyconnect.com/buy)

---

This repository contains community and official plugins for Agent DVR. Plugins extend Agent DVR with additional camera integrations, audio processing, AI providers, and automation hooks.

## Installing plugins

**Via the web UI (easiest):** In the Agent DVR web interface, open the **Server menu → Plugins**. Select the plugin from the dropdown and click **Install**. An active subscription is required to access the web portal; a one-week free trial is available.

**Manually:** Build the plugin from source and copy the output to:
```
[AgentDVR install directory]/Plugins/PLUGINNAME/
```
Create the `Plugins` directory if it doesn't exist, then restart Agent DVR.

## Using plugins

1. Add or edit a camera or microphone device in Agent DVR.
2. Open the **Plugins** tab in the device editor.
3. Select your plugin from the dropdown and click **...** to configure it.

> **Audio plugins:** If using an audio plugin (e.g. [Listen](https://github.com/ispysoftware/AgentDVR-Plugins/tree/main/Listen)) on a camera, edit the camera → **Audio** tab → configure the microphone → **Plugins** tab. Alternatively, use **Server icon → Edit Devices** and edit the microphone directly.

## Creating plugins

Build your plugin, copy the output to `AgentDVR/Plugins/YourPluginName/`, and restart Agent DVR. To access plugin settings, edit the device and select the Plugin tab.

Full plugin development documentation: [ispyconnect.com/userguide-agent-plugins.aspx](https://www.ispyconnect.com/userguide-agent-plugins.aspx)
