namespace DevToys.Api;

public record PickedFile(string FileName, Stream Stream)
{
    public string FileName { get; init; } = FileName;

    public Stream Stream { get; init; } = Stream;
}
