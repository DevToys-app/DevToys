// TODO: Add logs.
using DevToys.Core;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;

namespace DevToys.Blazor.Components.UIElements;

public partial class UIImageViewerPresenter : MefComponentBase
{
    private readonly DisposableSemaphore _semaphore = new();
    private CancellationTokenSource? _cancellationTokenSource;

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
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        UIImageViewer.ImageSourceChanged -= UIImageViewer_ImageSourceChanged;
        _semaphore.Dispose();
        await base.DisposeAsync();
    }

    protected override Task OnAfterRenderAsync(bool firstRender)
    {
        base.OnAfterRender(firstRender);
        if (firstRender)
        {
            DisplayImage();
        }

        return Task.CompletedTask;
    }

    private void UIImageViewer_ImageSourceChanged(object? sender, EventArgs e)
    {
        DisplayImage();
    }

    private Task OnViewImageButtonClickAsync()
    {
        Guard.IsNotNull(UIImageViewer.ImageSource);

        return UIImageViewer.ImageSource.Value.Match(
            (FileInfo imageFileInfo) =>
            {
                OSHelper.OpenFileInShell(imageFileInfo.FullName);
                return Task.CompletedTask;
            },
            async (Image image) =>
            {
                string? fileExtension = image.Metadata.DecodedImageFormat?.FileExtensions.FirstOrDefault() ?? "png";
                FileInfo tempFile = _fileStorage.CreateSelfDestroyingTempFile(fileExtension);
                await image.SaveAsync(tempFile.FullName);
                OSHelper.OpenFileInShell(tempFile.FullName);
            },
            async (SandboxedFileReader imagePickedFile) =>
            {
                FileInfo tempFile = _fileStorage.CreateSelfDestroyingTempFile(Path.GetExtension(imagePickedFile.FileName));
                using (FileStream tempFileStream = tempFile.OpenWrite())
                {
                    await imagePickedFile.CopyFileContentToAsync(tempFileStream, CancellationToken.None);
                }
                OSHelper.OpenFileInShell(tempFile.FullName);
            });
    }

    private Task OnCopyImageButtonClickAsync()
    {
        Guard.IsNotNull(UIImageViewer.ImageSource);

        return UIImageViewer.ImageSource.Value.Match(
            async (FileInfo imageFileInfo) =>
            {
                using FileStream fileStream = imageFileInfo.OpenRead();
                using Image image = await Image.LoadAsync(fileStream);
                await _clipboard.SetClipboardImageAsync(image);
            },
            (Image image) => _clipboard.SetClipboardImageAsync(image),
            async (SandboxedFileReader imagePickedFile) =>
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
                    using Stream stream = await imagePickedFile.GetNewAccessToFileContentAsync(CancellationToken.None);
                    using Image newImage = await Image.LoadAsync(stream);
                    await _clipboard.SetClipboardImageAsync(newImage);
                }
            });
    }

    private async Task OnSaveImageAsButtonClickAsync()
    {
        Guard.IsNotNull(UIImageViewer.ImageSource);

        string fileExtension
            = UIImageViewer.ImageSource.Value.Match(
                (FileInfo imageFileInfo) => imageFileInfo.Extension,
                (Image image) => image.Metadata.DecodedImageFormat?.FileExtensions.FirstOrDefault() ?? "png",
                (SandboxedFileReader imagePickedFile) => Path.GetExtension(imagePickedFile.FileName));

        if (!string.IsNullOrWhiteSpace(fileExtension))
        {
            var fileExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                fileExtension
            };
            foreach (string customFileExtension in UIImageViewer.CustomActionPerFileExtensionOnSaving.Keys)
            {
                fileExtensions.Add(customFileExtension);
            }

            await using FileStream? pickedFile = await _fileStorage.PickSaveFileAsync(fileExtensions.ToArray());
            if (pickedFile is not null)
            {
                string userSelectedFileExtension = Path.GetExtension(pickedFile.Name).ToLowerInvariant();
                if (UIImageViewer.CustomActionPerFileExtensionOnSaving.TryGetValue(userSelectedFileExtension, out Func<FileStream, ValueTask>? customAction))
                {
                    // The user has selected a file extension for which we have a custom action defined by the developer.
                    await customAction(pickedFile);
                    return;
                }

                await UIImageViewer.ImageSource.Value.Match(
                    async (FileInfo imageFileInfo) =>
                    {
                        using FileStream fileStream = imageFileInfo.OpenRead();
                        await fileStream.CopyToAsync(pickedFile);
                    },
                    (Image image) => image.SaveAsync(pickedFile, image.Metadata.DecodedImageFormat ?? PngFormat.Instance),
                    (SandboxedFileReader imagePickedFile) => imagePickedFile.CopyFileContentToAsync(pickedFile, CancellationToken.None));
            }
        }
    }

    private void DisplayImage()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();
        DisplayImageAsync(_cancellationTokenSource.Token).Forget();
    }

    private async Task DisplayImageAsync(CancellationToken cancellationToken)
    {
        await InvokeAsync(() =>
        {
            IsImageDisplayed = false;
            IsImageLoading = true;
            StateHasChanged();
        });

        try
        {
            using (await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false))
            {
                await TaskSchedulerAwaiter.SwitchOffMainThreadAsync(cancellationToken);

                if (UIImageViewer.ImageSource.HasValue)
                {
                    await UIImageViewer.ImageSource.Value.Match(
                    async (FileInfo imageFileInfo) =>
                    {
                        using FileStream fileStream = imageFileInfo.OpenRead();
                        using Image image = await Image.LoadAsync(fileStream, cancellationToken);
                        ImageSourceHtml = GetBase64FromImage(image);
                        ImageName = imageFileInfo.Name;
                        IsImageDisplayed = true;
                    },
                    (Image image) =>
                    {
                        ImageSourceHtml = GetBase64FromImage(image);
                        ImageName = Localization.Strings.UIImageViewer.UIImageViewer.UnknownImage;
                        IsImageDisplayed = true;
                        return Task.CompletedTask;
                    },
                    async (SandboxedFileReader imagePickedFile) =>
                    {
                        string fileExtension = Path.GetExtension(imagePickedFile.FileName);
                        using Stream stream = await imagePickedFile.GetNewAccessToFileContentAsync(cancellationToken);
                        if (string.Equals(fileExtension, ".bmp", StringComparison.OrdinalIgnoreCase)
                            || string.Equals(fileExtension, ".gif", StringComparison.OrdinalIgnoreCase)
                            || string.Equals(fileExtension, ".ico", StringComparison.OrdinalIgnoreCase)
                            || string.Equals(fileExtension, ".jpg", StringComparison.OrdinalIgnoreCase)
                            || string.Equals(fileExtension, ".jpeg", StringComparison.OrdinalIgnoreCase)
                            || string.Equals(fileExtension, ".png", StringComparison.OrdinalIgnoreCase)
                            || string.Equals(fileExtension, ".svg", StringComparison.OrdinalIgnoreCase)
                            || string.Equals(fileExtension, ".webp", StringComparison.OrdinalIgnoreCase))
                        {
                            var bytes = new Memory<byte>(new byte[stream.Length]);
                            await stream.ReadAsync(bytes, cancellationToken);
                            string base64 = Convert.ToBase64String(bytes.Span);
                            ImageSourceHtml
                                = fileExtension.ToLowerInvariant() switch
                                {
                                    ".bmp" => "data:image/bmp;base64," + base64,
                                    ".gif" => "data:image/gif;base64," + base64,
                                    ".ico" => "data:image/x-icon;base64," + base64,
                                    ".jpg" or ".jpeg" => "data:image/jpeg;base64," + base64,
                                    ".png" => "data:image/png;base64," + base64,
                                    ".svg" => "data:image/svg+xml;base64," + base64,
                                    ".webp" => "data:image/webp;base64," + base64,
                                    _ => throw new NotSupportedException(),
                                };
                        }
                        else
                        {
                            using Image newImage = await Image.LoadAsync(stream, cancellationToken);
                            ImageSourceHtml = GetBase64FromImage(newImage);
                        }

                        ImageName = imagePickedFile.FileName;
                        IsImageDisplayed = true;
                    });
                }
                else
                {
                    IsImageDisplayed = false;
                }
            }
        }
        catch (OperationCanceledException)
        {
            IsImageDisplayed = false;
            // Swallow
        }
        catch (Exception)
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

        return image.ToBase64String(image.Metadata.DecodedImageFormat ?? PngFormat.Instance);
    }
}
