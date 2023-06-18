///-------------------------------------------------------------------------------------------------------------
///  C# translation of the https://github.com/microsoft/monaco-editor/blob/main/website/typedoc/monaco.d.ts file
///-------------------------------------------------------------------------------------------------------------

namespace DevToys.Blazor.Components.Monaco.Editor;

public enum AccessibilitySupport
{
    /// <summary>
    /// This should be the browser case where it is not known if a screen reader is attached or no.
    /// </summary>
    Unknown = 0,

    Disabled = 1,

    Enabled = 2
}
