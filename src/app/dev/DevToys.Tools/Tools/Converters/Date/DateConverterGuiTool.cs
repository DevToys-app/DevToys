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
    AccessibleNameResourceName = nameof(DateConverter.AccessibleName))]
[AcceptedDataTypeName(PredefinedCommonDataTypeNames.Date)]
internal sealed partial class DateConverterGuiTool : IGuiTool, IDisposable
{
    /// <summary>
    /// Whether the tool should use the custom Epoch.
    /// </summary>
    private static readonly SettingDefinition<bool> customEpochSettings
       = new(name: $"{nameof(DateConverterGuiTool)}.{nameof(customEpochSettings)}", defaultValue: false);

    /// <summary>
    /// The Epoch to use.
    /// </summary>
    private static readonly SettingDefinition<DateTimeOffset> epochSettings
        = new(name: $"{nameof(DateConverterGuiTool)}.{nameof(epochSettings)}",
            defaultValue: DateTime.UnixEpoch);

    /// <summary>
    /// The DateTime to use.
    /// </summary>
    private static readonly SettingDefinition<DateTimeOffset> currentTimeUTCSettings
        = new(name: $"{nameof(DateConverterGuiTool)}.{nameof(currentTimeUTCSettings)}",
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

    private bool _useCustomEpoch;

    private DateTimeOffset _currentTimeUtc;

    private DateTimeOffset _customEpoch;

    private TimeZoneInfo _timeZoneInfo;

    private DateFormat _dateFormat;

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
                return Item(TimeZoneInfo.Local.DisplayName, TimeZoneInfo.Local.Id);
            }
            return Item(timeZoneInfo.DisplayName, timeZoneInfo.Id);
        }
        set
        {
            _settingsProvider.SetSetting(timeZoneIdSettings!, value.Value!.ToString());
        }
    }

    private readonly IUIInfoBar _errorInfoBar = InfoBar("error-info-bar");

    private readonly IUINumberInput _numberInputText = NumberInput("timestamp-input-value");

    private readonly IUISelectDropDownList _selectTimeZoneList = SelectDropDownList("timestamp-timezone-dropdown");

    private readonly IUISettingGroup _customEpochSetting = SettingGroup("timestamp-custom-epoch-setting");

    private readonly IUIDataGrid _dstInformation = DataGrid("timestamp-dst-information-data-grid");

    #region EpochUiInputs
    private readonly IUISwitch _useCustomEpochSwitch = Switch("timestamp-use-custom-epoch-switch");
    private readonly IUINumberInput _epochYearInputNumber = NumberInput("timestamp-epoch-input-year");
    private readonly IUINumberInput _epochMonthInputNumber = NumberInput("timestamp-epoch-input-month");
    private readonly IUINumberInput _epochDayInputNumber = NumberInput("timestamp-epoch-input-day");
    private readonly IUINumberInput _epochHourInputNumber = NumberInput("timestamp-epoch-input-hour");
    private readonly IUINumberInput _epochMinuteInputNumber = NumberInput("timestamp-epoch-input-minute");
    private readonly IUINumberInput _epochSecondsInputNumber = NumberInput("timestamp-epoch-input-second");
    private readonly IUINumberInput _epochMillisecondsInputNumber = NumberInput("timestamp-epoch-input-millisecond");
    #endregion

    #region DateTimeUiInputs
    private readonly IUINumberInput _timeYearInputNumber = NumberInput("timestamp-input-time-year");
    private readonly IUINumberInput _timeMonthInputNumber = NumberInput("timestamp-input-time-month");
    private readonly IUINumberInput _timeDayInputNumber = NumberInput("timestamp-input-time-day");
    private readonly IUINumberInput _timeHourInputNumber = NumberInput("timestamp-input-time-hour");
    private readonly IUINumberInput _timeMinuteInputNumber = NumberInput("timestamp-input-time-minute");
    private readonly IUINumberInput _timeSecondsInputNumber = NumberInput("timestamp-input-time-second");
    private readonly IUINumberInput _timeMillisecondsInputNumber = NumberInput("timestamp-input-time-millisecond");
    #endregion

    #region DstInformation

    private readonly IUILabel _dstDaylightSavingLabel = Label("timestamp-dst-daylight-label").Style(UILabelStyle.Body);
    private readonly IUILabel _dstOffsetLabel = Label("timestamp-dst-offset-label").Style(UILabelStyle.Body);
    private readonly IUILabel _dstTicksLabel = Label("timestamp-dst-ticks-label").Style(UILabelStyle.Body);
    private readonly IUILabel _dstLocalDateTimeLabel = Label("timestamp-dst-local-dateTime-label").Style(UILabelStyle.Body);
    private readonly IUILabel _dstUtcDateTimeLabel = Label("timestamp-dst-utc-dateTime-label").Style(UILabelStyle.Body);

    #endregion

    private CancellationTokenSource? _cancellationTokenSource;

    [ImportingConstructor]
    public DateConverterGuiTool(ISettingsProvider settingsProvider)
    {
        _logger = this.Log();
        _settingsProvider = settingsProvider;

        _timeZoneInfo = GetSelectedTimeZone();
        _dateFormat = settingsProvider.GetSetting(formatSettings);
        _currentTimeUtc = settingsProvider.GetSetting(currentTimeUTCSettings);
        _customEpoch = settingsProvider.GetSetting(epochSettings);
        _useCustomEpoch = settingsProvider.GetSetting(customEpochSettings);
        _numberInputText.Text(_currentTimeUtc.ToUnixTimeMilliseconds().ToString());
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
                            Setting("timestamp-timezone-setting")
                                .Icon("FluentSystemIcons", '\uE36E')
                                .Title(DateConverter.TimeZoneTitle)
                                .InteractiveElement(
                                    _selectTimeZoneList
                                    .Select(SelectedTimeZoneDropDownItem)
                                    .WithItems(BuildTimeZoneItems())
                                    .OnItemSelected(OnTimeZoneSelected)
                                ),
                            Setting("timestamp-format-setting")
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
            _timeYearInputNumber.Value(dateStrongTypedParsedData.Year);
            _timeMonthInputNumber.Value(dateStrongTypedParsedData.Month);
            _timeDayInputNumber.Value(dateStrongTypedParsedData.Day);
            _timeHourInputNumber.Value(dateStrongTypedParsedData.Hour);
            _timeMinuteInputNumber.Value(dateStrongTypedParsedData.Minute);
            _timeSecondsInputNumber.Value(dateStrongTypedParsedData.Second);
            _timeMillisecondsInputNumber.Value(dateStrongTypedParsedData.Millisecond);
        }
    }

    public void Dispose()
    {
        _cancellationTokenSource?.Dispose();
        _semaphore.Dispose();
    }

    private void OnTimeZoneSelected(IUIDropDownListItem? item)
    {
        if (item is null)
        {
            return;
        }
        if (_ignoreInputTextChange)
        {
            return;
        }
        SelectedTimeZoneDropDownItem = item;
        _timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(item.Value!.ToString()!);
        _settingsProvider.SetSetting(timeZoneIdSettings, _timeZoneInfo.Id);
        StartDateTimeConvert(_currentTimeUtc);
    }

    private void OnFormatChanged(DateFormat format)
    {
        _dateFormat = format;
        StartDateTimeConvert(_currentTimeUtc);
        if (format is not DateFormat.Seconds)
        {
            _timeMillisecondsInputNumber.Editable();
            return;
        }
        _timeMillisecondsInputNumber.ReadOnly();
    }

    private void OnCustomEpochChanged(bool useCustomEpoch)
    {
        _useCustomEpoch = useCustomEpoch;
        _settingsProvider.SetSetting(customEpochSettings, useCustomEpoch);
        StartDateTimeConvert(_currentTimeUtc);
    }

    private void OnTimeChanged(string value, DateValueType valueChanged)
    {
        if (_ignoreInputTextChange)
        {
            return;
        }

        ToolResult<DateTimeOffset> result = DateHelper.ChangeDateTime(
            Convert.ToInt32(value),
            _currentTimeUtc,
            _timeZoneInfo,
            valueChanged);
        if (!result.HasSucceeded)
        {
            _errorInfoBar.Description(DateConverter.InvalidValue);
            _errorInfoBar.Open();
            return;
        }
        _errorInfoBar.Close();
        _currentTimeUtc = result.Data;
        _settingsProvider.SetSetting(currentTimeUTCSettings, result.Data);
        StartDateTimeConvert(result.Data);
    }

    private void OnEpochChanged(string value, DateValueType valueChanged)
    {
        if (_ignoreInputTextChange)
        {
            return;
        }

        if (!_settingsProvider.GetSetting(customEpochSettings))
        {
            return;
        }

        ToolResult<DateTimeOffset> result = DateHelper.ChangeDateTime(
            Convert.ToInt32(value),
            _customEpoch,
            TimeZoneInfo.Utc,
            valueChanged);
        if (!result.HasSucceeded)
        {
            _errorInfoBar.Description(DateConverter.InvalidValue);
            _errorInfoBar.Open();
            return;
        }
        _errorInfoBar.Close();
        _customEpoch = result.Data;
        _settingsProvider.SetSetting(epochSettings, result.Data);
        StartDateTimeConvert(_currentTimeUtc);
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

        DateTimeOffset epoch = DateTime.UnixEpoch;
        if (_useCustomEpoch)
        {
            epoch = _customEpoch;
        }
        StartNumberConvert(Convert.ToInt64(number), epoch, _timeZoneInfo, _dateFormat);
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

        WorkTask = ConvertNumberAsync(
            number,
            epoch,
            timeZone,
            dateFormat,
            _cancellationTokenSource.Token);
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
            ToolResult<DateTimeOffset> result = await DateHelper.ConvertToDateTimeUtcAsync(
                number,
                epoch,
                selectedFormat,
                cancellationToken);

            _ignoreInputTextChange = true;
            DateTimeOffset convertedDateTime = TimeZoneInfo.ConvertTime(result.Data, timeZone);
            _timeYearInputNumber.Value(convertedDateTime.Year);
            _timeMonthInputNumber.Value(convertedDateTime.Month);
            _timeDayInputNumber.Value(convertedDateTime.Day);
            _timeHourInputNumber.Value(convertedDateTime.Hour);
            _timeMinuteInputNumber.Value(convertedDateTime.Minute);
            _timeSecondsInputNumber.Value(convertedDateTime.Second);
            _timeMillisecondsInputNumber.Value(convertedDateTime.Millisecond);
            _numberInputText.Text(number.ToString());
            _settingsProvider.SetSetting(currentTimeUTCSettings, convertedDateTime);
            _currentTimeUtc = convertedDateTime;
            ComputeDstInformation(convertedDateTime);
            _ignoreInputTextChange = false;
        }
    }

    private void StartDateTimeConvert(DateTimeOffset dateTimeOffset)
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();

        DateTimeOffset epoch = DateTime.UnixEpoch;
        if (_useCustomEpoch)
        {
            epoch = _customEpoch;
        }

        WorkTask = ConvertDateTimeOffsetAsync(
            dateTimeOffset,
            epoch,
            _timeZoneInfo,
            _dateFormat,
            _cancellationTokenSource.Token);
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
            ToolResult<long> result = await DateHelper.ConvertToLongAsync(
                dateTimeOffset,
                epoch,
                selectedFormat,
                cancellationToken);

            _ignoreInputTextChange = true;

            _numberInputText.Text(result.Data.ToString());
            DateTimeOffset convertedDateTime = TimeZoneInfo.ConvertTime(dateTimeOffset, timeZone);
            _timeYearInputNumber.Value(convertedDateTime.Year);
            _timeMonthInputNumber.Value(convertedDateTime.Month);
            _timeDayInputNumber.Value(convertedDateTime.Day);
            _timeHourInputNumber.Value(convertedDateTime.Hour);
            _timeMinuteInputNumber.Value(convertedDateTime.Minute);
            _timeSecondsInputNumber.Value(convertedDateTime.Second);
            _timeMillisecondsInputNumber.Value(convertedDateTime.Millisecond);
            ComputeDstInformation(convertedDateTime);
            _ignoreInputTextChange = false;
        }
    }

    private TimeZoneInfo GetSelectedTimeZone()
    {
        string timeZone = _settingsProvider.GetSetting(timeZoneIdSettings);
        return TimeZoneInfo.FindSystemTimeZoneById(timeZone);
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
                    EpochStack()
                );
    }

    private IUIStack EpochStack()
    {
        return
            Stack()
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
                            .Value(_settingsProvider.GetSetting(epochSettings).Year)
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
                            .Value(_settingsProvider.GetSetting(epochSettings).Month)
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
                            .Value(_settingsProvider.GetSetting(epochSettings).Day)
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
                            .Value(_settingsProvider.GetSetting(epochSettings).Hour)
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
                            .Value(_settingsProvider.GetSetting(epochSettings).Minute)
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
                            .Value(_settingsProvider.GetSetting(epochSettings).Second)
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
                            .Value(_settingsProvider.GetSetting(epochSettings).Millisecond)
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
                            _timeYearInputNumber
                            .Title(DateConverter.YearTitle)
                            .Value(_currentTimeUtc.Year)
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
                            _timeMonthInputNumber
                            .Title(DateConverter.MonthTitle)
                            .Value(_currentTimeUtc.Month)
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
                            _timeDayInputNumber
                            .Title(DateConverter.DayTitle)
                            .Value(_currentTimeUtc.Day)
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
                            _timeHourInputNumber
                            .Title(DateConverter.HourTitle)
                            .Value(_currentTimeUtc.Hour)
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
                            _timeMinuteInputNumber
                            .Title(DateConverter.MinutesTitle)
                            .Value(_currentTimeUtc.Minute)
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
                            _timeSecondsInputNumber
                            .Title(DateConverter.SecondsTitle)
                            .Value(_currentTimeUtc.Second)
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
                            _timeMillisecondsInputNumber
                            .Title(DateConverter.MillisecondsTitle)
                            .Value(_currentTimeUtc.Millisecond)
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
            Card()
            .WithChildren(
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
                                        .Text("Offset")
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
                                        .Text("Ticks")
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
                                        .Text("Local DateTime")
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
                                        .Text("UTC DateTime")
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
                string displayName = systemTimeZone[i].DisplayName;
                string timeZoneId = systemTimeZone[i].Id;
                timeZoneDropDownItems[i] = Item(displayName, timeZoneId);

                if (timeZoneId.Equals(timeZoneSelectedId, StringComparison.OrdinalIgnoreCase))
                {
                    SelectedTimeZoneDropDownItem = timeZoneDropDownItems[i];
                }
            }
        }
        return timeZoneDropDownItems.ToArray();
    }

    private void ComputeDstInformation(DateTimeOffset dateTimeOffset)
    {
        if (_timeZoneInfo.IsAmbiguousTime(dateTimeOffset))
        {
            _dstDaylightSavingLabel.Text(DateConverter.DSTAmbiguousTime);
        }
        else if (_timeZoneInfo.IsDaylightSavingTime(dateTimeOffset))
        {
            _dstDaylightSavingLabel.Text(DateConverter.DaylightSavingTime);
        }
        else if (_timeZoneInfo.SupportsDaylightSavingTime)
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
}
