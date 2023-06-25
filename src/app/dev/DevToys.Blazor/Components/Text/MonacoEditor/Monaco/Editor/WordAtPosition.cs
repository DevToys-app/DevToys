///-------------------------------------------------------------------------------------------------------------
///  C# translation of the https://github.com/microsoft/monaco-editor/blob/main/website/typedoc/monaco.d.ts file
///-------------------------------------------------------------------------------------------------------------

namespace DevToys.Blazor.Components.Monaco.Editor;

/// <summary>
/// Word inside a model.
/// </summary>
public class WordAtPosition
{
    /// <summary>
    /// The word.
    /// </summary>
    public string? Word { get; set; }

    /// <summary>
    /// The column where the word starts.
    /// </summary>
    public int StartColumn { get; set; }

    /// <summary>
    /// The column where the word ends.
    /// </summary>
    public int EndColumn { get; set; }
}
