using DevToys.Tools.SmartDetection;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;

namespace DevToys.Tools.Tools.EncodersDecoders.Base64Image;

[Export(typeof(IGuiTool))]
[Name("Base64ImageEncoderDecoder")]
[ToolDisplayInformation(
    IconFontName = "DevToys-Tools-Icons",
    IconGlyph = '\u0102',
    GroupName = PredefinedCommonToolGroupNames.EncodersDecoders,
    ResourceManagerAssemblyIdentifier = nameof(DevToysToolsResourceManagerAssemblyIdentifier),
    ResourceManagerBaseName = "DevToys.Tools.Tools.EncodersDecoders.Base64Image.Base64ImageEncoderDecoder",
    ShortDisplayTitleResourceName = nameof(Base64ImageEncoderDecoder.ShortDisplayTitle),
    LongDisplayTitleResourceName = nameof(Base64ImageEncoderDecoder.LongDisplayTitle),
    DescriptionResourceName = nameof(Base64ImageEncoderDecoder.Description),
    AccessibleNameResourceName = nameof(Base64ImageEncoderDecoder.AccessibleName),
    SearchKeywordsResourceName = nameof(Base64ImageEncoderDecoder.SearchKeywords))]
[AcceptedDataTypeName(PredefinedCommonDataTypeNames.Base64Image)]
[AcceptedDataTypeName(PredefinedCommonDataTypeNames.Image)]
[AcceptedDataTypeName("Base64ImageFile")]
internal sealed partial class Base64ImageEncoderDecoderGuiTool : IGuiTool, IDisposable
{
    private enum GridRows
    {
        FileInput,
        ImagePreviewer
    }

    private enum GridColumns
    {
        Stretch
    }

    private readonly DisposableSemaphore _semaphore = new();
    private readonly IFileStorage _fileStorage;
    private readonly ILogger _logger;
    private readonly IUIMultiLineTextInput _inputText = MultilineTextInput("base64-image-input-box");
    private readonly IUIFileSelector _fileSelector = FileSelector("base64-image-file-selector");
    private readonly IUIImageViewer _imageViewer = ImageViewer("base64-image-preview");

    private SandboxedFileReader? _selectedFile;
    private CancellationTokenSource? _cancellationTokenSource;
    private bool _ignoreBase64DataChange;

    [ImportingConstructor]
    public Base64ImageEncoderDecoderGuiTool(IFileStorage fileStorage)
    {
        _fileStorage = fileStorage;
        _logger = this.Log();
    }

    // For unit tests.
    internal Task? WorkTask { get; private set; }

    public UIToolView View
        => new(
            isScrollable: true,
            SplitGrid()
                .Vertical()
                .LeftPaneLength(new UIGridLength(4, UIGridUnitType.Fraction))
                .RightPaneLength(new UIGridLength(2, UIGridUnitType.Fraction))

                .WithLeftPaneChild(
                    _inputText
                        .Title(Base64ImageEncoderDecoder.Base64Input)
                        .AlwaysWrap()
                        .CanCopyWhenEditable()
                        .OnTextChanged(OnInputTextChanged))

                .WithRightPaneChild(
                    Grid()
                        .RowLargeSpacing()

                        .Rows(
                            (GridRows.FileInput, Auto),
                            (GridRows.ImagePreviewer, new UIGridLength(1, UIGridUnitType.Fraction)))

                        .Columns(
                            (GridColumns.Stretch, new UIGridLength(1, UIGridUnitType.Fraction)))

                        .Cells(
                            Cell(
                                GridRows.FileInput,
                                GridColumns.Stretch,

                                _fileSelector
                                    .CanSelectOneFile()
                                    .LimitFileTypesTo(Base64ImageFileDataTypeDetector.SupportedFileTypes)
                                    .OnFilesSelected(OnFileSelectedAsync)),

                            Cell(
                                GridRows.ImagePreviewer,
                                GridColumns.Stretch,

                                _imageViewer
                                    .Title(Base64ImageEncoderDecoder.Base64Image)))));

