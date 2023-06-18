///-------------------------------------------------------------------------------------------------------------
///  C# translation of the https://github.com/microsoft/monaco-editor/blob/main/website/typedoc/monaco.d.ts file
///-------------------------------------------------------------------------------------------------------------

namespace DevToys.Blazor.Components.Monaco.Editor;

/// <summary>
/// An event describing that the current language associated with a model has changed.
/// </summary>
public class ModelLanguageChangedEvent
{
    /// <summary>
    /// Previous language
    /// </summary>
    public string? OldLanguage { get; set; }

    /// <summary>
    /// New language
    /// </summary>
    public string? NewLanguage { get; set; }
}
