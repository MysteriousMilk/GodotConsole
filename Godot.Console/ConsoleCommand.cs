using System;

namespace Godot.Console
{
    /// <summary>
    /// Represents a command which can be used by the <see cref="GodotConsole"/> system.
    /// </summary>
    public partial class ConsoleCommand
    {
        protected Action<ConsoleCommand, Variant[]> cmdAction;
        protected bool isVariable = false;

        /// <summary>
        /// Command text used to invoke the command.
        /// </summary>
        public string CommandText
        {
            get;
            private set;
        }

        /// <summary>
        /// Indicates if the <see cref="ConsoleCommand"/> is a variable or not.
        /// </summary>
        public bool IsVariable => isVariable;

        /// <summary>
        /// Constructor for a <see cref="ConsoleCommand"/>.
        /// </summary>
        /// <param name="command">Text used to invoke the command.</param>
        /// <param name="action">Delegate to a method to invoke when the command is performed.</param>
        public ConsoleCommand(string command, Action<ConsoleCommand, Variant[]> action)
        {
            CommandText = command;
            cmdAction = action;
        }

        /// <summary>
        /// Runs the command by invoking the assigned action delegate.
        /// </summary>
        /// <param name="args">Arguments to be passed to the invoked method.</param>
        public void Invoke(Variant[] args) => cmdAction?.Invoke(this, args);
    }
}
