namespace DevToys.Api;

/// <summary>
/// Represents a read-only access to a file on the file system.
/// </summary>
/// <remarks>
/// The file can be read and accessed multiple times in parallel.
/// In some cases, the file's resulting stream is non-seekable.
/// Disposing the <see cref="SandboxedFileReader"/> will close the access to the file.
/// </remarks>
[DebuggerDisplay($"FileName = {{{nameof(FileName)}}}")]
public abstract class SandboxedFileReader : IDisposable
{
    public const int BufferSize = 4096; // 4 KB chunks

    private readonly DisposableSemaphore _semaphore = new();
    private bool _disposed;
    private List<Stream>? _fileAccesses;

    /// <summary>
    /// Initializes a new instance of the <see cref="SandboxedFileReader"/> class.
    /// </summary>
    protected SandboxedFileReader(string fileName)
    {
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

    protected abstract ValueTask<Stream> OpenReadFileAsync(CancellationToken cancellationToken);

    public void Dispose()
    {
        lock (_semaphore)
        {
            _disposed = true;
            _fileAccesses?.ForEach(x => x.Dispose());
            _semaphore.Dispose();
            Disposed?.Invoke(this, EventArgs.Empty);
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// Get a new stream that can be used to read the file. The stream gets disposed automatically
    /// when the <see cref="SandboxedFileReader"/> is disposed.
    /// </summary>
    /// <remarks>In some cases, the returned stream is non-seekable.</remarks>
    /// <exception cref="ObjectDisposedException">The <see cref="SandboxedFileReader"/> has been disposed.</exception>"
    public async Task<Stream> GetNewAccessToFileContentAsync(CancellationToken cancellationToken)
    {
        using (await _semaphore.WaitAsync(cancellationToken))
        {
            EnsureNotDisposed();
            Stream stream = await OpenReadFileAsync(cancellationToken);
            lock (_semaphore)
            {
                EnsureNotDisposed();
                _fileAccesses ??= new();
                _fileAccesses.Add(stream);
                return stream;
            }
        }
    }

    /// <summary>
    /// Copy the content of the file to the given stream.
    /// </summary>
    /// <exception cref="ObjectDisposedException">The <see cref="SandboxedFileReader"/> has been disposed.</exception>"
    public async Task CopyFileContentToAsync(Stream destinationStream, CancellationToken cancellationToken)
    {
        using (await _semaphore.WaitAsync(cancellationToken))
        {
            EnsureNotDisposed();
            using Stream stream = await OpenReadFileAsync(cancellationToken);
            await stream.CopyToAsync(destinationStream, BufferSize, cancellationToken);
        }
    }

    private void EnsureNotDisposed()
    {
        if (_disposed)
        {
            ThrowHelper.ThrowObjectDisposedException($"{nameof(SandboxedFileReader)}[{FileName}]");
        }
    }
}
