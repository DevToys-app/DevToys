///-------------------------------------------------------------------------------------------------------------
///  C# translation of the https://github.com/microsoft/monaco-editor/blob/main/website/typedoc/monaco.d.ts file
///-------------------------------------------------------------------------------------------------------------

namespace DevToys.Blazor.Components.Monaco.Editor;

public class StandaloneThemeData
{
    public string? Base { get; set; }

    public bool Inherit { get; set; }

    public List<TokenThemeRule>? Rules { get; set; }

    public List<string>? EncodedTokensColors { get; set; }

    public Dictionary<string, string>? Colors { get; set; }
}
