///-------------------------------------------------------------------------------------------------------------
///  C# translation of the https://github.com/microsoft/monaco-editor/blob/main/website/typedoc/monaco.d.ts file
///-------------------------------------------------------------------------------------------------------------

namespace DevToys.Blazor.Components.Monaco.Editor;

public class ModelContentChange
{
    /// <summary>
    /// The range that got replaced.
    /// </summary>
    public Range? Range { get; set; }

    /// <summary>
    /// The offset of the range that got replaced.
    /// </summary>
    public int RangeOffset { get; set; }

    /// <summary>
    /// The length of the range that got replaced.
    /// </summary>
    public int RangeLength { get; set; }

    /// <summary>
    /// The new text for the range.
    /// </summary>
    public string? Text { get; set; }
}
