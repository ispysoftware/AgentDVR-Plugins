# AgentDVR-Plugins: Listen


Download Agent DVR here:
https://www.ispyconnect.com/download.aspx

See General information on plugins here:
https://github.com/ispysoftware/AgentDVR-Plugins

The listen plugin uses machine learning to process live audio and recognise sounds. It raises events in Agent DVR you can use to perform actions.

![listen](https://user-images.githubusercontent.com/800093/163334854-ff528a23-98b6-4faa-a253-7ecb0686d25e.png)

Set the minimum confidence level (from 0 = no confidence to 100 = certain) and choose the types of sound you want to be notified of. If you want to raise alerts (you are recording on alert for example) - then check the "Raise Alerts" option.

This plugin raises 2 events. One called "Sound Detected" and one called "Sound Recognized". To use this, edit the microphone and select the Actions tab. Under **If** choose "Listen: Sound Recognized". Under **Then** select "Show Message". Under **Message** enter {MSG}. Click OK. When the plugin recognises a sound you selected in the configuration that is above the confidence level you chose it will display the sound type in the Agent UI at the top left. 

The Sound detected event is raised whenever the plugin recognizes a sound above your confidence threshold but it isn't in the **Listen For** list.

General notes on creating plugins:

https://www.ispyconnect.com/userguide-agent-plugins.aspx

