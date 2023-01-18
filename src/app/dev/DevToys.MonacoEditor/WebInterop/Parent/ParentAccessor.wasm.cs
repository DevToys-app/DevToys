#if __WASM__

using Uno;
using Uno.Extensions.Specialized;
using Uno.Foundation;
using Uno.Foundation.Interop;

namespace DevToys.MonacoEditor.WebInterop;

internal partial class ParentAccessor : IJSObject
{
    private JSObjectHandle? _handle;
    private bool _initialized;

    partial void PartialCtor()
    {
        // TODO: Remove???
        //getValue(null!);
        //setValue(null!, null!);
        //setValueWithType(null!, null!, null!);
        //getJsonValue(null!, null!);
        //callAction(null!);
        //callActionWithParameters(null!, null!, null!);
        //callEvent(null!, null!, null!, null!);
        //close();

        _initialized = true;
    }

    /// <inheritdoc />
    public JSObjectHandle? Handle
    {
        get
        {
            return _initialized ? (_handle ??= JSObjectHandle.Create(this)) : null;
        }
    }

    [Preserve]
    public object? getValue(string name)
    {
        if (Handle is null)
        {
            return null;
        }

        object? obj = GetValue(name);
        return obj;
    }

    [Preserve]
    public void setValue(string name, string value)
    {
        if (Handle is null)
        {
            return;
        }

        // TODO: this allocates a lot of string. Check if we can do better.
        string json = Desanitize(value);
        json = json.Replace(@"\\", @"\");
        json = json.Trim('"');
        json = json.Replace(@"\r\n", Environment.NewLine);
        json = json.Replace(@"\t", "\t");

        SetValue(name, json);
    }

    [Preserve]
    public void setValueWithType(string name, string value, string type)
    {
        if (Handle is null)
        {
            return;
        }

        SetValueWithType(name, value, type);
    }

    [Preserve]
    public void getJsonValue(string name, string returnId)
    {
        if (Handle is null)
        {
            return;
        }

        string json = GetJsonValue(name);

        try
        {
            string callbackMethod = $"returnValueCallback('{returnId}', {json});";
            string result = WebAssemblyRuntime.InvokeJS(callbackMethod);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Result Callback - error {e.Message}");
        }
    }

    [Preserve]
    public bool callAction(string name)
    {
        if (Handle is null)
        {
            return false;
        }

        bool result = CallAction(name);

        return result;
    }

    [Preserve]
    public bool callActionWithParameters(string name, string parameter1, string parameter2)
    {
        if (Handle is null)
        {
            return false;
        }

        string[] parameters
            = new[]
            {
                Desanitize(parameter1),
                Desanitize(parameter2)
            }
            .Where(x => x is not null)
            .ToArray();

        bool result = CallActionWithParameters(name, parameters);

        return result;
    }

    [Preserve]
    public async void callEvent(string name, string promiseId, string parameter1, string parameter2)
    {
        if (Handle is null)
        {
            return;
        }

        string[] parameters
            = new[]
            {
                Desanitize(parameter1),
                Desanitize(parameter2)
            }
            .Where(x => x is not null)
            .ToArray();

        string resultString = await CallEvent(name, parameters);

        try
        {
            string sanitized = Santize(resultString);

            string callbackMethod = $"asyncCallback('{promiseId}','{sanitized}');";

            string result = WebAssemblyRuntime.InvokeJS(callbackMethod);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Event Callback - error {e.Message}");
        }

        return;
    }

    [Preserve]
    public void close()
    {
        if (Handle is not null)
        {
            Dispose();
        }
    }

    internal static string Santize(string jsonString)
    {
        if (jsonString is null)
        {
            return string.Empty;
        }

        // TODO: this allocates a lot of string. Check if we can do better.
        string replacements = @"%&\""'{}:,";
        for (int i = 0; i < replacements.Length; i++)
        {
            jsonString = jsonString.Replace(replacements[i] + "", "%" + (int)replacements[i]);
        }

        return jsonString;
    }
}

#endif
