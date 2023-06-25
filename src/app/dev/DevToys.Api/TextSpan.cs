namespace DevToys.Api;

/// <summary>
/// Represents a span in a text.
/// </summary>
public record TextSpan
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TextSpan"/> class.
    /// </summary>
    /// <param name="startPosition">The position at which the span starts.</param>
    /// <param name="length">The length of the span.</param>
    public TextSpan(int startPosition, int length)
    {
        Guard.IsGreaterThanOrEqualTo(startPosition, 0);
        Guard.IsGreaterThanOrEqualTo(length, 0);
        StartPosition = startPosition;
        Length = length;
    }

    /// <summary>
    /// Gets the position at which the span starts.
    /// </summary>
    public int StartPosition { get; }

    /// <summary>
    /// Gets the length of the span.
    /// </summary>
    public int Length { get; }

    /// <summary>
    /// Gets the end position of the span.
    /// </summary>
    public int EndPosition => StartPosition + Length;
}
