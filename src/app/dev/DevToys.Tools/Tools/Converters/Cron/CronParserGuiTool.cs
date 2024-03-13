using DevToys.Tools.Helpers;
using DevToys.Tools.Models;

namespace DevToys.Tools.Tools.Converters.Cron;

[Export(typeof(IGuiTool))]
[Name("CronParser")]
[ToolDisplayInformation(
    IconFontName = "DevToys-Tools-Icons",
    IconGlyph = '\u0104',
    GroupName = PredefinedCommonToolGroupNames.Converters,
    ResourceManagerAssemblyIdentifier = nameof(DevToysToolsResourceManagerAssemblyIdentifier),
    ResourceManagerBaseName = "DevToys.Tools.Tools.Converters.Cron.CronParser",
    ShortDisplayTitleResourceName = nameof(CronParser.ShortDisplayTitle),
    LongDisplayTitleResourceName = nameof(CronParser.LongDisplayTitle),
    DescriptionResourceName = nameof(CronParser.Description),
    AccessibleNameResourceName = nameof(CronParser.AccessibleName))]
internal sealed class CronParserGuiTool : IGuiTool
{
    private const string DefaultCronWithSeconds = "* * * * * *";
    private const string DefaultCronWithoutSeconds = "* * * * *";

    /// <summary>
    /// Whether the tool should include seconds in Cron definition
    /// </summary>
    internal static readonly SettingDefinition<bool> includeSeconds
        = new(
            name: $"{nameof(CronParserGuiTool)}.{nameof(includeSeconds)}",
            defaultValue: true);

    /// <summary>
    /// How many lines of next occurrences the tool should generate
    /// </summary>
    private static readonly SettingDefinition<CronScheduleCount> outputCount
        = new(
            name: $"{nameof(CronParserGuiTool)}.{nameof(outputCount)}",
            defaultValue: CronScheduleCount.Five);

    private enum GridColumn
    {
        Stretch
    }

    private enum GridRow
    {
        Settings,
        Results
    }

    private readonly ISettingsProvider _settingsProvider;
    private readonly IUISingleLineTextInput _dateFormatText = SingleLineTextInput("cron-parser-date-format");
    private readonly IUISingleLineTextInput _cronExpressionText = SingleLineTextInput("cron-parser-cron-expression");
    private readonly IUISingleLineTextInput _outputCronDescriptionText = SingleLineTextInput("cron-parser-output-description");
    private readonly IUIMultiLineTextInput _outputScheduleText = MultilineTextInput("cron-parser-output-schedule");
    private readonly IUIInfoBar _infoBar = InfoBar("cron-parser-info-bar");

    [ImportingConstructor]
    public CronParserGuiTool(ISettingsProvider settingsProvider)
    {
        _settingsProvider = settingsProvider;

        if (_settingsProvider.GetSetting(includeSeconds))
        {
            _cronExpressionText.Text(DefaultCronWithSeconds);
        }
        else
        {
            _cronExpressionText.Text(DefaultCronWithoutSeconds);
        }

        _dateFormatText.Text("yyyy-MM-dd ddd HH:mm:ss");

        ParseCronExpression();
    }

    public UIToolView View
        => new(
            isScrollable: true,
            Grid()
                .ColumnLargeSpacing()
                .RowLargeSpacing()
                .Rows(
                    (GridRow.Settings, Auto),
                    (GridRow.Results, new UIGridLength(1, UIGridUnitType.Fraction)))
                .Columns(
                    (GridColumn.Stretch, new UIGridLength(1, UIGridUnitType.Fraction)))

            .Cells(
                Cell(
                    GridRow.Settings,
                    GridColumn.Stretch,
                    Stack()
                        .Vertical()
                        .LargeSpacing()
                        .WithChildren(

                            Stack()
                                .Vertical()
                                .WithChildren(

                                    Label().Text(CronParser.ConfigurationTitle),

                                    Setting()
                                        .Icon("FluentSystemIcons", '\uF18D')
                                        .Title(CronParser.UseSecondsTitle)
                                        .Description(CronParser.UseSecondsDescription)
                                        .Handle(
                                            _settingsProvider,
                                            includeSeconds,
                                            OnSettingChanged),

                                    Setting()
                                        .Icon("FluentSystemIcons", '\uf7ed')
                                        .Title(CronParser.OutputCountTitle)
                                        .Description(CronParser.OutputCountDescription)
                                        .Handle(
                                            _settingsProvider,
                                            outputCount,
                                            OnSettingChanged,
                                            Item("5", CronScheduleCount.Five),
                                            Item("10", CronScheduleCount.Ten),
                                            Item("25", CronScheduleCount.TwentyFive),
                                            Item("50", CronScheduleCount.Fifty),
                                            Item("100", CronScheduleCount.OneHundred)),

                                    Setting()
                                        .Icon("FluentSystemIcons", '\uF7E4')
                                        .Title(CronParser.OutputFormatTitle)
                                        .Description(CronParser.OutputFormatDescription)
                                        .InteractiveElement(
                                            _dateFormatText
                                                .HideCommandBar()
                                                .OnTextChanged(OnCronExpressionOrDateFormatChanged))),

                            _cronExpressionText
                                .Title(CronParser.InputTitle)
                                .CanCopyWhenEditable()
                                .OnTextChanged(OnCronExpressionOrDateFormatChanged),

                            _infoBar
                                .NonClosable()
                                .Warning()
                                .Close(),

                            _outputCronDescriptionText
                                .Title(CronParser.CronDescriptionTitle)
                                .ReadOnly())),

                Cell(
                    GridRow.Results,
                    GridColumn.Stretch,

                    _outputScheduleText
                        .Title(CronParser.OutputScheduleTitle)
                        .ReadOnly())));

    public void OnDataReceived(string dataTypeName, object? parsedData)
    {
    }

    private void OnSettingChanged(bool _)
    {
        OnSettingChanged();
    }

    private void OnSettingChanged(CronScheduleCount value)
    {
        OnSettingChanged();
    }

    private void OnCronExpressionOrDateFormatChanged(string _)
    {
        OnSettingChanged();
    }

    private void OnSettingChanged()
    {
        ParseCronExpression();
    }

    private void ParseCronExpression()
    {
        string cronExpression = _cronExpressionText.Text;
        string dateFormat = _dateFormatText.Text;
        bool seconds = _settingsProvider.GetSetting(includeSeconds);
        int maxScheduleCount = GetOutputCount();

        bool succeeded
            = CronHelper.TryGenerateNextSchedulesAndDescription(
                cronExpression,
                dateFormat,
                seconds,
                maxScheduleCount,
                out string schedule,
                out string description,
                out string error);

        _outputCronDescriptionText.Text(description);
        _outputScheduleText.Text(schedule);

        if (succeeded)
        {
            _infoBar.Close();
        }
        else
        {
            _infoBar.Title(error);
            _infoBar.Open();
        }
    }

    private int GetOutputCount()
    {
        return _settingsProvider.GetSetting(outputCount) switch
        {
            CronScheduleCount.Five => 5,
            CronScheduleCount.Ten => 10,
            CronScheduleCount.TwentyFive => 25,
            CronScheduleCount.Fifty => 50,
            CronScheduleCount.OneHundred => 100,
            _ => throw new NotSupportedException(),
        };
    }
}
