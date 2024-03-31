namespace DevToys.Blazor.BuiltInGroups;

[Export(typeof(GuiToolGroup))]
[Name(PredefinedCommonToolGroupNames.Graphic)]
internal sealed class GraphicGroup : GuiToolGroup
{
    [ImportingConstructor]
    internal GraphicGroup()
    {
        IconFontName = "DevToys-Tools-Icons";
        IconGlyph = '\u0129';
        DisplayTitle = Groups.GraphicGroupDisplayTitle;
        AccessibleName = Groups.GraphicGroupAccessibleName;
    }
}
