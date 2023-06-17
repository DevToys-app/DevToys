///-------------------------------------------------------------------------------------------------------------
///  C# translation of the https://github.com/microsoft/monaco-editor/blob/main/website/typedoc/monaco.d.ts file
///-------------------------------------------------------------------------------------------------------------

namespace DevToys.Blazor.Components.Monaco.Editor;

/// <summary>
/// Configuration options for unicode highlighting.
/// </summary>
public class UnicodeHighlightOptions
{
    /// <summary>
    /// Controls whether all non-basic ASCII characters are highlighted. Only characters between U+0020 and U+007E, tab, line-feed and carriage-return are considered basic ASCII.
    /// </summary>
    public bool? NonBasicASCII { get; set; }

    /// <summary>
    /// Controls whether characters that just reserve space or have no width at all are highlighted.
    /// </summary>
    public bool? InvisibleCharacters { get; set; }

    /// <summary>
    /// Controls whether characters are highlighted that can be confused with basic ASCII characters, except those that are common in the current user locale.
    /// </summary>
    public bool? AmbiguousCharacters { get; set; }

    /// <summary>
    /// Controls whether characters in comments should also be subject to unicode highlighting.
    /// </summary>
    public bool? IncludeComments { get; set; }

    /// <summary>
    /// Controls whether characters in strings should also be subject to unicode highlighting.
    /// </summary>
    public bool? IncludeStrings { get; set; }
}
