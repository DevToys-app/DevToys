using Microsoft.Extensions.Logging;

namespace DevToys.Tools.Tools.Text.Compare;

[Export(typeof(IGuiTool))]
[Name("TextCompare")]
[ToolDisplayInformation(
    IconFontName = "DevToys-Tools-Icons",
    IconGlyph = '\u0115',
    GroupName = PredefinedCommonToolGroupNames.Text,
    ResourceManagerAssemblyIdentifier = nameof(DevToysToolsResourceManagerAssemblyIdentifier),
    ResourceManagerBaseName = "DevToys.Tools.Tools.Text.Compare.TextCompare",
    ShortDisplayTitleResourceName = nameof(TextCompare.ShortDisplayTitle),
    LongDisplayTitleResourceName = nameof(TextCompare.LongDisplayTitle),
    DescriptionResourceName = nameof(TextCompare.Description),
    SearchKeywordsResourceName = nameof(TextCompare.SearchKeywords),
    AccessibleNameResourceName = nameof(TextCompare.AccessibleName))]
internal sealed class TextCompareGuiTool : IGuiTool
{
    /// <summary>
    /// Whether the text comparison should be inline or split,
    /// </summary>
    private static readonly SettingDefinition<bool> inline
        = new(
            name: $"{nameof(TextCompareGuiTool)}.{nameof(inline)}",
            defaultValue: false);

    private enum GridRows
    {
        Configuration,
        Text,
    }

    private enum GridColumns
    {
        Stretch
    }

    private readonly ILogger _logger;
    private readonly ISettingsProvider _settingsProvider;
    private readonly IUIDiffTextInput _diffTextInput = DiffTextInput("text-compare-diff-text-input");

    [ImportingConstructor]
    public TextCompareGuiTool(ISettingsProvider settingsProvider)
    {
        _logger = this.Log();
        _settingsProvider = settingsProvider;

        if (_settingsProvider.GetSetting(inline))
        {
            _diffTextInput.InlineView();
        }
        else
        {
            _diffTextInput.SplitView();
        }
    }

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
                                    .Text(TextCompare.Configuration),

                                Setting("text-compare-inline-setting")
                                    .Icon("FluentSystemIcons", '\uEB59')
                                    .Title(TextCompare.InlineDifference)
                                    .Handle(_settingsProvider, inline, OnInlineChanged))),

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
                                            .Title(TextCompare.OriginalText)
                                            .OnTextChanged(OnOriginalTextChanged))

                                    .WithRightPaneChild(
                                        MultilineTextInput()
                                            .Title(TextCompare.ModifiedText)
                                            .OnTextChanged(OnModifiedTextChanged)))

                            .WithBottomPaneChild(
                                _diffTextInput
                                    .Title(TextCompare.Difference)
                                    .ReadOnly()
                                    .Extendable()))));

    public void OnDataReceived(string dataTypeName, object? parsedData)
    {
    }

    private ValueTask OnInlineChanged(bool inline)
    {
        if (inline)
        {
            _diffTextInput.InlineView();
        }
        else
        {
            _diffTextInput.SplitView();
        }

        return ValueTask.CompletedTask;
    }

    private ValueTask OnOriginalTextChanged(string originalText)
    {
        _diffTextInput.OriginalText(originalText);
        return ValueTask.CompletedTask;
    }

    private ValueTask OnModifiedTextChanged(string modifiedText)
    {
        _diffTextInput.ModifiedText(modifiedText);
        return ValueTask.CompletedTask;
    }
}
