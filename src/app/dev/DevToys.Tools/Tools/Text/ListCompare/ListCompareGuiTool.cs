using System.Security.Authentication;
using System.Security.Cryptography;
using DevToys.Api;
using DevToys.Tools.Helpers;
using DevToys.Tools.Models;
using DevToys.Tools.Tools.EncodersDecoders.Base64Text;
using DevToys.Tools.Tools.Generators.HashAndChecksum;
using Microsoft.Extensions.Logging;

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

    /// <summary>
    /// The comparison mode to use.
    /// </summary>
    private static readonly SettingDefinition<ListComparisonMode> comparisonMode
        = new(
            name: $"{nameof(ListCompareGuiTool)}.{nameof(comparisonMode)}",
            defaultValue: ListComparisonMode.AInterB);

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
    private readonly object _lock = new();
    private readonly ILogger _logger;
    private readonly ISettingsProvider _settingsProvider;
    private readonly IUIMultiLineTextInput _diffListResult = MultilineTextInput("text-compare-diff-list-result");

    [ImportingConstructor]
    public ListCompareGuiTool(ISettingsProvider settingsProvider)
    {
        _logger = this.Log();
        _settingsProvider = settingsProvider;
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
                                        .Handle(_settingsProvider, caseSensitive, OnCaseSensitiveChanged),

                                    Setting("list-compare-comparison-mode")
                                        .Icon("FluentSystemIcons", '\uF1EE')
                                        .Title(ListCompare.ComparisonOptionTitle)
                                        .Handle(
                                            _settingsProvider,
                                            comparisonMode,
                                            onOptionSelected: OnComparisonModeChanged,
                                            Item(ListCompare.AInterB, ListComparisonMode.AInterB),
                                            Item(ListCompare.AUnionB, ListComparisonMode.AUnionB),
                                            Item(ListCompare.AOnly, ListComparisonMode.AOnly),
                                            Item(ListCompare.BOnly, ListComparisonMode.BOnly)
                                )
                            )
                        ),


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
                                _diffListResult
                                    .ReadOnly()
                                    .Extendable())
                            )));

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

    private void OnComparisonModeChanged(ListComparisonMode col)
    {
        StartCompare();
    }

    private void StartCompare()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();

        WorkTask = CompareAsync(_listA, _listB, _settingsProvider.GetSetting(caseSensitive), _settingsProvider.GetSetting(comparisonMode), _cancellationTokenSource.Token);

        String selectedComparisonModeTitle = string.Empty;
        switch (_settingsProvider.GetSetting(comparisonMode))
        {
            case ListComparisonMode.AInterB:
                selectedComparisonModeTitle = ListCompare.AInterB; break;
            case ListComparisonMode.AUnionB:
                selectedComparisonModeTitle = ListCompare.AUnionB; break;
            case ListComparisonMode.AOnly:
                selectedComparisonModeTitle = ListCompare.AOnly; break;
            case ListComparisonMode.BOnly:
                selectedComparisonModeTitle = ListCompare.BOnly; break;
        }
        _diffListResult.Title(selectedComparisonModeTitle);
    }

    private async Task CompareAsync(string listA, string listB, bool caseSensitive, ListComparisonMode listComparisonMode, CancellationToken cancellationToken)
    {
        using (await _semaphore.WaitAsync(cancellationToken))
        {
            ResultInfo<string> result = ListCompareHelper.Compare(
                listA,
                listB,
                caseSensitive,
                listComparisonMode,
                _logger);

            lock (_lock)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                Guard.IsNotNull(result);
                _diffListResult.Text(result.Data);
            }
        }
    }
}
