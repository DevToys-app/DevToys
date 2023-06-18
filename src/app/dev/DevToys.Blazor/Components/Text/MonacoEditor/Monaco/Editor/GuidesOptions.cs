///-------------------------------------------------------------------------------------------------------------
///  C# translation of the https://github.com/microsoft/monaco-editor/blob/main/website/typedoc/monaco.d.ts file
///-------------------------------------------------------------------------------------------------------------

namespace DevToys.Blazor.Components.Monaco.Editor;

public class GuidesOptions
{
    /// <summary>
    /// Enable rendering of bracket pair guides.
    /// Defaults to false.
    /// </summary>
    public string? BracketPairs { get; set; }

    /// <summary>
    /// Enable rendering of vertical bracket pair guides.
    /// Defaults to 'active'.
    /// </summary>
    public string? BracketPairsHorizontal { get; set; }

    /// <summary>
    /// Enable highlighting of the active bracket pair.
    /// Defaults to true.
    /// </summary>
    public bool? HighlightActiveBracketPair { get; set; }

    /// <summary>
    /// Enable rendering of indent guides.
    /// Defaults to true.
    /// </summary>
    public bool? Indentation { get; set; }

    /// <summary>
    /// Enable highlighting of the active indent guide.
    /// Defaults to true.
    /// </summary>
    public bool? HighlightActiveIndentation { get; set; }
}
