# GodotConsole
[![NuGet version (Godot.Console)](https://img.shields.io/badge/nuget-v2.0-blue?style=flat-square)](https://www.nuget.org/packages/Godot.Console/2.0/)\
[![NuGet version (Godot.Console)](https://img.shields.io/badge/nuget-v1.2.3-blue?style=flat-square)](https://www.nuget.org/packages/Godot.Console/1.2.3/)

C# console backend for Godot. This console supports tying functions to console commands and also tracking of console variables. This is intended to work with the .NET version of Godot (C#). Just drop the code in with your main project (or grab the nuget package) and you can start building a console and commands right away. See below for usage examples.

## Version Compatibility
Different versions of Godot.Console may support different version of the Godot SDK. See below.

**Godot.Console v2.\*** -> Godot SDK 4.2.2 or greater\
**Godot.Console v1.\*** -> Godot SDK 4.0.2 or greater

### What's New in v2.0
- Updated Godot SDK to v4.2.2
- ConsoleVariable backend now uses Variant instead of System.Object. This is a breaking change, but it should allow for better integration with the rest of Godot. This change also transforms ConsoleCommand arguments into Variants as well. This will allow for better type casing when retrieving the argument(s).
- Capability to read/write variables from/to a configuration file (ConfigFile)
- List of registered commands can now be retrieved

## Usage
#### General Overview
Set up an in-memory logger for the console. Here we will use a BBTextTarget so custom formatting can be included when displaying in the UI.
```C#
// set up a log target for the console
var consoleLogTarget = new BBTextTarget("UIConsoleLog");

// Create a configuration for the logger and register the target
LogConfiguration config = new LogConfiguration();
config.RegisterTarget(consoleTarget);

// Set the configuration
GodotLogger.SetConfiguration(config);

// pass the log target to the console
GodotConsole.SetLogTarget(consoleLogTarget);
```

Register a custom console command.
```C#
GodotConsole.RegisterCommand("exit", (cmd, args) => GetTree().Quit());
```

Track a console variable.
```C#
GodotConsole.RegisterVariable<bool>("fullscreen", false, (cmd, args) => OnFullScreenCommand(cmd, args));

public void OnFullScreenCommand(ConsoleCommand command, Variant[] args)
{
    bool val = args[0].AsBool();

    DisplayServer.WindowSetMode(val ? DisplayServer.WindowMode.Fullscreen : DisplayServer.WindowMode.Windowed);
}
```

Get the contents of the log which could then be displayed in the UI, for example.
```C#
richTextLabel.Text = GodotConsole.GetText();
```

Pass user entered text (from the UI console, for example) to the console backend for processing.
```C#
string cmdLineText = consoleInputLineEdit.Text;
GodotConsole.ParseCommand(cmdLineText);
```

#### Command Line Support
Parse the command line into console-tracked variables.
```C#
// register some variables that can be accessed from the command line (i.e --connect or --ip 127.0.0.1)
GodotConsole.RegisterVariable("connect", false);
GodotConsole.RegisterVariable("ip", "127.0.0.1");

// Map the command line to registered variables -- this will update the registered variables with
// what was specified on the command line
GodotConsole.MapCommandLineVars();
```

#### Config File Support
Write a configuration file. This will save off all tracked ConsoleVariables with the requested configuration.
```C#
// Register some variables and specify what configuration and section they belong to
GodotConsole.RegisterVariable("c_isFullscreen", true, "ClientSettings", "Display");
GodotConsole.RegisterVariable("c_resolution", new Vector2I(1280, 720), "ClientSettings", "Display");

string dir = Path.GetFullPath(ProjectSettings.GlobalizePath("user://configs"));
GodotConsole.WriteConfig("ClientSettings", dir);
```
Read a configuration file. The variable values stored in the config will replace the current variable value.\
Note: Any variables found in the config file that are not currently registered in the system will be discarded. Be sure to register all of your variables before loading a config file.
```C#
// Register some variables and specify what configuration and section they belong to
GodotConsole.RegisterVariable("c_isFullscreen", true, "ClientSettings", "Display");
GodotConsole.RegisterVariable("c_resolution", new Vector2I(1280, 720), "ClientSettings", "Display");

string dir = Path.GetFullPath(ProjectSettings.GlobalizePath("user://configs"));
GodotConsole.WriteConfig("ClientSettings", dir);
```