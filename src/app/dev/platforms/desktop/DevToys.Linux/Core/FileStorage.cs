﻿using DevToys.Api;
using DevToys.Core;
using DevToys.Linux.Strings.Other;
using Gtk;

namespace DevToys.Linux.Core;

[Export(typeof(IFileStorage))]
internal sealed class FileStorage : GObject.Object, IFileStorage
{
    [ImportingConstructor]
    public FileStorage()
        : base(
            Gtk.Internal.FileDialog.New(),
            ownedRef: true)
    {
    }

    public string AppCacheDirectory => Constants.AppCacheDirectory;

    internal Gtk.Window? MainWindow { private get; set; }

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

    public async ValueTask<string?> PickFolderAsync()
    {
        Guard.IsNotNull(MainWindow);
        var tcs = new TaskCompletionSource<Gio.File?>();

        var callbackHandler = new Gio.Internal.AsyncReadyCallbackAsyncHandler((sourceObject, res, data) =>
        {
            if (sourceObject is null)
            {
                tcs.SetException(new Exception("Missing source object"));
                return;
            }

            nint fileValue
                = Gtk.Internal.FileDialog.SelectFolderFinish(
                    sourceObject.Handle,
                    res.Handle,
                    out GLib.Internal.ErrorOwnedHandle? error);

            if (!error.IsInvalid)
            {
                tcs.SetException(new GLib.GException(error));
            }
            else if (fileValue == IntPtr.Zero)
            {
                tcs.SetResult(null);
            }
            else
            {
                tcs.SetResult(new Gio.FileHelper(fileValue, true));
            }
        });

        Gtk.Internal.FileDialog.SelectFolder(
            self: base.Handle,
            parent: MainWindow.Handle,
            cancellable: IntPtr.Zero,
            callback: callbackHandler.NativeCallback,
            userData: IntPtr.Zero
        );

        try
        {
            Gio.File? folder = await tcs.Task;
            if (folder is not null)
            {
                string? folderPath = folder.GetPath();
                return folderPath;
            }
        }
        catch
        {
        }

        return null;
    }

    public async ValueTask<SandboxedFileReader?> PickOpenFileAsync(params string[] fileTypes)
    {
        SandboxedFileReader[]? selectedFiles = await PickOpenFileInternalAsync(fileTypes, allowMultiple: false).ConfigureAwait(false);

        return selectedFiles is null || selectedFiles.Length == 0 ? null : selectedFiles[0];
    }

    public ValueTask<SandboxedFileReader[]> PickOpenFilesAsync(params string[] fileTypes)
    {
        return PickOpenFileInternalAsync(fileTypes, allowMultiple: true);
    }

    public async ValueTask<Stream?> PickSaveFileAsync(params string[] fileTypes)
    {
        Guard.IsNotNull(MainWindow);

        // Create a new GtkFileChooserNative object and set its properties
        using var fileChooser
            = Gtk.FileChooserNative.New(
                Other.SaveFile,
                MainWindow,
                Gtk.FileChooserAction.Save,
                Other.Save,
                Other.Cancel);
        fileChooser.Modal = true;
        fileChooser.SelectMultiple = false;

        // Create filters
        IReadOnlyList<FileFilter> filters = GenerateFilter(fileTypes);
        for (int i = 0; i < filters.Count; i++)
        {
            fileChooser.AddFilter(filters[i]);
        }

        var taskCompletionSource = new TaskCompletionSource<Stream?>();
        fileChooser.OnResponse += (_, e) =>
        {
            // Handle the result of the window.
            if (e.ResponseId != (int)Gtk.ResponseType.Accept)
            {
                taskCompletionSource.SetResult(null);
                return;
            }

            Gio.ListModel? fileListModel = fileChooser.GetFiles();

            var fileResult = new List<SandboxedFileReader>();
            if (fileListModel is not null)
            {
                uint fileCount = fileListModel.GetNItems();
                if (fileCount == 1)
                {
                    nint fileValue = fileListModel.GetItem(0);
                    var file = new Gio.FileHelper(fileValue, true);
                    string? filePath = file.GetPath();

                    if (!string.IsNullOrEmpty(filePath))
                    {
                        taskCompletionSource.SetResult(
                            new FileStream(
                                filePath,
                                FileMode.OpenOrCreate,
                                FileAccess.Write));
                        return;
                    }
                }
            }

            taskCompletionSource.SetResult(null);
        };

        // Show the dialog.
        fileChooser.Show();

        return await taskCompletionSource.Task;
    }

