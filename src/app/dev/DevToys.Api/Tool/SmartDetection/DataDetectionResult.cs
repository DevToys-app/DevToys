namespace DevToys.Api;

public record DataDetectionResult(bool Success, object? Data)
{
    public static readonly DataDetectionResult Unsuccessful = new(false, null);

    public Type? DataType => Data?.GetType();
}
