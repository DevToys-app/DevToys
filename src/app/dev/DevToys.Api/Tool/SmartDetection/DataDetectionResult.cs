namespace DevToys.Api;

public record DataDetectionResult(bool Success, object? Data)
{
    public readonly DataDetectionResult Unsuccessful = new(false, null);
}
