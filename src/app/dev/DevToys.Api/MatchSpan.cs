namespace DevToys.Api;

/// <summary>
/// Represents a span in text.
/// </summary>
public record MatchSpan
{
    /// <summary>
    /// Gets the position at which the span starts.
    /// </summary>
    public int StartPosition { get; }

    /// <summary>
    /// Gets the length of the span.
    /// </summary>
    public int Length { get; }

    public MatchSpan(int startPosition, int length)
    {
        StartPosition = startPosition;
        Length = length;
    }
}
