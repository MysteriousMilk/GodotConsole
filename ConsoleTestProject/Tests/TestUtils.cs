using Godot;
using Godot.Console;
using Godot.Logging;
using Godot.Logging.Targets;

namespace Godot.Console.Tests;

internal static class TestUtils
{
    /// <summary>
    /// Sets up the log and console (if necessary)
    /// </summary>
    internal static void SetupLogAndConsole()
    {
        MemoryTarget consoleLogTarget = GodotLogger.Instance.Configuration.GetTarget<MemoryTarget>("ConsoleLog");

        if (consoleLogTarget == null)
        {
            // setup logging and console
            consoleLogTarget = new MemoryTarget("ConsoleLog");

            LogConfiguration config = new LogConfiguration();
            config.RegisterTarget(consoleLogTarget);

            FormatRule debugRule = new FormatRule()
            {
                FormatText = "[${level}][${classname}] ${message}",
                FormatLogLevel = LogLevel.Debug
            };
            config.ApplyFormattingRule(debugRule);

            FormatRule infoRule = new FormatRule()
            {
                FormatText = "[${level}][${classname}] ${message}",
                FormatLogLevel = LogLevel.Info
            };
            config.ApplyFormattingRule(infoRule);

            FormatRule warningRule = new FormatRule()
            {
                FormatText = "[${level}][${classname}] ${message}",
                FormatLogLevel = LogLevel.Warn,
                TextColor = Colors.Yellow
            };
            config.ApplyFormattingRule(warningRule);

            FormatRule errorRule = new FormatRule()
            {
                FormatText = "[${level}][${classname}] ${message}",
                FormatLogLevel = LogLevel.Error,
                TextColor = Colors.Red
            };
            config.ApplyFormattingRule(errorRule);

            FormatRule exceptionRule = new FormatRule()
            {
                FormatText = "[${level}][${classname}] ${message}",
                FormatLogLevel = LogLevel.Exception,
                TextColor = Colors.Red
            };
            config.ApplyFormattingRule(exceptionRule);

            GodotLogger.SetConfiguration(config);
            GodotConsole.SetLogTarget(consoleLogTarget);
        }
    }

    internal static string GetMessagePrefix(LogLevel level, Type callingType)
    {
        return $"[{level.ToString()}][{callingType.Name}]";
    }
}
