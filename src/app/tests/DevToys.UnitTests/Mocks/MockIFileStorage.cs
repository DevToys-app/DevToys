using System.ComponentModel.Composition;
using System.IO;
using System.Threading.Tasks;

namespace DevToys.UnitTests.Mocks;

[Export(typeof(IFileStorage))]
internal class MockIFileStorage : IFileStorage
{
    public string AppCacheDirectory => throw new NotImplementedException();

    public bool FileExists(string relativeOrAbsoluteFilePath)
    {
        throw new NotImplementedException();
    }

    public Stream OpenReadFile(string relativeOrAbsoluteFilePath)
    {
        throw new NotImplementedException();
    }

    public Stream OpenWriteFile(string relativeOrAbsoluteFilePath, bool replaceIfExist)
    {
        throw new NotImplementedException();
    }

    public ValueTask<Stream> PickSaveFileAsync(params string[] fileTypes)
    {
        // TODO: prompt the user to type in the console a relative or absolute file path that has one of the file types indicated.
        throw new NotImplementedException();
    }

    public ValueTask<PickedFile> PickOpenFileAsync(params string[] fileTypes)
    {
        // TODO: prompt the user to type in the console a relative or absolute file path that has one of the file types indicated.
        throw new NotImplementedException();
    }

    public ValueTask<PickedFile[]> PickOpenFilesAsync(params string[] fileTypes)
    {
        throw new NotImplementedException();
    }

    public ValueTask<string> PickFolderAsync()
    {
        throw new NotImplementedException();
    }
}
