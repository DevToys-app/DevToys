using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using DevToys.Tools.Helpers;
using DevToys.Tools.Models;
using Microsoft.Extensions.Logging;

namespace DevToys.Tools.Tools.Converters.Date;

[Export(typeof(IGuiTool))]
[Name("DateConverter")]
[ToolDisplayInformation(
    IconFontName = "FluentSystemIcons",
    IconGlyph = '\uE250',
    GroupName = PredefinedCommonToolGroupNames.Converters,
    ResourceManagerAssemblyIdentifier = nameof(DevToysToolsResourceManagerAssemblyIdentifier),
    ResourceManagerBaseName = "DevToys.Tools.Tools.Converters.Date.DateConverter",
    ShortDisplayTitleResourceName = nameof(DateConverter.ShortDisplayTitle),
    LongDisplayTitleResourceName = nameof(DateConverter.LongDisplayTitle),
    DescriptionResourceName = nameof(DateConverter.Description),
    SearchKeywordsResourceName = nameof(DateConverter.SearchKeywords),
    AccessibleNameResourceName = nameof(DateConverter.AccessibleName))]
[AcceptedDataTypeName(PredefinedCommonDataTypeNames.Date)]
internal sealed partial class DateConverterGuiTool : IGuiTool, IDisposable
{
    /// <summary>
    /// Whether the tool should use the custom Epoch.
    /// </summary>
    private static readonly SettingDefinition<bool> useCustomEpochSettings
       = new(
           name: $"{nameof(DateConverterGuiTool)}.{nameof(useCustomEpochSettings)}",
           defaultValue: false);

    /// <summary>
    /// The Epoch to use.
    /// </summary>
    private static readonly SettingDefinition<DateTimeOffset> customEpochSettings
        = new(
            name: $"{nameof(DateConverterGuiTool)}.{nameof(customEpochSettings)}",
            defaultValue: DateTime.UnixEpoch);

    /// <summary>
    /// The DateTime to use.
    /// </summary>
    private static readonly SettingDefinition<DateTimeOffset> currentTimeSettings
        = new(
            name: $"{nameof(DateConverterGuiTool)}.{nameof(currentTimeSettings)}",
            defaultValue: DateTime.UtcNow);

    /// <summary>
    /// The timeZone to use.
    /// </summary>
    private static readonly SettingDefinition<string> timeZoneIdSettings
        = new(
            name: $"{nameof(DateConverterGuiTool)}.{nameof(timeZoneIdSettings)}",
            defaultValue: TimeZoneInfo.Local.Id);

    /// <summary>
    /// The Format to use.
    /// </summary>
    private static readonly SettingDefinition<DateFormat> formatSettings
        = new(
            name: $"{nameof(DateConverterGuiTool)}.{nameof(formatSettings)}",
            defaultValue: DateFormat.Seconds);

    private bool _ignoreInputTextChange;

    private readonly ILogger _logger;

    private readonly DisposableSemaphore _semaphore = new();

    private readonly ISettingsProvider _settingsProvider;

    private IUIDropDownListItem SelectedTimeZoneDropDownItem
    {
        get
        {
            TimeZoneInfo timeZoneInfo = GetSelectedTimeZone();
            if (timeZoneInfo is null)
            {
                return Item(FormatTimeZone(TimeZoneInfo.Local), TimeZoneInfo.Local.Id);
            }
            return Item(FormatTimeZone(timeZoneInfo), timeZoneInfo.Id);
        }
        set
        {
            _settingsProvider.SetSetting(timeZoneIdSettings, value.Value!.ToString() ?? string.Empty);
        }
    }

    private readonly IUIInfoBar _errorInfoBar = InfoBar("date-converter-error-info-bar");

    private readonly IUINumberInput _numberInputText = NumberInput("date-converter-number-input");

    private readonly IUISelectDropDownList _selectTimeZoneList = SelectDropDownList("date-converter-timezone-dropdown");

    private readonly IUISettingGroup _customEpochSetting = SettingGroup("date-converter-custom-epoch-setting");

    #region EpochUiInputs
    private readonly IUIStack _epochStack = Stack("date-converter-epoch-stack");
    private readonly IUISwitch _useCustomEpochSwitch = Switch("date-converter-use-custom-epoch-switch");
    private readonly IUINumberInput _epochYearInputNumber = NumberInput("date-converter-epoch-input-year");
    private readonly IUINumberInput _epochMonthInputNumber = NumberInput("date-converter-epoch-input-month");
    private readonly IUINumberInput _epochDayInputNumber = NumberInput("date-converter-epoch-input-day");
    private readonly IUINumberInput _epochHourInputNumber = NumberInput("date-converter-epoch-input-hour");
    private readonly IUINumberInput _epochMinuteInputNumber = NumberInput("date-converter-epoch-input-minute");
    private readonly IUINumberInput _epochSecondsInputNumber = NumberInput("date-converter-epoch-input-second");
    private readonly IUINumberInput _epochMillisecondsInputNumber = NumberInput("date-converter-epoch-input-millisecond");
    #endregion

