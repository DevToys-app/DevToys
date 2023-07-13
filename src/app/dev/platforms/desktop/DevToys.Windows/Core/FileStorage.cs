using System.IO;
using System.Text;
using DevToys.Api;
using DevToys.Windows.Helpers;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace DevToys.Windows.Core;

[Export(typeof(IFileStorage))]
internal sealed class FileStorage : IFileStorage
{
    public string AppCacheDirectory => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DevToys");

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

                return saveFileDialog.OpenFile();
            }

            return null;
        });
    }

    public async ValueTask<PickedFile?> PickOpenFileAsync(string[] fileTypes)
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
                return new PickedFile(openFileDialog.FileName, openFileDialog.OpenFile());
            }

            return null;
        });
    }

    public async ValueTask<PickedFile[]> PickOpenFilesAsync(string[] fileTypes)
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
                Stream[] streams = openFileDialog.OpenFiles();
                string[] fileNames = openFileDialog.FileNames;
                Guard.IsEqualTo(streams.Length, fileNames.Length);

                var result = new PickedFile[streams.Length];
                for (int i = 0; i < streams.Length; i++)
                {
                    result[i] = new(fileNames[i], streams[i]);
                }

                return result;
            }

            return Array.Empty<PickedFile>();
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

            // TODO: Localize.
            string allFiles = "All " + string.Join(", ", allFileTypesDescription);
            filters.Insert(0, $"{allFiles}|{string.Join(";", allFileTypes)}|");
        }

        return filters.ToString();
    }
}
