using DevToys.Tools.Helpers;
using DevToys.Tools.Models;
using DevToys.Tools.SmartDetection;
using Microsoft.Extensions.Logging;

namespace DevToys.Tools.Tools.Text.EscapeUnescape;

[Export(typeof(IGuiTool))]
[Name("EscapeUnescape")]
[ToolDisplayInformation(
    IconFontName = "DevToys-Tools-Icons",
    IconGlyph = '\u0130',
    GroupName = PredefinedCommonToolGroupNames.Text,
    ResourceManagerAssemblyIdentifier = nameof(DevToysToolsResourceManagerAssemblyIdentifier),
    ResourceManagerBaseName = "DevToys.Tools.Tools.Text.EscapeUnescape.EscapeUnescape",
    ShortDisplayTitleResourceName = nameof(EscapeUnescape.ShortDisplayTitle),
    LongDisplayTitleResourceName = nameof(EscapeUnescape.LongDisplayTitle),
    DescriptionResourceName = nameof(EscapeUnescape.Description),
    AccessibleNameResourceName = nameof(EscapeUnescape.AccessibleName))]
[AcceptedDataTypeName(EscapedCharactersDetector.InternalName)]
internal sealed class EscapeUnescapeGuiTool : IGuiTool, IDisposable
{
    /// <summary>
    /// Whether the tool should escape or unescape the text.
    /// </summary>
    private static readonly SettingDefinition<EncodingConversion> conversionMode
        = new(
            name: $"{nameof(EscapeUnescapeGuiTool)}.{nameof(conversionMode)}",
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
    private readonly IUISwitch _conversionModeSwitch = Switch("text-escape-unescape-conversion-mode-switch");
    private readonly IUIMultiLineTextInput _inputText = MultilineTextInput("text-escape-unescape-input-box");
    private readonly IUIMultiLineTextInput _outputText = MultilineTextInput("text-escape-unescape-output-box");

    private CancellationTokenSource? _cancellationTokenSource;

    [ImportingConstructor]
    public EscapeUnescapeGuiTool(ISettingsProvider settingsProvider)
    {
        _logger = this.Log();
        _settingsProvider = settingsProvider;

        if (_settingsProvider.GetSetting(conversionMode) == EncodingConversion.Encode)
        {
            _conversionModeSwitch.On();
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
                                    .Text(EscapeUnescape.ConfigurationTitle),

                                Setting("text-escape-unescape-conversion-mode-setting")
                                    .Icon("FluentSystemIcons", '\uF18D')
                                    .Title(EscapeUnescape.ConversionTitle)
                                    .Description(EscapeUnescape.ConversionDescription)

                                    .InteractiveElement(
                                        _conversionModeSwitch
                                            .OnText(EscapeUnescape.ConversionEncode)
                                            .OffText(EscapeUnescape.ConversionDecode)
                                            .OnToggle(OnConversionModeChanged)))),

                    Cell(
                        GridRows.Input,
                        GridColumns.Stretch,

                        _inputText
                            .Title(EscapeUnescape.InputTitle)
                            .OnTextChanged(OnInputTextChanged)),

                    Cell(
                        GridRows.Output,
                        GridColumns.Stretch,

                        _outputText
                            .Title(EscapeUnescape.OutputTitle)
                            .ReadOnly()
                            .Extendable())));

    public void OnDataReceived(string dataTypeName, object? parsedData)
    {
        if (dataTypeName == EscapedCharactersDetector.InternalName && parsedData is string text)
        {
            _conversionModeSwitch.Off(); // Switch to Unescape mode
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
        _settingsProvider.SetSetting(
            EscapeUnescapeGuiTool.conversionMode,
            conversionMode ? EncodingConversion.Encode : EncodingConversion.Decode);
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

    private async Task ConvertAsync(string input, EncodingConversion conversionMode, CancellationToken cancellationToken)
    {
        using (await _semaphore.WaitAsync(cancellationToken))
        {
            await TaskSchedulerAwaiter.SwitchOffMainThreadAsync(cancellationToken);

            ResultInfo<string> conversionResult = conversionMode switch
            {
                EncodingConversion.Encode
                    => StringHelper.EscapeString(
                        input,
                        _logger,
                        cancellationToken),

                EncodingConversion.Decode
                    => StringHelper.UnescapeString(
                        input,
                        _logger,
                        cancellationToken),

                _ => throw new NotSupportedException(),
            };

            cancellationToken.ThrowIfCancellationRequested();
            _outputText.Text(conversionResult.Data!);
        }
    }
}