    #region DateTimeUiInputs
    private readonly IUINumberInput _dateYearInputNumber = NumberInput("date-converter-input-time-year");
    private readonly IUINumberInput _dateMonthInputNumber = NumberInput("date-converter-input-time-month");
    private readonly IUINumberInput _dateDayInputNumber = NumberInput("date-converter-input-time-day");
    private readonly IUINumberInput _dateHourInputNumber = NumberInput("date-converter-input-time-hour");
    private readonly IUINumberInput _dateMinuteInputNumber = NumberInput("date-converter-input-time-minute");
    private readonly IUINumberInput _dateSecondsInputNumber = NumberInput("date-converter-input-time-second");
    private readonly IUINumberInput _dateMillisecondsInputNumber = NumberInput("date-converter-input-time-millisecond");
    #endregion

    #region DstInformation

    private readonly IUILabel _dstDaylightSavingLabel = Label("date-converter-dst-daylight-label").Style(UILabelStyle.Body);
    private readonly IUILabel _dstOffsetLabel = Label("date-converter-dst-offset-label").Style(UILabelStyle.Body);
    private readonly IUILabel _dstTicksLabel = Label("date-converter-dst-ticks-label").Style(UILabelStyle.Body);
    private readonly IUILabel _dstLocalDateTimeLabel = Label("date-converter-dst-local-dateTime-label").Style(UILabelStyle.Body);
    private readonly IUILabel _dstUtcDateTimeLabel = Label("date-converter-dst-utc-dateTime-label").Style(UILabelStyle.Body);

    #endregion

    private CancellationTokenSource? _cancellationTokenSource;

    [ImportingConstructor]
    public DateConverterGuiTool(ISettingsProvider settingsProvider)
    {
        _logger = this.Log();
        _settingsProvider = settingsProvider;

        switch (_settingsProvider.GetSetting(useCustomEpochSettings))
        {
            case true:
                _useCustomEpochSwitch.On();
                DateTimeOffset epoch = _settingsProvider.GetSetting(customEpochSettings);
                PopulateEpoch(epoch);
                _epochStack.Enable();
                break;

            case false:
                _useCustomEpochSwitch.Off();
                _epochStack.Disable();
                break;

            default:
                throw new NotSupportedException();
        }

        DateTimeOffset date = _settingsProvider.GetSetting(currentTimeSettings);
        PopulateDate(date);
    }

    internal Task? WorkTask { get; private set; }

    public UIToolView View
        => new(
            isScrollable: true,
            Grid()
                .ColumnLargeSpacing()
                .RowLargeSpacing()
                .Rows(
                    (DateConverterGridRow.Header, Auto),
                    (DateConverterGridRow.Content, new UIGridLength(1, UIGridUnitType.Fraction))
                )
                .Columns(
                    (DateConverterGridColumn.Content, new UIGridLength(1, UIGridUnitType.Fraction))
                )
            .Cells(
                Cell(
                    DateConverterGridRow.Header,
                    DateConverterGridColumn.Content,
                    Stack()
                        .Vertical()
                        .SmallSpacing()
                        .WithChildren(
                            _errorInfoBar
                                .Error()
                                .Close(),
                            EpochSettingsViewComponent(),
                            Setting("date-converter-timezone-setting")
                                .Icon("FluentSystemIcons", '\uE36E')
                                .Title(DateConverter.TimeZoneTitle)
                                .InteractiveElement(
                                    _selectTimeZoneList
                                    .Select(SelectedTimeZoneDropDownItem)
                                    .WithItems(BuildTimeZoneItems())
                                    .OnItemSelected(OnTimeZoneSelected)
                                ),
                            Setting("date-converter-format-setting")
                                .Icon("FluentSystemIcons", '\uF6F8')
                                .Title(DateConverter.FormatTitle)
                                .Handle(
                                    _settingsProvider,
                                    formatSettings,
                                    OnFormatChanged,
                                    Item(DateConverter.Ticks, DateFormat.Ticks),
                                    Item(DateConverter.Seconds, DateFormat.Seconds),
                                    Item(DateConverter.Milliseconds, DateFormat.Milliseconds)
                                )
                        )
                ),
                Cell(
                    DateConverterGridRow.Content,
                    DateConverterGridColumn.Content,
                    Stack()
                        .Vertical()
                        .SmallSpacing()
                        .WithChildren(
                            DstInformation(),
                            _numberInputText
                                .Title(DateConverter.TimestampTitle)
                                .OnTextChanged(OnTimeStampChanged)
                                .Step(1)
                                .Minimum(-2177452704000000)
                                // Ticks max value
                                .Maximum(3155861951990000000),
                            DateTimeStack()
                        )
                )
            )
        );

