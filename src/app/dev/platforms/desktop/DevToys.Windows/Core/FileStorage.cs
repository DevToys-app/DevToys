using System.IO;
using System.Text;
using DevToys.Api;
using DevToys.Core;
using DevToys.Windows.Helpers;
using DevToys.Windows.Strings.Other;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace DevToys.Windows.Core;

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

        return new FileStream(relativeOrAbsoluteFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, SandboxedFileReader.BufferSize, FileOptions.Asynchronous | FileOptions.SequentialScan);
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

    public async ValueTask<FileStream?> PickSaveFileAsync(params string[] fileTypes)
    {
        return await ThreadHelper.RunOnUIThreadAsync(() =>
        {
            var saveFileDialog = new SaveFileDialog
            {
                OverwritePrompt = true
            };

            if (fileTypes is not null)
            {
                saveFileDialog.Filter = GenerateFilter(fileTypes);
            }

            if (saveFileDialog.ShowDialog() == true)
            {
                if (File.Exists(saveFileDialog.FileName))
                {
                    // Clear the file.
                    using FileStream fileStream = File.Open(saveFileDialog.FileName, FileMode.Open);
                    fileStream.SetLength(0);
                    fileStream.Close();
                }

                return saveFileDialog.OpenFile() as FileStream;
            }

            return null;
        });
    }

    public async ValueTask<SandboxedFileReader?> PickOpenFileAsync(params string[] fileTypes)
    {
        return await ThreadHelper.RunOnUIThreadAsync(() =>
        {
            var openFileDialog = new OpenFileDialog
            {
                CheckFileExists = true,
                CheckPathExists = true,
                Multiselect = false,
                ShowReadOnly = true
            };

            if (fileTypes is not null)
            {
                openFileDialog.Filter = GenerateFilter(fileTypes);
            }

            if (openFileDialog.ShowDialog() == true)
            {
                return SandboxedFileReader.FromFileInfo(new FileInfo(openFileDialog.FileName));
            }

            return null;
        });
    }

    public async ValueTask<SandboxedFileReader[]> PickOpenFilesAsync(params string[] fileTypes)
    {
        return await ThreadHelper.RunOnUIThreadAsync(() =>
        {
            var openFileDialog = new OpenFileDialog
            {
                CheckFileExists = true,
                CheckPathExists = true,
                Multiselect = true,
                ShowReadOnly = true
            };

            if (fileTypes is not null)
            {
                openFileDialog.Filter = GenerateFilter(fileTypes);
            }

            if (openFileDialog.ShowDialog() == true)
            {
                string[] fileNames = openFileDialog.FileNames;

                var result = new SandboxedFileReader[fileNames.Length];
                for (int i = 0; i < fileNames.Length; i++)
                {
                    result[i] = SandboxedFileReader.FromFileInfo(new FileInfo(fileNames[i]));
                }

                return result;
            }

            return Array.Empty<SandboxedFileReader>();
        });
    }

    public async ValueTask<string?> PickFolderAsync()
    {
        return await ThreadHelper.RunOnUIThreadAsync(() =>
        {
            var openFolderDialog = new FolderBrowserDialog();

            if (openFolderDialog.ShowDialog() == DialogResult.OK)
            {
                return openFolderDialog.SelectedPath;
            }

            return null;
        });
    }

    public FileInfo CreateSelfDestroyingTempFile(string? desiredFileExtension = null)
    {
        return FileHelper.CreateTempFile(Constants.AppTempFolder, desiredFileExtension);
    }

    private static string GenerateFilter(string[] fileTypes)
    {
        var filters = new StringBuilder();
        var allFileTypesDescription = new List<string>();
        var allFileTypes = new List<string>();

        foreach (string fileType in fileTypes.Order())
        {
            if (string.Equals(fileType, "*.*", StringComparison.CurrentCultureIgnoreCase))
            {
                filters.Append($"*.*|*.*|");
                allFileTypesDescription.Add("*.*");
                allFileTypes.Add("*.*");
            }
            else
            {
                string lowercaseFileType = "*." + fileType.Trim('*').Trim('.').ToLower();
                string fileTypeDescription = fileType.Trim('*').Trim('.').ToUpper();
                filters.Append($"{fileTypeDescription}|{lowercaseFileType}|");
                allFileTypesDescription.Add(fileTypeDescription);
                allFileTypes.Add(lowercaseFileType);
            }
        }

        if (filters.Length > 0)
        {
            filters.Remove(filters.Length - 1, 1);

            string allFiles = string.Format(Other.AllFiles, string.Join(", ", allFileTypesDescription));
            filters.Insert(0, $"{allFiles}|{string.Join(";", allFileTypes)}|");
        }

        return filters.ToString();
    }
}
