#if __WASM__

using Uno;
using Uno.Foundation.Interop;

namespace DevToys.MonacoEditor.WebInterop;

internal partial class DebugLogger : IJSObject
{
    private JSObjectHandle? _handle;

    /// <inheritdoc />
    public JSObjectHandle Handle
    {
        get
        {
            return _handle ??= JSObjectHandle.Create(this);
        }
    }

    [Preserve]
    public void log(string message)
    {
        Log(message);
    }
}

#endif
