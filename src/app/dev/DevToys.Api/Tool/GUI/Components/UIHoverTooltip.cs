namespace DevToys.Api;

/// <summary>
/// Represents the Tooltip to display on hover
/// </summary>
public record UIHoverTooltip : TextSpan
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UIHoverTooltip"/> class.
    /// </summary>
    /// <param name="startPosition">The start position of the highlighted text span.</param>
    /// <param name="length">The length of the highlighted text span.</param>
    /// <param name="tooltip">The information to display on hover.</param>
    public UIHoverTooltip(int startPosition, int length, string tooltip)
        : base(startPosition, length)
    {
        Tooltip = tooltip;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UIHoverTooltip"/> class.
    /// </summary>
    /// <param name="span">The location in the text document where the tooltip must appear.</param>
    /// <param name="tooltip">The information to display on hover.</param>
    public UIHoverTooltip(TextSpan span, string tooltip)
        : base(span)
    {
        Tooltip = tooltip;
    }

    /// <summary>
    /// Contain the information to display on hover
    /// </summary>
    public string Tooltip { get; }
}
