using System;

namespace Godot.Console;

/// <summary>
/// Provides some helper functions for moving between specific static types and Variants.
/// </summary>
public static class VariantHelper
{
    /// <summary>
    /// Reinterprets a value (as specified by the generic type) to a <see cref="Variant">.
    /// Throws an <see cref="InvalidOperationException"/> if the specified generic type is not supported.
    /// Currently supports built in C# atomic types, StringName, and Vector variants.
    /// </summary>
    /// <typeparam name="T">Specified type of the value being passed.</typeparam>
    /// <param name="val">The value to reinterpret to a <see cref="Variant"/>.</param>
    /// <returns>A <see cref="Variant"/> represnetation of the passed value.</returns>
    public static Variant ToVariant<T>(T val) where T : IEquatable<T>
    {
        Variant varValue;

        if (typeof(T) == typeof(string))
            varValue = Variant.CreateFrom((string)Convert.ChangeType(val, typeof(string)));
        else if (typeof(T) == typeof(byte))
            varValue = Variant.CreateFrom((byte)Convert.ChangeType(val, typeof(byte)));
        else if (typeof(T) == typeof(ushort))
            varValue = Variant.CreateFrom((ushort)Convert.ChangeType(val, typeof(ushort)));
        else if (typeof(T) == typeof(short))
            varValue = Variant.CreateFrom((short)Convert.ChangeType(val, typeof(short)));
        else if (typeof(T) == typeof(int))
            varValue = Variant.CreateFrom((int)Convert.ChangeType(val, typeof(int)));
        else if (typeof(T) == typeof(uint))
            varValue = Variant.CreateFrom((uint)Convert.ChangeType(val, typeof(uint)));
        else if (typeof(T) == typeof(long))
            varValue = Variant.CreateFrom((long)Convert.ChangeType(val, typeof(long)));
        else if (typeof(T) == typeof(ulong))
            varValue = Variant.CreateFrom((ulong)Convert.ChangeType(val, typeof(ulong)));
        else if (typeof(T) == typeof(float))
            varValue = Variant.CreateFrom((float)Convert.ChangeType(val, typeof(float)));
        else if (typeof(T) == typeof(double))
            varValue = Variant.CreateFrom((double)Convert.ChangeType(val, typeof(double)));
        else if (typeof(T) == typeof(bool))
            varValue = Variant.CreateFrom((bool)Convert.ChangeType(val, typeof(bool)));
        else if (typeof(T) == typeof(StringName))
            varValue = Variant.CreateFrom((StringName)Convert.ChangeType(val, typeof(StringName)));
        else if (typeof(T) == typeof(Vector2))
            varValue = Variant.CreateFrom((Vector2)Convert.ChangeType(val, typeof(Vector2)));
        else if (typeof(T) == typeof(Vector2I))
            varValue = Variant.CreateFrom((Vector2I)Convert.ChangeType(val, typeof(Vector2I)));
        else if (typeof(T) == typeof(Vector3))
            varValue = Variant.CreateFrom((Vector3)Convert.ChangeType(val, typeof(Vector3)));
        else if (typeof(T) == typeof(Vector3I))
            varValue = Variant.CreateFrom((Vector3I)Convert.ChangeType(val, typeof(Vector3I)));
        else
            throw new InvalidOperationException($"The type {typeof(T).Name} is not supported by the \"ToVariant\" conversion.");

        return varValue;
    }

