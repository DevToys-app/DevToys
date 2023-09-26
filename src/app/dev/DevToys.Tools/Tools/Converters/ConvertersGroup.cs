namespace DevToys.Tools.Tools.Converters;

[Export(typeof(GuiToolGroup))]
[Name(PredefinedCommonToolGroupNames.Converters)]
internal class ConvertersGroup : GuiToolGroup
{
    [ImportingConstructor]
    internal ConvertersGroup()
    {
        IconFontName = "DevToys-Tools-Icons";
        IconGlyph = '\u0103';
        DisplayTitle = Converters.DisplayTitle;
        AccessibleName = Converters.AccessibleName;
    }
}