    public FileInfo CreateSelfDestroyingTempFile(string? desiredFileExtension = null)
    {
        return FileHelper.CreateTempFile(Constants.AppTempFolder, desiredFileExtension);
    }

    private async ValueTask<SandboxedFileReader[]> PickOpenFileInternalAsync(string[] fileTypes, bool allowMultiple)
    {
        Guard.IsNotNull(MainWindow);

        // Create a new GtkFileChooserNative object and set its properties
        using var fileChooser
            = Gtk.FileChooserNative.New(
                allowMultiple ? Other.OpenFiles : Other.OpenFile,
                MainWindow,
                Gtk.FileChooserAction.Open,
                Other.Open,
                Other.Cancel);
        fileChooser.Modal = true;
        fileChooser.SelectMultiple = allowMultiple;

        // Create filters
        IReadOnlyList<FileFilter> filters = GenerateFilter(fileTypes);
        for (int i = 0; i < filters.Count; i++)
        {
            fileChooser.AddFilter(filters[i]);
        }

        var taskCompletionSource = new TaskCompletionSource<SandboxedFileReader[]>();
        fileChooser.OnResponse += (_, e) =>
        {
            // Handle the result of the window.
            if (e.ResponseId != (int)Gtk.ResponseType.Accept)
            {
                taskCompletionSource.SetResult(Array.Empty<SandboxedFileReader>());
                return;
            }

            Gio.ListModel? fileListModel = fileChooser.GetFiles();

            var fileResult = new List<SandboxedFileReader>();
            if (fileListModel is not null)
            {
                uint fileCount = fileListModel.GetNItems();

                for (uint i = 0; i < fileCount; i++)
                {
                    nint fileValue = fileListModel.GetItem(i);
                    var file = new Gio.FileHelper(fileValue, true);
                    string? filePath = file.GetPath();

                    if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                    {
                        fileResult.Add(
                            new SandboxedFileReader(
                                Path.GetFileName(filePath),
                                new FileStream(
                                    filePath,
                                    FileMode.Open,
                                    FileAccess.Read)));
                    }
                }
            }
            taskCompletionSource.SetResult(fileResult.ToArray());
        };

        // Show the dialog.
        fileChooser.Show();

        SandboxedFileReader[]? files = await taskCompletionSource.Task;
        if (files is not null && files.Length > 0)
        {
            if (allowMultiple)
            {
                return files;
            }

            return new[] { files[0] };
        }

        return Array.Empty<SandboxedFileReader>();
    }

    private static IReadOnlyList<FileFilter> GenerateFilter(string[] fileTypes)
    {
        var filters = new List<FileFilter>();
        var allFileTypesDescription = new List<string>();
        var allFileTypes = new List<string>();

        foreach (string fileType in fileTypes.Order())
        {
            var filter = Gtk.FileFilter.New();
            if (string.Equals(fileType, "*.*", StringComparison.CurrentCultureIgnoreCase))
            {
                allFileTypesDescription.Add("*.*");
                allFileTypes.Add("*.*");

                filter.AddPattern("*");
                filter.Name = Other.AllFiles2;
            }
            else
            {
                string lowercaseFileType = "*." + fileType.Trim('*').Trim('.').ToLower();
                string fileTypeDescription = fileType.Trim('*').Trim('.').ToUpper();

                allFileTypesDescription.Add(fileTypeDescription);
                allFileTypes.Add(lowercaseFileType);

                filter.AddPattern(lowercaseFileType);
                filter.Name = fileTypeDescription;
            }

            filters.Add(filter);
        }

        if (filters.Count > 0)
        {
            var filter = Gtk.FileFilter.New();
            filter.Name = string.Format(Other.AllFiles, string.Join(", ", allFileTypesDescription));
            for (int i = 0; i < allFileTypes.Count; i++)
            {
                filter.AddPattern(allFileTypes[i]);
            }
            filters.Insert(0, filter);
        }

        return filters;
    }
}
