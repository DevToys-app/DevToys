///-------------------------------------------------------------------------------------------------------------
///  C# translation of the https://github.com/microsoft/monaco-editor/blob/main/website/typedoc/monaco.d.ts file
///-------------------------------------------------------------------------------------------------------------

namespace DevToys.Blazor.Components.Monaco.Editor;

/// <summary>
/// Configuration options for editor minimap
/// </summary>
public class EditorMinimapOptions
{
    /// <summary>
    /// Enable the rendering of the minimap.
    /// Defaults to true.
    /// </summary>
    public bool? Enabled { get; set; }

    /// <summary>
    /// Control the rendering of minimap.
    /// </summary>
    public bool? Autohide { get; set; }

    /// <summary>
    /// Control the side of the minimap in editor.
    /// Defaults to 'right'.
    /// </summary>
    public string? Side { get; set; }

    /// <summary>
    /// Control the minimap rendering mode.
    /// Defaults to 'actual'.
    /// </summary>
    public string? Size { get; set; }

    /// <summary>
    /// Control the rendering of the minimap slider.
    /// Defaults to 'mouseover'.
    /// </summary>
    public string? ShowSlider { get; set; }

    /// <summary>
    /// Render the actual text on a line (as opposed to color blocks).
    /// Defaults to true.
    /// </summary>
    public bool? RenderCharacters { get; set; }

    /// <summary>
    /// Limit the width of the minimap to render at most a certain number of columns.
    /// Defaults to 120.
    /// </summary>
    public int? MaxColumn { get; set; }

    /// <summary>
    /// Relative size of the font in the minimap. Defaults to 1.
    /// </summary>
    public float? Scale { get; set; }
}
