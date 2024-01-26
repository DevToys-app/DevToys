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
    /// <summary>
    /// The buffer size used for reading the file in chunks.
    /// </summary>
    public const int BufferSize = 4096; // 4 KB chunks

    private readonly DisposableSemaphore _semaphore = new();
    private bool _disposed;
    private List<Stream>? _fileAccesses;

    /// <summary>
    /// Creates a new instance of the <see cref="SandboxedFileReader"/> class from a <see cref="FileInfo"/> object.
    /// </summary>
    /// <param name="fileInfo">The <see cref="FileInfo"/> object representing the file.</param>
    /// <returns>A new instance of the <see cref="SandboxedFileReader"/> class.</returns>
    public static SandboxedFileReader FromFileInfo(FileInfo fileInfo)
    {
        Guard.IsNotNull(fileInfo);
        return new SimpleSandboxedFileReader(fileInfo);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SandboxedFileReader"/> class.
    /// </summary>
    /// <param name="fileName">The name of the file, including its extension.</param>
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

    /// <summary>
    /// Opens the file for reading asynchronously.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected abstract ValueTask<Stream> OpenReadFileAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Disposes the <see cref="SandboxedFileReader"/> and releases any resources used.
    /// </summary>
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
    /// Gets a new stream that can be used to read the file. The stream gets disposed automatically
    /// when the <see cref="SandboxedFileReader"/> is disposed.
    /// </summary>
    /// <remarks>In some cases, the returned stream is non-seekable.</remarks>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <exception cref="ObjectDisposedException">The <see cref="SandboxedFileReader"/> has been disposed.</exception>"
    /// <returns>A task representing the asynchronous operation.</returns>
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
    /// Copies the content of the file to the given stream.
    /// </summary>
    /// <param name="destinationStream">The destination stream.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
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
