namespace DevToys.Tools.Tools.Formatters;

[Export(typeof(GuiToolGroup))]
[Name(PredefinedCommonToolGroupNames.Formatters)]
internal class FormattersGroup : GuiToolGroup
{
    [ImportingConstructor]
    internal FormattersGroup()
    {
        IconFontName = "DevToys-Tools-Icons";
        IconGlyph = '\u0122';
        DisplayTitle = Formatters.DisplayTitle;
        AccessibleName = Formatters.AccessibleName;
    }
}
