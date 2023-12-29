using System.IO.Compression;
using DevToys.Tools.Helpers;
using Microsoft.Extensions.Logging;

namespace DevToys.Tools.Tools.EncodersDecoders.GZip;

[Export(typeof(IGuiTool))]
[Name("GZipEncoderDecoder")]
[ToolDisplayInformation(
    IconFontName = "DevToys-Tools-Icons",
    IconGlyph = '\u0120',
    GroupName = PredefinedCommonToolGroupNames.EncodersDecoders,
    ResourceManagerAssemblyIdentifier = nameof(DevToysToolsResourceManagerAssemblyIdentifier),
    ResourceManagerBaseName = "DevToys.Tools.Tools.EncodersDecoders.GZip.GZipEncoderDecoder",
    ShortDisplayTitleResourceName = nameof(GZipEncoderDecoder.ShortDisplayTitle),
    LongDisplayTitleResourceName = nameof(GZipEncoderDecoder.LongDisplayTitle),
    DescriptionResourceName = nameof(GZipEncoderDecoder.Description),
    AccessibleNameResourceName = nameof(GZipEncoderDecoder.AccessibleName))]
[AcceptedDataTypeName(PredefinedCommonDataTypeNames.GZip)]
internal sealed partial class GZipEncoderDecoderGuiTool : IGuiTool, IDisposable
{
    /// <summary>
    /// Whether the tool should compress or decompress GZip.
    /// </summary>
    private static readonly SettingDefinition<CompressionMode> compressionMode
        = new(
            name: $"{nameof(GZipEncoderDecoderGuiTool)}.{nameof(compressionMode)}",
            defaultValue: CompressionMode.Compress);

    private enum GridRows
    {
        Settings,
        Input,
        Output,
        CompressionRatio
    }

    private enum GridColumns
    {
        Stretch
    }

    private readonly DisposableSemaphore _semaphore = new();
    private readonly ILogger _logger;
    private readonly ISettingsProvider _settingsProvider;
    private readonly IUISwitch _compressionModeSwitch = Switch("gzip-compression-mode-switch");
    private readonly IUIMultiLineTextInput _inputText = MultilineTextInput("gzip-input-box");
    private readonly IUIMultiLineTextInput _outputText = MultilineTextInput("gzip-output-box");
    private readonly IUIInfoBar _compressionRatioInfoBar = InfoBar("gzip-compression-ratio-info-bar");

    private CancellationTokenSource? _cancellationTokenSource;

    [ImportingConstructor]
    public GZipEncoderDecoderGuiTool(ISettingsProvider settingsProvider)
    {
        _logger = this.Log();
        _settingsProvider = settingsProvider;

        switch (_settingsProvider.GetSetting(compressionMode))
        {
            case CompressionMode.Compress:
                _compressionModeSwitch.On();
                _inputText.AutoWrap();
                _outputText.AlwaysWrap();
                break;

            case CompressionMode.Decompress:
                _inputText.AlwaysWrap();
                _outputText.AutoWrap();
                break;

            default:
                throw new NotSupportedException();
        }
    }

    // For unit tests.
    internal Task? WorkTask { get; private set; }

    public UIToolView View
        => new(
            isScrollable: true,
            Grid()
                .RowLargeSpacing()

                .Rows(
                    (GridRows.Settings, Auto),
                    (GridRows.Input, new UIGridLength(1, UIGridUnitType.Fraction)),
                    (GridRows.Output, new UIGridLength(1, UIGridUnitType.Fraction)),
                    (GridRows.CompressionRatio, Auto))

                .Columns(
                    (GridColumns.Stretch, new UIGridLength(1, UIGridUnitType.Fraction)))

                .Cells(
                    Cell(
                        GridRows.Settings,
                        GridColumns.Stretch,

                        Stack()
                            .Vertical()
                            .SmallSpacing()

                            .WithChildren(
                                Label()
                                    .Text(GZipEncoderDecoder.ConfigurationTitle),

                                Setting("gzip-conversion-mode-setting")
                                    .Icon("FluentSystemIcons", '\uF18D')
                                    .Title(GZipEncoderDecoder.CompressionModeTitle)
                                    .Description(GZipEncoderDecoder.CompressionModeDescription)

                                    .InteractiveElement(
                                        _compressionModeSwitch
                                            .OnText(GZipEncoderDecoder.CompressionModeCompress)
                                            .OffText(GZipEncoderDecoder.CompressionModeDecompress)
                                            .OnToggle(OnCompressionModeChanged)))),

                    Cell(
                        GridRows.Input,
                        GridColumns.Stretch,

                        _inputText
                            .Title(GZipEncoderDecoder.InputTitle)
                            .OnTextChanged(OnInputTextChanged)),

                    Cell(
                        GridRows.Output,
                        GridColumns.Stretch,

                        _outputText
                            .Title(GZipEncoderDecoder.OutputTitle)
                            .ReadOnly()
                            .Extendable()),

                    Cell(
                        GridRows.CompressionRatio,
                        GridColumns.Stretch,

                        _compressionRatioInfoBar
                            .Informational()
                            .NonClosable()
                            .HideIcon()
                            .Open()
                            .Title(GZipEncoderDecoder.CompressionRatio))));

    public void OnDataReceived(string dataTypeName, object? parsedData)
    {
        if (dataTypeName == PredefinedCommonDataTypeNames.GZip && parsedData is string text)
        {
            _compressionModeSwitch.Off(); // Switch to Decompress mode
            _inputText.Text(text); // This will trigger a conversion.
        }
    }

    public void Dispose()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _semaphore.Dispose();
    }

    private void OnCompressionModeChanged(bool conversionMode)
    {
        _settingsProvider.SetSetting(compressionMode, conversionMode ? CompressionMode.Compress : CompressionMode.Decompress);
        _inputText.Text(_outputText.Text); // This will trigger a conversion.

        switch (_settingsProvider.GetSetting(compressionMode))
        {
            case CompressionMode.Compress:
                _inputText.AutoWrap();
                _outputText.AlwaysWrap();
                break;

            case CompressionMode.Decompress:
                _inputText.AlwaysWrap();
                _outputText.AutoWrap();
                break;

            default:
                throw new NotSupportedException();
        }
    }

    private void OnInputTextChanged(string text)
    {
        StartConvert(text);
    }

    private void StartConvert(string text)
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();

        WorkTask = ConvertAsync(text, _settingsProvider.GetSetting(compressionMode), _cancellationTokenSource.Token);
    }

    private async Task ConvertAsync(string input, CompressionMode compressionModeSetting, CancellationToken cancellationToken)
    {
        using (await _semaphore.WaitAsync(cancellationToken))
        {
            await TaskSchedulerAwaiter.SwitchOffMainThreadAsync(cancellationToken);

            (string data, double differencePercentage) conversionResult
                = await GZipHelper.CompressOrDecompressAsync(
                    input,
                    compressionModeSetting,
                    _logger,
                    cancellationToken);

            _compressionRatioInfoBar.Description(string.Format(GZipEncoderDecoder.CompressionRatioValue, conversionResult.differencePercentage));
            _outputText.Text(conversionResult.data);
        }
    }
}
