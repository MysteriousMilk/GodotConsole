using Godot.Logging;
using Godot.Logging.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Godot.Console
{
    public sealed class GodotConsole
    {
        #region Singleton Instance
        private static readonly GodotConsole instance = new GodotConsole();

        static GodotConsole()
        {
        }

        private GodotConsole()
        {
            consoleCommands = new Dictionary<string, ConsoleCommand>();
            recentCommands = new List<string>();
        }

        public static GodotConsole Instance
        {
            get => instance;
        }
        #endregion

        internal List<string> recentCommands;
        internal int currentCmdIndex = 0;
        internal Dictionary<string, ConsoleCommand> consoleCommands;
        internal MemoryTarget consoleLogTarget;

        /// <summary>
        /// Indicates whether or not the commands should be case sensitive.
        /// </summary>
        public bool IsCaseSensitive
        {
            get;
            set;
        } = false;

        /// <summary>
        /// Parse the command line text into the command and command arguments.
        /// </summary>
        /// <param name="commandLineText">The full command line text.</param>
        /// <param name="command">[Out] The parsed command.</param>
        /// <param name="args">[Out] The parsed command line arguments.</param>
        internal void Parse(string commandLineText, out string command, out object[] args)
        {
            string[] tokens = commandLineText.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
            List<string> argList = new List<string>();

            command = string.Empty;

            if (tokens.Length > 0)
            {
                command = tokens[0];
                argList.AddRange(tokens.Skip(1));
            }

            args = argList.ToArray();
        }

        /// <summary>
        /// Adds the last entered command to a list so that it can be recalled easily.
        /// </summary>
        /// <param name="command">The command to added to the list of recent commands.</param>
        internal void AddRecentCommand(string command)
        {
            string lastCmd = string.Empty;

            if (recentCommands.Count > 0)
            {
                lastCmd = recentCommands.Last();
            }

            if (lastCmd != command)
            {
                recentCommands.Add(command);
                currentCmdIndex = recentCommands.Count;
            }
         }

        /// <summary>
        /// Links the console to a logging MemoryTarget.
        /// </summary>
        /// <param name="target">The logging MemoryTarget.</param>
        public static void SetLogTarget(MemoryTarget target) =>
            Instance.consoleLogTarget = target;

        /// <summary>
        /// Retrieves a registered command by name.
        /// </summary>
        /// <param name="commandName">The name of the command to retrieve.</param>
        /// <returns>The command.</returns>
        public static ConsoleCommand GetCommand(string commandName)
        {
            if (!Instance.IsCaseSensitive)
                commandName = commandName.ToLower();

            Instance.consoleCommands.TryGetValue(commandName, out ConsoleCommand cmd);
            return cmd;
        }

        /// <summary>
        /// Registers a command with the console system.
        /// </summary>
        /// <param name="commandName">The command name / text.</param>
        /// <param name="commandAction">
        /// The function delegate to execute when the command is invoked.
        /// The first parameter is the command name as a string. The second parameter is 
        /// the passed command arguments.
        /// </param>
        public static void RegisterCommand(string commandName, Action<ConsoleCommand, object[]> commandAction)
        {
            string cmd = commandName;

            if (!Instance.IsCaseSensitive)
                cmd = cmd.ToLower();

            if (Instance.consoleCommands.ContainsKey(cmd))
                throw new InvalidOperationException("Attempt to register console command which has already been registered.");

            ConsoleCommand gdCmd = new ConsoleCommand(cmd, commandAction);
            Instance.consoleCommands.Add(cmd, gdCmd);
        }

        /// <summary>
        /// Registers a variable with the console system.
        /// </summary>
        /// <param name="variableName">The name of the variable.</param>
        /// <param name="defaultValue">The default value for the variable when the system starts up.</param>
        /// <param name="commandAction">
        /// The function delegate to execute when the command is invoked.
        /// The first parameter is the command name as a string. The second parameter is 
        /// the passed command arguments.
        /// </param>
        public static void RegisterVariable<T>(string variableName, T defaultValue,  Action<ConsoleCommand, object[]> commandAction)
        {
            string cmd = variableName;

            if (!Instance.IsCaseSensitive)
                cmd = cmd.ToLower();

            if (Instance.consoleCommands.ContainsKey(cmd))
                throw new InvalidOperationException("Attempt to register console command which has already been registered.");

            ConsoleVariable<T> gdVar = new ConsoleVariable<T>(cmd, defaultValue, commandAction);
            Instance.consoleCommands.Add(cmd, gdVar);
        }

        /// <summary>
        /// Invokes a registered command.
        /// </summary>
        /// <param name="commandName">The name/text of the command to invoke.</param>
        /// <param name="args">The arguments to pass to the command function.</param>
        public static void InvokeCommand(string commandName, object[] args)
        {
            var command = GetCommand(commandName);

            if (command != null)
            {
                if (args.Length < 1)
                {
                    Instance.LogConsoleVariable(command.CommandText);
                    return;
                }

                if (!Instance.CompareToConsoleVariable(command.CommandText, args[0]))
                {
                    Instance.UpdateConsoleVariable(command.CommandText, args[0]);

                    command.Invoke(args);
                }
            }
            else
            {
                GodotLogger.LogWarning("Command \'" + commandName + "\' is not a valid command.");
            }
        }

        /// <summary>
        /// Parses a command string into a command and command arguments. The command is then invoked with the 
        /// parsed command and arguments.
        /// </summary>
        /// <param name="commandLineText">The command line text to parse.</param>
        public static void ParseCommand(string commandLineText)
        {
            GodotLogger.LogCommand(commandLineText);
            Instance.AddRecentCommand(commandLineText);

            Instance.Parse(commandLineText, out string commandText, out object[] args);
            InvokeCommand(commandText, args);
        }

        /// <summary>
        /// Gets all text from the console log target.
        /// </summary>
        /// <returns>The full console text as a string.</returns>
        public static string GetText()
        {
            string consoleText = string.Empty;

            if (Instance.consoleLogTarget == null)
                Instance.consoleLogTarget = GodotLogger.Instance.Configuration.GetTarget<MemoryTarget>();

            if (Instance.consoleLogTarget != null)
                consoleText = Instance.consoleLogTarget.ToString();

            return consoleText;
        }

        /// <summary>
        /// Cycles through the recent commands list, moving backwards through the list.
        /// </summary>
        /// <returns></returns>
        public static string NextCommand()
        {
            string command = string.Empty;

            GD.Print(Instance.currentCmdIndex);

            if (Instance.recentCommands.Count == 0)
                return string.Empty;

            Instance.currentCmdIndex = Math.Max(Instance.currentCmdIndex - 1, 0);
            command = Instance.recentCommands[Instance.currentCmdIndex];

            return command;
        }

        /// <summary>
        /// Cycles through the recent commands list, moving forward through the list.
        /// </summary>
        /// <returns></returns>
        public static string PreviousCommand()
        {
            string command = string.Empty;

            GD.Print(Instance.currentCmdIndex);

            if (Instance.recentCommands.Count == 0)
                return string.Empty;

            Instance.currentCmdIndex = Math.Min(Instance.currentCmdIndex + 1, Instance.recentCommands.Count - 1);
            command = Instance.recentCommands[Instance.currentCmdIndex];

            return command;
        }

        private bool CompareToConsoleVariable(string command, object val)
        {
            bool compare = false;
            if (consoleCommands.TryGetValue(command, out ConsoleCommand val2))
            {
                if (val2 is IConsoleVariable cvar)
                {
                    compare = cvar.Compare(val);
                }
            }
            return compare;
        }

        private void UpdateConsoleVariable(string command, object val)
        {
            if (consoleCommands.ContainsKey(command) &&
                consoleCommands[command] is IConsoleVariable cvar)
            {
                cvar.SetValue(val);
            }
        }

        private void LogConsoleVariable(string command)
        {
            if (consoleCommands.ContainsKey(command) &&
                consoleCommands[command] is IConsoleVariable cvar)
            {
                GodotLogger.LogCommand(cvar.ToString());
            }
        }
    }
}
