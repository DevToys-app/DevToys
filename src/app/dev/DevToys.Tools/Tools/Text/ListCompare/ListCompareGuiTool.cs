using DevToys.Tools.Helpers;
using Microsoft.Extensions.Logging;
using static System.Net.Mime.MediaTypeNames;

namespace DevToys.Tools.Tools.Text.ListCompare;

[Export(typeof(IGuiTool))]
[Name("ListCompare")]
[ToolDisplayInformation(
    IconFontName = "DevToys-Tools-Icons",
    IconGlyph = '\u0137',
    GroupName = PredefinedCommonToolGroupNames.Text,
    ResourceManagerAssemblyIdentifier = nameof(DevToysToolsResourceManagerAssemblyIdentifier),
    ResourceManagerBaseName = "DevToys.Tools.Tools.Text.ListCompare.ListCompare",
    ShortDisplayTitleResourceName = nameof(ListCompare.ShortDisplayTitle),
    LongDisplayTitleResourceName = nameof(ListCompare.LongDisplayTitle),
    DescriptionResourceName = nameof(ListCompare.Description),
    AccessibleNameResourceName = nameof(ListCompare.AccessibleName))]
internal sealed class ListCompareGuiTool : IGuiTool
{
    /// <summary>
    /// Whether the list comparison should be case sensitive or not,
    /// </summary>
    private static readonly SettingDefinition<bool> caseSensitive
        = new(
            name: $"{nameof(ListCompareGuiTool)}.{nameof(caseSensitive)}",
            defaultValue: false);

    private CancellationTokenSource? _cancellationTokenSource;

    private enum GridRows
    {
        Configuration,
        Text,
    }

    private enum GridColumns
    {
        Stretch
    }

    private string _listA = string.Empty;
    private string _listB = string.Empty;

    private readonly DisposableSemaphore _semaphore = new();
    private readonly ILogger _logger;
    private readonly ISettingsProvider _settingsProvider;
    private readonly IUIDiffListInput _diffListInputAInterB = DiffListInput("text-compare-diff-list-input-AInterB");
    private readonly IUIDiffListInput _diffListInputAOnly = DiffListInput("text-compare-diff-list-input-AOnly");
    private readonly IUIDiffListInput _diffListInputBOnly = DiffListInput("text-compare-diff-list-input-BOnly");

    [ImportingConstructor]
    public ListCompareGuiTool(ISettingsProvider settingsProvider)
    {
        _logger = this.Log();
        _settingsProvider = settingsProvider;

        if (_settingsProvider.GetSetting(caseSensitive))
        {
            _diffListInputAInterB.InlineView();
            _diffListInputAOnly.InlineView();
            _diffListInputBOnly.InlineView();
        }
        else
        {
            _diffListInputAInterB.SplitView();
            _diffListInputAOnly.SplitView();
            _diffListInputBOnly.SplitView();
        }
    }

    // For unit tests.
    internal Task? WorkTask { get; private set; }

    public UIToolView View
        => new(
            isScrollable: true,
            Grid()
                .ColumnMediumSpacing()

                .Rows(
                    (GridRows.Configuration, Auto),
                    (GridRows.Text, new UIGridLength(1, UIGridUnitType.Fraction)))

                .Columns(
                    (GridColumns.Stretch, new UIGridLength(1, UIGridUnitType.Fraction)))

                .Cells(
                    Cell(
                        GridRows.Configuration,
                        GridColumns.Stretch,

                        Stack()
                            .Vertical()

                            .WithChildren(
                                Label()
                                    .Text(ListCompare.Configuration),

                                Setting("list-compare-case_insensitive_comparison")
                                    .Icon("FluentSystemIcons", '\uEB59')
                                    .Title(ListCompare.TextCaseSensitiveComparison)
                                    .Handle(_settingsProvider, caseSensitive, OnCaseSensitiveChanged))),

                    Cell(
                        GridRows.Text,
                        GridColumns.Stretch,

                        SplitGrid()
                            .Horizontal()
                            .TopPaneLength(new UIGridLength(1, UIGridUnitType.Fraction))
                            .BottomPaneLength(new UIGridLength(2, UIGridUnitType.Fraction))

                            .WithTopPaneChild(
                                SplitGrid()
                                    .Vertical()
                                    .WithLeftPaneChild(
                                        MultilineTextInput()
                                            .Title(ListCompare.ListA)
                                            .OnTextChanged(OnListAChanged))


                                    .WithRightPaneChild(
                                        MultilineTextInput()
                                            .Title(ListCompare.ListB)
                                            .OnTextChanged(OnListBChanged)))

                            .WithBottomPaneChild(
                                SplitGrid()
                                    .Vertical()

                                    .WithLeftPaneChild(
                                        _diffListInputAInterB
                                            .Title(ListCompare.AInterB)
                                            .CompareModeAInterB()
                                            .ReadOnly()
                                            .Extendable())
                                    .WithRightPaneChild(
                                        SplitGrid()
                                        .Vertical()
                                         .WithLeftPaneChild(
                                            _diffListInputAOnly
                                                .Title(ListCompare.AOnly)
                                                .CompareModeAOnly()
                                                .ReadOnly()
                                                .Extendable())

                                        .WithRightPaneChild(
                                            _diffListInputBOnly
                                            .Title(ListCompare.BOnly)
                                            .CompareModeBOnly()
                                            .ReadOnly()
                                            .Extendable())
                                    )
                            ))));

    public void OnDataReceived(string dataTypeName, object? parsedData)
    {
    }

    private ValueTask OnCaseSensitiveChanged(bool caseSensitive)
    {
        StartCompare();
        return ValueTask.CompletedTask;
    }

    private ValueTask OnListAChanged(string listA)
    {
        _listA = listA;
        StartCompare();
        return ValueTask.CompletedTask;
    }

    private ValueTask OnListBChanged(string listB)
    {
        _listB = listB;
        StartCompare();
        return ValueTask.CompletedTask;
    }


    private void StartCompare()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();

        WorkTask = CompareAsync(_listA, _listB, _settingsProvider.GetSetting(caseSensitive), _cancellationTokenSource.Token);
    }

    private async Task CompareAsync(string listA, string listB, bool caseSensitive, CancellationToken cancellationToken)
    {
        using (await _semaphore.WaitAsync(cancellationToken))
        {
            await TaskSchedulerAwaiter.SwitchOffMainThreadAsync(cancellationToken);

            ResultInfo<string> compareResultAInterB = await ListCompareHelper.CompareAsync(
                listA,
                listB,
                caseSensitive,
                ListComparisonMode.AInterB,
                _logger,
                cancellationToken);

            ResultInfo<string> compareResultAOnly = await ListCompareHelper.CompareAsync(
                listA,
                listB,
                caseSensitive,
                ListComparisonMode.AOnly,
                _logger,
                cancellationToken);


            ResultInfo<string> compareResultBOnly = await ListCompareHelper.CompareAsync(
                listA,
                listB,
                caseSensitive,
                ListComparisonMode.BOnly,
                _logger,
                cancellationToken);


            cancellationToken.ThrowIfCancellationRequested();
            _diffListInputAInterB.Text(compareResultAInterB.Data);
            _diffListInputAOnly.Text(compareResultAOnly.Data);
            _diffListInputBOnly.Text(compareResultBOnly.Data);
        }
    }

}
