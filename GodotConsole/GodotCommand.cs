﻿using System;

namespace Godot.Console
{
    /// <summary>
    /// Represents a command which can be used by the <see cref="GodotConsole"/> system.
    /// </summary>
    public class GodotCommand
    {
        private Action<string, object[]> cmdAction;

        /// <summary>
        /// Command text used to invoke the command.
        /// </summary>
        public string CommandText
        {
            get;
            private set;
        }

        /// <summary>
        /// Constructor for a <see cref="GodotCommand"/>.
        /// </summary>
        /// <param name="command">Text used to invoke the command.</param>
        /// <param name="action">Delegate to a method to invoke when the command is performed.</param>
        public GodotCommand(string command, Action<string, object[]> action)
        {
            CommandText = command;
            cmdAction = action;
        }

        /// <summary>
        /// Runs the command by invoking the assigned action delegate.
        /// </summary>
        /// <param name="args">Arguments to be passed to the invoked method.</param>
        public void Invoke(object[] args)
        {
            if (cmdAction != null)
                cmdAction.Invoke(CommandText, args);
        }
    }
}
