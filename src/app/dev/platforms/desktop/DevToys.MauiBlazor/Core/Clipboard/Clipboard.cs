using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Streams;

namespace DevToys.MauiBlazor.Core.Clipboard;

[Export(typeof(Api.Core.IClipboard))]
internal sealed partial class Clipboard : Api.Core.IClipboard
{
    public async Task<object?> GetClipboardDataAsync()
    {
        if (Microsoft.Maui.ApplicationModel.DataTransfer.Clipboard.Default.HasText)
        {
            return await Microsoft.Maui.ApplicationModel.DataTransfer.Clipboard.Default.GetTextAsync();
        }
#if __WINDOWS__
        else
        {
            Windows.ApplicationModel.DataTransfer.DataPackageView package = Windows.ApplicationModel.DataTransfer.Clipboard.GetContent();
            if (package.Contains(StandardDataFormats.StorageItems))
            {
                IReadOnlyList<IStorageItem> storageitems = await package.GetStorageItemsAsync();
                var files = new List<FileInfo>();
                foreach (IStorageItem storageItem in storageitems)
                {
                    files.Add(new FileInfo(storageItem.Path));
                }
                return files;
            }
            else if (package.Contains(StandardDataFormats.Bitmap))
            {
                RandomAccessStreamReference imageStreamRef = await package.GetBitmapAsync();
                using IRandomAccessStreamWithContentType streamWithContentType = await imageStreamRef.OpenReadAsync();
                using Stream stream = streamWithContentType.AsStream();
                return await SixLabors.ImageSharp.Image.LoadAsync(stream);
            }
        }
#elif __MACCATALYST__
#endif
        // TODO: On Mac, use AppKit API to get file and bitmap from clipboard.

        return null;
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
}
