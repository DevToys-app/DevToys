using DevToys.Tools.Helpers;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using ZXing;
using ZXing.Common;
using ZXing.QrCode.Internal;
using ZXing.Rendering;
using static ZXing.Rendering.SvgRenderer;

namespace DevToys.Tools.Tools.EncodersDecoders.QRCode;

[Export(typeof(IGuiTool))]
[Name("QRCodeEncoderDecoder")]
[ToolDisplayInformation(
    IconFontName = "FluentSystemIcons",
    IconGlyph = '\uE9EC',
    GroupName = PredefinedCommonToolGroupNames.EncodersDecoders,
    ResourceManagerAssemblyIdentifier = nameof(DevToysToolsResourceManagerAssemblyIdentifier),
    ResourceManagerBaseName = "DevToys.Tools.Tools.EncodersDecoders.QRCode.QRCodeEncoderDecoder",
    ShortDisplayTitleResourceName = nameof(QRCodeEncoderDecoder.ShortDisplayTitle),
    LongDisplayTitleResourceName = nameof(QRCodeEncoderDecoder.LongDisplayTitle),
    DescriptionResourceName = nameof(QRCodeEncoderDecoder.Description),
    AccessibleNameResourceName = nameof(QRCodeEncoderDecoder.AccessibleName),
    SearchKeywordsResourceName = nameof(QRCodeEncoderDecoder.SearchKeywords))]
[AcceptedDataTypeName(PredefinedCommonDataTypeNames.Image)]
[AcceptedDataTypeName(PredefinedCommonDataTypeNames.Text)]
internal sealed partial class QRCodeEncoderDecoderGuiTool : IGuiTool, IDisposable
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
    private readonly IUIMultiLineTextInput _inputText = MultilineTextInput("qrcode-input-box");
    private readonly IUIFileSelector _fileSelector = FileSelector("qrcode-file-selector");
    private readonly IUIImageViewer _imageViewer = ImageViewer("qrcode-preview");
    private readonly IUIButton _exportAsSvgButton = Button("qrcode-export-as-svg-button");

    private SandboxedFileReader? _selectedFile;
    private CancellationTokenSource? _cancellationTokenSource;
    private bool _ignoreTextChange;

    [ImportingConstructor]
    public QRCodeEncoderDecoderGuiTool(IFileStorage fileStorage)
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
                        .Title(QRCodeEncoderDecoder.InputText)
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
                                    .LimitFileTypesToImages()
                                    .OnFilesSelected(OnFileSelectedAsync)),

                            Cell(
                                GridRows.ImagePreviewer,
                                GridColumns.Stretch,

                                _imageViewer
                                    .Title(QRCodeEncoderDecoder.ImageWithQrCode)
                                    .ManuallyHandleSaveAs("svg", OnSaveAsSvgAsync)))));

    public async void OnDataReceived(string dataTypeName, object? parsedData)
    {
        if (dataTypeName == PredefinedCommonDataTypeNames.Text && parsedData is string text)
        {
            _inputText.Text(text); // This will trigger a conversion.
        }
        else if (dataTypeName == PredefinedCommonDataTypeNames.Image && parsedData is Image<Rgba32> image)
        {
            FileInfo temporaryFile = _fileStorage.CreateSelfDestroyingTempFile("png");

            using (image)
            {
                using Stream fileStream = _fileStorage.OpenWriteFile(temporaryFile.FullName, replaceIfExist: true);
                await image.SaveAsPngAsync(fileStream);
            }

            _fileSelector.WithFiles(SandboxedFileReader.FromFileInfo(temporaryFile)); // This will trigger a conversion.
        }
    }

    public void Dispose()
    {
        _selectedFile?.Dispose();
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _semaphore.Dispose();
    }

    private async ValueTask OnSaveAsSvgAsync(FileStream fileStream)
    {
        string svg = QrCodeHelper.GenerateSvgQrCode(_inputText.Text);
        using var fileWriter = new StreamWriter(fileStream);
        await fileWriter.WriteAsync(svg);
    }

    private void OnInputTextChanged(string text)
    {
        if (!_ignoreTextChange)
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

        WorkTask = DecodeQRCodeAsync(file, _cancellationTokenSource.Token);
    }

    private void StartConvert(string text)
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();

        WorkTask = EncodeQRCodeAsync(text, _cancellationTokenSource.Token);
    }

    private async Task EncodeQRCodeAsync(string input, CancellationToken cancellationToken)
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

                Image<Rgba32> image = QrCodeHelper.GenerateQrCode(input); // Do not dispose the image. Will be disposed by the image viewer when the image is replaced.

                _imageViewer.WithImage(image, disposeAutomatically: true);
                _exportAsSvgButton.Enable();
            }
        }
        catch
        {
            _imageViewer.Clear();
        }
    }

    private async Task DecodeQRCodeAsync(SandboxedFileReader file, CancellationToken cancellationToken)
    {
        _exportAsSvgButton.Disable();

        await TaskSchedulerAwaiter.SwitchOffMainThreadAsync(cancellationToken);

        using (await _semaphore.WaitAsync(cancellationToken))
        {
            using Stream stream = await file.GetNewAccessToFileContentAsync(cancellationToken);

            string output = await QrCodeHelper.ReadQrCodeAsync(stream, cancellationToken);

            _ignoreTextChange = true;
            _inputText.Text(output);
            _ignoreTextChange = false;
        }
    }
}