    public async void OnDataReceived(string dataTypeName, object? parsedData)
    {
        if (dataTypeName == PredefinedCommonDataTypeNames.Base64Image && parsedData is string base64Image)
        {
            _inputText.Text(base64Image); // This will trigger a conversion.
        }
        else if (dataTypeName == PredefinedCommonDataTypeNames.Image && parsedData is Image<SixLabors.ImageSharp.PixelFormats.Rgba32> image)
        {
            FileInfo temporaryFile = _fileStorage.CreateSelfDestroyingTempFile("png");

            using (image)
            {
                using Stream fileStream = _fileStorage.OpenWriteFile(temporaryFile.FullName, replaceIfExist: true);
                await image.SaveAsPngAsync(fileStream);
            }

            _fileSelector.WithFiles(SandboxedFileReader.FromFileInfo(temporaryFile)); // This will trigger a conversion.
        }
        else if (dataTypeName == "Base64ImageFile" && parsedData is FileInfo file)
        {
            _fileSelector.WithFiles(SandboxedFileReader.FromFileInfo(file)); // This will trigger a conversion.
        }
    }

    public void Dispose()
    {
        _selectedFile?.Dispose();
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _semaphore.Dispose();
    }

    private void OnInputTextChanged(string text)
    {
        if (!_ignoreBase64DataChange)
        {
            StartConvert(text);
        }
    }

    private ValueTask OnFileSelectedAsync(SandboxedFileReader[] files)
    {
        Guard.HasSizeEqualTo(files, 1);

        _selectedFile?.Dispose();
        _selectedFile = files[0];
        _imageViewer.WithPickedFile(_selectedFile, false);

        StartConvert(_selectedFile);
        return ValueTask.CompletedTask;
    }

    private void StartConvert(SandboxedFileReader file)
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();

        WorkTask = ConvertImageToBase64Async(file, _cancellationTokenSource.Token);
    }

    private void StartConvert(string text)
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();

        WorkTask = ConvertBase64ToImageAsync(text, _cancellationTokenSource.Token);
    }

    private async Task ConvertBase64ToImageAsync(string input, CancellationToken cancellationToken)
    {
        await TaskSchedulerAwaiter.SwitchOffMainThreadAsync(cancellationToken);

        try
        {
            using (await _semaphore.WaitAsync(cancellationToken))
            {
                if (string.IsNullOrWhiteSpace(input))
                {
                    _imageViewer.Clear();
                    return;
                }

                string? trimmedData = input?.Trim();

                string fileExtension;
                if (trimmedData!.StartsWith("data:image/bmp;base64,", StringComparison.OrdinalIgnoreCase))
                {
                    fileExtension = "bmp";
                }
                else if (trimmedData!.StartsWith("data:image/gif;base64,", StringComparison.OrdinalIgnoreCase))
                {
                    fileExtension = "gif";
                }
                else if (trimmedData!.StartsWith("data:image/x-icon;base64,", StringComparison.OrdinalIgnoreCase))
                {
                    fileExtension = "ico";
                }
                else if (trimmedData!.StartsWith("data:image/jpeg;base64,", StringComparison.OrdinalIgnoreCase))
                {
                    fileExtension = "jpeg";
                }
                else if (trimmedData!.StartsWith("data:image/png;base64,", StringComparison.OrdinalIgnoreCase))
                {
                    fileExtension = "png";
                }
                else if (trimmedData!.StartsWith("data:image/svg+xml;base64,", StringComparison.OrdinalIgnoreCase))
                {
                    fileExtension = "svg";
                }
                else if (trimmedData!.StartsWith("data:image/webp;base64,", StringComparison.OrdinalIgnoreCase))
                {
                    fileExtension = "webp";
                }
                else
                {
                    return;
                }

                input = trimmedData.Substring(trimmedData.IndexOf(',') + 1);
                byte[] bytes = Convert.FromBase64String(input);

                FileInfo tempFile = _fileStorage.CreateSelfDestroyingTempFile(fileExtension);
                using (FileStream tempFileStream = tempFile.OpenWrite())
                {
                    await tempFileStream.WriteAsync(bytes, cancellationToken);
                }

                _imageViewer.WithFile(tempFile);
            }
        }
        catch
        {
            _imageViewer.Clear();
        }
    }

    private async Task ConvertImageToBase64Async(SandboxedFileReader file, CancellationToken cancellationToken)
    {
        await TaskSchedulerAwaiter.SwitchOffMainThreadAsync(cancellationToken);

        using (await _semaphore.WaitAsync(cancellationToken))
        {
            using Stream stream = await file.GetNewAccessToFileContentAsync(cancellationToken);

            var bytes = new Memory<byte>(new byte[stream.Length]);
            await stream.ReadAsync(bytes, CancellationToken.None);
            string base64 = Convert.ToBase64String(bytes.Span);

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            string fileExtension = Path.GetExtension(file.FileName);
            string output
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

            _ignoreBase64DataChange = true;
            _inputText.Text(output);
            _ignoreBase64DataChange = false;
        }
    }
}
