using OneOf;

namespace DevToys.Api;

/// <summary>
/// Extension methods for working with OneOf types.
/// </summary>
public static class OneOfExtensions
{
    /// <summary>
    /// Gets a stream asynchronously based on the input OneOf type.
    /// </summary>
    /// <param name="input">The input OneOf type.</param>
    /// <param name="fileStorage">The file storage implementation.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the stream and a flag indicating if the stream was successfully retrieved.</returns>
    public static Task<ResultInfo<Stream>> GetStreamAsync(
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

    /// <summary>
    /// Gets a stream asynchronously based on the input OneOf type.
    /// </summary>
    /// <param name="input">The input OneOf type.</param>
    /// <param name="fileStorage">The file storage implementation.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the stream and a flag indicating if the stream was successfully retrieved.</returns>
    public static async Task<ResultInfo<Stream>> GetStreamAsync(
        this OneOf<FileInfo, string> input,
        IFileStorage fileStorage,
        CancellationToken cancellationToken)
    {
        return await
            input.Match(
                inputFile =>
                {
                    Guard.IsNotNull(fileStorage);
                    if (!fileStorage.FileExists(inputFile.FullName))
                    {
                        return Task.FromResult(new ResultInfo<Stream>(Stream.Null, false));
                    }

                    return Task.FromResult(new ResultInfo<Stream>(fileStorage.OpenReadFile(inputFile.FullName), true));
                },
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
                    return new ResultInfo<Stream>((Stream)stringStream, true);
                });
    }

    /// <summary>
    /// Reads all text asynchronously based on the input OneOf type.
    /// </summary>
    /// <param name="input">The input OneOf type.</param>
    /// <param name="fileStorage">The file storage implementation.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the text content and a flag indicating if the content was successfully read.</returns>
    public static async Task<ResultInfo<string>> ReadAllTextAsync(
        this OneOf<string, FileInfo> input,
        IFileStorage fileStorage,
        CancellationToken cancellationToken)
    {
        if (input.IsT1)
        {
            return await ReadAllTextAsync(OneOf<FileInfo, string>.FromT0(input.AsT1), fileStorage, cancellationToken);
        }

        return await ReadAllTextAsync(OneOf<FileInfo, string>.FromT1(input.AsT0), fileStorage, cancellationToken);
    }

    /// <summary>
    /// Reads all text asynchronously based on the input OneOf type.
    /// </summary>
    /// <param name="input">The input OneOf type.</param>
    /// <param name="fileStorage">The file storage implementation.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the text content and a flag indicating if the content was successfully read.</returns>
    public static async Task<ResultInfo<string>> ReadAllTextAsync(
        this OneOf<FileInfo, string> input,
        IFileStorage fileStorage,
        CancellationToken cancellationToken)
    {
        return await input.Match(
            async inputFile =>
            {
                Guard.IsNotNull(fileStorage);
                if (!fileStorage.FileExists(inputFile.FullName))
                {
                    return new ResultInfo<string>("", false);
                }

                using Stream fileStream = fileStorage.OpenReadFile(inputFile.FullName);
                using var reader = new StreamReader(fileStream);
                string content = await reader.ReadToEndAsync(cancellationToken);
                return new ResultInfo<string>(content, true);
            },
            inputString => Task.FromResult(new ResultInfo<string>(inputString, true)));
    }
}
