///-------------------------------------------------------------------------------------------------------------
///  C# translation of the https://github.com/microsoft/monaco-editor/blob/main/website/typedoc/monaco.d.ts file
///-------------------------------------------------------------------------------------------------------------

namespace DevToys.Blazor.Components.Monaco;

public class MarkdownString
{
    public string? Value { get; set; }

    public bool? IsTrusted { get; set; }

    public bool? SupportThemeIcons { get; set; }

    public bool? SupportHtml { get; set; }

    public UriComponents? BaseUri { get; set; }
}
