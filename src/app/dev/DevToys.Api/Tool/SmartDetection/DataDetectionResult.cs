namespace DevToys.Api;

public record DataDetectionResult(bool Succeess, object? Data)
{
    public readonly DataDetectionResult Unsuccessful = new(false, null);
}
