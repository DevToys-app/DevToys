using DevToys.Tools.Helpers;
using DevToys.Tools.SmartDetection;

namespace DevToys.Tools.Tools.EncodersDecoders.Certificate;

[Export(typeof(IGuiTool))]
[Name("CertificateDecoder")]
[ToolDisplayInformation(
    IconFontName = "DevToys-Tools-Icons",
    IconGlyph = '\u0135',
    GroupName = PredefinedCommonToolGroupNames.EncodersDecoders,
    ResourceManagerAssemblyIdentifier = nameof(DevToysToolsResourceManagerAssemblyIdentifier),
    ResourceManagerBaseName = "DevToys.Tools.Tools.EncodersDecoders.Certificate.CertificateDecoder",
    ShortDisplayTitleResourceName = nameof(CertificateDecoder.ShortDisplayTitle),
    LongDisplayTitleResourceName = nameof(CertificateDecoder.LongDisplayTitle),
    DescriptionResourceName = nameof(CertificateDecoder.Description),
    AccessibleNameResourceName = nameof(CertificateDecoder.AccessibleName),
    SearchKeywordsResourceName = nameof(CertificateDecoder.SearchKeywords))]
[AcceptedDataTypeName(CertificateDetector.InternalName)]
internal sealed class CertificateDecoderGuiTool : IGuiTool, IDisposable
{
    private enum GridRows
    {
        TopAuto,
        Stretch,
    }

    private enum GridColumns
    {
        Stretch
    }

    private readonly DisposableSemaphore _semaphore = new();
    private readonly IUIPasswordInput _password = PasswordInput("certificate-decoder-password-input");
    private readonly IUIFileSelector _fileSelector = FileSelector("certificate-decoder-file-selector");
    private readonly IUIMultiLineTextInput _inputText = MultilineTextInput("certificate-decoder-input");
    private readonly IUIMultiLineTextInput _outputText = MultilineTextInput("certificate-decoder-output");

    private CancellationTokenSource? _cancellationTokenSource;

    // For unit tests.
    internal Task? WorkTask { get; private set; }

    public UIToolView View
        => new(
            isScrollable: false,
            SplitGrid()
                .Vertical()
                .LeftPaneLength(new UIGridLength(1, UIGridUnitType.Fraction))
                .RightPaneLength(new UIGridLength(1, UIGridUnitType.Fraction))

                .WithLeftPaneChild(
                    Grid()
                        .ColumnSmallSpacing()

                        .Rows(
                            (GridRows.TopAuto, Auto),
                            (GridRows.Stretch, new UIGridLength(1, UIGridUnitType.Fraction)))

                        .Columns(
                            (GridColumns.Stretch, new UIGridLength(1, UIGridUnitType.Fraction)))

                        .Cells(
                            Cell(
                                GridRows.TopAuto,
                                GridColumns.Stretch,

                                Stack()
                                    .Vertical()
                                    .LargeSpacing()
                                    .WithChildren(

                                        _password
                                            .Title(CertificateDecoder.PasswordTitle)
                                            .OnTextChanged(OnPasswordChanged),

                                        _fileSelector
                                            .CanSelectOneFile()
                                            .LimitFileTypesTo("pem", "crt", "pfx", "cer")
                                            .OnFilesSelected(OnFileSelectedAsync))),

                            Cell(
                                GridRows.Stretch,
                                GridColumns.Stretch,

                                _inputText
                                    .Title(CertificateDecoder.InputTitle)
                                    .CanCopyWhenEditable()
                                    .AlwaysWrap()
                                    .OnTextChanged(OnInputTextChanged))))

                .WithRightPaneChild(
                    _outputText
                        .ReadOnly()
                        .Extendable()
                        .Title(CertificateDecoder.OutputTitle)));

    public void OnDataReceived(string dataTypeName, object? parsedData)
    {
        if (dataTypeName == CertificateDetector.InternalName && parsedData is string text)
        {
            _inputText.Text(text); // This will trigger a decoding task.
        }
    }

    public void Dispose()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _semaphore.Dispose();
    }

    private async ValueTask OnFileSelectedAsync(SandboxedFileReader[] files)
    {
        Guard.HasSizeEqualTo(files, 1);

        using SandboxedFileReader selectedFile = files[0];
        using var memStream = new MemoryStream();

        await selectedFile.CopyFileContentToAsync(memStream, CancellationToken.None);

        byte[] bytes = memStream.ToArray();
        string rawCertificateString = CertificateHelper.GetRawCertificateString(bytes);

        _inputText.Text(rawCertificateString); // This will trigger a decoding task.
    }

    private void OnInputTextChanged(string text)
    {
        StartDecoding();
    }

    private void OnPasswordChanged(string text)
    {
        StartDecoding();
    }

    private void StartDecoding()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();

        WorkTask = DecodeAsync(_inputText.Text, _password.Text, _cancellationTokenSource.Token);
    }

    private async Task DecodeAsync(string rawCertificateString, string password, CancellationToken cancellationToken)
    {
        await TaskSchedulerAwaiter.SwitchOffMainThreadAsync(cancellationToken);

        string? decoded = null;
        if (!string.IsNullOrWhiteSpace(rawCertificateString))
        {
            CertificateHelper.TryDecodeCertificate(
                this.Log(),
                rawCertificateString,
                password,
                out decoded);
        }

        _outputText.Text(decoded ?? string.Empty);
    }
}
