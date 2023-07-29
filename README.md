# GodotConsole
[![NuGet version (Godot.Logging)](https://img.shields.io/badge/nuget-v1.2-blue?style=flat-square)](https://www.nuget.org/packages/Godot.Console/1.2.0/)

.C# console backend for Godot. This console supports tying functions to console commands and also tracking of console variables. This is intended to work with the .NET version of Godot (C#). Just drop the code in with your main project (or grab the nuget package) and you can start building a console and commands right away. See below for usage examples.

## Repository
The main branch [*origin/main*] will be kept in line with the latest release of Godot. Currently it is syncing with the latest Godot 4 beta. There is a release for the Godot 3.5.x line.

## Usage
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

public void OnFullScreenCommand(ConsoleCommand command, object[] args)
{
    bool val = (bool)(command as IConsoleVariable).GetValue();

    OS.WindowFullscreen = val;
}
```

Get the contents of the log which could then be displayed in the UI, for example.
```C#
richTextLabel.Text = GodotConsole.GetText();
```

Pass user entered text (for the UI console, for example) to the console backend for processing.
```C#
string cmdLineText = consoleInputLineEdit.Text;
GodotConsole.ParseCommand(cmdLineText);
```

Parse the command line into console-tracked variables.
```C#
// register some variables that can be accessed from the command line (i.e --connect or --ip 127.0.0.1)
GodotConsole.RegisterVariable("connect", false);
GodotConsole.RegisterVariable("ip", "127.0.0.1");

// map the command line to registered variables
GodotConsole.MapCommandLineVars();
```