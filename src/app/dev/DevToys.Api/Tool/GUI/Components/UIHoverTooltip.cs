namespace DevToys.Api;

/// <summary>
/// Represents the Tooltip to display on hover
/// </summary>
public record UIHoverTooltip
{
    /// <summary>
    /// Contain the position of the span to search
    /// </summary>
    public TextSpan Span { get; }

    /// <summary>
    /// Contain the information we want to display on hover
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Create a new isntance of the <see cref="UIHoverTooltip"/> class.
    /// </summary>
    public UIHoverTooltip(TextSpan span, string description)
    {
        Span = span;
        Description = description;
    }
}
