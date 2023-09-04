using DevToys.Api;

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

    public async ValueTask<PickedFile?> PickOpenFileAsync(params string[] fileTypes)
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
                = Gtk.Internal.FileDialog.OpenFinish(
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

        Gtk.Internal.FileDialog.Open(
            self: base.Handle,
            parent: MainWindow.Handle,
            cancellable: IntPtr.Zero,
            callback: callbackHandler.NativeCallback,
            userData: IntPtr.Zero
        );

        try
        {
            Gio.File? file = await tcs.Task;
            if (file is not null)
            {
                string? filePath = file.GetPath();
                if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                {
                    return new PickedFile(Path.GetFileName(filePath), new FileStream(filePath, FileMode.Open, FileAccess.Read));
                }
            }
        }
        catch
        {
        }

        return null;
    }

    public async ValueTask<PickedFile[]> PickOpenFilesAsync(params string[] fileTypes)
    {
        Guard.IsNotNull(MainWindow);
        var tcs = new TaskCompletionSource<Gio.ListModel?>();

        var callbackHandler = new Gio.Internal.AsyncReadyCallbackAsyncHandler((sourceObject, res, data) =>
        {
            if (sourceObject is null)
            {
                tcs.SetException(new Exception("Missing source object"));
                return;
            }

            nint fileValue
                = Gtk.Internal.FileDialog.OpenMultipleFinish(
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
                tcs.SetResult(new Gio.ListModelHelper(fileValue, true));
            }
        });

        Gtk.Internal.FileDialog.OpenMultiple(
            self: base.Handle,
            parent: MainWindow.Handle,
            cancellable: IntPtr.Zero,
            callback: callbackHandler.NativeCallback,
            userData: IntPtr.Zero
        );

        try
        {
            Gio.ListModel? files = await tcs.Task;
            if (files is not null)
            {
                var fileResult = new List<PickedFile>();
                uint fileCount = files.GetNItems();
                for (uint i = 0; i < fileCount; i++)
                {
                    nint fileValue = files.GetItem(i);
                    var file = new Gio.FileHelper(fileValue, true);
                    string? filePath = file.GetPath();
                    if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                    {
                        fileResult.Add(new PickedFile(Path.GetFileName(filePath), new FileStream(filePath, FileMode.Open, FileAccess.Read)));
                    }
                }
                return fileResult.ToArray();
            }
        }
        catch
        {
        }

        return Array.Empty<PickedFile>();
    }

    public async ValueTask<Stream?> PickSaveFileAsync(params string[] fileTypes)
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
                = Gtk.Internal.FileDialog.SaveFinish(
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

        Gtk.Internal.FileDialog.Save(
            self: base.Handle,
            parent: MainWindow.Handle,
            cancellable: IntPtr.Zero,
            callback: callbackHandler.NativeCallback,
            userData: IntPtr.Zero
        );

        try
        {
            Gio.File? file = await tcs.Task;
            if (file is not null)
            {
                string? filePath = file.GetPath();
                if (!string.IsNullOrEmpty(filePath))
                {
                    return new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);
                }
            }
        }
        catch
        {
        }

        return null;
    }
}
