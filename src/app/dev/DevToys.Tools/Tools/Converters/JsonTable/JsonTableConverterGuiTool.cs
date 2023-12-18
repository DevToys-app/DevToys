using System.Data;
using DevToys.Tools.Tools.Converters.JsonTable;
using Microsoft.Extensions.Logging;
using static DevToys.Tools.Helpers.JsonTableHelper;

namespace DevToys.Tools.Tools.Converters.JsonYaml;

[Export(typeof(IGuiTool))]
[Name("JsonTableConverter")]
[ToolDisplayInformation(
    IconFontName = "DevToys-Tools-Icons",
    IconGlyph = '\u0109',
    GroupName = PredefinedCommonToolGroupNames.Converters,
    ResourceManagerAssemblyIdentifier = nameof(DevToysToolsResourceManagerAssemblyIdentifier),
    ResourceManagerBaseName = "DevToys.Tools.Tools.Converters.JsonTable.JsonTableConverter",
    ShortDisplayTitleResourceName = nameof(JsonTableConverter.ShortDisplayTitle),
    LongDisplayTitleResourceName = nameof(JsonTableConverter.LongDisplayTitle),
    DescriptionResourceName = nameof(JsonTableConverter.Description),
    AccessibleNameResourceName = nameof(JsonTableConverter.AccessibleName),
    SearchKeywordsResourceName = nameof(JsonTableConverter.SearchKeywords))]
[AcceptedDataTypeName(PredefinedCommonDataTypeNames.JsonArray)]
internal sealed partial class JsonTableConverterGuiTool : IGuiTool, IDisposable
{
    /// <summary>
    /// Whether the tool should copy to clipboard as CSV or TSV.
    /// </summary>
    /// <remarks>
    /// Default to tab-separated-values because copy-to-Excel is the most likely use-case.
    /// </remarks>
    private static readonly SettingDefinition<CopyFormat> copyFormat
        = new(name: $"{nameof(JsonTableConverterGuiTool)}.{nameof(copyFormat)}", defaultValue: CopyFormat.TSV);

    private enum GridColumn
    {
        Content
    }

    private enum GridRow
    {
        Header,
        Content,
    }

    private readonly ILogger _logger;
    private readonly ISettingsProvider _settingsProvider;
    private readonly IClipboard _clipboard;

    private readonly IUIMultiLineTextInput _inputTextArea = MultilineTextInput("json-input-text-area");
    private readonly IUIDataGrid _outputDataGrid = DataGrid("output-data-grid");
    private readonly IUIButton _copyButton = Button("copy-data-grid");

    private readonly DisposableSemaphore _semaphore = new(); // one convert at a time
    private CancellationTokenSource? _cancellationTokenSource;

    [ImportingConstructor]
    public JsonTableConverterGuiTool(ISettingsProvider settingsProvider, IClipboard clipboard)
    {
        _logger = this.Log();
        _settingsProvider = settingsProvider;
        _clipboard = clipboard;
    }

    internal Task? WorkTask { get; private set; }

    public UIToolView View
    {
        get
        {
            _copyButton
                .Icon("FluentSystemIcons", '\uF32B')
                .OnClick(OnCopyDataGrid)
                .Disable(); // disable until valid JSON is input

            return new(
                isScrollable: false,
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
                            .Text(JsonTableConverter.Configuration),

                            Setting("clipboard-copy-format-setting")
                            .Icon("FluentSystemIcons", '\uF7ED')
                            .Title(JsonTableConverter.ClipboardFormatTitle)
                            .Description(JsonTableConverter.ClipboardFormatDescription)
                            .Handle(
                                _settingsProvider,
                                copyFormat,
                                null,
                                Item(JsonTableConverter.CopyFormatTSV, CopyFormat.TSV),
                                Item(JsonTableConverter.CopyFormatCSV, CopyFormat.CSV),
                                Item(JsonTableConverter.CopyFormatFSV, CopyFormat.FSV)
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
                                    .Title(JsonTableConverter.Input)
                                    .Language("json")
                                    .OnTextChanged(OnInputTextChanged))
                            .WithRightPaneChild(
                                _outputDataGrid
                                    .Title(JsonTableConverter.Output)
                                    .CommandBarExtraContent(_copyButton)
                                    .Extendable())
                    )
                )
            );
        }
    }

    // Smart detection handler.
    public void OnDataReceived(string dataTypeName, object? parsedData)
    {
        if (dataTypeName == PredefinedCommonDataTypeNames.JsonArray &&
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

    private void OnInputTextChanged(string text)
    {
        StartConvert(ConvertTarget.DataGrid);
    }

    private void StartConvert(ConvertTarget target)
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();

        WorkTask = ConvertAsync(target, _cancellationTokenSource.Token);
    }

    private async Task ConvertAsync(ConvertTarget target, CancellationToken cancellationToken)
    {
        using (await _semaphore.WaitAsync(cancellationToken))
        {
            await TaskSchedulerAwaiter.SwitchOffMainThreadAsync(cancellationToken);

            ConvertResult conversionResult = ConvertFromJson(_inputTextArea.Text, _settingsProvider.GetSetting(copyFormat), cancellationToken);

            if (conversionResult.Error == ConvertResultError.None && conversionResult.Data != null)
            {
                _copyButton.Enable();
                switch (target)
                {
                    case ConvertTarget.DataGrid:
                        SetDataGridData(conversionResult.Data);
                        break;

                    case ConvertTarget.Clipboard:
                        await _clipboard.SetClipboardTextAsync(conversionResult.Text);
                        break;
                }
            }
            else
            {
                _copyButton.Disable();
                SetDataGridError();
            }
        }
    }

    private enum ConvertTarget { DataGrid, Clipboard }

    private void SetDataGridData(DataGridContents table)
    {
        IUIDataGridRow[] rows = table.Rows.Select(r => Row(null, r)).ToArray();
        _outputDataGrid.WithColumns(table.Headings);
        _outputDataGrid.WithRows(rows);
    }

    private void SetDataGridError()
    {
        _outputDataGrid.WithColumns(JsonTableConverter.JsonError);
        _outputDataGrid.WithRows();
    }

    private ValueTask OnCopyDataGrid()
    {
        StartConvert(ConvertTarget.Clipboard);
        return ValueTask.CompletedTask;
    }
}
