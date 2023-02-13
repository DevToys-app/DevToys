using DevToys.Api.Core;

namespace DevToys.CLI.Core.FileStorage;

[Export(typeof(IFileStorage))]
internal sealed class FileStorage : IFileStorage
{
    [ImportingConstructor]
    internal FileStorage()
    {
        AppCacheDirectory = Path.Combine(AppContext.BaseDirectory, "Cache");
    }

    public string AppCacheDirectory { get; }

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

    public ValueTask<string?> PickSaveFileAsync(string[] fileTypes)
    {
        // TODO: prompt the user to type in the console a relative or absolute file path that has one of the file types indicated.
        throw new NotImplementedException();
    }
}
