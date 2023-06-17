///-------------------------------------------------------------------------------------------------------------
///  C# translation of the https://github.com/microsoft/monaco-editor/blob/main/website/typedoc/monaco.d.ts file
///-------------------------------------------------------------------------------------------------------------

namespace DevToys.Blazor.Components.Monaco.Editor;

/// <summary>
/// Options which apply for all editors.
/// </summary>
public interface IGlobalEditorOptions
{
    /// <summary>
    /// The number of spaces a tab is equal to.
    /// This setting is overridden based on the file contents when `detectIndentation` is on.
    /// Defaults to 4.
    /// </summary>
    int? TabSize { get; set; }

    /// <summary>
    /// Insert spaces when pressing `Tab`.
    /// This setting is overridden based on the file contents when `detectIndentation` is on.
    /// Defaults to true.
    /// </summary>
    bool? InsertSpaces { get; set; }

    /// <summary>
    /// Controls whether `tabSize` and `insertSpaces` will be automatically detected when a file is opened based on the file contents.
    /// Defaults to true.
    /// </summary>
    bool? DetectIndentation { get; set; }

    /// <summary>
    /// Remove trailing auto inserted whitespace.
    /// Defaults to true.
    /// </summary>
    bool? TrimAutoWhitespace { get; set; }

    /// <summary>
    /// Special handling for large files to disable certain memory intensive features.
    /// Defaults to true.
    /// </summary>
    bool? LargeFileOptimizations { get; set; }

    /// <summary>
    /// Controls whether completions should be computed based on words in the document.
    /// Defaults to true.
    /// </summary>
    bool? WordBasedSuggestions { get; set; }

    /// <summary>
    /// Controls whether word based completions should be included from opened documents of the same language or any language.
    /// </summary>
    bool? WordBasedSuggestionsOnlySameLanguage { get; set; }

    /// <summary>
    /// Keep peek editors open even when double clicking their content or when hitting `Escape`.
    /// Defaults to false.
    /// </summary>
    bool? StablePeek { get; set; }

    /// <summary>
    /// Lines above this length will not be tokenized for performance reasons.
    /// Defaults to 20000.
    /// </summary>
    int? MaxTokenizationLineLength { get; set; }

    /// <summary>
    /// Theme to be used for rendering.
    /// The current out-of-the-box available themes are: 'vs' (default), 'vs-dark', 'hc-black', 'hc-light'.
    /// You can create custom themes via `monaco.editor.defineTheme`.
    /// To switch a theme, use `monaco.editor.setTheme`.
    /// //////NOTE//////: The theme might be overwritten if the OS is in high contrast mode, unless `autoDetectHighContrast` is set to false.
    /// </summary>
    string Theme { get; set; }

    /// <summary>
    /// If enabled, will automatically change to high contrast theme if the OS is using a high contrast theme.
    /// Defaults to true.
    /// </summary>
    bool? AutoDetectHighContrast { get; set; }
}
