using System.Data;
using DevToys.Tools.Tools.Converters.JsonTable;
using Microsoft.Extensions.Logging;
using static DevToys.Tools.Helpers.JsonTableHelper;

namespace DevToys.Tools.Tools.Converters.JsonYaml;

[Export(typeof(IGuiTool))]
[Name("JsonTableConverter")]
[ToolDisplayInformation(
    IconFontName = "FluentSystemIcons",
    IconGlyph = '\uF0D8',
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
    private readonly ILogger _logger;
    private readonly IClipboard _clipboard;
    private readonly IFileStorage _fileStorage;

    private readonly IUIMultiLineTextInput _inputTextArea = MultilineTextInput("json-input-text-area");
    private readonly IUIDataGrid _outputDataGrid = DataGrid("output-data-grid");
    private readonly IUIStack _copyOrSaveStack = Stack("copy-or-save-data-grid");

    private readonly DisposableSemaphore _semaphore = new(); // one convert at a time
    private CancellationTokenSource? _cancellationTokenSource;

    [ImportingConstructor]
    public JsonTableConverterGuiTool(IClipboard clipboard, IFileStorage fileStorage)
    {
        _logger = this.Log();
        _clipboard = clipboard;
        _fileStorage = fileStorage;
    }

    internal Task? WorkTask { get; private set; }

    public UIToolView View
        => new(
            isScrollable: false,
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
                        .Extendable()
                        .CommandBarExtraContent(
                            _copyOrSaveStack
                                .Horizontal()
                                .Disable() // disable until valid JSON is input
                                .WithChildren(

                                    DropDownButton()
                                        .Icon("FluentSystemIcons", '\uF32B')
                                        .Text(JsonTableConverter.CopyAs)
                                        .WithMenuItems(
                                            DropDownMenuItem(JsonTableConverter.CopyFormatCSV).OnClick(OnCopyCSV),
                                            DropDownMenuItem(JsonTableConverter.CopyFormatTSV).OnClick(OnCopyTSV),
                                            DropDownMenuItem(JsonTableConverter.CopyFormatFSV).OnClick(OnCopyFSV)),

                                    DropDownButton()
                                        .Icon("FluentSystemIcons", '\uF67F')
                                        .Text(JsonTableConverter.SaveAs)
                                        .WithMenuItems(
                                            DropDownMenuItem(JsonTableConverter.CopyFormatCSV).OnClick(OnSaveCSV),
                                            DropDownMenuItem(JsonTableConverter.CopyFormatTSV).OnClick(OnSaveTSV),
                                            DropDownMenuItem(JsonTableConverter.CopyFormatFSV).OnClick(OnSaveFSV))))));

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
        StartConvert(ConvertTarget.DataGrid, null);
    }

    private void StartConvert(ConvertTarget target, CopyFormat? copyFormat)
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();

        WorkTask = ConvertAsync(target, copyFormat, _cancellationTokenSource.Token);
    }

    private async Task ConvertAsync(ConvertTarget target, CopyFormat? copyFormat, CancellationToken cancellationToken)
    {
        using (await _semaphore.WaitAsync(cancellationToken))
        {
            await TaskSchedulerAwaiter.SwitchOffMainThreadAsync(cancellationToken);

            if (target == ConvertTarget.Clipboard || target == ConvertTarget.Save)
            {
                Guard.IsNotNull(copyFormat);
            }

            ConvertResult conversionResult = ConvertFromJson(_inputTextArea.Text, copyFormat, cancellationToken);

            if (conversionResult.Error == ConvertResultError.None && conversionResult.Data != null)
            {
                _copyOrSaveStack.Enable();
                switch (target)
                {
                    case ConvertTarget.DataGrid:
                        SetDataGridData(conversionResult.Data);
                        break;

                    case ConvertTarget.Clipboard:
                        await _clipboard.SetClipboardTextAsync(conversionResult.Text);
                        break;

                    case ConvertTarget.Save:
                        {
                            using Stream? fileStream = await _fileStorage.PickSaveFileAsync("txt", "csv");
                            if (fileStream is not null)
                            {
                                using var writer = new StreamWriter(fileStream);
                                writer.Write(conversionResult.Text);
                            }
                        }
                        break;
                }
            }
            else
            {
                _copyOrSaveStack.Disable();
                SetDataGridError();
            }
        }
    }

    private enum ConvertTarget
    {
        DataGrid,
        Clipboard,
        Save
    }

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

    private ValueTask OnCopyCSV()
    {
        StartConvert(ConvertTarget.Clipboard, CopyFormat.CSV);
        return ValueTask.CompletedTask;
    }

    private ValueTask OnCopyTSV()
    {
        StartConvert(ConvertTarget.Clipboard, CopyFormat.TSV);
        return ValueTask.CompletedTask;
    }

    private ValueTask OnCopyFSV()
    {
        StartConvert(ConvertTarget.Clipboard, CopyFormat.FSV);
        return ValueTask.CompletedTask;
    }

    private ValueTask OnSaveCSV()
    {
        StartConvert(ConvertTarget.Save, CopyFormat.CSV);
        return ValueTask.CompletedTask;
    }

    private ValueTask OnSaveTSV()
    {
        StartConvert(ConvertTarget.Save, CopyFormat.TSV);
        return ValueTask.CompletedTask;
    }

    private ValueTask OnSaveFSV()
    {
        StartConvert(ConvertTarget.Save, CopyFormat.FSV);
        return ValueTask.CompletedTask;
    }
}
