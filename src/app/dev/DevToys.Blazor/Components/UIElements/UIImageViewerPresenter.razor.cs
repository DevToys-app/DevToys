// TODO: Add logs.
using SixLabors.ImageSharp;

namespace DevToys.Blazor.Components.UIElements;

public partial class UIImageViewerPresenter : MefComponentBase
{
#pragma warning disable IDE0044 // Add readonly modifier
    [Import]
    private IFileStorage _fileStorage = default!;

    [Import]
    private IClipboard _clipboard = default!;
#pragma warning restore IDE0044 // Add readonly modifier

    [Parameter]
    public IUIImageViewer UIImageViewer { get; set; } = default!;

    protected bool IsImageDisplayed { get; set; }

    protected bool IsImageLoading { get; set; }

    protected string? ImageSourceHtml { get; set; }

    protected string? ImageName { get; set; }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        UIImageViewer.ImageSourceChanged += UIImageViewer_ImageSourceChanged;
    }

    public override async ValueTask DisposeAsync()
    {
        UIImageViewer.ImageSourceChanged -= UIImageViewer_ImageSourceChanged;
        await base.DisposeAsync();
    }

    protected override Task OnAfterRenderAsync(bool firstRender)
    {
        base.OnAfterRender(firstRender);
        if (firstRender)
        {
            DisplayImageAsync().Forget();
        }

        return Task.CompletedTask;
    }

    private void UIImageViewer_ImageSourceChanged(object? sender, EventArgs e)
    {
        DisplayImageAsync().Forget();
    }

    private async Task OnViewImageButtonClickAsync()
    {
        if (UIImageViewer.ImageSource.TryGetFirst(out FileInfo? imageFileInfo) && imageFileInfo is not null)
        {
            Shell.OpenFileInShell(imageFileInfo.FullName);
        }
        else if (UIImageViewer.ImageSource.TryGetSecond(out Image? image) && image is not null)
        {
            FileInfo tempFile = _fileStorage.CreateSelfDestroyingTempFile(image.Metadata.DecodedImageFormat!.FileExtensions.FirstOrDefault());
            await image.SaveAsync(tempFile.FullName);
            Shell.OpenFileInShell(tempFile.FullName);
        }
        else if (UIImageViewer.ImageSource.TryGetThird(out SandboxedFileReader? imagePickedFile) && imagePickedFile is not null)
        {
            FileInfo tempFile = _fileStorage.CreateSelfDestroyingTempFile(Path.GetExtension(imagePickedFile.FileName));
            using (FileStream tempFileStream = tempFile.OpenWrite())
            {
                await imagePickedFile.CopyFileContentToAsync(tempFileStream, CancellationToken.None);
            }
            Shell.OpenFileInShell(tempFile.FullName);
        }
    }

    private async Task OnCopyImageButtonClickAsync()
    {
        if (UIImageViewer.ImageSource.TryGetFirst(out FileInfo? imageFileInfo) && imageFileInfo is not null)
        {
            using FileStream fileStream = imageFileInfo.OpenRead();
            using Image image = await Image.LoadAsync(fileStream);
            await _clipboard.SetClipboardImageAsync(image);
        }
        else if (UIImageViewer.ImageSource.TryGetSecond(out Image? image) && image is not null)
        {
            await _clipboard.SetClipboardImageAsync(image);
        }
        else if (UIImageViewer.ImageSource.TryGetThird(out SandboxedFileReader? imagePickedFile) && imagePickedFile is not null)
        {
            if (string.Equals(Path.GetExtension(imagePickedFile.FileName), ".svg", StringComparison.OrdinalIgnoreCase))
            {
                FileInfo tempFile = _fileStorage.CreateSelfDestroyingTempFile("svg");
                using (FileStream tempFileStream = tempFile.OpenWrite())
                {
                    await imagePickedFile.CopyFileContentToAsync(tempFileStream, CancellationToken.None);
                }

                await _clipboard.SetClipboardFilesAsync(new[] { tempFile });
            }
            else
            {
                using MemoryStream memoryStream = await imagePickedFile.GetFileCopyAsync(CancellationToken.None);
                using Image newImage = await Image.LoadAsync(memoryStream);
                await _clipboard.SetClipboardImageAsync(newImage);
            }
        }
    }

    private async Task OnSaveImageAsButtonClickAsync()
    {
        string? fileExtension = null;
        if (UIImageViewer.ImageSource.TryGetFirst(out FileInfo? imageFileInfo) && imageFileInfo is not null)
        {
            fileExtension = imageFileInfo.Extension;
        }
        else if (UIImageViewer.ImageSource.TryGetSecond(out Image? image) && image is not null)
        {
            fileExtension = image.Metadata.DecodedImageFormat!.FileExtensions.FirstOrDefault();
        }
        else if (UIImageViewer.ImageSource.TryGetThird(out SandboxedFileReader? imagePickedFile) && imagePickedFile is not null)
        {
            fileExtension = Path.GetExtension(imagePickedFile.FileName);
        }

        if (!string.IsNullOrWhiteSpace(fileExtension))
        {
            Stream? pickedFile = await _fileStorage.PickSaveFileAsync(fileExtension);
            if (pickedFile is not null)
            {
                if (imageFileInfo is not null)
                {
                    using FileStream fileStream = imageFileInfo.OpenRead();
                    await fileStream.CopyToAsync(pickedFile);
                }
                else if (UIImageViewer.ImageSource.TryGetSecond(out Image? image) && image is not null)
                {
                    await image.SaveAsync(pickedFile, image.Metadata.DecodedImageFormat!);
                }
                else if (UIImageViewer.ImageSource.TryGetThird(out SandboxedFileReader? imagePickedFile) && imagePickedFile is not null)
                {
                    await imagePickedFile.CopyFileContentToAsync(pickedFile, CancellationToken.None);
                }

                await pickedFile.DisposeAsync();
            }
        }
    }

    private async Task DisplayImageAsync()
    {
        await InvokeAsync(() =>
        {
            IsImageDisplayed = false;
            IsImageLoading = true;
            StateHasChanged();
        });

        try
        {
            await TaskSchedulerAwaiter.SwitchOffMainThreadAsync(CancellationToken.None);

            if (UIImageViewer.ImageSource.TryGetFirst(out FileInfo? imageFileInfo) && imageFileInfo is not null)
            {
                using FileStream fileStream = imageFileInfo.OpenRead();
                using Image image = await Image.LoadAsync(fileStream);
                ImageSourceHtml = GetBase64FromImage(image);
                ImageName = imageFileInfo.Name;
                IsImageDisplayed = true;
            }
            else if (UIImageViewer.ImageSource.TryGetSecond(out Image? image) && image is not null)
            {
                ImageSourceHtml = GetBase64FromImage(image);
                ImageName = Localization.Strings.UIImageViewer.UIImageViewer.UnknownImage;
                IsImageDisplayed = true;
            }
            else if (UIImageViewer.ImageSource.TryGetThird(out SandboxedFileReader? imagePickedFile) && imagePickedFile is not null)
            {
                using MemoryStream memoryStream = await imagePickedFile.GetFileCopyAsync(CancellationToken.None);
                if (string.Equals(Path.GetExtension(imagePickedFile.FileName), ".svg", StringComparison.OrdinalIgnoreCase))
                {
                    byte[] bytes = memoryStream.ToArray();
                    string base64 = Convert.ToBase64String(bytes);
                    ImageSourceHtml = "data:image/svg+xml;base64," + base64;
                }
                else
                {
                    using Image newImage = await Image.LoadAsync(memoryStream);
                    ImageSourceHtml = GetBase64FromImage(newImage);
                }

                ImageName = imagePickedFile.FileName;
                IsImageDisplayed = true;
            }
            else
            {
                IsImageDisplayed = false;
            }
        }
        catch (Exception ex)
        {
            IsImageDisplayed = false;
            // TODO: Log this. Also, maybe display a message to the user that the image cannot be displayed?
        }

        await InvokeAsync(() =>
        {
            IsImageLoading = false;
            StateHasChanged();
        });
    }

    private static string GetBase64FromImage(Image image)
    {
        Guard.IsNotNull(image);

        return image.ToBase64String(image.Metadata.DecodedImageFormat!);
    }
}
