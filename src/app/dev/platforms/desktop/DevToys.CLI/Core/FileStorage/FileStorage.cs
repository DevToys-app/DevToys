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

    public ValueTask<FileStream?> PickSaveFileAsync(params string[] fileTypes)
    {
        do
        {
            Console.WriteLine(CliStrings.PromptSaveFile);
            string? filePath = Console.ReadLine();

            if (filePath is null || string.IsNullOrWhiteSpace(filePath))
            {
                return new ValueTask<FileStream?>(Task.FromResult<FileStream?>(null));
            }

            // Remove quotes in case the user copy-pasted the file path from the file explorer.
            filePath = TrimFilePath(filePath);

            if (IsFileOfType(filePath, fileTypes))
            {
                return new ValueTask<FileStream?>(File.OpenWrite(filePath));
            }

            Console.Error.WriteLine(CliStrings.InvalidFileType, string.Join(", ", fileTypes));
        } while (true);
    }

    public ValueTask<SandboxedFileReader?> PickOpenFileAsync(params string[] fileTypes)
    {
        do
        {
            Console.WriteLine(CliStrings.PromptOpenFile);
            string? filePath = Console.ReadLine();

            if (filePath is null || string.IsNullOrWhiteSpace(filePath))
            {
                return new ValueTask<SandboxedFileReader?>(Task.FromResult<SandboxedFileReader?>(null));
            }

            // Remove quotes in case the user copy-pasted the file path from the file explorer.
            filePath = TrimFilePath(filePath);

            var fileInfo = new FileInfo(filePath);
            if (fileInfo.Exists)
            {
                if (IsFileOfType(filePath, fileTypes))
                {
                    return new ValueTask<SandboxedFileReader?>(SandboxedFileReader.FromFileInfo(fileInfo));
                }
                else
                {
                    Console.Error.WriteLine(CliStrings.InvalidFileType, string.Join(", ", fileTypes));
                }
            }
            else
            {
                Console.Error.WriteLine(CliStrings.FileNotFound, fileInfo.FullName);
            }
        } while (true);
    }

    public ValueTask<SandboxedFileReader[]> PickOpenFilesAsync(params string[] fileTypes)
    {
        do
        {
            Console.WriteLine(CliStrings.PromptOpenFiles);
            string? filePaths = Console.ReadLine();

            if (filePaths is null || string.IsNullOrWhiteSpace(filePaths))
            {
                return new ValueTask<SandboxedFileReader[]>(Task.FromResult(Array.Empty<SandboxedFileReader>()));
            }

            string[] paths = filePaths.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var filesInfo = new FileInfo[paths.Length];
            bool succeeded = true;

            for (int i = 0; i < paths.Length; i++)
            {
                string filePath = paths[i];

                // Remove quotes in case the user copy-pasted the file path from the file explorer.
                filePath = TrimFilePath(filePath);

                var fileInfo = new FileInfo(filePath);
                if (fileInfo.Exists)
                {
                    if (IsFileOfType(filePath, fileTypes))
                    {
                        filesInfo[i] = fileInfo;
                    }
                    else
                    {
                        succeeded = false;
                        Console.Error.WriteLine(CliStrings.InvalidFileType, string.Join(", ", fileTypes));
                        break;
                    }
                }
                else
                {
                    succeeded = false;
                    Console.Error.WriteLine(CliStrings.FileNotFound, fileInfo.FullName);
                    break;
                }
            }

            if (succeeded)
            {
                var readers = new SandboxedFileReader[filesInfo.Length];
                for (int i = 0; i < filesInfo.Length; i++)
                {
                    readers[i] = SandboxedFileReader.FromFileInfo(filesInfo[i]);
                }

                return new ValueTask<SandboxedFileReader[]>(Task.FromResult(readers));
            }
        } while (true);
    }

    public ValueTask<string?> PickFolderAsync()
    {
        Console.WriteLine(CliStrings.PromptOpenFolder);
        string? folderPath = Console.ReadLine();

        if (folderPath is null || string.IsNullOrWhiteSpace(folderPath))
        {
            return new ValueTask<string?>(Task.FromResult<string?>(null));
        }

        // Remove quotes in case the user copy-pasted the file path from the file explorer.
        folderPath = TrimFilePath(folderPath);

        DirectoryInfo directoryInfo = Directory.CreateDirectory(folderPath);

        return new ValueTask<string?>(Task.FromResult<string?>(directoryInfo.FullName));
    }

    public FileInfo CreateSelfDestroyingTempFile(string? desiredFileExtension = null)
    {
        return FileHelper.CreateTempFile(Constants.AppTempFolder, desiredFileExtension);
    }

    private static bool IsFileOfType(string filePath, string[] fileTypes)
    {
        if (AreAnyFileTypesValid(fileTypes))
        {
            return true;
        }

        string fileExtension = Path.GetExtension(filePath);
        for (int i = 0; i < fileTypes.Length; i++)
        {
            string fileType = fileTypes[i];
            string editedFileType = "." + fileType.Trim('*').Trim('.').ToLower();
            if (string.Equals(fileExtension, editedFileType, StringComparison.CurrentCultureIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static bool AreAnyFileTypesValid(string[] fileTypes)
    {
        if (fileTypes is null || fileTypes.Length == 0)
        {
            return true;
        }

        return Array.Exists(fileTypes, fileType => string.Equals(fileType, "*.*", StringComparison.CurrentCultureIgnoreCase));
    }

    private static string TrimFilePath(string filePath)
    {
        // Remove quotes in case the user copy-pasted the file path from the file explorer.
        // We do these trim in a loop in case the user input is something like `  "" C:\file.txt ""  `.

        int fileLength;

        do
        {
            fileLength = filePath.Length;
            filePath = filePath.Trim(' ').Trim('\"').Trim('\'');
        } while (filePath.Length != fileLength);

        return filePath;
    }
}
