///-------------------------------------------------------------------------------------------------------------
///  C# translation of the https://github.com/microsoft/monaco-editor/blob/main/website/typedoc/monaco.d.ts file
///-------------------------------------------------------------------------------------------------------------

namespace DevToys.Blazor.Components.Monaco.Editor;

/// <summary>
/// Configuration options for editor hover
/// </summary>
public class EditorHoverOptions
{
    /// <summary>
    /// Enable the hover.
    /// Defaults to true.
    /// </summary>
    public bool? Enabled { get; set; }

    /// <summary>
    /// Delay for showing the hover.
    /// Defaults to 300.
    /// </summary>
    public int? Delay { get; set; }

    /// <summary>
    /// Is the hover sticky such that it can be clicked and its contents selected?
    /// Defaults to true.
    /// </summary>
    public bool? Sticky { get; set; }

    /// <summary>
    /// Should the hover be shown above the line if possible?
    /// Defaults to false.
    /// </summary>
    public bool? Above { get; set; }
}
