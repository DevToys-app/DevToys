namespace DevToys.MauiBlazor.Components;

public partial class FontIcon : StyledLayoutComponentBase
{
    [Parameter]
    public char Glyph { get; set; }

    [Parameter]
    public string FontFamily { get; set; } = string.Empty;

    protected override void AppendClasses(ClassHelper helper)
    {
        helper.Append("font-icon");
        helper.Append(FontFamily);
        base.AppendClasses(helper);
    }
}
