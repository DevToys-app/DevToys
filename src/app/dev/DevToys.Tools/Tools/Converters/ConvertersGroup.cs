namespace DevToys.Tools.Tools.Converters;

[Export(typeof(GuiToolGroup))]
[Name("Converters")]
internal class ConvertersGroup : GuiToolGroup
{
    [ImportingConstructor]
    internal ConvertersGroup()
    {
        IconFontName = "FluentSystemIcons";
        IconGlyph = '\uE2A2';
        DisplayTitle = Converters.DisplayTitle;
        AccessibleName = Converters.AccessibleName;
    }
}
