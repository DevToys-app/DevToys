using DevToys.Api;
using DevToys.CLI.Strings.CliStrings;
using DevToys.Core;

namespace DevToys.CLI.Core.FileStorage;

[Export(typeof(IFileStorage))]
internal sealed class FileStorage : IFileStorage
{
    [ImportingConstructor]
    internal FileStorage()
    {
        AppCacheDirectory = Constants.AppCacheDirectory;
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

        return new FileStream(relativeOrAbsoluteFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan);
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

    public ValueTask<Stream?> PickSaveFileAsync(params string[] fileTypes)
    {
        Console.WriteLine(CliStrings.PromptSaveFile);
        string? filePath = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return new ValueTask<Stream?>(Task.FromResult<Stream?>(null));
        }
        return new ValueTask<Stream?>(File.OpenWrite(filePath!));
    }

    public ValueTask<SandboxedFileReader?> PickOpenFileAsync(params string[] fileTypes)
    {
        // TODO: prompt the user to type in the console a relative or absolute file path that has one of the file types indicated.
        throw new NotImplementedException();
    }

    public ValueTask<SandboxedFileReader[]> PickOpenFilesAsync(params string[] fileTypes)
    {
        // TODO: prompt the user to type in the console a relative or absolute file path that has one of the file types indicated.
        throw new NotImplementedException();
    }

    public ValueTask<string?> PickFolderAsync()
    {
        // TODO: prompt the user to type in the console a relative or absolute file path that has one of the file types indicated.
        throw new NotImplementedException();
    }

    public FileInfo CreateSelfDestroyingTempFile(string? desiredFileExtension = null)
    {
        return FileHelper.CreateTempFile(Constants.AppTempFolder, desiredFileExtension);
    }
}
