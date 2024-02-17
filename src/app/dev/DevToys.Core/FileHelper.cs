using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace DevToys.Core;

public static class FileHelper
{
    private static readonly ILogger logger = typeof(FileHelper).Log();

    private static readonly ConcurrentBag<FileInfo> tempFiles = new();

    public static FileInfo CreateTempFile(string baseFolder, string? desiredFileExtension)
    {
        string tempFilePath;

        do
        {
            tempFilePath = Path.Combine(baseFolder, Path.GetFileName(Path.GetTempFileName()));
            if (!string.IsNullOrWhiteSpace(desiredFileExtension))
            {
                tempFilePath = Path.ChangeExtension(tempFilePath, desiredFileExtension);
            }
        } while (File.Exists(tempFilePath));

        Directory.CreateDirectory(baseFolder);
        File.Create(tempFilePath).Dispose();
        var fileInfo = new FileInfo(tempFilePath);
        tempFiles.Add(fileInfo);
        return fileInfo;
    }

    public static void ClearTempFiles(string baseFolder)
    {
        DateTime startTime = DateTime.UtcNow;

        FileInfo[] files = tempFiles.ToArray();
        foreach (FileInfo tempFile in files)
        {
            try
            {
                if (File.Exists(tempFile.FullName))
                {
                    File.Delete(tempFile.FullName);
                }
            }
            catch
            {
                // Swallow.
            }
        }

        if (Directory.Exists(baseFolder))
        {
            foreach (string tempFilePath in Directory.GetFiles(baseFolder, string.Empty, SearchOption.AllDirectories))
            {
                // We found a file that isn't tracked by the current instance of the app.
                // If the file is older than 2 minutes, let's destroy it.

                var tempFileInfo = new FileInfo(tempFilePath);
                if (DateTime.Now - tempFileInfo.CreationTime > TimeSpan.FromMinutes(2))
                {
                    try
                    {
                        File.Delete(tempFilePath);
                    }
                    catch
                    {
                        // Swallow.
                    }
                }
            }
        }

        double elapsedMilliseconds = (DateTime.UtcNow - startTime).TotalMilliseconds;
        logger.LogInformation("Cleared temp files in {elapsedMilliseconds}ms", elapsedMilliseconds);
    }
}
