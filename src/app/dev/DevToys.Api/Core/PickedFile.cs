namespace DevToys.Api;

[DebuggerDisplay($"FileName = {{{nameof(FileName)}}}")]
public record PickedFile : IDisposable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PickedFile"/> class.
    /// </summary>
    public PickedFile(string fileName, Stream stream)
    {
        FileName = Path.GetFileName(fileName);
        Stream = stream;
    }

    /// <summary>
    /// Gets or sets the name of the file, including its extension.
    /// </summary>
    public string FileName { get; init; }

    /// <summary>
    /// Gets or sets a stream allowing to read the content of the file.
    /// </summary>
    /// <remarks>
    /// The stream is disposed when the <see cref="PickedFile"/> is disposed.
    /// </remarks>
    public Stream Stream { get; init; }

    /// <summary>
    /// Raised when the <see cref="PickedFile"/> is disposed.
    /// </summary>
    public event EventHandler? Disposed;

    public void Dispose()
    {
        Stream?.Dispose();
        Disposed?.Invoke(this, EventArgs.Empty);
        GC.SuppressFinalize(this);
    }
}
