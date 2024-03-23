using DevToys.Tools.Helpers;
using DevToys.Tools.Models;
using Microsoft.Extensions.Logging;

namespace DevToys.Tools.Tools.Formatters.Json;

[Export(typeof(IGuiTool))]
[Name("JsonFormatter")]
[ToolDisplayInformation(
    IconFontName = "DevToys-Tools-Icons",
    IconGlyph = '\u0108',
    GroupName = PredefinedCommonToolGroupNames.Formatters,
    ResourceManagerAssemblyIdentifier = nameof(DevToysToolsResourceManagerAssemblyIdentifier),
    ResourceManagerBaseName = "DevToys.Tools.Tools.Formatters.Json.JsonFormatter",
    ShortDisplayTitleResourceName = nameof(JsonFormatter.ShortDisplayTitle),
    LongDisplayTitleResourceName = nameof(JsonFormatter.LongDisplayTitle),
    DescriptionResourceName = nameof(JsonFormatter.Description),
    SearchKeywordsResourceName = nameof(JsonFormatter.SearchKeywords),
    AccessibleNameResourceName = nameof(JsonFormatter.AccessibleName))]
[AcceptedDataTypeName(PredefinedCommonDataTypeNames.Json)]
internal sealed partial class JsonFormatterGuiTool : IGuiTool, IDisposable
{
    /// <summary>
    /// Which indentation the tool need to use.
    /// </summary>
    private static readonly SettingDefinition<Indentation> indentationMode
        = new(name: $"{nameof(JsonFormatterGuiTool)}.{nameof(indentationMode)}", defaultValue: Indentation.TwoSpaces);

    /// <summary>
    /// Whether properties within the JSON should be sorted alphabetically
    /// </summary>
    private static readonly SettingDefinition<bool> sortProperties
        = new(name: $"{nameof(JsonFormatterGuiTool)}.{nameof(sortProperties)}", defaultValue: false);

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
    private readonly IUIMultiLineTextInput _inputTextArea = MultilineTextInput("json-input-text-area");
    private readonly IUIMultiLineTextInput _outputTextArea = MultilineTextInput("json-output-text-area");

    private CancellationTokenSource? _cancellationTokenSource;

    [ImportingConstructor]
    public JsonFormatterGuiTool(ISettingsProvider settingsProvider)
    {
        _logger = this.Log();
        _settingsProvider = settingsProvider;
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
                        Label().Text(JsonFormatter.Configuration),
                        Setting("json-text-indentation-setting")
                        .Icon("FluentSystemIcons", '\uF6F8')
                        .Title(JsonFormatter.Indentation)
                        .Handle(
                            _settingsProvider,
                            indentationMode,
                            OnIndentationModelChanged,
                            Item(JsonFormatter.TwoSpaces, Indentation.TwoSpaces),
                            Item(JsonFormatter.FourSpaces, Indentation.FourSpaces),
                            Item(JsonFormatter.OneTab, Indentation.OneTab),
                            Item(JsonFormatter.Minified, Indentation.Minified)
                        ),
                        Setting("json-text-sortProperties-setting")
                        .Icon("FluentSystemIcons", '\uf802')
                        .Title(JsonFormatter.SortProperties)
                        .Handle(
                            _settingsProvider,
                            sortProperties,
                            OnSettingChanged
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
                                .Title(JsonFormatter.Input)
                                .Language("json")
                                .OnTextChanged(OnInputTextChanged))
                        .WithRightPaneChild(
                            _outputTextArea
                                .Title(JsonFormatter.Output)
                                .Language("json")
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
            _inputTextArea.Text(jsonStrongTypedParsedData);
        }
    }

    public void Dispose()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _semaphore.Dispose();
    }

    private void OnSettingChanged(bool value)
    {
        StartFormat(_inputTextArea.Text);
    }

    private void OnIndentationModelChanged(Indentation indentationMode)
    {
        StartFormat(_inputTextArea.Text);
    }

    private void OnInputTextChanged(string text)
    {
        StartFormat(text);
    }

    private void StartFormat(string text)
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();

        WorkTask = FormatAsync(text, _settingsProvider.GetSetting(indentationMode), _settingsProvider.GetSetting(sortProperties), _cancellationTokenSource.Token);
    }

    private async Task FormatAsync(string input, Indentation indentationSetting, bool sortPropertiesSetting, CancellationToken cancellationToken)
    {
        using (await _semaphore.WaitAsync(cancellationToken))
        {
            await TaskSchedulerAwaiter.SwitchOffMainThreadAsync(cancellationToken);

            ResultInfo<string> formatResult = await JsonHelper.FormatAsync(
                input,
                indentationSetting,
                sortPropertiesSetting,
                _logger,
                cancellationToken);

            _outputTextArea.Text(formatResult.Data!);
        }
    }
}
