///-------------------------------------------------------------------------------------------------------------
///  C# translation of the https://github.com/microsoft/monaco-editor/blob/main/website/typedoc/monaco.d.ts file
///-------------------------------------------------------------------------------------------------------------

namespace DevToys.Blazor.Components.Monaco.Editor;

/// <summary>
/// Configuration options for editor scrollbars
/// </summary>
public class EditorScrollbarOptions
{
    /// <summary>
    /// The size of arrows (if displayed).
    /// Defaults to 11.
    /// //////NOTE//////: This option cannot be updated using `updateOptions()`
    /// </summary>
    public int? ArrowSize { get; set; }

    /// <summary>
    /// Render vertical scrollbar.
    /// Defaults to 'auto'.
    /// </summary>
    public string? Vertical { get; set; }

    /// <summary>
    /// Render horizontal scrollbar.
    /// Defaults to 'auto'.
    /// </summary>
    public string? Horizontal { get; set; }

    /// <summary>
    /// Cast horizontal and vertical shadows when the content is scrolled.
    /// Defaults to true.
    /// //////NOTE//////: This option cannot be updated using `updateOptions()`
    /// </summary>
    public bool? UseShadows { get; set; }

    /// <summary>
    /// Render arrows at the top and bottom of the vertical scrollbar.
    /// Defaults to false.
    /// //////NOTE//////: This option cannot be updated using `updateOptions()`
    /// </summary>
    public bool? VerticalHasArrows { get; set; }

    /// <summary>
    /// Render arrows at the left and right of the horizontal scrollbar.
    /// Defaults to false.
    /// //////NOTE//////: This option cannot be updated using `updateOptions()`
    /// </summary>
    public bool? HorizontalHasArrows { get; set; }

    /// <summary>
    /// Listen to mouse wheel events and react to them by scrolling.
    /// Defaults to true.
    /// </summary>
    public bool? HandleMouseWheel { get; set; }

    /// <summary>
    /// Always consume mouse wheel events (always call preventDefault() and stopPropagation() on the browser events).
    /// Defaults to true.
    /// //////NOTE//////: This option cannot be updated using `updateOptions()`
    /// </summary>
    public bool? AlwaysConsumeMouseWheel { get; set; }

    /// <summary>
    /// Height in pixels for the horizontal scrollbar.
    /// Defaults to 10 (px).
    /// </summary>
    public int? HorizontalScrollbarSize { get; set; }

    /// <summary>
    /// Width in pixels for the vertical scrollbar.
    /// Defaults to 10 (px).
    /// </summary>
    public int? VerticalScrollbarSize { get; set; }

    /// <summary>
    /// Width in pixels for the vertical slider.
    /// Defaults to `verticalScrollbarSize`.
    /// //////NOTE//////: This option cannot be updated using `updateOptions()`
    /// </summary>
    public int? VerticalSliderSize { get; set; }

    /// <summary>
    /// Height in pixels for the horizontal slider.
    /// Defaults to `horizontalScrollbarSize`.
    /// //////NOTE//////: This option cannot be updated using `updateOptions()`
    /// </summary>
    public int? HorizontalSliderSize { get; set; }

    /// <summary>
    /// Scroll gutter clicks move by page vs jump to position.
    /// Defaults to false.
    /// </summary>
    public bool? ScrollByPage { get; set; }
}
