///-------------------------------------------------------------------------------------------------------------
///  C# translation of the https://github.com/microsoft/monaco-editor/blob/main/website/typedoc/monaco.d.ts file
///-------------------------------------------------------------------------------------------------------------

namespace DevToys.Blazor.Components.Monaco.Editor;

/// <summary>
/// Configuration options for quick suggestions
/// </summary>
public class QuickSuggestionsOptions
{
    public bool? Other { get; set; }

    public bool? Comments { get; set; }

    public bool? Strings { get; set; }
}
