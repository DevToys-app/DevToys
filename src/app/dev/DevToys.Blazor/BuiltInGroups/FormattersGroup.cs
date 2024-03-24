namespace DevToys.Blazor.BuiltInGroups;

[Export(typeof(GuiToolGroup))]
[Name(PredefinedCommonToolGroupNames.Formatters)]
internal class FormattersGroup : GuiToolGroup
{
    [ImportingConstructor]
    internal FormattersGroup()
    {
        IconFontName = "DevToys-Tools-Icons";
        IconGlyph = '\u0123';
        DisplayTitle = Groups.FormattersGroupDisplayTitle;
        AccessibleName = Groups.FormattersGroupAccessibleName;
    }
}
