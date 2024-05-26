using System.Reflection;
using System.Runtime.CompilerServices;

namespace DevToys.Api;

/// <summary>
/// Represents the definition of a setting in the application.
/// </summary>
/// <typeparam name="T">The type of value of the setting</typeparam>
[DebuggerDisplay($"Name = {{{nameof(Name)}}}")]
public readonly struct SettingDefinition<T> : IEquatable<SettingDefinition<T>>
{
    /// <summary>
    /// Gets the name of the setting.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the default value of the setting.
    /// </summary>
    public T DefaultValue { get; }

    /// <summary>
    /// Gets a callback that can be used to serialize the value of the setting.
    /// </summary>
    public Func<T, string>? Serialize { get; }

    /// <summary>
    /// Gets a callback that can be used to deserialize the value of the setting.
    /// </summary>
    public Func<string, T>? Deserialize { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingDefinition{T}"/> structure.
    /// </summary>
    /// <param name="name">The name of the setting. Should be unique.</param>
    /// <param name="defaultValue">The default value of the setting.</param>
    public SettingDefinition(string name, T defaultValue)
    {
        Guard.IsNotNullOrWhiteSpace(name);

        if (name.Contains('='))
        {
            // For portable apps, settings are stored in a .ini file where the format is "setting_name=value".
            // Therefore, the setting name shouldn't contain "=".
            ThrowHelper.ThrowArgumentException(nameof(name), "Setting name cannot contain '='.");
        }

        Name = GenerateName(name);
        DefaultValue = defaultValue;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingDefinition{T}"/> structure.
    /// </summary>
    /// <param name="name">The name of the setting. Should be unique.</param>
    /// <param name="defaultValue">The default value of the setting.</param>
    /// <param name="serialize">A callback that can be used to serialize the value of the setting.</param>
    /// <param name="deserialize">A callback that can be used to deserialize the value of the setting.</param>
    public SettingDefinition(string name, T defaultValue, Func<T, string> serialize, Func<string, T> deserialize)
        : this(name, defaultValue)
    {
        Guard.IsNotNull(serialize);
        Guard.IsNotNull(deserialize);
        Serialize = serialize;
        Deserialize = deserialize;
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
    public override bool Equals(object? obj)
    {
        if (obj is SettingDefinition<T> definition)
        {
            return Equals(definition);
        }

        return false;
    }

    /// <summary>
    /// Determines whether the specified <see cref="SettingDefinition{T}"/> is equal to the current <see cref="SettingDefinition{T}"/>.
    /// </summary>
    /// <param name="other">The <see cref="SettingDefinition{T}"/> to compare with the current <see cref="SettingDefinition{T}"/>.</param>
    /// <returns>true if the specified <see cref="SettingDefinition{T}"/> is equal to the current <see cref="SettingDefinition{T}"/>; otherwise, false.</returns>
    public bool Equals(SettingDefinition<T> other)
    {
        return string.Equals(other.Name, Name, StringComparison.Ordinal)
            && other.DefaultValue is not null
            && other.DefaultValue.Equals(DefaultValue);
    }

    /// <summary>
    /// Returns the hash code for this <see cref="SettingDefinition{T}"/>.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
        return (Name.GetHashCode() ^ 47)
             * (DefaultValue is null ? 13 : DefaultValue.GetHashCode() ^ 73);
    }

    /// <summary>
    /// Determines whether two specified <see cref="SettingDefinition{T}"/> objects have the same value.
    /// </summary>
    /// <param name="left">The first <see cref="SettingDefinition{T}"/> to compare.</param>
    /// <param name="right">The second <see cref="SettingDefinition{T}"/> to compare.</param>
    /// <returns>true if the value of left is the same as the value of right; otherwise, false.</returns>
    public static bool operator ==(SettingDefinition<T> left, SettingDefinition<T> right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two specified <see cref="SettingDefinition{T}"/> objects have different values.
    /// </summary>
    /// <param name="left">The first <see cref="SettingDefinition{T}"/> to compare.</param>
    /// <param name="right">The second <see cref="SettingDefinition{T}"/> to compare.</param>
    /// <returns>true if the value of left is different from the value of right; otherwise, false.</returns>
    public static bool operator !=(SettingDefinition<T> left, SettingDefinition<T> right)
    {
        return !(left == right);
    }

    /// <summary>
    /// Generates the name of the setting by using the calling assembly, namespace, and class name.
    /// This is used to prevent collision between various extensions that may use the same setting name.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string GenerateName(string baseName)
    {
        // Using Stack Trace might not give us the right information in AOT scenarios as the call stack may be different.
        // Good thing is: as DevToys use MEF, we don't use AOT. So, we are good to go.
        // But this is something we may need to revisit in the future.
        var stackTrace = new StackTrace();
        StackFrame[] stackFrames = stackTrace.GetFrames();
        var currentAssembly = Assembly.GetExecutingAssembly();

        foreach (StackFrame frame in stackFrames)
        {
            MethodBase? method = frame.GetMethod();
            Type? type = method?.ReflectedType;
            Assembly? assembly = type?.Assembly;

            if (type is not null
                && assembly is not null
                && assembly != currentAssembly)
            {
                string? callingAssemblyName = assembly.GetName().Name;
                string settingName = $"{callingAssemblyName}.{baseName}";

                if (settingName.Length > 255)
                {
                    ThrowHelper.ThrowArgumentOutOfRangeException(nameof(settingName), "Setting name is limited to 255 characters.");
                }

                return settingName;
            }
        }

        return baseName;
    }
}
