namespace DevToys.Api;

/// <summary>
/// Provides a platform agnostic way to interact with the file system of the operating system the app is running on.
/// </summary>
public interface IFileStorage
{
    /// <summary>
    /// Gets application's directory to store cache data.
    /// Cache data can be used for any data that needs to persist longer than temporary data, but shouldn't be data that is
    /// required to operate the app, as the operating system may clear this storage.
    /// </summary>
    string AppCacheDirectory { get; }

    /// <summary>
    /// Prompt the user to select a location to save a file, and decide of the file name.
    /// </summary>
    /// <param name="fileTypes">The list of file types the user can choose. For example, ".txt". Use "*" for any file type.</param>
    /// <returns>If succeeded, returns a write-only stream corresponding to the file the user selected, otherwise, returns null.</returns>
    ValueTask<FileStream?> PickSaveFileAsync(params string[] fileTypes);

    /// <summary>
    /// Prompt the user to select a file to open.
    /// </summary>
    /// <remarks>The returned items contain a stream. It won't be disposed automatically. It is important to dispose the stream yourself, when not needed anymore</remarks>
    /// <param name="fileTypes">The list of file types the user can choose. For example, ".txt". Use "*" for any file type.</param>
    /// <returns>If succeeded, returns a read-only stream corresponding to the file the user selected, otherwise, returns null.</returns>
    ValueTask<SandboxedFileReader?> PickOpenFileAsync(params string[] fileTypes);

    /// <summary>
    /// Prompt the user to select many files to open.
    /// </summary>
    /// <remarks>The returned items contain a stream. It won't be disposed automatically. It is important to dispose the stream yourself, when not needed anymore</remarks>
    /// <param name="fileTypes">The list of file types the user can choose. For example, ".txt". Use "*" for any file type.</param>
    /// <returns>If succeeded, returns a read-only stream corresponding to the file the user selected, otherwise, returns null.</returns>
    ValueTask<SandboxedFileReader[]> PickOpenFilesAsync(params string[] fileTypes);

    /// <summary>
    /// Prompt the user to select a folder.
    /// </summary>
    /// <returns>If succeeded, returns the absolute path to the folder the user selected, otherwise, returns null.</returns>
    ValueTask<string?> PickFolderAsync();

    /// <summary>
    /// Determines whether the file indicated by the given <paramref name="relativeOrAbsoluteFilePath"/> exists.
    /// If a relative path is indicated, use the <see cref="AppCacheDirectory"/> as working directory.
    /// </summary>
    /// <param name="relativeOrAbsoluteFilePath">The path to the file to check.</param>
    /// <returns>Returns true if the file exist</returns>
    bool FileExists(string relativeOrAbsoluteFilePath);

    /// <summary>
    /// Tries to open the given <paramref name="relativeOrAbsoluteFilePath"/> with read access rights.
    /// If a relative path is indicated, use the <see cref="AppCacheDirectory"/> as working directory.
    /// </summary>
    /// <param name="relativeOrAbsoluteFilePath">The path to the file to read.</param>
    /// <returns>Returns a read-only stream if the file exist and can be read, otherwise, raise an exception.</returns>
    FileStream OpenReadFile(string relativeOrAbsoluteFilePath);

    /// <summary>
    /// Tries to open the given <paramref name="relativeOrAbsoluteFilePath"/> with write access rights. The file will be created if it doesn't exist.
    /// If a relative path is indicated, use the <see cref="AppCacheDirectory"/> as working directory.
    /// </summary>
    /// <param name="relativeOrAbsoluteFilePath">The path to the file to write.</param>
    /// <param name="replaceIfExist">If true and that the file indicated by <paramref name="relativeOrAbsoluteFilePath"/> already exist, overwrite it. Otherwise, open it without replacing it.</param>
    /// <returns>Returns a write-only stream.</returns>
    FileStream OpenWriteFile(string relativeOrAbsoluteFilePath, bool replaceIfExist);

    /// <summary>
    /// Creates a new temporary file in <see cref="AppCacheDirectory"/> that will be deleted when the app stops, or the next time is starts.
    /// </summary>
    /// <param name="desiredFileExtension">(optional) The extension the temporary file should use.</param>
    /// <returns>Returns information to the file.</returns>
    FileInfo CreateSelfDestroyingTempFile(string? desiredFileExtension = null);
}
