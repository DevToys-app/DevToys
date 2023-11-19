using DevToys.Tools.Helpers;
using DevToys.Tools.Models;
using DevToys.Tools.Strings.GlobalStrings;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace DevToys.Tools.Tools.Graphic.ImageConverter;

internal sealed class ImageConverterTaskItemGui : IUIListItem, IDisposable
{
    public static readonly string[] SizesStrings
        = {
            GlobalStrings.Bytes,
            GlobalStrings.Kilobytes,
            GlobalStrings.Megabytes,
            GlobalStrings.Gigabytes,
            GlobalStrings.Terabytes
        };

    private enum GridRows
    {
        Top,
        Bottom
    }

    private enum GridColumns
    {
        Progress,
        FileNameAndOriginalSize,
        CompressedSize,
        ActionButtons
    }

    private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
    private readonly IFileStorage _fileStorage;
    private readonly Lazy<IUIElement> _ui;
    private readonly IUISetting _setting = Setting();
    private readonly ImageConverterGuiTool _imageConverterGuiTool;

    internal ImageConverterTaskItemGui(ImageConverterGuiTool imageConverterGuiTool, SandboxedFileReader inputFile, IFileStorage fileStorage)
    {
        Guard.IsNotNull(imageConverterGuiTool);
        Guard.IsNotNull(inputFile);
        Guard.IsNotNull(fileStorage);

        _imageConverterGuiTool = imageConverterGuiTool;
        _fileStorage = fileStorage;
        Value = inputFile;
        InputFile = inputFile;

        ComputePropertiesAsync().ForgetSafely();

        _ui
            = new Lazy<IUIElement>(
                _setting
                    .Icon("FluentSystemIcons", '\uF488')
                    .Title(inputFile.FileName)

                    .InteractiveElement(
                        Stack()
                            .Horizontal()
                            .MediumSpacing()

                            .WithChildren(
                                Stack()
                                    .Horizontal()
                                    .SmallSpacing()

                                    .WithChildren(
                                        Button()
                                            .Icon("FluentSystemIcons", '\uF67F')
                                            .OnClick(OnSaveAsAsync),
                                        Button()
                                            .Icon("FluentSystemIcons", '\uF34C')
                                            .OnClick(OnDeleteAsync)))));
    }

    public IUIElement UIElement => _ui.Value;

    public object? Value { get; }

    internal SandboxedFileReader InputFile { get; }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
        InputFile.Dispose();
    }

    internal async Task SaveAsync(string filePath, ImageConverterSupportedFormat format)
    {
        using Image<Rgba32> originalImage = await ImageHelper.LoadImageFromFileAsync(InputFile, _cancellationTokenSource.Token);

        using FileStream outputFileStream = File.Create(filePath);
        await ImageHelper.SaveAsync(outputFileStream, originalImage, format, _cancellationTokenSource.Token);
    }

    private async ValueTask OnSaveAsAsync()
    {
        // Load the image first, in case if the user will overwrite it (so we can read it before PickSaveFileAsync opens an access to the file).
        using Image<Rgba32> originalImage = await ImageHelper.LoadImageFromFileAsync(InputFile, _cancellationTokenSource.Token);

        // Ask the user to pick up a file.
        Stream? outputFileStream = await _fileStorage.PickSaveFileAsync(_imageConverterGuiTool.UserSelectedImageFormat.ToString());
        if (outputFileStream is not null)
        {
            await ImageHelper.SaveAsync(outputFileStream, originalImage, _imageConverterGuiTool.UserSelectedImageFormat, _cancellationTokenSource.Token);
        }
    }

    private ValueTask OnDeleteAsync()
    {
        _imageConverterGuiTool.ItemList.Items.Remove(this);
        return ValueTask.CompletedTask;
    }

    private async Task ComputePropertiesAsync()
    {
        await TaskSchedulerAwaiter.SwitchOffMainThreadAsync(_cancellationTokenSource.Token);

        using Stream fileStream = await InputFile.GetNewAccessToFileContentAsync(_cancellationTokenSource.Token);

        long storageFileSize = fileStream.Length;
        string? fileSize = HumanizeFileSize(storageFileSize, ImageConverter.FileSizeDisplay);

        _setting.Description(fileSize);
    }

    private static string HumanizeFileSize(double fileSize, string fileSizeDisplay)
    {
        int order = 0;
        while (fileSize >= 1024 && order < SizesStrings.Length - 1)
        {
            order++;
            fileSize /= 1024;
        }

        string fileSizeString = string.Format(fileSizeDisplay, fileSize, SizesStrings[order]);
        return fileSizeString;
    }
}
