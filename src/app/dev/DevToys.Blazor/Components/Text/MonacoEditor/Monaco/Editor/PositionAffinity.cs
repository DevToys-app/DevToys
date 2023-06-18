///-------------------------------------------------------------------------------------------------------------
///  C# translation of the https://github.com/microsoft/monaco-editor/blob/main/website/typedoc/monaco.d.ts file
///-------------------------------------------------------------------------------------------------------------

namespace DevToys.Blazor.Components.Monaco.Editor;

public enum PositionAffinity
{
    /// <summary>
    /// Prefers the left most position.
    /// </summary>
    Left = 0,

    /// <summary>
    /// Prefers the right most position.
    /// </summary>
    Right = 1,

    /// <summary>
    /// No preference.
    /// </summary>
    None = 2,

    /// <summary>
    /// If the given position is on injected text, prefers the position left of it.
    /// </summary>
    LeftOfInjectedText = 3,

    /// <summary>
    /// If the given position is on injected text, prefers the position right of it.
    /// </summary>
    RightOfInjectedText = 4
}
