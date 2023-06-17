///-------------------------------------------------------------------------------------------------------------
///  C# translation of the https://github.com/microsoft/monaco-editor/blob/main/website/typedoc/monaco.d.ts file
///-------------------------------------------------------------------------------------------------------------

namespace DevToys.Blazor.Components.Monaco.Editor;

/// <summary>
/// Configuration options for editor inlayHints
/// </summary>
public class EditorInlayHintsOptions
{
    /// <summary>
    /// Enable the inline hints.
    /// Defaults to true.
    /// </summary>
    public string? Enabled { get; set; }

    /// <summary>
    /// Font size of inline hints.
    /// Default to 90% of the editor font size.
    /// </summary>
    public float? FontSize { get; set; }

    /// <summary>
    /// Font family of inline hints.
    /// Defaults to editor font family.
    /// </summary>
    public string? FontFamily { get; set; }

    /// <summary>
    /// Enables the padding around the inlay hint.
    /// Defaults to false.
    /// </summary>
    public bool? Padding { get; set; }
}
