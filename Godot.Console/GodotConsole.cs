using Godot.Logging;
using Godot.Logging.Targets;
using System;
using System.Collections.Generic;
using System.Linq;

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
            variableConfigAssignment = new Dictionary<string, Dictionary<string, IConsoleVariable>>();
        }

        public static GodotConsole Instance
        {
            get => instance;
        }
        #endregion

        internal List<string> recentCommands;
        internal int currentCmdIndex = 0;
        internal Dictionary<string, ConsoleCommand> consoleCommands;
        internal Dictionary<string, Dictionary<string, IConsoleVariable>> variableConfigAssignment;
        internal MemoryTarget consoleLogTarget;
        internal bool echo = true;

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
        internal void Parse(string commandLineText, out string command, out string[] args)
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
        /// Assigns a <see cref="IConsoleVariable"/> to a specific config file.
        /// </summary>
        /// <param name="configName">The config file to assign the variable to.</param>
        /// <param name="variable">The console variable.</param>
        internal void AssignVariableToConfig(string configName, IConsoleVariable variable)
        {
            if (!variableConfigAssignment.ContainsKey(configName))
                variableConfigAssignment.Add(configName, new Dictionary<string, IConsoleVariable>());
            variableConfigAssignment[configName].Add(variable.VariableName, variable);
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
        /// Gets the list of <see cref="ConsoleCommand"/> currently registered with the console.
        /// </summary>
        /// <returns>List of <see cref="ConsoleCommand"/> objects.</returns>
        public static IEnumerable<ConsoleCommand> GetCommandList()
        {
            return Instance.consoleCommands.Values;
        }

        /// <summary>
        /// Gets the list of registered console commands as their registered command string.
        /// </summary>
        /// <returns>List of console commands text strings.</returns>
        public static IEnumerable<string> GetCommandStringList()
        {
            return Instance.consoleCommands.Values.Select(x => x.CommandText);
        }

        /// <summary>
        /// Retrieves a registered console variable by name.
        /// </summary>
        /// <param name="commandName">The name of the variable to retrieve.</param>
        /// <returns>The variable.</returns>
        public static IConsoleVariable GetVariable(string cvarName)
        {
            if (!Instance.IsCaseSensitive)
                cvarName = cvarName.ToLower();

            Instance.consoleCommands.TryGetValue(cvarName, out ConsoleCommand cmd);
            return cmd as IConsoleVariable;
        }

        /// <summary>
        /// Retrieves a registered console variable by name.
        /// </summary>
        /// <param name="commandName">The name of the variable to retrieve.</param>
        /// <returns>The variable.</returns>
        public static ConsoleVariable<T> GetVariable<T>(string cvarName) where T : IEquatable<T>
        {
            if (!Instance.IsCaseSensitive)
                cvarName = cvarName.ToLower();

            Instance.consoleCommands.TryGetValue(cvarName, out ConsoleCommand cmd);

            var cvar = cmd as IConsoleVariable;
            if (cvar == null)
                return null;

            if (typeof(T) != cvar.GetValueType())
                return null;

            return (ConsoleVariable<T>)cvar;
        }

        /// <summary>
        /// Retrieves a registered console variable by name.
        /// </summary>
        /// <param name="commandName">The name of the variable to retrieve.</param>
        /// <returns>The variable.</returns>
        public static T GetVariableValue<T>(string cvarName) where T : IEquatable<T>
        {
            if (!Instance.IsCaseSensitive)
                cvarName = cvarName.ToLower();

            Instance.consoleCommands.TryGetValue(cvarName, out ConsoleCommand cmd);

            var cvar = cmd as IConsoleVariable;
            if (cvar == null)
                return default;

            if (typeof(T) != cvar.GetValueType())
                return default;

            return (cvar as ConsoleVariable<T>).Value;
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
        public static void RegisterCommand(string commandName, Action<ConsoleCommand, Variant[]> commandAction)
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
        public static void RegisterVariable<T>(string variableName, T defaultValue) where T : IEquatable<T>
        {
            string cmd = variableName;

            if (!Instance.IsCaseSensitive)
                cmd = cmd.ToLower();

            if (Instance.consoleCommands.ContainsKey(cmd))
                throw new InvalidOperationException("Attempt to register console command which has already been registered.");

            ConsoleVariable<T> gdVar = new ConsoleVariable<T>(cmd, defaultValue, null);
            Instance.consoleCommands.Add(cmd, gdVar);
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
        public static void RegisterVariable<T>(string variableName, T defaultValue,  Action<ConsoleCommand, Variant[]> commandAction) where T : IEquatable<T>
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
        /// Registers a variable with the console system.
        /// </summary>
        /// <param name="variableName">The name of the variable.</param>
        /// <param name="defaultValue">The default value for the variable when the system starts up.</param>
        /// <param name="config">The name of the config file to associate the variable with.</param>
        /// <param name="configSection">Section within the config file to assign the variable to.</param>
        public static void RegisterVariable<T>(string variableName, T defaultValue, string config, string configSection) where T : IEquatable<T>
        {
            string cmd = variableName;

            if (!Instance.IsCaseSensitive)
                cmd = cmd.ToLower();

            if (Instance.consoleCommands.ContainsKey(cmd))
                throw new InvalidOperationException("Attempt to register console command which has already been registered.");

            ConsoleVariable<T> gdVar = new ConsoleVariable<T>(cmd, defaultValue, config, configSection, null);
            Instance.consoleCommands.Add(cmd, gdVar);

            if (!string.IsNullOrEmpty(config) && !string.IsNullOrEmpty(configSection))
                Instance.AssignVariableToConfig(config, gdVar);
        }

        /// <summary>
        /// Registers a variable with the console system.
        /// </summary>
        /// <param name="variableName">The name of the variable.</param>
        /// <param name="defaultValue">The default value for the variable when the system starts up.</param>
        /// <param name="config">The name of the config file to associate the variable with.</param>
        /// <param name="configSection">Section within the config file to assign the variable to.</param>
        /// <param name="commandAction">
        /// The function delegate to execute when the command is invoked.
        /// The first parameter is the command name as a string. The second parameter is 
        /// the passed command arguments.
        /// </param>
        public static void RegisterVariable<T>(string variableName, T defaultValue, string config, string configSection, Action<ConsoleCommand, Variant[]> commandAction) where T : IEquatable<T>
        {
            string cmd = variableName;

            if (!Instance.IsCaseSensitive)
                cmd = cmd.ToLower();

            if (Instance.consoleCommands.ContainsKey(cmd))
                throw new InvalidOperationException("Attempt to register console command which has already been registered.");

            ConsoleVariable<T> gdVar = new ConsoleVariable<T>(cmd, defaultValue, config, configSection, commandAction);
            Instance.consoleCommands.Add(cmd, gdVar);

            if (!string.IsNullOrEmpty(config) && !string.IsNullOrEmpty(configSection))
                Instance.AssignVariableToConfig(config, gdVar);
        }

        /// <summary>
        /// Updates a <see cref="ConsoleVariable{T}"/> without invoking the attached callback.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="variableName">The name of the variable to update.</param>
        /// <param name="varValue">The new value to set.</param>
        public static void UpdateVariable<T>(string variableName, T varValue) where T : IEquatable<T>
        {
            string cmd = variableName;

            if (!VariantHelper.IsSupportedType(typeof(T)))
                throw new ArgumentException("Specified generic type not supported by ConsoleVariable.", "varValue");

            if (!Instance.IsCaseSensitive)
                cmd = cmd.ToLower();

            Instance.UpdateConsoleVariable(cmd, VariantHelper.ToVariant(varValue));
        }

        /// <summary>
        /// Removes all commands and variables from being tracked by the console.
        /// This essentially resets the console back to a fresh state.
        /// </summary>
        public static void RemoveAllCommands()
        {
            Instance.consoleCommands.Clear();
            Instance.recentCommands.Clear();
            Instance.variableConfigAssignment.Clear();
        }

        /// <summary>
        /// Checks if a <see cref="ConsoleVariable{T}"/>'s value matches the given value.
        /// </summary>
        /// <typeparam name="T">Generic <see cref="ConsoleVariable{T}"/> type.</typeparam>
        /// <param name="variableName">The name of the <see cref="ConsoleVariable{T}"/>.</param>
        /// <param name="val">The value to check equality on.</param>
        /// <returns>True if the <see cref="ConsoleVariable{T}"/>'s value matches the given value, False if not.</returns>
        public static bool VariableEquals<T>(string variableName, T val) where T : IEquatable<T>
        {
            string cmd = variableName;

            if (!VariantHelper.IsSupportedType(typeof(T)))
                throw new ArgumentException("Specified generic type not supported by ConsoleVariable.", "varValue");

            if (!Instance.IsCaseSensitive)
                cmd = cmd.ToLower();

            if (!Instance.consoleCommands.ContainsKey(cmd))
                return false;

            if (Instance.consoleCommands[cmd] is IConsoleVariable cvar)
                return cvar.Compare(VariantHelper.ToVariant(val));

            return false;
        }

        /// <summary>
        /// Toggles on echo. Inputted commands will be outputted to the console/log.
        /// </summary>
        public static void EchoOn()
        {
            Instance.echo = true;
        }

        /// <summary>
        /// Toggles off echo. Inputted commands will not be outputted to the console/log.
        /// </summary>
        public static void EchoOff()
        {
            Instance.echo = true;
        }

        /// <summary>
        /// Invokes a registered command.
        /// </summary>
        /// <param name="commandName">The name/text of the command to invoke.</param>
        /// <param name="args">The arguments to pass to the command function.</param>
        public static void InvokeCommand(string commandName, Variant[] args)
        {
            var command = GetCommand(commandName);

            if (command != null)
            {
                if (Instance.echo)
                    Instance.LogConsoleVariable(command.CommandText);

                if (args.Length > 0 && !Instance.CompareToConsoleVariable(command.CommandText, args[0]))
                {
                    Instance.UpdateConsoleVariable(command.CommandText, args[0]);
                }

                command.Invoke(args);
            }
            else
            {
                GodotLogger.LogWarning($"Command \'{commandName}\' is not a valid command.");
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

            Instance.Parse(commandLineText, out string commandText, out string[] args);

            Variant[] variantArgs = new Variant[args.Length];
            for (int i = 0; i < args.Length; i++)
                variantArgs[i] = VariantHelper.ToVariant(args[i]);

            InvokeCommand(commandText, variantArgs);
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

        /// <summary>
        /// Writes the specified configuration file to the given directory.
        /// </summary>
        /// <param name="configName">
        /// The configuration to save.
        /// Only variables tagged against this configuration will be saved to the file.
        /// </param>
        /// <param name="directory">The directory to save the config file in.</param>
        /// <returns>True if the configuration was saved successfully, False if not.</returns>
        public static bool WriteConfig(string configName, string directory)
        {
            DirAccess dir = DirAccess.Open(directory);

            if (dir == null)
            {
                GodotLogger.LogError($"Error writing config. Could not open the directory {directory}");
                return false;
            }

            string filename = Path.GetFullPath(Path.Combine(dir.GetCurrentDir(), configName + ".cfg"));
            if (dir.FileExists(filename))
                dir.Remove(filename);

            ConfigFile config = new ConfigFile();

            foreach (var configVar in Instance.variableConfigAssignment[configName].Values)
            {
                config.SetValue(configVar.ConfigSectionName, configVar.VariableName, configVar.GetValue());
            }

            var err = config.Save(filename);
            if (err != Error.Ok)
            {
                GodotLogger.LogError($"Error writing config file - {err}");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Loads the specified configuration from a file.
        /// Variables in the configuration file must already be registered or they will be discarded.
        /// </summary>
        /// <param name="configName">The configuration to load.</param>
        /// <param name="directory">The directory to load the config file from.</param>
        /// <returns>True if the configuration was loaded successfully, False if not.</returns>
        public static bool LoadConfig(string configName, string directory)
        {
            DirAccess dir = DirAccess.Open(directory);

            if (dir == null)
            {
                GodotLogger.LogError($"Error reading the config. Could not open the directory {directory}");
                return false;
            }

            string filename = Path.GetFullPath(Path.Combine(dir.GetCurrentDir(), configName + ".cfg"));
            if (!dir.FileExists(filename))
            {
                GodotLogger.LogError("Error reading the config because the file does not exist.");
                return false;
            }

            ConfigFile config = new ConfigFile();

            var err = config.Load(filename);
            if (err != Error.Ok)
            {
                GodotLogger.LogError($"Error reading config file - {err}");
                return false;
            }

            foreach (string section in config.GetSections())
            {
                foreach (string varName in config.GetSectionKeys(section))
                {
                    if (Instance.consoleCommands.TryGetValue(varName, out ConsoleCommand cmd) &&
                        cmd is IConsoleVariable cvar && 
                        cvar.ConfigSectionName == section)
                    {
                        cvar.SetValue(config.GetValue(section, varName));
                    }
                }
            }

            return true;
        }

        public static void MapCommandLineVars()
        {
            var parsed = ParseCommandLine(OS.GetCmdlineArgs());
            
            foreach (var argPair in parsed)
            {
                var command = GetCommand(argPair.Key);
                if (command != null && command is IConsoleVariable cvar)
                {
                    if (string.IsNullOrEmpty(argPair.Value) && cvar.GetValueType() == typeof(bool))
                        cvar.SetValue(true);
                    else
                        cvar.SetValue(argPair.Value);
                }
                else
                {
                    GodotLogger.LogWarning($"Unknown command line argument, {argPair.Key}");
                }
            }
        }

        private static bool IsArg(string arg)
        {
            return arg.StartsWith("-") || arg.StartsWith("--");
        }

        private static bool IsValue(string arg)
        {
            return !IsArg(arg);
        }

        private static Dictionary<string, string> ParseCommandLine(string[] args)
        {
            Dictionary<string, string> parsedArgs = new Dictionary<string, string>();

            for (int i = 0; i < args.Length; i++)
            {
                if ((args[i].StartsWith("-") && args[i].Length == 2) ||
                    (args[i].StartsWith("--")))
                {
                    string argName = args[i].TrimPrefix("-").TrimPrefix("-");

                    if ((i < args.Length - 1) && IsValue(args[i + 1]))
                    {
                        parsedArgs.Add(argName, args[i + 1]);
                        i++;
                    }
                    else
                    {
                        parsedArgs.Add(argName, string.Empty);
                    }
                }
            }

            return parsedArgs;
        }

        private bool CompareToConsoleVariable(string command, Variant val)
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

        private void UpdateConsoleVariable(string command, Variant val)
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
