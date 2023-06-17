///-------------------------------------------------------------------------------------------------------------
///  C# translation of the https://github.com/microsoft/monaco-editor/blob/main/website/typedoc/monaco.d.ts file
///-------------------------------------------------------------------------------------------------------------

namespace DevToys.Blazor.Components.Monaco.Editor;

/// <summary>
/// The options to create a diff editor.
/// </summary>
public class StandaloneDiffEditorConstructionOptions : DiffEditorConstructionOptions
{
    /// <summary>
    /// Initial theme to be used for rendering.
    /// The current out-of-the-box available themes are: 'vs' (default), 'vs-dark', 'hc-black', 'hc-light'.
    /// You can create custom themes via `monaco.editor.defineTheme`.
    /// To switch a theme, use `monaco.editor.setTheme`.
    /// //////NOTE//////: The theme might be overwritten if the OS is in high contrast mode, unless `autoDetectHighContrast` is set to false.
    /// </summary>
    public string? Theme { get; set; }

    /// <summary>
    /// If enabled, will automatically change to high contrast theme if the OS is using a high contrast theme.
    /// Defaults to true.
    /// </summary>
    public bool? AutoDetectHighContrast { get; set; }
}
