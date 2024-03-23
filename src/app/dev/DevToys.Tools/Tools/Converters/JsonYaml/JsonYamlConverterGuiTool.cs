using DevToys.Tools.Helpers;
using DevToys.Tools.Models;
using Microsoft.Extensions.Logging;

namespace DevToys.Tools.Tools.Converters.JsonYaml;

[Export(typeof(IGuiTool))]
[Name("JsonYamlConverter")]
[ToolDisplayInformation(
    IconFontName = "DevToys-Tools-Icons",
    IconGlyph = '\u0109',
    GroupName = PredefinedCommonToolGroupNames.Converters,
    ResourceManagerAssemblyIdentifier = nameof(DevToysToolsResourceManagerAssemblyIdentifier),
    ResourceManagerBaseName = "DevToys.Tools.Tools.Converters.JsonYaml.JsonYamlConverter",
    ShortDisplayTitleResourceName = nameof(JsonYamlConverter.ShortDisplayTitle),
    LongDisplayTitleResourceName = nameof(JsonYamlConverter.LongDisplayTitle),
    DescriptionResourceName = nameof(JsonYamlConverter.Description),
    AccessibleNameResourceName = nameof(JsonYamlConverter.AccessibleName))]
[AcceptedDataTypeName(PredefinedCommonDataTypeNames.Json)]
[AcceptedDataTypeName(PredefinedCommonDataTypeNames.Yaml)]
internal sealed partial class JsonYamlConverterGuiTool : IGuiTool, IDisposable
{
    private const string JsonLanguage = "json";
    private const string YamlLanguage = "yaml";

    /// <summary>
    /// Whether the tool should convert Json to Yaml or Yaml to Json.
    /// </summary>
    private static readonly SettingDefinition<JsonToYamlConversion> conversionMode
        = new(name: $"{nameof(JsonYamlConverterGuiTool)}.{nameof(conversionMode)}", defaultValue: JsonToYamlConversion.JsonToYaml);

    /// <summary>
    /// Which indentation the tool need to use.
    /// </summary>
    private static readonly SettingDefinition<Indentation> indentationMode
        = new(name: $"{nameof(JsonYamlConverterGuiTool)}.{nameof(indentationMode)}", defaultValue: Indentation.TwoSpaces);

    private enum GridColumn
    {
        Content
    }

    private enum GridRow
    {
        Header,
        Content,
        Footer
    }

    private readonly DisposableSemaphore _semaphore = new();
    private readonly ILogger _logger;
    private readonly ISettingsProvider _settingsProvider;
    private readonly IUIMultiLineTextInput _inputTextArea = MultilineTextInput("json-to-yaml-input-text-area");
    private readonly IUIMultiLineTextInput _outputTextArea = MultilineTextInput("json-to-yaml-output-text-area");

    private CancellationTokenSource? _cancellationTokenSource;

    [ImportingConstructor]
    public JsonYamlConverterGuiTool(ISettingsProvider settingsProvider)
    {
        _logger = this.Log();
        _settingsProvider = settingsProvider;

        switch (_settingsProvider.GetSetting(conversionMode))
        {
            case JsonToYamlConversion.JsonToYaml:
                SetJsonToYamlConversion();
                break;
            case JsonToYamlConversion.YamlToJson:
                SetYamlToJsonConversion();
                break;
            default:
                throw new NotSupportedException();
        }
    }

    internal Task? WorkTask { get; private set; }

