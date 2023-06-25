///-------------------------------------------------------------------------------------------------------------
///  C# translation of the https://github.com/microsoft/monaco-editor/blob/main/website/typedoc/monaco.d.ts file
///-------------------------------------------------------------------------------------------------------------

namespace DevToys.Blazor.Components.Monaco.Editor;

/// <summary>
/// Configuration options for the diff editor.
/// </summary>
public class DiffEditorOptions : EditorOptions, IDiffEditorBaseOptions
{
    public bool? EnableSplitViewResizing { get; set; }

    public bool? RenderSideBySide { get; set; }

    public int? MaxComputationTime { get; set; }

    public int? MaxFileSize { get; set; }

    public bool? IgnoreTrimWhitespace { get; set; }

    public bool? RenderIndicators { get; set; }

    public bool? OriginalEditable { get; set; }

    public bool? DiffCodeLens { get; set; }

    public bool? RenderOverviewRuler { get; set; }

    public string? DiffWordWrap { get; set; }

    public bool? RenderMarginRevertIcon { get; set; }
}
