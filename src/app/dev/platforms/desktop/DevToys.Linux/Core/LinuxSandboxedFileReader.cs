using System.IO;
using DevToys.Api;

namespace DevToys.Linux.Core;

internal sealed class LinuxSandboxedFileReader : SandboxedFileReader
{
    private readonly string _fullPath;
    private readonly IFileStorage _fileStorage;

    public LinuxSandboxedFileReader(string fullPath, IFileStorage fileStorage)
        : base(fullPath)
    {
        Guard.IsNotNull(fileStorage);
        Guard.IsNotNullOrWhiteSpace(fullPath);
        _fileStorage = fileStorage;
        _fullPath = fullPath;
    }

    protected override ValueTask<Stream> OpenReadFileAsync(CancellationToken cancellationToken)
    {
        return ValueTask.FromResult<Stream>(_fileStorage.OpenReadFile(_fullPath));
    }
}
