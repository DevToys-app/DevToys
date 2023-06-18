///-------------------------------------------------------------------------------------------------------------
///  C# translation of the https://github.com/microsoft/monaco-editor/blob/main/website/typedoc/monaco.d.ts file
///-------------------------------------------------------------------------------------------------------------

namespace DevToys.Blazor.Components.Monaco.Editor;

public class MouseTargetViewZoneData
{
    public string? ViewZoneId { get; set; }

    public Position? PositionBefore { get; set; }

    public Position? PositionAfter { get; set; }

    public Position? Position { get; set; }

    public int AfterLineNumber { get; set; }
}
