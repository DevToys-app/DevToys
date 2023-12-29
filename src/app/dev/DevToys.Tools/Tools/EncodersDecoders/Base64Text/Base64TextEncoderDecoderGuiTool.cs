using DevToys.Tools.Helpers;
using DevToys.Tools.Models;
using Microsoft.Extensions.Logging;

namespace DevToys.Tools.Tools.EncodersDecoders.Base64Text;

[Export(typeof(IGuiTool))]
[Name("Base64TextEncoderDecoder")]
[ToolDisplayInformation(
    IconFontName = "DevToys-Tools-Icons",
    IconGlyph = '\u0100',
    GroupName = PredefinedCommonToolGroupNames.EncodersDecoders,
    ResourceManagerAssemblyIdentifier = nameof(DevToysToolsResourceManagerAssemblyIdentifier),
    ResourceManagerBaseName = "DevToys.Tools.Tools.EncodersDecoders.Base64Text.Base64TextEncoderDecoder",
    ShortDisplayTitleResourceName = nameof(Base64TextEncoderDecoder.ShortDisplayTitle),
    LongDisplayTitleResourceName = nameof(Base64TextEncoderDecoder.LongDisplayTitle),
    DescriptionResourceName = nameof(Base64TextEncoderDecoder.Description),
    AccessibleNameResourceName = nameof(Base64TextEncoderDecoder.AccessibleName),
    SearchKeywordsResourceName = nameof(Base64TextEncoderDecoder.SearchKeywords))]
[AcceptedDataTypeName(PredefinedCommonDataTypeNames.Base64Text)]
[AcceptedDataTypeName(PredefinedCommonDataTypeNames.Text)]
internal sealed partial class Base64TextEncoderDecoderGuiTool : IGuiTool, IDisposable
{
    /// <summary>
    /// Whether the tool should encode or decode Base64.
    /// </summary>
    private static readonly SettingDefinition<EncodingConversion> conversionMode
        = new(
            name: $"{nameof(Base64TextEncoderDecoderGuiTool)}.{nameof(conversionMode)}",
            defaultValue: EncodingConversion.Encode);

    /// <summary>
    /// Whether the tool should encode/decode in Unicode or ASCII.
    /// </summary>
    private static readonly SettingDefinition<Base64Encoding> encoder
        = new(
            name: $"{nameof(Base64TextEncoderDecoderGuiTool)}.{nameof(encoder)}",
            defaultValue: DefaultEncoding);

    private enum GridRows
    {
        Settings,
        Input,
        Output
    }

    private enum GridColumns
    {
        Stretch
    }

    private const Base64Encoding DefaultEncoding = Base64Encoding.Utf8;

    private readonly DisposableSemaphore _semaphore = new();
    private readonly ILogger _logger;
    private readonly ISettingsProvider _settingsProvider;
    private readonly IUISwitch _conversionModeSwitch = Switch("base64-text-conversion-mode-switch");
    private readonly IUIMultiLineTextInput _inputText = MultilineTextInput("base64-text-input-box");
    private readonly IUIMultiLineTextInput _outputText = MultilineTextInput("base64-text-output-box");

    private CancellationTokenSource? _cancellationTokenSource;

