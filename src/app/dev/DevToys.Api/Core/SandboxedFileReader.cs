namespace DevToys.Api;

/// <summary>
/// Represents a read-only access to a file on the file system.
/// The file can be read and accessed multiple times in parallel by getting a copy of it.
/// Disposing the <see cref="SandboxedFileReader"/> will close the access to the file.
/// </summary>
[DebuggerDisplay($"FileName = {{{nameof(FileName)}}}")]
public record SandboxedFileReader : IDisposable
{
    private readonly Stream _stream;
    private readonly Stream? _nonSeekableStream;
    private readonly DisposableSemaphore _semaphore = new();
    private bool _isNonSeekableStreamCloned;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="SandboxedFileReader"/> class.
    /// </summary>
    public SandboxedFileReader(string fileName, Stream stream)
    {
        Guard.CanRead(stream);
        if (!stream.CanSeek)
        {
            _nonSeekableStream = stream;
            _stream = new MemoryStream();
        }
        else
        {
            _isNonSeekableStreamCloned = true;
            _stream = stream;
        }
        FileName = Path.GetFileName(fileName);
    }

    /// <summary>
    /// Gets or sets the name of the file, including its extension.
    /// </summary>
    public string FileName { get; init; }

    /// <summary>
    /// Raised when the <see cref="SandboxedFileReader"/> is disposed.
    /// </summary>
    public event EventHandler? Disposed;

    /// <summary>
    /// Gets an access to a copy of the file in memory.
    /// </summary>
    public async Task<MemoryStream> GetFileCopyAsync(CancellationToken cancellationToken)
    {
        if (_disposed)
        {
            ThrowHelper.ThrowObjectDisposedException($"{nameof(SandboxedFileReader)}[{FileName}]");
        }

        using (await _semaphore.WaitAsync(cancellationToken))
        {
            await CloneNonSeekableStreamIfNeededAsync();
            _stream.Seek(0, SeekOrigin.Begin);
            var memoryStream = new MemoryStream();
            await _stream.CopyToAsync(memoryStream, cancellationToken);
            memoryStream.Seek(0, SeekOrigin.Begin);
            return memoryStream;
        }
    }

    /// <summary>
    /// Copy the content of the file to the given stream.
    /// </summary>
    public async Task CopyFileContentToAsync(Stream destinationStream, CancellationToken cancellationToken)
    {
        if (_disposed)
        {
            ThrowHelper.ThrowObjectDisposedException($"{nameof(SandboxedFileReader)}[{FileName}]");
        }

        using (await _semaphore.WaitAsync(cancellationToken))
        {
            await CloneNonSeekableStreamIfNeededAsync();
            _stream.Seek(0, SeekOrigin.Begin);
            await _stream.CopyToAsync(destinationStream, cancellationToken);
        }
    }

    public void Dispose()
    {
        _disposed = true;
        _semaphore.Dispose();
        _stream.Dispose();
        _nonSeekableStream?.Dispose();
        Disposed?.Invoke(this, EventArgs.Empty);
        GC.SuppressFinalize(this);
    }

    private async Task CloneNonSeekableStreamIfNeededAsync()
    {
        if (!_isNonSeekableStreamCloned)
        {
            Guard.IsNotNull(_nonSeekableStream);
            await _nonSeekableStream.CopyToAsync(_stream, CancellationToken.None);
            _isNonSeekableStreamCloned = true;
        }
    }
}
