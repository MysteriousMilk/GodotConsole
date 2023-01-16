using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot.Logging;

namespace Godot.Console
{
    public interface IConsoleVariable
    {
        bool Compare(object val);
        object GetValue();
        void SetValue(object val);
    }

    /// <summary>
    /// Represents a command variable which can be used by the <see cref="ConsoleCommand"/> system.
    /// </summary>
    public partial class ConsoleVariable<T> : ConsoleCommand, IConsoleVariable
    {
        /// <summary>
        /// The current value of the console variable.
        /// </summary>
        public T Value
        {
            get;
            set;
        }

        /// <summary>
        /// Constructor for a <see cref="ConsoleCommand"/>.
        /// </summary>
        /// <param name="command">Text used to invoke the command.</param>
        /// <param name="defaultValue">The default value of the console variable.</param>
        /// <param name="action">Delegate to a method to invoke when the command is performed.</param>
        public ConsoleVariable(string command, T defaultValue, Action<ConsoleCommand, object[]> action) : base(command, action)
        {
            Value = defaultValue;
        }

        public bool Compare(object val)
        {
            if (typeof(T) == val.GetType())
            {
                return Value.Equals(val);
            }
            else if (val is string strVal)
            {
                if (typeof(T) == typeof(short) || typeof(T) == typeof(int) || typeof(T) == typeof(long))
                {
                    if (long.TryParse(strVal, out long valFromStr))
                    {
                        long curVal = (long)Convert.ChangeType(Value, typeof(long));
                        return valFromStr == curVal;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (typeof(T) == typeof(float) || typeof(T) == typeof(double))
                {
                    if (double.TryParse(strVal, out double valFromStr))
                    {
                        double curVal = (double)Convert.ChangeType(Value, typeof(double));
                        return valFromStr == curVal;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (typeof(T) == typeof(bool))
                {
                    if (bool.TryParse(strVal, out bool valFromStr))
                    {
                        bool curVal = (bool)Convert.ChangeType(Value, typeof(bool));
                        return valFromStr == curVal;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return false;
        }

        public object GetValue()
        {
            return Value;
        }

        public void SetValue(object val)
        {
            bool success = false;

            if (typeof(T) == val.GetType())
            {
                Value = (T)val;
                success = true;
            }
            else if (val is string strVal)
            {
                if (typeof(T) == typeof(short) && short.TryParse(strVal, out short shortFromStr))
                {
                    Value = (T)(object)shortFromStr;
                    success = true;
                }
                else if (typeof(T) == typeof(int) && int.TryParse(strVal, out int intFromStr))
                {
                    Value = (T)(object)intFromStr;
                    success = true;
                }
                else if (typeof(T) == typeof(long) && long.TryParse(strVal, out long longFromStr))
                {
                    Value = (T)(object)longFromStr;
                    success = true;
                }
                else if (typeof(T) == typeof(float) && float.TryParse(strVal, out float floatFromStr))
                {
                    Value = (T)(object)floatFromStr;
                    success = true;
                }
                else if (typeof(T) == typeof(double) && double.TryParse(strVal, out double doubleFromStr))
                {
                    Value = (T)(object)doubleFromStr;
                    success = true;
                }
                else if (typeof(T) == typeof(bool) && bool.TryParse(strVal, out bool boolFromStr))
                {
                    Value = (T)(object)boolFromStr;
                    success = true;
                }
            }

            if (!success)
                GodotLogger.LogWarning("Value for variable \'" + CommandText + "\' is in an irregular format.");
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append(CommandText);
            builder.Append(" ");
            builder.Append(Value.ToString());
            return builder.ToString();
        }
    }
}