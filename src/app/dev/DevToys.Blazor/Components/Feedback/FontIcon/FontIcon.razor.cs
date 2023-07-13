namespace DevToys.Blazor.Components;

public partial class FontIcon : StyledComponentBase
{
    protected string? StyleValue => new StyleBuilder()
        .AddStyle("height", Height.ToPx())
        .AddStyle("width", Width.ToPx())
        .AddStyle("font-size", Width.ToPx())
        .AddStyle("line-height", Height.ToPx())
        .AddStyle("transform", $"rotate({Rotation}deg)", Rotation != 0)
        .AddStyle(Style)
        .Build();

    public FontIcon()
    {
        VerticalAlignment = UIVerticalAlignment.Center;
        HorizontalAlignment = UIHorizontalAlignment.Center;
        Height = 16;
        Width = 16;
    }

    [Parameter]
    public char Glyph { get; set; }

    [Parameter]
    public string FontFamily { get; set; } = "FluentSystemIcons";

    [Parameter]
    public int Rotation { get; set; }

    protected override void OnParametersSet()
    {
        CSS.Clear();
        CSS.Add(FontFamily);

        base.OnParametersSet();
    }
}
