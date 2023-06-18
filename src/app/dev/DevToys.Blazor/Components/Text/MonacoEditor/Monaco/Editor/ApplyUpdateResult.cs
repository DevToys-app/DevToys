///-------------------------------------------------------------------------------------------------------------
///  C# translation of the https://github.com/microsoft/monaco-editor/blob/main/website/typedoc/monaco.d.ts file
///-------------------------------------------------------------------------------------------------------------

namespace DevToys.Blazor.Components.Monaco.Editor;

public class ApplyUpdateResult<T>
{
    public T NewValue { get; }

    public bool DidChange { get; }

    public ApplyUpdateResult(T newValue, bool didChange)
    {
        NewValue = newValue;
        DidChange = didChange;
    }
}
