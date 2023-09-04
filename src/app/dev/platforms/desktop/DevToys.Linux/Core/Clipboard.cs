using DevToys.Api;

namespace DevToys.Linux.Core;

[Export(typeof(IClipboard))]
internal sealed partial class Clipboard : IClipboard
{
    public Task<string?> GetClipboardBitmapAsync()
    {
        throw new NotImplementedException();
    }

    public Task<object?> GetClipboardDataAsync()
    {
        throw new NotImplementedException();
    }

    public Task<FileInfo[]?> GetClipboardFilesAsync()
    {
        throw new NotImplementedException();
    }

    public Task<string?> GetClipboardTextAsync()
    {
        throw new NotImplementedException();
    }

    public Task SetClipboardBitmapAsync(string? data)
    {
        throw new NotImplementedException();
    }

    public Task SetClipboardFilesAsync(FileInfo[]? filePaths)
    {
        throw new NotImplementedException();
    }

    public Task SetClipboardTextAsync(string? data)
    {
        throw new NotImplementedException();
    }
}
