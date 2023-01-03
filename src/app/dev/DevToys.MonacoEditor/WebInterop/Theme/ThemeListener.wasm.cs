#if __WASM__

using DevToys.MonacoEditor.WebInterop.Parent;
using Newtonsoft.Json;
using Uno;
using Uno.Foundation;
using Uno.Foundation.Interop;

namespace DevToys.MonacoEditor.WebInterop.Theme;

internal partial class ThemeListener : IJSObject
{
    partial void PartialCtor()
    {
        Handle = JSObjectHandle.Create(this);

        // TODO: Remove?
        // getCurrentThemeName(null);
        // getIsHighContrast(null);
    }

    public JSObjectHandle Handle { get; private set; }

    [Preserve]
    public void getAccentColorHtmlHex(string returnId)
    {
        getJsonValue(returnId, AccentColorHtmlHex);
    }

    [Preserve]
    public void getCurrentThemeName(string returnId)
    {
        getJsonValue(returnId, CurrentThemeName);
    }

    [Preserve]
    public void getIsHighContrast(string returnId)
    {
        getJsonValue(returnId, IsHighContrast);
    }

    private void getJsonValue<T>(string returnId, T obj)
    {
        if (Handle == null)
        {
            return;
        }

        string returnJson = "";
        if (obj != null)
        {
            returnJson = JsonConvert.SerializeObject(obj, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            // System.Diagnostics.Debug.WriteLine($"Json Object - {returnJson}");
        }
        returnJson = ParentAccessor.Santize(returnJson);

        try
        {
            string callbackMethod = $"returnValueCallback('{returnId}','{returnJson}');";
            string result = WebAssemblyRuntime.InvokeJS(callbackMethod);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Result Callback - error {e.Message}");
        }
    }
}

#endif
