///-------------------------------------------------------------------------------------------------------------
///  C# translation of the https://github.com/microsoft/monaco-editor/blob/main/website/typedoc/monaco.d.ts file
///-------------------------------------------------------------------------------------------------------------

namespace DevToys.Blazor.Components.Monaco;

/// <summary>
/// A range in the editor. (startLineNumber,startColumn) is <= (endLineNumber,endColumn)
/// </summary>
public class Range
{
    /// <summary>
    /// Line number on which the range starts (starts at 1).
    /// </summary>
    public int StartLineNumber { get; set; }

    /// <summary>
    /// Column on which the range starts in line `startLineNumber` (starts at 1).
    /// </summary>
    public int StartColumn { get; set; }

    /// <summary>
    /// Line number on which the range ends.
    /// </summary>
    public int EndLineNumber { get; set; }

    /// <summary>
    /// Column on which the range ends in line `endLineNumber`.
    /// </summary>
    public int EndColumn { get; set; }

    public Range() { }

    public Range(int startLineNumber, int startColumn, int endLineNumber, int endColumn)
    {
        StartLineNumber = startLineNumber;
        StartColumn = startColumn;
        EndLineNumber = endLineNumber;
        EndColumn = endColumn;
    }
}
