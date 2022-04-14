# AgentDVR-Plugins: Barcode Recognition


Download Agent DVR here:
https://www.ispyconnect.com/download.aspx

See General information on plugins here:
https://github.com/ispysoftware/AgentDVR-Plugins

The barcode plugin scans a camera feed once a second for barcodes and raises events in Agent DVR you can use to perform actions.

This plugin is Windows Only.

![barcode_config](https://user-images.githubusercontent.com/800093/163331670-08364a2c-0ca7-47cf-a6b1-4d03f0c81517.png)

Choose from the list of available barcode types. Be sure to only select the ones you are using to maximise performance.

![barcode_types](https://user-images.githubusercontent.com/800093/163331676-98f1ed6a-083d-464b-a7c1-00323d55ac07.png)

This plugin raises an event called "Barcode Recognized". To use this, edit the camera and select the Actions tab. Under **If** choose "Barcode: Barcode Recognized". Under **Then** select "Show Message". Under **Message** enter {MSG}. Click OK. When the plugin recognises a barcode it will display the barcode in the Agent UI at the top left. 

General notes on creating plugins:

https://www.ispyconnect.com/userguide-agent-plugins.aspx

