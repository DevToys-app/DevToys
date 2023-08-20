namespace DevToys.Tools.Tools.EncodersDecoders;

[Export(typeof(GuiToolGroup))]
[Name(PredefinedCommonToolGroupNames.EncodersDecoders)]
internal sealed class EncodersDecodersGroup : GuiToolGroup
{
    [ImportingConstructor]
    internal EncodersDecodersGroup()
    {
        IconFontName = "DevToys-Tools-Icons";
        IconGlyph = '\u0105';
        DisplayTitle = EncodersDecoders.DisplayTitle;
        AccessibleName = EncodersDecoders.AccessibleName;
    }
}
