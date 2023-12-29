using DevToys.Tools.Helpers;
using DevToys.Tools.Models;
using Microsoft.Extensions.Logging;

namespace DevToys.Tools.Tools.EncodersDecoders.Html;

[Export(typeof(IGuiTool))]
[Name("HtmlEncoderDecoder")]
[ToolDisplayInformation(
    IconFontName = "DevToys-Tools-Icons",
    IconGlyph = '\u0107',
    GroupName = PredefinedCommonToolGroupNames.EncodersDecoders,
    ResourceManagerAssemblyIdentifier = nameof(DevToysToolsResourceManagerAssemblyIdentifier),
    ResourceManagerBaseName = "DevToys.Tools.Tools.EncodersDecoders.Html.HtmlEncoderDecoder",
    ShortDisplayTitleResourceName = nameof(HtmlEncoderDecoder.ShortDisplayTitle),
    LongDisplayTitleResourceName = nameof(HtmlEncoderDecoder.LongDisplayTitle),
    DescriptionResourceName = nameof(HtmlEncoderDecoder.Description),
    AccessibleNameResourceName = nameof(HtmlEncoderDecoder.AccessibleName))]
internal sealed partial class HtmlEncoderDecoderGuiTool : IGuiTool, IDisposable
{
    /// <summary>
    /// Whether the tool should encode or decode Html.
    /// </summary>
    private static readonly SettingDefinition<EncodingConversion> conversionMode
        = new(
            name: $"{nameof(HtmlEncoderDecoderGuiTool)}.{nameof(conversionMode)}",
            defaultValue: EncodingConversion.Encode);

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

    private readonly DisposableSemaphore _semaphore = new();
    private readonly ILogger _logger;
    private readonly ISettingsProvider _settingsProvider;
    private readonly IUISwitch _conversionModeSwitch = Switch("html-conversion-mode-switch");
    private readonly IUIMultiLineTextInput _inputText = MultilineTextInput("html-input-box");
    private readonly IUIMultiLineTextInput _outputText = MultilineTextInput("html-output-box");

    private CancellationTokenSource? _cancellationTokenSource;

    [ImportingConstructor]
    public HtmlEncoderDecoderGuiTool(ISettingsProvider settingsProvider)
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
                                    .Text(HtmlEncoderDecoder.ConfigurationTitle),

                                Setting("html-conversion-mode-setting")
                                    .Icon("FluentSystemIcons", '\uF18D')
                                    .Title(HtmlEncoderDecoder.ConversionTitle)
                                    .Description(HtmlEncoderDecoder.ConversionDescription)

                                    .InteractiveElement(
                                        _conversionModeSwitch
                                            .OnText(HtmlEncoderDecoder.ConversionEncode)
                                            .OffText(HtmlEncoderDecoder.ConversionDecode)
                                            .OnToggle(OnConversionModeChanged)))),

                    Cell(
                        GridRows.Input,
                        GridColumns.Stretch,

                        _inputText
                            .Title(HtmlEncoderDecoder.InputTitle)
                            .OnTextChanged(OnInputTextChanged)),

                    Cell(
                        GridRows.Output,
                        GridColumns.Stretch,

                        _outputText
                            .Title(HtmlEncoderDecoder.OutputTitle)
                            .ReadOnly()
                            .Extendable())));

    public void OnDataReceived(string dataTypeName, object? parsedData)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _semaphore.Dispose();
    }

    private void OnConversionModeChanged(bool conversionMode)
    {
        _settingsProvider.SetSetting(HtmlEncoderDecoderGuiTool.conversionMode, conversionMode ? EncodingConversion.Encode : EncodingConversion.Decode);
        _inputText.Text(_outputText.Text); // This will trigger a conversion.
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

        WorkTask = ConvertAsync(text, _settingsProvider.GetSetting(conversionMode), _cancellationTokenSource.Token);
    }

    private async Task ConvertAsync(string input, EncodingConversion conversionModeSetting, CancellationToken cancellationToken)
    {
        using (await _semaphore.WaitAsync(cancellationToken))
        {
            await TaskSchedulerAwaiter.SwitchOffMainThreadAsync(cancellationToken);

            _outputText.Text(
                HtmlHelper.EncodeOrDecode(
                    input,
                    conversionModeSetting,
                    _logger,
                    cancellationToken));
        }
    }
}
