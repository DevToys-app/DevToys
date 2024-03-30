namespace DevToys.Api;

/// <summary>
/// Represents a highlighted span in a text.
/// </summary>
public record UIHighlightedTextSpan : TextSpan
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UIHighlightedTextSpan"/> class.
    /// </summary>
    /// <param name="startPosition">The start position of the highlighted text span.</param>
    /// <param name="length">The length of the highlighted text span.</param>
    /// <param name="color">The color of the highlighted text span.</param>
    public UIHighlightedTextSpan(int startPosition, int length, UIHighlightedTextSpanColor color = UIHighlightedTextSpanColor.Default)
        : base(startPosition, length)
    {
        Color = color;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UIHighlightedTextSpan"/> class based on the original <see cref="TextSpan"/>.
    /// </summary>
    /// <param name="span">The location where the text document should be highlighted <see cref="TextSpan"/>.</param>
    /// <param name="color">The color of the highlighted text span.</param>
    public UIHighlightedTextSpan(TextSpan span, UIHighlightedTextSpanColor color = UIHighlightedTextSpanColor.Default)
        : base(span)
    {
        Color = color;
    }

    /// <summary>
    /// Gets the color of the highlighted text span.
    /// </summary>
    public UIHighlightedTextSpanColor Color { get; }
}
