using OneOf;

namespace DevToys.Api.Core;

public static class OneOfExtensions
{
    public static Task<Stream> GetStreamAsync(
        this OneOf<string, FileInfo> input,
        IFileStorage fileStorage,
        CancellationToken cancellationToken)
    {
        if (input.IsT1)
        {
            return GetStreamAsync(OneOf<FileInfo, string>.FromT0(input.AsT1), fileStorage, cancellationToken);
        }

        return GetStreamAsync(OneOf<FileInfo, string>.FromT1(input.AsT0), fileStorage, cancellationToken);
    }

    public static Task<Stream> GetStreamAsync(
        this OneOf<FileInfo, string> input,
        IFileStorage fileStorage,
        CancellationToken cancellationToken)
    {
        return
            input.Match(
                inputFile => Task.FromResult(fileStorage.OpenReadFile(inputFile.FullName)),
                async inputString =>
                {
                    var stringStream = new MemoryStream();
                    var writer = new StreamWriter(stringStream);
                    if (inputString is not null)
                    {
                        await writer.WriteAsync(inputString.AsMemory(), cancellationToken);
                    }
                    writer.Flush();
                    stringStream.Position = 0;
                    return (Stream)stringStream;
                });
    }

    public static Task<string> ReadAllTextAsync(
        this OneOf<string, FileInfo> input,
        IFileStorage fileStorage,
        CancellationToken cancellationToken)
    {
        if (input.IsT1)
        {
            return ReadAllTextAsync(OneOf<FileInfo, string>.FromT0(input.AsT1), fileStorage, cancellationToken);
        }

        return ReadAllTextAsync(OneOf<FileInfo, string>.FromT1(input.AsT0), fileStorage, cancellationToken);
    }

    public static Task<string> ReadAllTextAsync(
        this OneOf<FileInfo, string> input,
        IFileStorage fileStorage,
        CancellationToken cancellationToken)
    {
        return
            input.Match(
                async inputFile =>
                {
                    using Stream fileStream = fileStorage.OpenReadFile(inputFile.FullName);
                    using var reader = new StreamReader(fileStream);
                    string content = await reader.ReadToEndAsync(cancellationToken);
                    return content;
                },
                inputString => Task.FromResult(inputString));
    }
}
