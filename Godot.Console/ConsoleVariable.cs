using System;
using System.Text;

namespace Godot.Console
{
    public interface IConsoleVariable
    {
        /// <summary>
        /// Configuration file this variable belongs to.
        /// </summary>
        public string ConfigName
        {
            get;
            set;
        }

        /// <summary>
        /// Section of the configuration file that this variable belongs to.
        /// </summary>
        public string ConfigSectionName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the name of the variable
        /// </summary>
        public string VariableName
        {
            get;
        }

        /// <summary>
        /// Indicates if the <see cref="ConsoleCommand"/> is a variable or not.
        /// </summary>
        public bool IsVariable { get; }

        /// <summary>
        /// Compares this variable to the given value.
        /// </summary>
        /// <param name="val">The value to compare to this variable.</param>
        /// <returns>True if the values match, false if not.</returns>
        bool Compare(Variant val);

        /// <summary>
        /// Gets the value assigned to this variable as an object.
        /// </summary>
        /// <returns>The variable's value as an object.</returns>
        Variant GetValue();

        /// <summary>
        /// Assigns a value to the variable.
        /// </summary>
        /// <param name="val">The value to assign to the variable.</param>
        void SetValue(Variant val);

        /// <summary>
        /// Returns the type associated with the variable.
        /// </summary>
        /// <returns>The variable type.</returns>
        Type GetValueType();
    }

    /// <summary>
    /// Represents a command variable which can be used by the <see cref="ConsoleCommand"/> system.
    /// </summary>
    public partial class ConsoleVariable<T> : ConsoleCommand, IConsoleVariable where T : IEquatable<T>
    {
        private T _genericVal = default;
        private Variant _value;

        /// <summary>
        /// The current value of the console variable.
        /// </summary>
        public T Value
        {
            get => _genericVal;
            set
            {
                _genericVal = value;
                _value = VariantHelper.ToVariant<T>(value);
            }
        }

        /// <summary>
        /// Configuration file this variable belongs to.
        /// </summary>
        public string ConfigName
        {
            get;
            set;
        }

        /// <summary>
        /// Section of the configuration file that this variable belongs to.
        /// </summary>
        public string ConfigSectionName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the name of the variable.
        /// </summary>
        public string VariableName => CommandText;

        /// <summary>
        /// Constructor for a <see cref="ConsoleCommand"/>.
        /// </summary>
        /// <param name="command">Text used to invoke the command.</param>
        /// <param name="defaultValue">The default value of the console variable.</param>
        /// <param name="action">Delegate to a method to invoke when the command is performed.</param>
        public ConsoleVariable(string command, T defaultValue, Action<ConsoleCommand, Variant[]> action) : base(command, action)
        {
            if (!IsSupportedType(defaultValue.GetType()))
                throw new ArgumentException("Specified generic type not supported by ConsoleVariable.", "defaultValue");

            Value = defaultValue;
            ConfigName = null;
            ConfigSectionName = null;
            isVariable = true;
        }

        /// <summary>
        /// Constructor for a <see cref="ConsoleCommand"/>.
        /// </summary>
        /// <param name="command">Text used to invoke the command.</param>
        /// <param name="defaultValue">The default value of the console variable.</param>
        /// <param name="config">The name of the config file to associate the variable with.</param>
        /// <param name="configSection">Section within the config file to assign the variable to.</param>
        /// <param name="action">Delegate to a method to invoke when the command is performed.</param>
        public ConsoleVariable(string command, T defaultValue, string config, string configSection, Action<ConsoleCommand, Variant[]> action) : base(command, action)
        {
            if (!IsSupportedType(defaultValue.GetType()))
                throw new ArgumentException("Specified generic type not supported by ConsoleVariable.", "defaultValue");

            Value = defaultValue;
            ConfigName = config;
            ConfigSectionName = configSection;
            isVariable = true;
        }

        /// <summary>
        /// Returns the type associated with the variable.
        /// </summary>
        /// <returns>The variable type.</returns>
        public Type GetValueType()
        {
            return typeof(T);
        }

        /// <summary>
        /// Compares this variable to the given value.
        /// </summary>
        /// <param name="val">The value to compare to this variable.</param>
        /// <returns>True if the values match, false if not.</returns>
        public bool Compare(Variant val)
        {
           return _value.VariantEquals(val);
        }

        /// <summary>
        /// Gets the value assigned to this variable as an object.
        /// </summary>
        /// <returns>The variable's value as an object.</returns>
        public Variant GetValue()
        {
            return _value;
        }

        /// <summary>
        /// Assigns a value to the variable.
        /// </summary>
        /// <param name="val">The value to assign to the variable.</param>
        public void SetValue(Variant val)
        {
            _value = val;
            _genericVal = VariantHelper.FromVariant<T>(_value);
        }

        internal bool IsSupportedType(Type type)
        {
            return VariantHelper.IsSupportedType(type);
        }

        
        /// <summary>
        /// Returns a display string represnetation of the variable an its value.
        /// </summary>
        /// <returns>Variable and value as a string.</returns>
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