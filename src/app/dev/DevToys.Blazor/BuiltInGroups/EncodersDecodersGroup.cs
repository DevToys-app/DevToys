namespace DevToys.Blazor.BuiltInGroups;

[Export(typeof(GuiToolGroup))]
[Name(PredefinedCommonToolGroupNames.EncodersDecoders)]
internal sealed class EncodersDecodersGroup : GuiToolGroup
{
    [ImportingConstructor]
    internal EncodersDecodersGroup()
    {
        IconFontName = "DevToys-Tools-Icons";
        IconGlyph = '\u0105';
        DisplayTitle = Groups.EncodersDecodersGroupDisplayTitle;
        AccessibleName = Groups.EncodersDecodersGroupAccessibleName;
    }
}
