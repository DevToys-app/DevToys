namespace DevToys.Blazor.BuiltInGroups;

[Export(typeof(GuiToolGroup))]
[Name(PredefinedCommonToolGroupNames.Text)]
internal sealed class TextGroup : GuiToolGroup
{
    [ImportingConstructor]
    internal TextGroup()
    {
        IconFontName = "DevToys-Tools-Icons";
        IconGlyph = '\u0132';
        DisplayTitle = Groups.TextGroupDisplayTitle;
        AccessibleName = Groups.TextGroupAccessibleName;
    }
}
