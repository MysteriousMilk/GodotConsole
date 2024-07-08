using GdUnit4;
using Godot.Logging;
using Godot.Logging.Targets;

using static GdUnit4.Assertions;

namespace Godot.Console.Tests;

[TestSuite]
public class LogUnitTests
{
    [Before]
    public void BeforeTestSuite()
    {
        TestUtils.SetupLogAndConsole();
    }

    [BeforeTest]
    public void Setup()
    {
        GodotLogger.Instance.ClearLog();
    }

    [TestCase]
    public void LogClear()
    {
        string message = "This is an info message.";

        GodotLogger.LogInfo(message);

        GodotLogger.Instance.ClearLog();

        var log = GodotLogger.Instance.Configuration.GetTarget<MemoryTarget>("ConsoleLog");
        string logText = log.ToString();

        AssertString(logText).IsEmpty();
    }

    [TestCase]
    public void LogWriteInfo()
    {
        string message = "This is an info message.";

        GodotLogger.LogInfo(message);

        var log = GodotLogger.Instance.Configuration.GetTarget<MemoryTarget>("ConsoleLog");
        string logText = log.ToString();

        AssertString(logText).StartsWith(TestUtils.GetMessagePrefix(LogLevel.Info, GetType()));
        AssertString(logText).Contains(message);
    }

    [TestCase]
    public void LogWriteWarning()
    {
        string message = "This is a warning message.";

        GodotLogger.LogWarning(message);

        var log = GodotLogger.Instance.Configuration.GetTarget<MemoryTarget>("ConsoleLog");
        string logText = log.ToString();

        AssertString(logText).StartsWith(TestUtils.GetMessagePrefix(LogLevel.Warn, GetType()));
        AssertString(logText).Contains(message);
    }

    [TestCase]
    public void LogWriteError()
    {
        string message = "This is an error message.";

        GodotLogger.LogError(message);

        var log = GodotLogger.Instance.Configuration.GetTarget<MemoryTarget>("ConsoleLog");
        string logText = log.ToString();

        AssertString(logText).StartsWith(TestUtils.GetMessagePrefix(LogLevel.Error, GetType()));
        AssertString(logText).Contains(message);
    }

    [TestCase]
    public void LogException()
    {
        string message = "This is an error message.";
        string exMsg = "An exception occurred.";

        try
        {
            throw new Exception(exMsg);
        }
        catch (Exception ex)
        {
            GodotLogger.LogException(ex, message);
        }

        var log = GodotLogger.Instance.Configuration.GetTarget<MemoryTarget>("ConsoleLog");
        string logText = log.ToString();

        AssertString(logText).StartsWith(TestUtils.GetMessagePrefix(LogLevel.Exception, GetType()));
        AssertString(logText).Contains(message);
        AssertString(logText).Contains(exMsg);
    }
}