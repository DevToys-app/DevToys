namespace DevToys.Api;

/// <summary>
/// Represents the result of data detection.
/// </summary>
/// <param name="Success">A value indicating whether the data detection was successful.</param>
/// <param name="Data">The detected data.</param>
public record DataDetectionResult(bool Success, object? Data)
{
    /// <summary>
    /// Gets a value indicating whether the data detection was successful.
    /// </summary>
    public bool Success { get; init; } = Success;

    /// <summary>
    /// Gets the detected data.
    /// </summary>
    public object? Data { get; init; } = Data;

    /// <summary>
    /// Gets the type of the detected data.
    /// </summary>
    public Type? DataType => Data?.GetType();

    /// <summary>
    /// Represents an unsuccessful data detection result.
    /// </summary>
    public static readonly DataDetectionResult Unsuccessful = new(false, null);
}
