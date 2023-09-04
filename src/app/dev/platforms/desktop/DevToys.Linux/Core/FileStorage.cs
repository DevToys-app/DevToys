using DevToys.Api;

namespace DevToys.Linux.Core;

[Export(typeof(IFileStorage))]
internal sealed class FileStorage : IFileStorage
{
    public string AppCacheDirectory => Constants.AppCacheDirectory;

    public bool FileExists(string relativeOrAbsoluteFilePath)
    {
        if (!Path.IsPathRooted(relativeOrAbsoluteFilePath))
        {
            relativeOrAbsoluteFilePath = Path.Combine(AppCacheDirectory, relativeOrAbsoluteFilePath);
        }

        return File.Exists(relativeOrAbsoluteFilePath);
    }

    public Stream OpenReadFile(string relativeOrAbsoluteFilePath)
    {
        if (!Path.IsPathRooted(relativeOrAbsoluteFilePath))
        {
            relativeOrAbsoluteFilePath = Path.Combine(AppCacheDirectory, relativeOrAbsoluteFilePath);
        }

        if (!File.Exists(relativeOrAbsoluteFilePath))
        {
            throw new FileNotFoundException("Unable to find the indicated file.", relativeOrAbsoluteFilePath);
        }

        return File.OpenRead(relativeOrAbsoluteFilePath);
    }

    public Stream OpenWriteFile(string relativeOrAbsoluteFilePath, bool replaceIfExist)
    {
        if (!Path.IsPathRooted(relativeOrAbsoluteFilePath))
        {
            relativeOrAbsoluteFilePath = Path.Combine(AppCacheDirectory, relativeOrAbsoluteFilePath);
        }

        if (File.Exists(relativeOrAbsoluteFilePath) && replaceIfExist)
        {
            File.Delete(relativeOrAbsoluteFilePath);
        }

        string parentDirectory = Path.GetDirectoryName(relativeOrAbsoluteFilePath)!;
        if (!Directory.Exists(parentDirectory))
        {
            Directory.CreateDirectory(parentDirectory);
        }

        return File.OpenWrite(relativeOrAbsoluteFilePath);
    }

    public ValueTask<string?> PickFolderAsync()
    {
        throw new NotImplementedException();
    }

    public ValueTask<PickedFile?> PickOpenFileAsync(params string[] fileTypes)
    {
        throw new NotImplementedException();
    }

    public ValueTask<PickedFile[]> PickOpenFilesAsync(params string[] fileTypes)
    {
        throw new NotImplementedException();
    }

    public ValueTask<Stream?> PickSaveFileAsync(params string[] fileTypes)
    {
        throw new NotImplementedException();
    }
}
