///-------------------------------------------------------------------------------------------------------------
///  C# translation of the https://github.com/microsoft/monaco-editor/blob/main/website/typedoc/monaco.d.ts file
///-------------------------------------------------------------------------------------------------------------

namespace DevToys.Blazor.Components.Monaco.Editor;

public class BracketPairColorizationOptions
{
    /// <summary>
    /// Enable or disable bracket pair colorization.
    /// </summary>
    public bool? Enabled { get; set; }

    /// <summary>
    /// Use independent color pool per bracket type.
    /// </summary>
    public bool? IndependentColorPoolPerBracketType { get; set; }
}
