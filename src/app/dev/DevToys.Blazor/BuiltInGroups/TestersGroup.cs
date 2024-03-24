namespace DevToys.Blazor.BuiltInGroups;

[Export(typeof(GuiToolGroup))]
[Name(PredefinedCommonToolGroupNames.Testers)]
internal sealed class TestersGroup : GuiToolGroup
{
    [ImportingConstructor]
    internal TestersGroup()
    {
        IconFontName = "FluentSystemIcons";
        IconGlyph = '\uE33B';
        DisplayTitle = Groups.TestersGroupDisplayTitle;
        AccessibleName = Groups.TestersGroupAccessibleName;
    }
}
