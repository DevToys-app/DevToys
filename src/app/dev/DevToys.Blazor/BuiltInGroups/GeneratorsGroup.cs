namespace DevToys.Blazor.BuiltInGroups;

[Export(typeof(GuiToolGroup))]
[Name(PredefinedCommonToolGroupNames.Generators)]
internal sealed class GeneratorsGroup : GuiToolGroup
{
    [ImportingConstructor]
    internal GeneratorsGroup()
    {
        IconFontName = "DevToys-Tools-Icons";
        IconGlyph = '\u0126';
        DisplayTitle = Groups.GeneratorsGroupDisplayTitle;
        AccessibleName = Groups.GeneratorsGroupAccessibleName;
    }
}
