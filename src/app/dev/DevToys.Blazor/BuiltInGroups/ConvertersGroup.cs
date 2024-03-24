namespace DevToys.Blazor.BuiltInGroups;

[Export(typeof(GuiToolGroup))]
[Name(PredefinedCommonToolGroupNames.Converters)]
internal class ConvertersGroup : GuiToolGroup
{
    [ImportingConstructor]
    internal ConvertersGroup()
    {
        IconFontName = "DevToys-Tools-Icons";
        IconGlyph = '\u0103';
        DisplayTitle = Groups.ConvertersGroupDisplayTitle;
        AccessibleName = Groups.ConvertersGroupAccessibleName;
    }
}
