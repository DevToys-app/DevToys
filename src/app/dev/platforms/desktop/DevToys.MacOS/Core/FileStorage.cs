using CommunityToolkit.Maui.Storage;
using DevToys.Api;
using DevToys.MacOS.Helpers;

namespace DevToys.MacOS.Core;

[Export(typeof(IFileStorage))]
internal sealed class FileStorage : IFileStorage
{
    private const string TempFolderName = "Temp";

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

    public async ValueTask<Stream?> PickSaveFileAsync(string[] fileTypes)
    {
        return await ThreadHelper.RunOnUIThreadAsync(async () =>
        {
            string? fileExtension = fileTypes.FirstOrDefault(fileType => fileType.Replace(".", string.Empty).Replace("*", string.Empty).Length > 0);
            if (!string.IsNullOrWhiteSpace(fileExtension))
            {
                fileExtension = fileExtension.Trim('*').Trim('.').ToLower();
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

    public async ValueTask<SandboxedFileReader?> PickOpenFileAsync(string[] fileTypes)
    {
        return await ThreadHelper.RunOnUIThreadAsync(async () =>
        {
            fileTypes = fileTypes.Select(fileType => fileType.Trim('*').Trim('.').ToLower()).ToArray();

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
                return new SandboxedFileReader(fileResult.FileName, await fileResult.OpenReadAsync());
            }

            return null;
        });
    }

    public async ValueTask<SandboxedFileReader[]> PickOpenFilesAsync(string[] fileTypes)
    {
        return await ThreadHelper.RunOnUIThreadAsync(async () =>
        {
            fileTypes = fileTypes.Select(fileType => fileType.Trim('*').Trim('.').ToLower()).ToArray();

            var otpions = new PickOptions()
            {
                FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.macOS, fileTypes },
                    { DevicePlatform.MacCatalyst, fileTypes }
                })
            };

            IEnumerable<FileResult>? fileResults = await FilePicker.Default.PickMultipleAsync(otpions);
            if (fileResults is not null)
            {
                var result = new List<SandboxedFileReader>();
                foreach (FileResult file in fileResults)
                {
                    result.Add(new SandboxedFileReader(file.FileName, await file.OpenReadAsync()));
                }

                return result.ToArray();
            }

            return Array.Empty<SandboxedFileReader>();
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

    public FileInfo CreateSelfDestroyingTempFile(string? desiredFileExtension = null)
    {
        return FileHelper.CreateTempFile(Constants.AppTempFolder, desiredFileExtension);
    }
}
