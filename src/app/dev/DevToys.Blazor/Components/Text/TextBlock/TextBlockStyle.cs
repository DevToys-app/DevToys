namespace DevToys.Blazor.Components;

public sealed class TextBlockAppearance
{
    internal static readonly TextBlockAppearance Caption = new("span", "caption");
    internal static readonly TextBlockAppearance Body = new("span", "body");
    internal static readonly TextBlockAppearance BodyStrong = new("h5", "body-strong");
    internal static readonly TextBlockAppearance BodyLarge = new("h5", "body-large");
    internal static readonly TextBlockAppearance Subtitle = new("h4", "subtitle");
    internal static readonly TextBlockAppearance Title = new("h3", "title");
    internal static readonly TextBlockAppearance TitleLarge = new("h2", "title-large");
    internal static readonly TextBlockAppearance Display = new("h1", "display");

    private TextBlockAppearance(string tagName, string className)
    {
        Guard.IsNotNullOrWhiteSpace(tagName);
        Guard.IsNotNullOrWhiteSpace(className);
        Tag = tagName;
        Class = className;
    }

    public string Tag { get; }

    public string Class { get; }
}