    /// <summary>
    /// Reinterprets a value (stored as a string) to a <see cref="Variant">.
    /// Attempts to get determine the correct <see cref="Variant"> type based on string conversion.
    /// </summary>
    /// <param name="val">The value to create <see cref="Variant"> from, in string form.</param>
    /// <returns>A new <see cref="Variant"> object based on the value string.</returns>
    public static Variant ToVariant(string val)
    {
        Variant varValue;

        val.Trim();

        string[] tokens = val.Split(',', StringSplitOptions.RemoveEmptyEntries);

        if (tokens.Length < 2)
        {
            val.TrimEnd(',');
            if (long.TryParse(val, out long intVal))
                varValue = Variant.CreateFrom(intVal);
            else if (double.TryParse(val, out double doubleVal))
                varValue = Variant.CreateFrom(doubleVal);
            else if (bool.TryParse(val, out bool boolVal))
                varValue = Variant.CreateFrom(boolVal);
            else
                varValue = Variant.CreateFrom(val);
        }
        else
        {
            long[] vecIntArray = new long[tokens.Length];
            double[] vecDoubleArray = new double[tokens.Length];
            bool isDouble = false;

            for (int i = 0; i < tokens.Length; i++)
            {
                if (!isDouble && long.TryParse(tokens[i], out long intVal))
                {
                    vecIntArray[i] = intVal;
                }
                else if (double.TryParse(tokens[i], out double doubleVal))
                {
                    vecDoubleArray[i] = doubleVal;
                    isDouble = true;
                }
            }

            if (tokens.Length == 2)
            {
                varValue = isDouble ?
                    Variant.CreateFrom(new Vector2((float)vecDoubleArray[0], (float)vecDoubleArray[1])) :
                    Variant.CreateFrom(new Vector2I((int)vecIntArray[0], (int)vecIntArray[1]));
            }
            else
            {
                varValue = isDouble ?
                    Variant.CreateFrom(new Vector3((float)vecDoubleArray[0], (float)vecDoubleArray[1], (float)vecDoubleArray[2])) :
                    Variant.CreateFrom(new Vector3I((int)vecIntArray[0], (int)vecIntArray[1], (int)vecIntArray[2]));
            }
        }

        return varValue;
    }

    /// <summary>
    /// Reinterprets a <see cref="Variant"> type to a C# static typed variable, as specified by the given generic type.
    /// Throws an <see cref="InvalidOperationException"/> if the specified generic type is not supported.
    /// Currently supports built in C# atomic types, StringName, and Vector variants.
    /// </summary>
    /// <typeparam name="T"><see cref="Type"/> that the <see cref="Variant"> should be represented as.</typeparam>
    /// <param name="val">The <see cref="Variant"> value to reinterpret.</param>
    /// <returns>Value of the <see cref="Variant"> as a C# static typed variable.</returns>
    public static T FromVariant<T>(Variant val) where T : IEquatable<T>
    {
        T varValue = default(T);
        Type type = typeof(T);

        if (type == typeof(string))
            varValue = (T)Convert.ChangeType(val.AsString(), type);
        else if (type == typeof(byte))
            varValue = (T)Convert.ChangeType(val.AsByte(), type);
        else if (type == typeof(ushort))
            varValue = (T)Convert.ChangeType(val.AsUInt16(), type);
        else if (type == typeof(short))
            varValue = (T)Convert.ChangeType(val.AsInt16(), type);
        else if (type == typeof(int))
            varValue = (T)Convert.ChangeType(val.AsInt32(), type);
        else if (type == typeof(uint))
            varValue = (T)Convert.ChangeType(val.AsUInt32(), type);
        else if (type == typeof(long))
            varValue = (T)Convert.ChangeType(val.AsInt64(), type);
        else if (type == typeof(ulong))
            varValue = (T)Convert.ChangeType(val.AsUInt64(), type);
        else if (type == typeof(float))
            varValue = (T)Convert.ChangeType(val.AsSingle(), type);
        else if (type == typeof(double))
            varValue = (T)Convert.ChangeType(val.AsDouble(), type);
        else if (type == typeof(bool))
            varValue = (T)Convert.ChangeType(val.AsBool(), type);
        else if (type == typeof(StringName))
            varValue = (T)Convert.ChangeType(val.AsStringName(), type);
        else if (type == typeof(Vector2))
            varValue = (T)Convert.ChangeType(val.AsVector2(), type);
        else if (type == typeof(Vector2I))
            varValue = (T)Convert.ChangeType(val.AsVector2I(), type);
        else if (type == typeof(Vector3))
            varValue = (T)Convert.ChangeType(val.AsVector3(), type);
        else if (type == typeof(Vector3I))
            varValue = (T)Convert.ChangeType(val.AsVector3I(), type);
        else
            throw new InvalidOperationException($"The type {typeof(T).Name} is not supported by the \"FromVariant\" conversion.");

        return varValue;
    }

