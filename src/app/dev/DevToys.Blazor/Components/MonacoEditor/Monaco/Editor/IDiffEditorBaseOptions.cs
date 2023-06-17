///-------------------------------------------------------------------------------------------------------------
///  C# translation of the https://github.com/microsoft/monaco-editor/blob/main/website/typedoc/monaco.d.ts file
///-------------------------------------------------------------------------------------------------------------

namespace DevToys.Blazor.Components.Monaco.Editor;

public interface IDiffEditorBaseOptions
{
    /// <summary>
    /// Allow the user to resize the diff editor split view.
    /// Defaults to true.
    /// </summary>
    bool? EnableSplitViewResizing { get; set; }

    /// <summary>
    /// Render the differences in two side-by-side editors.
    /// Defaults to true.
    /// </summary>
    bool? RenderSideBySide { get; set; }

    /// <summary>
    /// Timeout in milliseconds after which diff computation is cancelled.
    /// Defaults to 5000.
    /// </summary>
    int? MaxComputationTime { get; set; }

    /// <summary>
    /// Maximum supported file size in MB.
    /// Defaults to 50.
    /// </summary>
    int? MaxFileSize { get; set; }

    /// <summary>
    /// Compute the diff by ignoring leading/trailing whitespace
    /// Defaults to true.
    /// </summary>
    bool? IgnoreTrimWhitespace { get; set; }

    /// <summary>
    /// Render +/- indicators for added/deleted changes.
    /// Defaults to true.
    /// </summary>
    bool? RenderIndicators { get; set; }

    /// <summary>
    /// Shows icons in the glyph margin to revert changes.
    /// Default to true.
    /// </summary>
    bool? RenderMarginRevertIcon { get; set; }

    /// <summary>
    /// Original model should be editable?
    /// Defaults to false.
    /// </summary>
    bool? OriginalEditable { get; set; }

    /// <summary>
    /// Should the diff editor enable code lens?
    /// Defaults to false.
    /// </summary>
    bool? DiffCodeLens { get; set; }

    /// <summary>
    /// Is the diff editor should render overview ruler
    /// Defaults to true
    /// </summary>
    bool? RenderOverviewRuler { get; set; }

    /// <summary>
    /// Control the wrapping of the diff editor.
    /// </summary>
    string? DiffWordWrap { get; set; }
}
