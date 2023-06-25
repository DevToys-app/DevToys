///-------------------------------------------------------------------------------------------------------------
///  C# translation of the https://github.com/microsoft/monaco-editor/blob/main/website/typedoc/monaco.d.ts file
///-------------------------------------------------------------------------------------------------------------

namespace DevToys.Blazor.Components.Monaco.Editor;

/// <summary>
/// The options to create an editor.
/// </summary>
public sealed class StandaloneEditorConstructionOptions : EditorConstructionOptions, IGlobalEditorOptions
{
    /// <summary>
    /// The initial model associated with this code editor.
    /// </summary>
    public TextModel? Model { get; set; }

    /// <summary>
    /// The initial value of the auto created model in the editor.
    /// To not automatically create a model, use `model: null`.
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    /// The initial language of the auto created model in the editor.
    /// To not automatically create a model, use `model: null`.
    /// </summary>
    public string? Language { get; set; }

    /// <summary>
    /// Initial theme to be used for rendering.
    /// The current out-of-the-box available themes are: 'vs' (default), 'vs-dark', 'hc-black', 'hc-light'.
    /// You can create custom themes via `monaco.editor.defineTheme`.
    /// To switch a theme, use `monaco.editor.setTheme`.
    /// //////NOTE//////: The theme might be overwritten if the OS is in high contrast mode, unless `autoDetectHighContrast` is set to false.
    /// </summary>
    public string Theme { get; set; } = BuiltinTheme.Vs;

    /// <summary>
    /// If enabled, will automatically change to high contrast theme if the OS is using a high contrast theme.
    /// Defaults to true.
    /// </summary>
    public bool? AutoDetectHighContrast { get; set; } = true;

    /// <summary>
    /// An URL to open when Ctrl+H (Windows and Linux) or Cmd+H (OSX) is pressed in
    /// the accessibility help dialog in the editor.
    ///
    /// Defaults to "https://go.microsoft.com/fwlink/?linkid=852450"
    /// </summary>
    public string? AccessibilityHelpUrl { get; set; }

    public int? TabSize { get; set; }

    public bool? InsertSpaces { get; set; }

    public bool? DetectIndentation { get; set; }

    public bool? TrimAutoWhitespace { get; set; }

    public bool? LargeFileOptimizations { get; set; }

    public bool? WordBasedSuggestions { get; set; }

    public bool? WordBasedSuggestionsOnlySameLanguage { get; set; }

    public bool? StablePeek { get; set; }

    public int? MaxTokenizationLineLength { get; set; }
}
