///-------------------------------------------------------------------------------------------------------------
///  C# translation of the https://github.com/microsoft/monaco-editor/blob/main/website/typedoc/monaco.d.ts file
///-------------------------------------------------------------------------------------------------------------

namespace DevToys.Blazor.Components.Monaco.Editor;

// This class is not included in the monaco.d.ts file. It's added for translating the updateOptions method parameter
public class EditorUpdateOptions : EditorOptions, IGlobalEditorOptions
{
    public int? TabSize { get; set; }

    public bool? InsertSpaces { get; set; }

    public bool? DetectIndentation { get; set; }

    public bool? TrimAutoWhitespace { get; set; }

    public bool? LargeFileOptimizations { get; set; }

    public bool? WordBasedSuggestions { get; set; }

    public bool? WordBasedSuggestionsOnlySameLanguage { get; set; }

    public bool? StablePeek { get; set; }

    public int? MaxTokenizationLineLength { get; set; }

    public string Theme { get; set; } = default!;

    public bool? AutoDetectHighContrast { get; set; }
}
