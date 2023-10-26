namespace DevToys.Tools.Tools.Generators;

[Export(typeof(GuiToolGroup))]
[Name(PredefinedCommonToolGroupNames.Generators)]
internal sealed class GeneratorsGroup : GuiToolGroup
{
    [ImportingConstructor]
    internal GeneratorsGroup()
    {
        IconFontName = "DevToys-Tools-Icons";
        IconGlyph = '\u0126';
        DisplayTitle = Generators.DisplayTitle;
        AccessibleName = Generators.AccessibleName;
    }
}
