using System.Reflection;
using Newtonsoft.Json;
using Windows.Foundation.Metadata;

namespace DevToys.MonacoEditor.WebInterop;

/// <summary>
/// Class to help in accessing .NET values from JavaScript.
/// Not Thread Safe.
/// </summary>
[AllowForWeb]
internal sealed partial class ParentAccessor : IDisposable
{
    private readonly WeakReference<IParentAccessorAcceptor> _parent;
    private readonly Type _typeinfo;
    private readonly Dictionary<string, Action<string[]>> _actionParameters;
    private readonly Dictionary<string, Action> _actions = new();
    private readonly Dictionary<string, Func<string[], Task<string>>> _events = new();

    private List<Assembly> Assemblies { get; } = new List<Assembly>();

    /// <summary>
    /// Constructs a new reflective parent Accessor for the provided object.
    /// </summary>
    /// <param name="parent">Object to provide Property Access.</param>
    internal ParentAccessor(IParentAccessorAcceptor parent)
    {
        this._parent = new WeakReference<IParentAccessorAcceptor>(parent);
        _typeinfo = parent.GetType();
        _actionParameters = new Dictionary<string, Action<string[]>>();

        PartialCtor();
    }

    partial void PartialCtor();

    public void Dispose()
    {
        _actions.Clear();
        _events.Clear();
    }

    /// <summary>
    /// Registers an action from the .NET side which can be called from within the JavaScript code.
    /// </summary>
    /// <param name="name">String Key.</param>
    /// <param name="action">Action to perform.</param>
    internal void RegisterAction(string name, Action action)
    {
        _actions[name] = action;
    }

    internal void RegisterActionWithParameters(string name, Action<string[]> action)
    {
        _actionParameters[name] = action;
    }

    /// <summary>
    /// Registers an event from the .NET side which can be called with the given jsonified string arguments within the JavaScript code.
    /// </summary>
    /// <param name="name">String Key.</param>
    /// <param name="function">Event to call.</param>
    internal void RegisterEvent(string name, Func<string[], Task<string>> function)
    {
        _events[name] = function;
    }

    /// <summary>
    /// Calls an Event registered before with the <see cref="RegisterEvent(string, Func{string[], Task{string}})"/>.
    /// </summary>
    /// <param name="name">Name of event to call.</param>
    /// <param name="parameters">JSON string Parameters.</param>
    /// <returns></returns>
    public Task<string> CallEvent(string name, string[] parameters)
    {
        Debug.WriteLine($"Event {name}");
        if (_events.ContainsKey(name))
        {
            Debug.WriteLine($"Parameters: {parameters != null} - {parameters?.Length.ToString() ?? "N/A"}");
            parameters ??= Array.Empty<string>();
            return _events[name].Invoke(parameters);
        }

        return Task.FromResult(string.Empty);
    }

    /// <summary>
    /// Adds an Assembly to use for looking up types by name for <see cref="SetValue(string, string, string)"/>.
    /// </summary>
    /// <param name="assembly">Assembly to add.</param>
    internal void AddAssemblyForTypeLookup(Assembly assembly)
    {
        Assemblies.Add(assembly);
    }

