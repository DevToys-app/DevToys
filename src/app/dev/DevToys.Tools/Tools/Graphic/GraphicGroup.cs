namespace DevToys.Tools.Tools.Graphic;

[Export(typeof(GuiToolGroup))]
[Name(PredefinedCommonToolGroupNames.Graphic)]
internal sealed class GraphicGroup : GuiToolGroup
{
    [ImportingConstructor]
    internal GraphicGroup()
    {
        IconFontName = "DevToys-Tools-Icons";
        IconGlyph = '\u0129';
        DisplayTitle = Graphic.DisplayTitle;
        AccessibleName = Graphic.AccessibleName;
    }
}
