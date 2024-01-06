namespace DevToys.Tools.Tools.Testers;

[Export(typeof(GuiToolGroup))]
[Name(PredefinedCommonToolGroupNames.Testers)]
internal sealed class TestersGroup : GuiToolGroup
{
    [ImportingConstructor]
    internal TestersGroup()
    {
        IconFontName = "FluentSystemIcons";
        IconGlyph = '\uE33B';
        DisplayTitle = Testers.DisplayTitle;
        AccessibleName = Testers.AccessibleName;
    }
}