    public UIToolView View
        => new(
            isScrollable: true,
            Grid()
                .ColumnLargeSpacing()
                .RowLargeSpacing()
                .Rows(
                    (GridRow.Header, Auto),
                    (GridRow.Content, new UIGridLength(1, UIGridUnitType.Fraction))
                )
                .Columns(
                    (GridColumn.Content, new UIGridLength(1, UIGridUnitType.Fraction))
                )
            .Cells(
                Cell(
                    GridRow.Header,
                    GridColumn.Content,
                    Stack().Vertical().WithChildren(
                        Label()
                        .Text(JsonYamlConverter.Configuration),
                        Setting("json-to-yaml-text-conversion-setting")
                        .Icon("FluentSystemIcons", '\uF18D')
                        .Title(JsonYamlConverter.ConversionTitle)
                        .Description(JsonYamlConverter.ConversionDescription)
                        .Handle(
                            _settingsProvider,
                            conversionMode,
                            OnConversionModeChanged,
                            Item(JsonYamlConverter.JsonToYaml, JsonToYamlConversion.JsonToYaml),
                            Item(JsonYamlConverter.YamlToJson, JsonToYamlConversion.YamlToJson)
                        ),
                        Setting("json-to-yaml-text-indentation-setting")
                        .Icon("FluentSystemIcons", '\uF6F8')
                        .Title(JsonYamlConverter.Indentation)
                        .Handle(
                            _settingsProvider,
                            indentationMode,
                            OnIndentationModelChanged,
                            Item(JsonYamlConverter.TwoSpaces, Indentation.TwoSpaces),
                            Item(JsonYamlConverter.FourSpaces, Indentation.FourSpaces)
                        )
                    )
                ),
                Cell(
                    GridRow.Content,
                    GridColumn.Content,
                    SplitGrid()
                        .Vertical()
                        .WithLeftPaneChild(
                            _inputTextArea
                                .Title(JsonYamlConverter.Input)
                                .OnTextChanged(OnInputTextChanged))
                        .WithRightPaneChild(
                            _outputTextArea
                                .Title(JsonYamlConverter.Output)
                                .ReadOnly()
                                .Extendable())
                )
            )
        );

    // Smart detection handler.
    public void OnDataReceived(string dataTypeName, object? parsedData)
    {
        if (dataTypeName == PredefinedCommonDataTypeNames.Json &&
            parsedData is string jsonStrongTypedParsedData)
        {
            _inputTextArea.Language(JsonLanguage);
            _outputTextArea.Language(YamlLanguage);
            _settingsProvider.SetSetting(conversionMode, JsonToYamlConversion.JsonToYaml);
            _inputTextArea.Text(jsonStrongTypedParsedData);
        }

        if (dataTypeName == PredefinedCommonDataTypeNames.Yaml &&
            parsedData is string yamlStrongTypedParsedData)
        {
            _inputTextArea.Language(YamlLanguage);
            _outputTextArea.Language(JsonLanguage);
            _settingsProvider.SetSetting(conversionMode, JsonToYamlConversion.YamlToJson);
            _inputTextArea.Text(yamlStrongTypedParsedData);
        }
    }

    public void Dispose()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _semaphore.Dispose();
    }

    private void OnConversionModeChanged(JsonToYamlConversion conversionMode)
    {
        switch (conversionMode)
        {
            case JsonToYamlConversion.JsonToYaml:
                SetJsonToYamlConversion();
                break;
            case JsonToYamlConversion.YamlToJson:
                SetYamlToJsonConversion();
                break;
            default:
                throw new NotSupportedException();
        }

        _inputTextArea.Text(_outputTextArea.Text);
    }

    private void OnIndentationModelChanged(Indentation indentationMode)
    {
        StartConvert(_inputTextArea.Text);
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

        WorkTask = ConvertAsync(text, _settingsProvider.GetSetting(conversionMode), _settingsProvider.GetSetting(indentationMode), _cancellationTokenSource.Token);
    }

    private async Task ConvertAsync(string input, JsonToYamlConversion conversionModeSetting, Indentation indentationModeSetting, CancellationToken cancellationToken)
    {
        using (await _semaphore.WaitAsync(cancellationToken))
        {
            await TaskSchedulerAwaiter.SwitchOffMainThreadAsync(cancellationToken);

            ResultInfo<string> conversionResult = await JsonYamlHelper.ConvertAsync(
                input,
                conversionModeSetting,
                indentationModeSetting,
                _logger,
                cancellationToken);
            _outputTextArea.Text(conversionResult.Data!);
        }
    }

    private void SetJsonToYamlConversion()
    {
        _inputTextArea
            .Language(JsonLanguage);
        _outputTextArea
            .Language(YamlLanguage);
    }

    private void SetYamlToJsonConversion()
    {
        _inputTextArea
            .Language(YamlLanguage);
        _outputTextArea
            .Language(JsonLanguage);
    }
}
