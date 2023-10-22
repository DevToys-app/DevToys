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
internal sealed class SimpleSandboxedFileReader : SandboxedFileReader
{
    private readonly FileInfo _fileInfo;

    internal SimpleSandboxedFileReader(FileInfo fileInfo)
        : base(fileInfo.Name)
    {
        if (!fileInfo.Exists)
        {
            throw new FileNotFoundException("Unable to find the indicated file.", fileInfo.FullName);
        }

        _fileInfo = fileInfo;
    }

    protected override ValueTask<Stream> OpenReadFileAsync(CancellationToken cancellationToken)
    {
        if (!_fileInfo.Exists)
        {
            throw new FileNotFoundException("Unable to find the indicated file.", _fileInfo.FullName);
        }

        return ValueTask.FromResult(
            (Stream)new FileStream(
                _fileInfo.FullName,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                SandboxedFileReader.BufferSize,
                FileOptions.Asynchronous | FileOptions.SequentialScan));
    }
}