    /// <summary>
    /// Indicates if the given <see cref="Type"/> is supported by the <see cref="Variant"> reinterpretations in this class.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static bool IsSupportedType(Type type)
    {
        if (type == typeof(string) ||
            type == typeof(byte) ||
            type == typeof(short) ||
            type == typeof(ushort) ||
            type == typeof(int) ||
            type == typeof(uint) ||
            type == typeof(long) ||
            type == typeof(ulong) ||
            type == typeof(float) ||
            type == typeof(double) ||
            type == typeof(bool) ||
            type == typeof(StringName) ||
            type == typeof(Vector2) ||
            type == typeof(Vector2I) ||
            type == typeof(Vector3) ||
            type == typeof(Vector3I))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Equality check for two <see cref="Variant"/>.
    /// </summary>
    /// <param name="left">The first <see cref="Variant"/> in the equality check.</param>
    /// <param name="right">The second <see cref="Variant"/> in the equality check.</param>
    /// <returns>True if the two <see cref="Variant"/>s are equal, False if not.</returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    internal static bool VariantEquals(this Variant left, Variant right)
    {
        if (left.VariantType != right.VariantType)
        {
            return false;
        }

        return left.VariantType switch
        {
            Variant.Type.Nil => true,
            Variant.Type.Bool => left.AsBool().Equals(right.AsBool()),
            Variant.Type.Int => left.AsInt64().Equals(right.AsInt64()),
            Variant.Type.Float => left.AsDouble().Equals(right.AsDouble()),
            Variant.Type.String => left.AsString().Equals(right.AsString()),
            Variant.Type.Vector2 => left.AsVector2().Equals(right.AsVector2()),
            Variant.Type.Vector2I => left.AsVector2I().Equals(right.AsVector2I()),
            Variant.Type.Rect2 => left.AsRect2().Equals(right.AsRect2()),
            Variant.Type.Rect2I => left.AsRect2I().Equals(right.AsRect2I()),
            Variant.Type.Vector3 => left.AsVector3().Equals(right.AsVector3()),
            Variant.Type.Vector3I => left.AsVector3I().Equals(right.AsVector3I()),
            Variant.Type.Transform2D => left.AsTransform2D().Equals(right.AsTransform2D()),
            Variant.Type.Vector4 => left.AsVector4().Equals(right.AsVector4()),
            Variant.Type.Vector4I => left.AsVector4I().Equals(right.AsVector4I()),
            Variant.Type.Plane => left.AsPlane().Equals(right.AsPlane()),
            Variant.Type.Quaternion => left.AsQuaternion().Equals(right.AsQuaternion()),
            Variant.Type.Aabb => left.AsAabb().Equals(right.AsAabb()),
            Variant.Type.Basis => left.AsBasis().Equals(right.AsBasis()),
            Variant.Type.Transform3D => left.AsTransform3D().Equals(right.AsTransform3D()),
            Variant.Type.Projection => left.AsProjection().Equals(right.AsProjection()),
            Variant.Type.Color => left.AsColor().Equals(right.AsColor()),
            Variant.Type.StringName => left.AsStringName().Equals(right.AsStringName()),
            Variant.Type.NodePath => left.AsNodePath().Equals(right.AsNodePath()),
            Variant.Type.Rid => left.AsRid().Equals(right.AsRid()),
            Variant.Type.Object => left.AsGodotObject().Equals(right.AsGodotObject()),
            Variant.Type.Callable => left.AsCallable().Equals(right),
            Variant.Type.Signal => left.AsSignal().Equals(right.AsSignal()),
            Variant.Type.Dictionary => left.AsGodotDictionary().Equals(right.AsGodotDictionary()),
            Variant.Type.Array => left.AsGodotArray().Equals(right.AsGodotArray()),
            Variant.Type.PackedByteArray => left.AsByteArray().Equals(right.AsByteArray()),
            Variant.Type.PackedInt32Array => left.AsInt32Array().Equals(right.AsInt32Array()),
            Variant.Type.PackedInt64Array => left.AsInt64Array().Equals(right.AsInt64Array()),
            Variant.Type.PackedFloat32Array => left.AsFloat32Array().Equals(right.AsFloat32Array()),
            Variant.Type.PackedFloat64Array => left.AsFloat64Array().Equals(right.AsFloat64Array()),
            Variant.Type.PackedStringArray => left.AsStringArray().Equals(right.AsStringArray()),
            Variant.Type.PackedVector2Array => left.AsVector2Array().Equals(right.AsVector2Array()),
            Variant.Type.PackedVector3Array => left.AsVector3Array().Equals(right.AsVector3Array()),
            Variant.Type.PackedColorArray => left.AsColorArray().Equals(right.AsColorArray()),
            _ => throw new ArgumentOutOfRangeException(nameof(left))
        };
    }
}