    // Smart detection handler.
    public void OnDataReceived(string dataTypeName, object? parsedData)
    {
        if (dataTypeName == PredefinedCommonDataTypeNames.Date &&
            parsedData is DateTimeOffset dateStrongTypedParsedData)
        {
            PopulateDate(dateStrongTypedParsedData);
        }
    }

    public void Dispose()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _semaphore.Dispose();
    }

    private void OnTimeZoneSelected(IUIDropDownListItem? item)
    {
        if (item is null || item.Value is null)
        {
            return;
        }

        if (_ignoreInputTextChange)
        {
            return;
        }

        SelectedTimeZoneDropDownItem = item;
        DateTimeOffset currentTime = _settingsProvider.GetSetting(DateConverterGuiTool.currentTimeSettings);
        StartDateTimeConvert(currentTime);
    }

    private void OnFormatChanged(DateFormat format)
    {
        _settingsProvider.SetSetting(DateConverterGuiTool.formatSettings, format);
        if (format is not DateFormat.Seconds)
        {
            _dateMillisecondsInputNumber.Editable();
        }
        else
        {
            _dateMillisecondsInputNumber.ReadOnly();
        }

        DateTimeOffset currentTime = _settingsProvider.GetSetting(DateConverterGuiTool.currentTimeSettings);
        StartDateTimeConvert(currentTime);
    }

    private void OnCustomEpochChanged(bool useCustomEpoch)
    {
        _settingsProvider.SetSetting(DateConverterGuiTool.useCustomEpochSettings, useCustomEpoch);
        DateTimeOffset currentTime = _settingsProvider.GetSetting(DateConverterGuiTool.currentTimeSettings);
        if (useCustomEpoch)
        {
            _epochStack.Enable();
        }
        else
        {
            _epochStack.Disable();
        }

        StartDateTimeConvert(currentTime);
    }

    private void OnTimeChanged(string value, DateValueType valueChanged)
    {
        if (_ignoreInputTextChange)
        {
            return;
        }

        TimeZoneInfo timeZoneInfo = GetSelectedTimeZone();
        DateTimeOffset currentTime = _settingsProvider.GetSetting(DateConverterGuiTool.currentTimeSettings);

        ResultInfo<DateTimeOffset> result = DateHelper.ChangeDateTime(
            Convert.ToInt32(value),
            currentTime,
            timeZoneInfo,
            valueChanged);
        if (!result.HasSucceeded)
        {
            _errorInfoBar.Description(DateConverter.InvalidValue);
            _errorInfoBar.Open();
            return;
        }

        _errorInfoBar.Close();
        _settingsProvider.SetSetting(DateConverterGuiTool.currentTimeSettings, result.Data);
        StartDateTimeConvert(result.Data);
    }

    private void OnEpochChanged(string value, DateValueType valueChanged)
    {
        if (_ignoreInputTextChange)
        {
            return;
        }

        DateTimeOffset epochToUse = _settingsProvider.GetSetting(DateConverterGuiTool.customEpochSettings);

        ResultInfo<DateTimeOffset> result = DateHelper.ChangeDateTime(
            Convert.ToInt32(value),
            epochToUse,
            TimeZoneInfo.Utc,
            valueChanged);

        if (!result.HasSucceeded)
        {
            _errorInfoBar.Description(DateConverter.InvalidValue);
            _errorInfoBar.Open();
            return;
        }

        _errorInfoBar.Close();
        _settingsProvider.SetSetting(DateConverterGuiTool.customEpochSettings, result.Data);

        if (!_settingsProvider.GetSetting(useCustomEpochSettings))
        {
            return;
        }

        DateTimeOffset currentTime = _settingsProvider.GetSetting(DateConverterGuiTool.currentTimeSettings);
        StartDateTimeConvert(currentTime);
    }

    private void OnTimeStampChanged(string value)
    {
        if (_ignoreInputTextChange)
        {
            return;
        }

        if (string.IsNullOrEmpty(value))
        {
            _errorInfoBar.Description(DateConverter.InvalidValue);
            _errorInfoBar.Open();
            return;
        }

        // need to convert to double first to have a proper format
        // the input is a double could be nice to allow a custom format like double / long / ...
        if (!double.TryParse(value, out double number))
        {
            _errorInfoBar.Description(DateConverter.InvalidValue);
            _errorInfoBar.Open();
            return;
        }

        _errorInfoBar.Close();

        bool useCustomEpoch = _settingsProvider.GetSetting(DateConverterGuiTool.useCustomEpochSettings);
        DateFormat format = _settingsProvider.GetSetting(DateConverterGuiTool.formatSettings);
        TimeZoneInfo timeZoneInfo = GetSelectedTimeZone();
        DateTimeOffset epochToUse = DateTime.UnixEpoch;
        if (useCustomEpoch)
        {
            epochToUse = _settingsProvider.GetSetting(DateConverterGuiTool.customEpochSettings);
        }

        StartNumberConvert(Convert.ToInt64(number), epochToUse, timeZoneInfo, format);
    }

    private void StartNumberConvert(
        long number,
        DateTimeOffset epoch,
        TimeZoneInfo timeZone,
        DateFormat dateFormat)
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();

        _ignoreInputTextChange = true;
        WorkTask = ConvertNumberAsync(
            number,
            epoch,
            timeZone,
            dateFormat,
            _cancellationTokenSource.Token);
        _ignoreInputTextChange = false;
    }

    private async Task ConvertNumberAsync(
        long number,
        DateTimeOffset epoch,
        TimeZoneInfo timeZone,
        DateFormat selectedFormat,
        CancellationToken cancellationToken)
    {
        using (await _semaphore.WaitAsync(cancellationToken))
        {
            ResultInfo<DateTimeOffset> result = DateHelper.ConvertToDateTimeUtc(
                number,
                epoch,
                selectedFormat);

            DateTimeOffset convertedDateTime = TimeZoneInfo.ConvertTime(result.Data, timeZone);
            PopulateDate(convertedDateTime);
            _numberInputText.Text(number.ToString());

            _settingsProvider.SetSetting(currentTimeSettings, convertedDateTime);

            ComputeDstInformation(convertedDateTime, timeZone);
        }
    }

    private void StartDateTimeConvert(DateTimeOffset dateTimeOffset)
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();

        bool useCustomEpoch = _settingsProvider.GetSetting(DateConverterGuiTool.useCustomEpochSettings);
        DateFormat format = _settingsProvider.GetSetting(DateConverterGuiTool.formatSettings);
        TimeZoneInfo timeZoneInfo = GetSelectedTimeZone();
        DateTimeOffset epochToUse = DateTime.UnixEpoch;
        if (useCustomEpoch)
        {
            epochToUse = _settingsProvider.GetSetting(DateConverterGuiTool.customEpochSettings);
        }

        _ignoreInputTextChange = true;
        WorkTask = ConvertDateTimeOffsetAsync(
            dateTimeOffset,
            epochToUse,
            timeZoneInfo,
            format,
            _cancellationTokenSource.Token);
        _ignoreInputTextChange = false;
    }

    private async Task ConvertDateTimeOffsetAsync(
        DateTimeOffset dateTimeOffset,
        DateTimeOffset epoch,
        TimeZoneInfo timeZone,
        DateFormat selectedFormat,
        CancellationToken cancellationToken)
    {
        using (await _semaphore.WaitAsync(cancellationToken))
        {
            ResultInfo<long> result = DateHelper.ConvertToLong(
                dateTimeOffset,
                epoch,
                selectedFormat);

            _numberInputText.Text(result.Data.ToString());
            DateTimeOffset convertedDateTime = TimeZoneInfo.ConvertTime(dateTimeOffset, timeZone);
            PopulateDate(convertedDateTime);
            ComputeDstInformation(convertedDateTime, timeZone);
        }
    }

    private TimeZoneInfo GetSelectedTimeZone()
    {
        string timeZoneId = _settingsProvider.GetSetting(timeZoneIdSettings);
        return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
    }

    private IUISettingGroup EpochSettingsViewComponent()
    {
        return
            _customEpochSetting
                .Icon("FluentSystemIcons", '\uE250')
                .Title(DateConverter.EpochSwitchTitle)
                .Description(DateConverter.EpochSwitchDescription)
                .InteractiveElement(
                    _useCustomEpochSwitch
                        .OnText(DateConverter.Yes)
                        .OffText(DateConverter.No)
                        .OnToggle(OnCustomEpochChanged)
                )
                .WithChildren(
                    EpochTimeStack()
                );
    }

    private IUIStack EpochTimeStack()
    {
        return _epochStack
            .Vertical()
            .WithChildren(
                Grid("epoch-date-grid")
                .RowSmallSpacing()
                .ColumnSmallSpacing()
                .Rows(
                    (DateConverterGridDateRow.Content, Auto)
                )
                .Columns(
                    (DateConverterGridDateColumn.Year, new UIGridLength(1, UIGridUnitType.Fraction)),
                    (DateConverterGridDateColumn.Month, new UIGridLength(1, UIGridUnitType.Fraction)),
                    (DateConverterGridDateColumn.Day, new UIGridLength(1, UIGridUnitType.Fraction))
                )
                .Cells(
                    Cell(
                        DateConverterGridRow.Header,
                        DateConverterGridDateColumn.Year,
                        Stack()
                        .Vertical()
                        .SmallSpacing()
                        .WithChildren(
                            _epochYearInputNumber
                            .Title(DateConverter.YearTitle)
                            .HideCommandBar()
                            .OnTextChanged((value) =>
                            {
                                OnEpochChanged(value, DateValueType.Year);
                            })
                            .Minimum(0)
                            .Maximum(9999)
                        )
                    ),
                    Cell(
                        DateConverterGridRow.Header,
                        DateConverterGridDateColumn.Month,
                        Stack()
                        .Vertical()
                        .SmallSpacing()
                        .WithChildren(
                            _epochMonthInputNumber
                            .Title(DateConverter.MonthTitle)
                            .HideCommandBar()
                            .OnTextChanged((value) =>
                            {
                                OnEpochChanged(value, DateValueType.Month);
                            })
                            .Minimum(1)
                            .Maximum(12)
                        )
                    ),
                    Cell(
                        DateConverterGridRow.Header,
                        DateConverterGridDateColumn.Day,
                        Stack()
                        .Vertical()
                        .SmallSpacing()
                        .WithChildren(
                            _epochDayInputNumber
                            .Title(DateConverter.DayTitle)
                            .HideCommandBar()
                            .OnTextChanged((value) =>
                            {
                                OnEpochChanged(value, DateValueType.Day);
                            })
                            .Minimum(1)
                            .Maximum(31)
                        )
                    )
                ),
                Grid("epoch-time-grid")
                .RowSmallSpacing()
                .ColumnSmallSpacing()
                .Rows(
                    (DateConverterGridDateRow.Content, Auto)
                )
                .Columns(
                    (DateConverterGridTimeColumn.Hour, new UIGridLength(1, UIGridUnitType.Fraction)),
                    (DateConverterGridTimeColumn.Minutes, new UIGridLength(1, UIGridUnitType.Fraction)),
                    (DateConverterGridTimeColumn.Seconds, new UIGridLength(1, UIGridUnitType.Fraction)),
                    (DateConverterGridTimeColumn.Milliseconds, new UIGridLength(1, UIGridUnitType.Fraction))
                )
                .Cells(
                    Cell(
                        DateConverterGridRow.Header,
                        DateConverterGridTimeColumn.Hour,
                        Stack()
                        .Vertical()
                        .SmallSpacing()
                        .WithChildren(
                            _epochHourInputNumber
                            .Title(DateConverter.HourTitle)
                            .HideCommandBar()
                            .OnTextChanged((value) =>
                            {
                                OnEpochChanged(value, DateValueType.Hour);
                            })
                            .Minimum(0)
                            .Maximum(24)
                        )
                    ),
                    Cell(
                        DateConverterGridRow.Header,
                        DateConverterGridTimeColumn.Minutes,
                        Stack()
                        .Vertical()
                        .SmallSpacing()
                        .WithChildren(
                            _epochMinuteInputNumber
                            .Title(DateConverter.MinutesTitle)
                            .HideCommandBar()
                            .OnTextChanged((value) =>
                            {
                                OnEpochChanged(value, DateValueType.Minute);
                            })
                            .Minimum(0)
                            .Maximum(59)
                        )
                    ),
                    Cell(
                        DateConverterGridRow.Header,
                        DateConverterGridTimeColumn.Seconds,
                        Stack()
                        .Vertical()
                        .SmallSpacing()
                        .WithChildren(
                            _epochSecondsInputNumber
                            .Title(DateConverter.SecondsTitle)
                            .HideCommandBar()
                            .OnTextChanged((value) =>
                            {
                                OnEpochChanged(value, DateValueType.Second);
                            })
                            .Minimum(0)
                            .Maximum(59)
                        )
                    ),
                    Cell(
                        DateConverterGridRow.Header,
                        DateConverterGridTimeColumn.Milliseconds,
                        Stack()
                        .Vertical()
                        .SmallSpacing()
                        .WithChildren(
                            _epochMillisecondsInputNumber
                            .Title(DateConverter.MillisecondsTitle)
                            .HideCommandBar()
                            .OnTextChanged((value) =>
                            {
                                OnEpochChanged(value, DateValueType.Millisecond);
                            })
                            .Minimum(0)
                            .Maximum(999)
                    )
                )
            )
        );
    }

    private IUIStack DateTimeStack()
    {
        return
            Stack()
            .Vertical()
            .WithChildren(
                Grid("dateTime-date-grid")
                .RowSmallSpacing()
                .ColumnSmallSpacing()
                .Rows(
                    (DateConverterGridDateRow.Content, Auto)
                )
                .Columns(
                    (DateConverterGridDateColumn.Year, new UIGridLength(1, UIGridUnitType.Fraction)),
                    (DateConverterGridDateColumn.Month, new UIGridLength(1, UIGridUnitType.Fraction)),
                    (DateConverterGridDateColumn.Day, new UIGridLength(1, UIGridUnitType.Fraction))
                )
                .Cells(
                    Cell(
                        DateConverterGridRow.Header,
                        DateConverterGridDateColumn.Year,
                        Stack()
                        .Vertical()
                        .SmallSpacing()
                        .WithChildren(
                            _dateYearInputNumber
                            .Title(DateConverter.YearTitle)
                            .HideCommandBar()
                            .OnTextChanged((value) =>
                            {
                                OnTimeChanged(value, DateValueType.Year);
                            })
                            .Step(1)
                            .Minimum(1)
                            .Maximum(9999)
                        )
                    ),
                    Cell(
                        DateConverterGridRow.Header,
                        DateConverterGridDateColumn.Month,
                        Stack()
                        .Vertical()
                        .SmallSpacing()
                        .WithChildren(
                            _dateMonthInputNumber
                            .Title(DateConverter.MonthTitle)
                            .HideCommandBar()
                            .OnTextChanged((value) =>
                            {
                                OnTimeChanged(value, DateValueType.Month);
                            })
                            .Step(1)
                            .Minimum(0)
                            .Maximum(13)
                        )
                    ),
                    Cell(
                        DateConverterGridRow.Header,
                        DateConverterGridDateColumn.Day,
                        Stack()
                        .Vertical()
                        .SmallSpacing()
                        .WithChildren(
                            _dateDayInputNumber
                            .Title(DateConverter.DayTitle)
                            .HideCommandBar()
                            .OnTextChanged((value) =>
                            {
                                OnTimeChanged(value, DateValueType.Day);
                            })
                            .Step(1)
                            .Minimum(-1)
                            .Maximum(32)
                        )
                    )
                ),
                Grid("dateTime-time-grid")
                .RowSmallSpacing()
                .ColumnSmallSpacing()
                .Rows(
                    (DateConverterGridDateRow.Content, Auto)
                )
                .Columns(
                    (DateConverterGridTimeColumn.Hour, new UIGridLength(1, UIGridUnitType.Fraction)),
                    (DateConverterGridTimeColumn.Minutes, new UIGridLength(1, UIGridUnitType.Fraction)),
                    (DateConverterGridTimeColumn.Seconds, new UIGridLength(1, UIGridUnitType.Fraction)),
                    (DateConverterGridTimeColumn.Milliseconds, new UIGridLength(1, UIGridUnitType.Fraction))
                )
                .Cells(
                    Cell(
                        DateConverterGridRow.Header,
                        DateConverterGridTimeColumn.Hour,
                        Stack()
                        .Vertical()
                        .SmallSpacing()
                        .WithChildren(
                            _dateHourInputNumber
                            .Title(DateConverter.HourTitle)
                            .HideCommandBar()
                            .OnTextChanged((value) =>
                            {
                                OnTimeChanged(value, DateValueType.Hour);
                            })
                            .Step(1)
                            .Minimum(-1)
                            .Maximum(24)
                        )
                    ),
                    Cell(
                        DateConverterGridRow.Header,
                        DateConverterGridTimeColumn.Minutes,
                        Stack()
                        .Vertical()
                        .SmallSpacing()
                        .WithChildren(
                            _dateMinuteInputNumber
                            .Title(DateConverter.MinutesTitle)
                            .HideCommandBar()
                            .OnTextChanged((value) =>
                            {
                                OnTimeChanged(value, DateValueType.Minute);
                            })
                            .Step(1)
                            .Minimum(-1)
                            .Maximum(60)
                        )
                    ),
                    Cell(
                        DateConverterGridRow.Header,
                        DateConverterGridTimeColumn.Seconds,
                        Stack()
                        .Vertical()
                        .SmallSpacing()
                        .WithChildren(
                            _dateSecondsInputNumber
                            .Title(DateConverter.SecondsTitle)
                            .HideCommandBar()
                            .OnTextChanged((value) =>
                            {
                                OnTimeChanged(value, DateValueType.Second);
                            })
                            .Step(1)
                            .Minimum(-1)
                            .Maximum(60)
                        )
                    ),
                    Cell(
                        DateConverterGridRow.Header,
                        DateConverterGridTimeColumn.Milliseconds,
                        Stack()
                        .Vertical()
                        .SmallSpacing()
                        .WithChildren(
                            _dateMillisecondsInputNumber
                            .Title(DateConverter.MillisecondsTitle)
                            .HideCommandBar()
                            .OnTextChanged((value) =>
                            {
                                OnTimeChanged(value, DateValueType.Millisecond);
                            })
                            .Step(1)
                            .Minimum(-1)
                            .Maximum(1001)
                        )
                    )
                ));
    }

    private IUICard DstInformation()
    {
        return
            Card(
                Stack()
                .Vertical()
                .WithChildren(
                    Grid()
                        .Rows(
                            (DateConvertDstGridRow.Daylight, Auto),
                            (DateConvertDstGridRow.Offset, Auto),
                            (DateConvertDstGridRow.DateTime, Auto)
                        )
                        .Columns(
                            (DateConvertDstGridColumn.LeftTitle, new UIGridLength(1, UIGridUnitType.Fraction)),
                            (DateConvertDstGridColumn.LeftContent, new UIGridLength(1, UIGridUnitType.Fraction)),
                            (DateConvertDstGridColumn.RightTitle, new UIGridLength(1, UIGridUnitType.Fraction)),
                            (DateConvertDstGridColumn.RightContent, new UIGridLength(1, UIGridUnitType.Fraction))
                        )
                        .Cells(
                            Cell(
                                DateConvertDstGridRow.Daylight,
                                DateConvertDstGridColumn.LeftTitle,
                                Stack()
                                    .Vertical()
                                    .SmallSpacing()
                                    .WithChildren(
                                        _dstDaylightSavingLabel
                                    )
                            ),
                            Cell(
                                DateConvertDstGridRow.Offset,
                                DateConvertDstGridColumn.LeftTitle,
                                Stack()
                                    .Vertical()
                                    .SmallSpacing()
                                    .WithChildren(
                                        Label()
                                            .Style(UILabelStyle.Body)
                                            .Text(DateConverter.OffsetTitle)
                                    )
                            ),
                            Cell(
                                DateConvertDstGridRow.Offset,
                                DateConvertDstGridColumn.LeftContent,
                                Stack()
                                    .Vertical()
                                    .SmallSpacing()
                                    .WithChildren(
                                        _dstOffsetLabel
                                    )
                            ),
                            Cell(
                                DateConvertDstGridRow.Offset,
                                DateConvertDstGridColumn.RightTitle,
                                Stack()
                                    .Vertical()
                                    .SmallSpacing()
                                    .WithChildren(
                                        Label()
                                            .Style(UILabelStyle.Body)
                                            .Text(DateConverter.UtcTicksTitle)
                                    )
                            ),
                            Cell(
                                DateConvertDstGridRow.Offset,
                                DateConvertDstGridColumn.RightContent,
                                Stack()
                                    .Vertical()
                                    .SmallSpacing()
                                    .WithChildren(
                                        _dstTicksLabel
                                    )
                            ),
                            Cell(
                                DateConvertDstGridRow.DateTime,
                                DateConvertDstGridColumn.LeftTitle,
                                Stack()
                                    .Vertical()
                                    .SmallSpacing()
                                    .WithChildren(
                                        Label()
                                            .Style(UILabelStyle.Body)
                                            .Text(DateConverter.LocalDateTime)
                                    )
                            ),
                            Cell(
                                DateConvertDstGridRow.DateTime,
                                DateConvertDstGridColumn.LeftContent,
                                Stack()
                                    .Vertical()
                                    .SmallSpacing()
                                    .WithChildren(
                                        _dstLocalDateTimeLabel
                                    )
                            ),
                            Cell(
                                DateConvertDstGridRow.DateTime,
                                DateConvertDstGridColumn.RightTitle,
                                Stack()
                                    .Vertical()
                                    .SmallSpacing()
                                    .WithChildren(
                                        Label()
                                            .Style(UILabelStyle.Body)
                                            .Text(DateConverter.UTCDateTime)
                                    )
                            ),
                            Cell(
                                DateConvertDstGridRow.DateTime,
                                DateConvertDstGridColumn.RightContent,
                                Stack()
                                    .Vertical()
                                    .SmallSpacing()
                                    .WithChildren(
                                        _dstUtcDateTimeLabel
                                    )
                            )
                        )
                )
            );
    }

    private IUIDropDownListItem[] BuildTimeZoneItems()
    {
        ReadOnlyCollection<TimeZoneInfo> systemTimeZone = TimeZoneInfo.GetSystemTimeZones();
        string timeZoneSelectedId = _settingsProvider.GetSetting(timeZoneIdSettings);
        var timeZoneDropDownItems = new IUIDropDownListItem[systemTimeZone.Count];

        if (!Regex.IsMatch(systemTimeZone.ElementAt(0).DisplayName, @"^\(UTC.*\).+$"))
        {
            // version < .Net6
            // This implementation mitigates the changes in the strings
            // that are obtained when optimized in release builds,
            // as the target of external tools is .net6 or earlier.
            // zone.DisplayName : "(UTC+09:00) 大阪、札幌、東京"( >= .net6) or "東京 (標準時)"( < .net6)
            for (int i = 0; i < systemTimeZone.Count; i++)
            {
                string displayName = $"(UTC{systemTimeZone[i].BaseUtcOffset.Hours:+00;-00;}:{systemTimeZone[i].BaseUtcOffset.Minutes:00;00;}) " + systemTimeZone[i].DisplayName;
                if (systemTimeZone[i].Id == TimeZoneInfo.Utc.Id)
                {
                    displayName = "(UTC) " + systemTimeZone[i].DisplayName;
                }
                timeZoneDropDownItems[i] = Item(displayName, systemTimeZone[i].Id);
            }
        }
        else
        {
            // version >= .Net6
            for (int i = 0; i < systemTimeZone.Count; i++)
            {
                string timeZoneId = systemTimeZone[i].Id;
                timeZoneDropDownItems[i] = Item(FormatTimeZone(systemTimeZone[i]), timeZoneId);

                if (timeZoneId.Equals(timeZoneSelectedId, StringComparison.OrdinalIgnoreCase))
                {
                    SelectedTimeZoneDropDownItem = timeZoneDropDownItems[i];
                }
            }
        }
        return timeZoneDropDownItems.ToArray();
    }

    private void ComputeDstInformation(DateTimeOffset dateTimeOffset, TimeZoneInfo timeZone)
    {
        if (timeZone.IsAmbiguousTime(dateTimeOffset))
        {
            _dstDaylightSavingLabel.Text(DateConverter.DSTAmbiguousTime);
        }
        else if (timeZone.IsDaylightSavingTime(dateTimeOffset))
        {
            _dstDaylightSavingLabel.Text(DateConverter.DaylightSavingTime);
        }
        else if (timeZone.SupportsDaylightSavingTime)
        {
            _dstDaylightSavingLabel.Text(DateConverter.SupportsDaylightSavingTime);
        }
        else
        {
            _dstDaylightSavingLabel.Text(DateConverter.DisabledDaylightSavingTime);
        }
        _dstOffsetLabel.Text(dateTimeOffset.Offset.ToString());
        _dstTicksLabel.Text(dateTimeOffset.UtcDateTime.Ticks.ToString());
        _dstLocalDateTimeLabel.Text(dateTimeOffset.LocalDateTime.ToString("yyyy/MM/dd HH:mm:ss"));
        _dstUtcDateTimeLabel.Text(dateTimeOffset.UtcDateTime.ToString("yyyy/MM/dd HH:mm:ss"));
    }

    private void PopulateEpoch(DateTimeOffset epoch)
    {
        _epochYearInputNumber.Value(epoch.Year);
        _epochMonthInputNumber.Value(epoch.Month);
        _epochDayInputNumber.Value(epoch.Day);
        _epochHourInputNumber.Value(epoch.Hour);
        _epochMinuteInputNumber.Value(epoch.Minute);
        _epochSecondsInputNumber.Value(epoch.Second);
        _epochMillisecondsInputNumber.Value(epoch.Millisecond);
    }

    private void PopulateDate(DateTimeOffset date)
    {
        _dateYearInputNumber.Value(date.Year);
        _dateMonthInputNumber.Value(date.Month);
        _dateDayInputNumber.Value(date.Day);
        _dateHourInputNumber.Value(date.Hour);
        _dateMinuteInputNumber.Value(date.Minute);
        _dateSecondsInputNumber.Value(date.Second);
        _dateMillisecondsInputNumber.Value(date.Millisecond);
    }

    private static string FormatTimeZone(TimeZoneInfo timeZoneInfo)
    {
        string displayName = $"(UTC{timeZoneInfo.BaseUtcOffset.Hours:+00;-00;}:{timeZoneInfo.BaseUtcOffset.Minutes:00;00;}) " + timeZoneInfo.StandardName;
        if (timeZoneInfo.Id == TimeZoneInfo.Utc.Id)
        {
            displayName = "(UTC) " + timeZoneInfo.StandardName;
        }
        return displayName;
    }
}
