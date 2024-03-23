using System.ComponentModel.Composition;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using DevToys.Core;

namespace DevToys.UnitTests.Mocks;

[Export(typeof(IFileStorage))]
internal class MockIFileStorage : IFileStorage
{
    public bool FileExistsResult { get; set; } = true;

    public string AppCacheDirectory => throw new NotImplementedException();

    public bool FileExists(string relativeOrAbsoluteFilePath)
    {
        return FileExistsResult;
    }

    public FileStream OpenReadFile(string relativeOrAbsoluteFilePath)
    {
        return new FileStream(relativeOrAbsoluteFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan);
    }

    public FileStream OpenWriteFile(string relativeOrAbsoluteFilePath, bool replaceIfExist)
    {
        throw new NotImplementedException();
    }

    public ValueTask<FileStream?> PickSaveFileAsync(params string[] fileTypes)
    {
        // TODO: prompt the user to type in the console a relative or absolute file path that has one of the file types indicated.
        throw new NotImplementedException();
    }

    public ValueTask<SandboxedFileReader?> PickOpenFileAsync(params string[] fileTypes)
    {
        // TODO: prompt the user to type in the console a relative or absolute file path that has one of the file types indicated.
        throw new NotImplementedException();
    }

    public ValueTask<SandboxedFileReader[]> PickOpenFilesAsync(params string[] fileTypes)
    {
        throw new NotImplementedException();
    }

    public ValueTask<string?> PickFolderAsync()
    {
        throw new NotImplementedException();
    }

    public FileInfo CreateSelfDestroyingTempFile(string desiredFileExtension = null)
    {
        var assembly = Assembly.GetExecutingAssembly();
        string? assemblyDirectory = Path.GetDirectoryName(assembly.Location);
        return FileHelper.CreateTempFile(assemblyDirectory!, desiredFileExtension);
    }
}
