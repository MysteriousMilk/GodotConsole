using Godot.Logging;
using Godot.Logging.Targets;

namespace Godot.Console.Tests
{
    public class ConsoleUnitTests
    {
        private string GetMessagePrefix(LogLevel level)
        {
            return $"[{level.ToString()}][{GetType().Name}]";
        }

        [OneTimeSetUp]
        public void InitialSetup()
        {
            // setup logging and console
            var consoleLogTarget = new MemoryTarget("ConsoleLog");

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

            GodotLogger.SetConfiguration(config);
            GodotConsole.SetLogTarget(consoleLogTarget);
        }

        [SetUp]
        public void Setup()
        {
            GodotLogger.Instance.ClearLog();
        }

        [Test]
        public void LogClear()
        {
            string message = "This is an info message.";

            GodotLogger.LogInfo(message);

            GodotLogger.Instance.ClearLog();

            var log = GodotLogger.Instance.Configuration.GetTarget<MemoryTarget>("ConsoleLog");
            string logText = log.ToString();

            Assert.IsTrue(string.IsNullOrWhiteSpace(logText));
        }

        [Test]
        public void LogWriteInfo()
        {
            string message = "This is an info message.";

            GodotLogger.LogInfo(message);

            var log = GodotLogger.Instance.Configuration.GetTarget<MemoryTarget>("ConsoleLog");
            string logText = log.ToString();

            Assert.True(logText.StartsWith(GetMessagePrefix(LogLevel.Info)));
            Assert.IsTrue(logText.Contains(message));
        }

        [Test]
        public void LogWriteWarning()
        {
            string message = "This is an info message.";

            GodotLogger.LogWarning(message);

            var log = GodotLogger.Instance.Configuration.GetTarget<MemoryTarget>("ConsoleLog");
            string logText = log.ToString();

            Assert.True(logText.StartsWith(GetMessagePrefix(LogLevel.Warn)));
            Assert.IsTrue(logText.Contains(message));
        }
    }
}