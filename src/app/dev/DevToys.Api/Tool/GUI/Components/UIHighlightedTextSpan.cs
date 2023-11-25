namespace DevToys.Api;

public record UIHighlightedTextSpan : TextSpan
{
    public UIHighlightedTextSpan(int startPosition, int length, UIHighlightedTextSpanColor color = UIHighlightedTextSpanColor.Default)
        : base(startPosition, length)
    {
        Color = color;
    }

    protected UIHighlightedTextSpan(TextSpan original, UIHighlightedTextSpanColor color = UIHighlightedTextSpanColor.Default)
        : base(original)
    {
        Color = color;
    }

    /// <summary>
    /// Gets the color of the highlighted text span.
    /// </summary>
    public UIHighlightedTextSpanColor Color { get; }
}