    /// <summary>
    /// Calls an Action registered before with <see cref="RegisterAction(string, Action)"/>.
    /// </summary>
    /// <param name="name">String Key.</param>
    /// <returns>True if method was found in registration.</returns>
    public bool CallAction(string name)
    {
        if (_actions.ContainsKey(name))
        {
            _actions[name]?.Invoke();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Calls an Action registered before with <see cref="RegisterActionWithParameters(string, Action{string[]})"/>.
    /// </summary>
    /// <param name="name">String Key.</param>
    /// <param name="parameters">Parameters to be passed to Action.</param>
    /// <returns>True if method was found in registration.</returns>
    public bool CallActionWithParameters(string name, string[] parameters)
    {
        if (_actionParameters.ContainsKey(name))
        {
            _actionParameters[name]?.Invoke(parameters);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Returns the winrt primative object value for the specified Property.
    /// </summary>
    /// <param name="name">Property name on Parent Object.</param>
    /// <returns>Property Value or null.</returns>
    public object? GetValue(string name)
    {
        if (_parent.TryGetTarget(out IParentAccessorAcceptor? target) && target is not null)
        {
            PropertyInfo? propinfo = _typeinfo.GetProperty(name);
            Guard.IsNotNull(propinfo);
            return propinfo.GetValue(target);
        }

        return null;
    }

    public string GetJsonValue(string name)
    {
        if (_parent.TryGetTarget(out IParentAccessorAcceptor? target) && target is not null)
        {
            PropertyInfo? propinfo = _typeinfo.GetProperty(name);
            Guard.IsNotNull(propinfo);
            object? obj = propinfo.GetValue(target);

            if (obj is not null)
            {
                string json = JsonConvert.SerializeObject(obj, new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore
                });

                // Serialize the json to sanitize it and pass it as a string to the web page.
                return JsonConvert.SerializeObject(json);
            }
        }

        return "\"{}\"";
    }

    /// <summary>
    /// Returns the winrt primative object value for a child property off of the specified Property.
    /// 
    /// Useful for providing complex types to users of Parent but still access primatives in JavaScript.
    /// </summary>
    /// <param name="name">Parent Property name.</param>
    /// <param name="child">Property's Property name to retrieve.</param>
    /// <returns>Value of Child Property or null.</returns>
    public object? GetChildValue(string name, string child)
    {
        if (_parent.TryGetTarget(out IParentAccessorAcceptor? target) && target is not null)
        {
            // TODO: Support params for multi-level digging?
            PropertyInfo? propinfo = _typeinfo.GetProperty(name);
            Guard.IsNotNull(propinfo);
            object? prop = propinfo.GetValue(target);
            if (prop is not null)
            {
                PropertyInfo? childinfo = prop.GetType().GetProperty(child);
                Guard.IsNotNull(childinfo);
                return childinfo.GetValue(prop);
            }
        }

        return null;
    }

    /// <summary>
    /// Sets the value for the specified Property.
    /// </summary>
    /// <param name="name">Parent Property name.</param>
    /// <param name="value">Value to set.</param>
    public void SetValue(string name, object value)
    {
        if (_parent.TryGetTarget(out IParentAccessorAcceptor? target) && target is not null)
        {
            PropertyInfo? propinfo = _typeinfo.GetProperty(name); // TODO: Cache these?
            Guard.IsNotNull(propinfo);
            target.IsSettingValue = true;
            propinfo.SetValue(target, value);
            target.IsSettingValue = false;
        }
    }

    /// <summary>
    /// Sets the value for the specified Property after deserializing the value as the given type name.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <param name="type"></param>
    public void SetValueWithType(string name, string value, string type)
    {
        if (_parent.TryGetTarget(out IParentAccessorAcceptor? target) && target is not null)
        {
            PropertyInfo? propinfo = _typeinfo.GetProperty(name);
            Guard.IsNotNull(propinfo);
            Type typeobj = LookForTypeByName(type);

            // TODO: this allocates a lot of string. Check if we can do better.
            string json = Desanitize(value);
            json = json.Replace(@"\\", @"\");
            json = json.Trim('"', ' ');
            json = json.Replace(@"\r\n", Environment.NewLine);
            json = json.Replace(@"\t", "\t");

            object? obj = JsonConvert.DeserializeObject(json, typeobj);

            target.IsSettingValue = true;
            propinfo.SetValue(target, obj);
            target.IsSettingValue = false;
        }
    }

    private Type LookForTypeByName(string name)
    {
        // First search locally
        var result = Type.GetType(name);

        if (result is not null)
        {
            return result;
        }

        // Search in Other Assemblies
        foreach (Assembly assembly in Assemblies)
        {
            foreach (Type typeInfo in assembly.ExportedTypes)
            {
                if (typeInfo.Name == name)
                {
                    return typeInfo;
                }
            }
        }

        throw new Exception($"Unable to solve the type '{name}'");
    }

    private static string Desanitize(string parameter)
    {
        if (parameter is null)
        {
            return string.Empty;
        }

        // TODO: this allocates a lot of string. Check if we can do better.
        string replacements = @"&\""'{}:,%";

        for (int i = 0; i < replacements.Length; i++)
        {
            parameter = parameter.Replace($"%{(int)replacements[i]}", (char)replacements[i] + "");
        }

        parameter = parameter.Replace(@"\\""", @"""");

        return parameter;
    }
}
