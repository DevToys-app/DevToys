using CommunityToolkit.Maui.Storage;
using DevToys.Api;
using DevToys.MacOS.Helpers;

namespace DevToys.MacOS.Core;

[Export(typeof(IFileStorage))]
internal sealed class FileStorage : IFileStorage
{
    public string AppCacheDirectory => Path.Combine(FileSystem.CacheDirectory, "com.etiennebaudoux.devtoys");

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

    public async ValueTask<Stream?> PickSaveFileAsync(string[] fileTypes)
    {
        return await ThreadHelper.RunOnUIThreadAsync(async () =>
        {
            string? fileExtension = fileTypes.FirstOrDefault(fileType => fileType.Replace(".", string.Empty).Replace("*", string.Empty).Length > 0);
            if (!string.IsNullOrWhiteSpace(fileExtension))
            {
                fileExtension = fileExtension.Trim('*').Trim('.');
            }
            else
            {
                fileExtension = "txt";
            }

            FileSaverResult fileSaverResult;
            using (var stream = new MemoryStream())
            {
                fileSaverResult = await FileSaver.Default.SaveAsync(string.Empty, "document." + fileExtension, stream, CancellationToken.None);
            }

            if (fileSaverResult is not null && fileSaverResult.IsSuccessful)
            {
                return new FileStream(fileSaverResult.FilePath, FileMode.OpenOrCreate, FileAccess.Write);
            }

            return null;
        });
    }

    public async ValueTask<Stream?> PickOpenFileAsync(string[] fileTypes)
    {
        return await ThreadHelper.RunOnUIThreadAsync(async () =>
        {
            fileTypes = fileTypes.Select(fileType => fileType.Trim('*').Trim('.')).ToArray();

            var otpions = new PickOptions()
            {
                FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.macOS, fileTypes },
                    { DevicePlatform.MacCatalyst, fileTypes }
                })
            };

            FileResult? fileResult = await FilePicker.Default.PickAsync(otpions);
            if (fileResult is not null)
            {
                return await fileResult.OpenReadAsync();
            }

            return null;
        });
    }

    public async ValueTask<string?> PickFolderAsync()
    {
        return await ThreadHelper.RunOnUIThreadAsync(async () =>
        {
            FolderPickerResult folderPickerResult = await FolderPicker.Default.PickAsync(CancellationToken.None);
            if (folderPickerResult is not null && folderPickerResult.IsSuccessful)
            {
                return folderPickerResult.Folder.Path;
            }

            return null;
        });
    }
}
