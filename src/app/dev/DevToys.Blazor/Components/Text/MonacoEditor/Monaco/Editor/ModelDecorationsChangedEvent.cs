///-------------------------------------------------------------------------------------------------------------
///  C# translation of the https://github.com/microsoft/monaco-editor/blob/main/website/typedoc/monaco.d.ts file
///-------------------------------------------------------------------------------------------------------------

namespace DevToys.Blazor.Components.Monaco.Editor;

/// <summary>
/// An event describing that model decorations have changed.
/// </summary>
public class ModelDecorationsChangedEvent
{
    public bool AffectsMinimap { get; set; }

    public bool AffectsOverviewRuler { get; set; }
}