    [ImportingConstructor]
    public Base64TextEncoderDecoderGuiTool(ISettingsProvider settingsProvider)
    {
        _logger = this.Log();
        _settingsProvider = settingsProvider;

        switch (_settingsProvider.GetSetting(conversionMode))
        {
            case EncodingConversion.Encode:
                _conversionModeSwitch.On();
                _inputText.AutoWrap();
                _outputText.AlwaysWrap();
                break;

            case EncodingConversion.Decode:
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
                    (GridRows.Output, new UIGridLength(1, UIGridUnitType.Fraction)))

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
                                    .Text(Base64TextEncoderDecoder.ConfigurationTitle),

                                SettingGroup("base64-text-conversion-mode-setting")
                                    .Icon("FluentSystemIcons", '\uF18D')
                                    .Title(Base64TextEncoderDecoder.ConversionTitle)
                                    .Description(Base64TextEncoderDecoder.ConversionDescription)

                                    .InteractiveElement(
                                        _conversionModeSwitch
                                            .OnText(Base64TextEncoderDecoder.ConversionEncode)
                                            .OffText(Base64TextEncoderDecoder.ConversionDecode)
                                            .OnToggle(OnConversionModeChanged))

                                    .WithSettings(
                                        Setting("base64-text-encoding-setting")
                                            .Title(Base64TextEncoderDecoder.EncodingTitle)
                                            .Description(Base64TextEncoderDecoder.EncodingDescription)

                                            .Handle(
                                                _settingsProvider,
                                                encoder,
                                                onOptionSelected: OnEncodingModeChanged,
                                                Item(Base64TextEncoderDecoder.Utf8, Base64Encoding.Utf8),
                                                Item(Base64TextEncoderDecoder.Ascii, Base64Encoding.Ascii))))),

                    Cell(
                        GridRows.Input,
                        GridColumns.Stretch,

                        _inputText
                            .Title(Base64TextEncoderDecoder.InputTitle)
                            .OnTextChanged(OnInputTextChanged)),

                    Cell(
                        GridRows.Output,
                        GridColumns.Stretch,

                        _outputText
                            .Title(Base64TextEncoderDecoder.OutputTitle)
                            .ReadOnly()
                            .Extendable())));

    public void OnDataReceived(string dataTypeName, object? parsedData)
    {
        if (dataTypeName == PredefinedCommonDataTypeNames.Base64Text && parsedData is string base64Text)
        {
            _conversionModeSwitch.Off(); // Switch to Decode mode
            _inputText.Text(base64Text); // This will trigger a conversion.
        }
        else if (dataTypeName == PredefinedCommonDataTypeNames.Text && parsedData is string text)
        {
            _conversionModeSwitch.On(); // Switch to Encode mode
            _inputText.Text(text); // This will trigger a conversion.
        }
    }

    public void Dispose()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _semaphore.Dispose();
    }

    private void OnConversionModeChanged(bool conversionMode)
    {
        _settingsProvider.SetSetting(Base64TextEncoderDecoderGuiTool.conversionMode, conversionMode ? EncodingConversion.Encode : EncodingConversion.Decode);
        _inputText.Text(_outputText.Text); // This will trigger a conversion.

        switch (_settingsProvider.GetSetting(Base64TextEncoderDecoderGuiTool.conversionMode))
        {
            case EncodingConversion.Encode:
                _inputText.AutoWrap();
                _outputText.AlwaysWrap();
                break;

            case EncodingConversion.Decode:
                _inputText.AlwaysWrap();
                _outputText.AutoWrap();
                break;

            default:
                throw new NotSupportedException();
        }
    }

    private void OnEncodingModeChanged(Base64Encoding encodingMode)
    {
        StartConvert(_inputText.Text);
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

        WorkTask = ConvertAsync(text, _settingsProvider.GetSetting(encoder), _cancellationTokenSource.Token);
    }

    private async Task ConvertAsync(string input, Base64Encoding encoderSetting, CancellationToken cancellationToken)
    {
        using (await _semaphore.WaitAsync(cancellationToken))
        {
            await TaskSchedulerAwaiter.SwitchOffMainThreadAsync(cancellationToken);

            string conversionResult;

            switch (_settingsProvider.GetSetting(conversionMode))
            {
                case EncodingConversion.Encode:
                    conversionResult
                        = Base64Helper.FromTextToBase64(
                            input,
                            encoderSetting,
                            _logger,
                            cancellationToken);
                    break;

                case EncodingConversion.Decode:
                    if (!string.IsNullOrEmpty(input) && !Base64Helper.IsBase64DataStrict(input))
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        _outputText.Text(Base64TextEncoderDecoder.InvalidBase64);
                        return;
                    }

                    conversionResult
                        = Base64Helper.FromBase64ToText(
                            input,
                           encoderSetting,
                            _logger,
                            cancellationToken);
                    break;

                default:
                    throw new NotSupportedException();
            }

            cancellationToken.ThrowIfCancellationRequested();
            _outputText.Text(conversionResult);
        }
    }
}
