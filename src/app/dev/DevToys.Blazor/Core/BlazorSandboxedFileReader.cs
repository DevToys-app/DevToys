using Microsoft.AspNetCore.Components.Forms;

namespace DevToys.Blazor.Core;

internal sealed class BlazorSandboxedFileReader : SandboxedFileReader
{
    private readonly AnyType<IBrowserFile, FileInfo> _file;
    private readonly IFileStorage? _fileStorage;

    public BlazorSandboxedFileReader(IBrowserFile browserFile)
        : base(browserFile.Name)
    {
        _file = new(browserFile);
        _fileStorage = null;
    }

    public BlazorSandboxedFileReader(FileInfo file, IFileStorage fileStorage)
        : base(file.Name)
    {
        Guard.IsNotNull(fileStorage);
        _file = file;
        _fileStorage = fileStorage;
    }

    protected override ValueTask<Stream> OpenReadFileAsync(CancellationToken cancellationToken)
    {
        if (_file.Value is IBrowserFile browserFile)
        {
            return ValueTask.FromResult(
                browserFile.OpenReadStream(browserFile.Size, cancellationToken));
        }
        else if (_file.Value is FileInfo fileInfo)
        {
            Guard.IsNotNull(_fileStorage);
            return ValueTask.FromResult<Stream>(_fileStorage.OpenReadFile(fileInfo.FullName));
        }

        throw new NotSupportedException();
    }
}
