# GodotConsole
C# console backend for Godot. This console supports tying functions to console commands and also tracking of console variables. This is intended to work with the Mono version of Godot (C#). Just drop the code in with your main project and you can start building a console and commands right away. See below for usage examples.

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