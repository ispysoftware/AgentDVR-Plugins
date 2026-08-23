# AgentDVR-Plugins: Demo


Download Agent DVR here:
https://www.ispyconnect.com/download

See General information on plugins here:
https://github.com/ispysoftware/AgentDVR-Plugins

This plugin is for developers to show the various integration points available when building plugins.

## Commands

The demo overrides `Command(string)` to show how to accept commands from the config UI (buttons with `"action":"plugincommand"`) and from the HTTP API:

```
http://localhost:8090/command.cgi?cmd=plugincommand&ot=2&oid=1&command=status
```

| Command | Effect |
|---|---|
| `reset` | Re-parse tripwires/polygons on the next frame |
| `mirror`, `mirror:on`, `mirror:off` | Toggle or set the mirror effect |
| `volume:0-100` | Set the volume effect level |
| `alert` | Raise the "Box Bounce" custom event |
| `status` | Returns current state as JSON |

General notes on creating plugins:

https://www.ispyconnect.com/docs/agent/plugins#create-your-own-plugin
