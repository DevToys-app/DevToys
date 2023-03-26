using System.IO;
using DevToys.Api.Core;
using DevToys.UI.Framework.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Dispatching;
using Uno.Extensions;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Streams;

namespace DevToys.Wasdk.Core.Clipboard;

[Export(typeof(IClipboard))]
internal sealed partial class Clipboard : IClipboard
{
    private readonly ILogger _logger;
    private readonly Lazy<DispatcherQueue> _dispatcherQueue
        = new(() =>
        {
            UI.Views.MainWindow? mainWindow = ((App)App.Current).MainWindow;
            Guard.IsNotNull(mainWindow);
            return mainWindow.DispatcherQueue;
        });

    [ImportingConstructor]
    public Clipboard()
    {
        _logger = this.Log();
    }

    public Task<object?> GetClipboardDataAsync()
    {
        return _dispatcherQueue.Value.RunOnUIThreadAsync<object?>(async () =>
        {
            try
            {
                DataPackageView package = Windows.ApplicationModel.DataTransfer.Clipboard.GetContent();
                if (package.Contains(StandardDataFormats.Text))
                {
                    return await package.GetTextAsync();
                }
                else if (package.Contains(StandardDataFormats.StorageItems))
                {
                    IReadOnlyList<IStorageItem> storageitems = await package.GetStorageItemsAsync();
                    var files = new List<FileInfo>();
                    foreach (IStorageItem storageItem in storageitems)
                    {
                        files.Add(new FileInfo(storageItem.Path));
                    }
                    return files;
                }
#if !__WASM__
                else if (package.Contains(StandardDataFormats.Bitmap))
                {
                    RandomAccessStreamReference imageStreamRef = await package.GetBitmapAsync();
                    using IRandomAccessStreamWithContentType streamWithContentType = await imageStreamRef.OpenReadAsync();
                    using Stream stream = streamWithContentType.AsStream();
                    return await Image.LoadAsync(stream);
                }
#endif
            }
            catch (Exception ex)
            {
                LogGetClipboardFailed(ex);
            }

            return null;
        });
    }

    public Task SetClipboardBitmapAsync(string? data)
    {
        // TODO
        throw new NotImplementedException();
    }

    public Task SetClipboardFilesAsync(string? data)
    {
        // TODO
        throw new NotImplementedException();
    }

    public Task SetClipboardTextAsync(string? data)
    {
        // TODO
        throw new NotImplementedException();
    }

    [LoggerMessage(1, LogLevel.Warning, "Failed to retrieve the clipboard data.")]
    partial void LogGetClipboardFailed(Exception ex);
}
