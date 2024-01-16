using DevToys.Tools.Helpers;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DevToys.Tools.Tools.Testers.JsonPathTester;

[Export(typeof(IGuiTool))]
[Name("JSONPathTester")]
[ToolDisplayInformation(
    IconFontName = "DevToys-Tools-Icons",
    IconGlyph = '\u0108', // TODO: Create an icon for this tool
    GroupName = PredefinedCommonToolGroupNames.Testers,
    ResourceManagerAssemblyIdentifier = nameof(DevToysToolsResourceManagerAssemblyIdentifier),
    ResourceManagerBaseName = "DevToys.Tools.Tools.Testers.JsonPathTester.JsonPathTester",
    ShortDisplayTitleResourceName = nameof(JsonPathTester.ShortDisplayTitle),
    LongDisplayTitleResourceName = nameof(JsonPathTester.LongDisplayTitle),
    DescriptionResourceName = nameof(JsonPathTester.Description),
    AccessibleNameResourceName = nameof(JsonPathTester.AccessibleName),
    SearchKeywordsResourceName = nameof(JsonPathTester.SearchKeywords))]
[AcceptedDataTypeName(PredefinedCommonDataTypeNames.Json)]
internal sealed class JsonPathTesterGuiTool : IGuiTool, IDisposable
{
    private static readonly IUIDataGridRow[] cheatSheetRows
        = new[]
        {
            CreateCheatSheetRow(@"$", JsonPathTester.CheatSheetRootObjectElement),
            CreateCheatSheetRow(@"@", JsonPathTester.CheatSheetCurrentObjectElement),
            CreateCheatSheetRow(@"object.property", JsonPathTester.CheatSheetDotNotedChildOperator),
            CreateCheatSheetRow(@"['object'].['property']", JsonPathTester.CheatSheetBracketNotedChildOperator),
            CreateCheatSheetRow(@"..property", JsonPathTester.CheatSheetRecursiveDescent),
            CreateCheatSheetRow(@"*", JsonPathTester.CheatSheetWildcard),
            CreateCheatSheetRow(@"[n]", JsonPathTester.CheatSheetSubscriptOperator),
            CreateCheatSheetRow(@"[n1,n2]", JsonPathTester.CheatSheetUnionOperator),
            CreateCheatSheetRow(@"[start:end:step]", JsonPathTester.CheatSheetArraySliceOperator),
            CreateCheatSheetRow(@"?(expression)", JsonPathTester.CheatSheetFilterExpression),
            CreateCheatSheetRow(@"(expression)", JsonPathTester.CheatSheetScriptExpression)
        };

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
    private readonly ILogger _logger;
    private readonly ISettingsProvider _settingsProvider;
    private readonly IUIMultiLineTextInput _inputJson = MultilineTextInput("input-json-json-path-tester");
    private readonly IUISingleLineTextInput _inputJsonPath = SingleLineTextInput("input-json-path-json-path-tester");
    private readonly IUIMultiLineTextInput _outputJson = MultilineTextInput("output-json-path-tester");

    private CancellationTokenSource? _cancellationTokenSource;
    private string _lastTreatedInputJson = string.Empty;
    private JObject? _inputJsonObjectBackup = null;

    [ImportingConstructor]
    public JsonPathTesterGuiTool(ISettingsProvider settingsProvider)
    {
        _logger = this.Log();
        _settingsProvider = settingsProvider;
    }

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
                    _inputJson
                        .Title(JsonPathTester.InputTitle)
                        .Language("json")
                        .OnTextChanged(OnInputJsonChanged))

                .WithRightPaneChild(
                    SplitGrid()
                        .Horizontal()

                        .WithTopPaneChild(
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

                                        _inputJsonPath
                                            .Title(JsonPathTester.InputJsonPathTitle)
                                            .OnTextChanged(OnInputJsonPathChanged)),

                                    Cell(
                                        GridRows.Stretch,
                                        GridColumns.Stretch,

                                        _outputJson
                                            .ReadOnly()
                                            .Extendable()
                                            .Language("json")
                                            .Title(JsonPathTester.Output))))

                        .WithBottomPaneChild(
                            DataGrid()
                                .Title(JsonPathTester.CheatSheetTitle)
                                .Extendable()
                                .WithColumns(JsonPathTester.CheatSheetSyntax, JsonPathTester.CheatSheetDescription)
                                .WithRows(cheatSheetRows))));

    public void OnDataReceived(string dataTypeName, object? parsedData)
    {
        if (dataTypeName == PredefinedCommonDataTypeNames.Json && parsedData is string json)
        {
            _inputJson.Text(json);
        }
    }

    public void Dispose()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _semaphore.Dispose();
    }

    private void OnInputJsonChanged(string json)
    {
        StartTest();
    }

    private void OnInputJsonPathChanged(string jsonPath)
    {
        StartTest();
    }

    private void StartTest()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();

        WorkTask = TestJsonPathAsync(_inputJson.Text, _inputJsonPath.Text, _cancellationTokenSource.Token);
    }

    private async Task TestJsonPathAsync(string json, string jsonPath, CancellationToken cancellationToken)
    {
        using (await _semaphore.WaitAsync(cancellationToken))
        {
            await TaskSchedulerAwaiter.SwitchOffMainThreadAsync(cancellationToken);

            string result = JsonPathTester.NoMatchFound;
            try
            {
                if (!string.IsNullOrWhiteSpace(jsonPath))
                {
                    if (string.Equals(_lastTreatedInputJson, json, StringComparison.Ordinal))
                    {
                        // We can reuse the previous JObject. Let's avoid parsing it too often to improve performance.
                        _inputJsonObjectBackup ??= JObject.Parse(json);
                    }
                    else
                    {
                        // We need to parse the new JObject.
                        _inputJsonObjectBackup = JObject.Parse(json);
                        _lastTreatedInputJson = json;
                    }

                    cancellationToken.ThrowIfCancellationRequested();

                    JObject jsonObject = _inputJsonObjectBackup;

                    ResultInfo<string> resultInfo = JsonHelper.TestJsonPath(jsonObject, jsonPath, _logger, cancellationToken);
                    if (resultInfo.HasSucceeded)
                    {
                        result = resultInfo.Data;
                    }
                }
            }
            catch (Exception ex) when (ex is OperationCanceledException or JsonReaderException)
            {
                // Ignore
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while testing JsonPath.");
            }
            finally
            {
                _outputJson.Text(result);
            }
        }
    }

    private static IUIDataGridRow CreateCheatSheetRow(string syntax, string description)
        => Row(
            null,
            details: null,
            Cell(
                Label()
                    .NeverWrap()
                    .Style(UILabelStyle.BodyStrong)
                    .Text($"\t{syntax}\t")),
            Cell(
                Label()
                    .NeverWrap()
                    .Text(description)));
}
