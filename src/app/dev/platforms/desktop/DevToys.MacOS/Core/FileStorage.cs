using System.Runtime.Versioning;
using DevToys.Api;
using DevToys.Core;
using DevToys.MacOS.Core.Helpers;
using UniformTypeIdentifiers;

namespace DevToys.MacOS.Core;

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

    public FileStream OpenReadFile(string relativeOrAbsoluteFilePath)
    {
        if (!Path.IsPathRooted(relativeOrAbsoluteFilePath))
        {
            relativeOrAbsoluteFilePath = Path.Combine(AppCacheDirectory, relativeOrAbsoluteFilePath);
        }

        if (!File.Exists(relativeOrAbsoluteFilePath))
        {
            throw new FileNotFoundException("Unable to find the indicated file.", relativeOrAbsoluteFilePath);
        }

        return new FileStream(relativeOrAbsoluteFilePath, FileMode.Open, FileAccess.Read, FileShare.Read,
            SandboxedFileReader.BufferSize, FileOptions.Asynchronous | FileOptions.SequentialScan);
    }

    public FileStream OpenWriteFile(string relativeOrAbsoluteFilePath, bool replaceIfExist)
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

    public async ValueTask<FileStream?> PickSaveFileAsync(string[] fileTypes)
    {
        return await ThreadHelper.RunOnUIThreadAsync(() =>
        {
            UTType[]? utTypes;
            if (fileTypes.Contains("*.*") || fileTypes.Contains(string.Empty) || fileTypes.Contains(null) ||
                fileTypes.Contains(".*"))
            {
                utTypes = null;
            }
            else
            {
                utTypes = fileTypes
                    .Select(fileType => UTType.CreateFromExtension(fileType.Trim('*').Trim('.').ToLower()))
                    .Where(utType => utType != null)
                    .ToArray()!;
            }

            var savePanel = new NSSavePanel();
            if (utTypes is not null)
            {
                savePanel.AllowedContentTypes = utTypes;
            }

            bool result = savePanel.RunModal() == 1; // 1 indicates OK button was pressed

            if (result)
            {
                string? filePath = savePanel.Url?.Path;
                if (!string.IsNullOrEmpty(filePath))
                {
                    return new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);
                }
            }

            return null;
        });
    }

    public async ValueTask<SandboxedFileReader?> PickOpenFileAsync(string[] fileTypes)
    {
        return await ThreadHelper.RunOnUIThreadAsync(() =>
        {
            UTType[]? utTypes;
            if (fileTypes.Contains("*.*") || fileTypes.Contains(string.Empty) || fileTypes.Contains(null) ||
                fileTypes.Contains(".*"))
            {
                utTypes = null;
            }
            else
            {
                utTypes = fileTypes
                    .Select(fileType => UTType.CreateFromExtension(fileType.Trim('*').Trim('.').ToLower()))
                    .Where(utType => utType != null)
                    .ToArray()!;
            }

            var openPanel = new NSOpenPanel();
            openPanel.CanChooseFiles = true;
            openPanel.CanChooseDirectories = false;
            if (utTypes is not null)
            {
                openPanel.AllowedContentTypes = utTypes;
            }

            bool result = openPanel.RunModal() == 1; // 1 indicates OK button was pressed

            if (result)
            {
                NSUrl selectedUrl = openPanel.Url;
                if (selectedUrl.Path is not null)
                {
                    var fileInfo = new FileInfo(selectedUrl.Path);
                    return SandboxedFileReader.FromFileInfo(fileInfo);
                }
            }

            return null;
        });
    }

    public async ValueTask<SandboxedFileReader[]> PickOpenFilesAsync(string[] fileTypes)
    {
        return await ThreadHelper.RunOnUIThreadAsync(() =>
        {
            UTType[]? utTypes;
            if (fileTypes.Contains("*.*") || fileTypes.Contains(string.Empty) || fileTypes.Contains(null) ||
                fileTypes.Contains(".*"))
            {
                utTypes = null;
            }
            else
            {
                utTypes = fileTypes
                    .Select(fileType => UTType.CreateFromExtension(fileType.Trim('*').Trim('.').ToLower()))
                    .Where(utType => utType != null)
                    .ToArray()!;
            }

            var openPanel = new NSOpenPanel();
            openPanel.CanChooseFiles = true;
            openPanel.CanChooseDirectories = false;
            openPanel.AllowsMultipleSelection = true;
            if (utTypes is not null)
            {
                openPanel.AllowedContentTypes = utTypes;
            }

            bool result = openPanel.RunModal() == 1; // 1 indicates OK button was pressed

            if (result)
            {
                NSUrl[] selectedUrls = openPanel.Urls;
                if (selectedUrls.Any())
                {
                    var fileList = new List<SandboxedFileReader>();
                    foreach (NSUrl url in selectedUrls)
                    {
                        if (url.Path is not null)
                        {
                            var fileInfo = new FileInfo(url.Path);
                            fileList.Add(SandboxedFileReader.FromFileInfo(fileInfo));
                        }
                    }

                    return fileList.ToArray();
                }
            }

            return Array.Empty<SandboxedFileReader>();
        });
    }

    public async ValueTask<string?> PickFolderAsync()
    {
        return await ThreadHelper.RunOnUIThreadAsync(() =>
        {
            var openPanel = new NSOpenPanel
            {
                CanChooseFiles = false,
                CanChooseDirectories = true,
                AllowsMultipleSelection = false
            };

            bool result = openPanel.RunModal() == 1; // 1 indicates OK button was pressed

            if (result)
            {
                NSUrl selectedUrl = openPanel.Url;
                if (selectedUrl.Path != null)
                {
                    return selectedUrl.Path;
                }
            }

            return null;
        });
    }

    public FileInfo CreateSelfDestroyingTempFile(string? desiredFileExtension = null)
    {
        return FileHelper.CreateTempFile(Constants.AppTempFolder, desiredFileExtension);
    }
}
