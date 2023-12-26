using System.Globalization;
using System.Threading.Tasks;
using DevToys.Core.Tools.Metadata;
using DevToys.Tools.Models;
using DevToys.Tools.Tools.Converters.Date;

namespace DevToys.UnitTests.Tools.Converters;

public class DateConverterGuiToolTests : MefBasedTest
{
    private const string ToolName = "DateConverter";
    private const string NumberInputText = "date-converter-number-input";
    private const string SelectTimeZoneList = "date-converter-timezone-dropdown";

    #region Settings
    private const string DateFormatSetting = "date-converter-format-setting";
    #endregion

    #region EpochUiInputs
    private const string UseCustomEpochSwitch = "date-converter-use-custom-epoch-switch";
    private const string EpochYearInputNumber = "date-converter-epoch-input-year";
    private const string EpochMonthInputNumber = "date-converter-epoch-input-month";
    private const string EpochDayInputNumber = "date-converter-epoch-input-day";
    private const string EpochHourInputNumber = "date-converter-epoch-input-hour";
    private const string EpochMinuteInputNumber = "date-converter-epoch-input-minute";
    private const string EpochSecondsInputNumber = "date-converter-epoch-input-second";
    private const string EpochMillisecondsInputNumber = "date-converter-epoch-input-millisecond";
    #endregion

    #region DateTimeUiInputs
    private const string DateYearInputNumber = "date-converter-input-time-year";
    private const string DateMonthInputNumber = "date-converter-input-time-month";
    private const string DateDayInputNumber = "date-converter-input-time-day";
    private const string DateHourInputNumber = "date-converter-input-time-hour";
    private const string DateMinuteInputNumber = "date-converter-input-time-minute";
    private const string DateSecondsInputNumber = "date-converter-input-time-second";
    private const string DateMillisecondsInputNumber = "date-converter-input-time-millisecond";
    #endregion

    public DateConverterGuiToolTests()
    : base(typeof(DateConverterGuiTool).Assembly)
    { }

    #region DateTimeSeconds

    [Theory(DisplayName = "Convert valid dateTime with custom epoch and Seconds format should return valid timestamp to Seconds")]
    [InlineData("1870-01-01T00:00:00.0000000Z", "1870-01-01T00:00:00.0000000Z", "UTC", 0)]
    [InlineData("1923-11-23T19:58:07.0000000Z", "1870-01-01T00:00:00.0000000Z", "UTC", 1700683087)]
    [InlineData("1800-11-22T19:58:07.0000000Z", "1870-01-01T00:00:00.0000000Z", "UTC", -2180836913)]
    [InlineData("1870-01-01T00:00:00.0000000Z", "1870-01-01T00:00:00.0000000Z", "Pacific Standard Time", 28800)]
    [InlineData("1923-11-23T19:58:07.0000000Z", "1870-01-01T00:00:00.0000000Z", "Pacific Standard Time", 1700711887)]
    [InlineData("1800-11-22T19:58:07.0000000Z", "1870-01-01T00:00:00.0000000Z", "Pacific Standard Time", -2180808113)]
    public async Task ConvertValidDateTimeWithCustomEpochAndSecondsFormatShouldReturnValidTimestampInSeconds(
        string dateTimeString,
        string epochString,
        string timeZoneString,
        long exceptedTimestamp)
    {
        var date = DateTimeOffset.Parse(dateTimeString, CultureInfo.InvariantCulture);
        var epoch = DateTimeOffset.Parse(epochString, CultureInfo.InvariantCulture);

        DateConverterGuiTool tool = GetToolInstance<DateConverterGuiTool>(MefProvider, ToolName);
        IUISwitch epochSettings = GetGUIElementById<IUISwitch>(tool.View, UseCustomEpochSwitch);
        IUISelectDropDownList timeZoneDropDownList = GetGUIElementById<IUISelectDropDownList>(tool.View, SelectTimeZoneList);
        IUISetting formatSetting = GetGUIElementById<IUISetting>(tool.View, DateFormatSetting);
        var formatDropdownList = formatSetting.InteractiveElement as IUISelectDropDownList;

        epochSettings.On();

        int timezoneIndex = Array.FindIndex(timeZoneDropDownList.Items, w => w.Value.Equals(timeZoneString));
        timeZoneDropDownList.Select(timezoneIndex);

        int formatIndex = Array.FindIndex(formatDropdownList.Items, w => w.Value.Equals(DateFormat.Seconds));
        formatDropdownList.Select(formatIndex);

        GetGUIElementById<IUINumberInput>(tool.View, EpochYearInputNumber).Text(epoch.Year.ToString());
        GetGUIElementById<IUINumberInput>(tool.View, EpochMonthInputNumber).Text(epoch.Month.ToString());
        GetGUIElementById<IUINumberInput>(tool.View, EpochDayInputNumber).Text(epoch.Day.ToString());
        GetGUIElementById<IUINumberInput>(tool.View, EpochHourInputNumber).Text(epoch.Hour.ToString());
        GetGUIElementById<IUINumberInput>(tool.View, EpochMinuteInputNumber).Text(epoch.Minute.ToString());
        GetGUIElementById<IUINumberInput>(tool.View, EpochSecondsInputNumber).Text(epoch.Second.ToString());
        GetGUIElementById<IUINumberInput>(tool.View, EpochMillisecondsInputNumber).Text(epoch.Millisecond.ToString());

        GetGUIElementById<IUINumberInput>(tool.View, DateYearInputNumber).Text(date.Year.ToString());
        GetGUIElementById<IUINumberInput>(tool.View, DateMonthInputNumber).Text(date.Month.ToString());
        GetGUIElementById<IUINumberInput>(tool.View, DateDayInputNumber).Text(date.Day.ToString());
        GetGUIElementById<IUINumberInput>(tool.View, DateHourInputNumber).Text(date.Hour.ToString());
        GetGUIElementById<IUINumberInput>(tool.View, DateMinuteInputNumber).Text(date.Minute.ToString());
        GetGUIElementById<IUINumberInput>(tool.View, DateSecondsInputNumber).Text(date.Second.ToString());
        GetGUIElementById<IUINumberInput>(tool.View, DateMillisecondsInputNumber).Value(0);

        await tool.WorkTask;

        GetGUIElementById<IUINumberInput>(tool.View, NumberInputText).Text.Should().Be(exceptedTimestamp.ToString());
    }

    [Theory(DisplayName = "Convert valid dateTime with unix epoch and Seconds format should return valid timestamp to Seconds")]
    [InlineData("1970-01-01T00:00:00.0000000Z", "UTC", 0)]
    [InlineData("2023-11-22T19:58:07.0000000Z", "UTC", 1700683087)]
    [InlineData("1900-11-22T19:58:07.0000000Z", "UTC", -2180836913)]
    [InlineData("1970-01-01T00:00:00.0000000Z", "Pacific Standard Time", 28800)]
    [InlineData("2023-11-22T19:58:07.0000000Z", "Pacific Standard Time", 1700711887)]
    [InlineData("1900-11-22T19:58:07.0000000Z", "Pacific Standard Time", -2180808113)]
    public async Task ConvertValidDateTimeWithUnixEpochAndSecondsFormatShouldReturnValidTimestampInSeconds(
        string dateTimeString,
        string timeZoneString,
        long exceptedTimestamp)
    {
        var date = DateTimeOffset.Parse(dateTimeString, CultureInfo.InvariantCulture);

        DateConverterGuiTool tool = GetToolInstance<DateConverterGuiTool>(MefProvider, ToolName);
        IUISwitch epochSettings = GetGUIElementById<IUISwitch>(tool.View, UseCustomEpochSwitch);
        IUISelectDropDownList timeZoneDropDownList = GetGUIElementById<IUISelectDropDownList>(tool.View, SelectTimeZoneList);
        IUISetting formatSetting = GetGUIElementById<IUISetting>(tool.View, DateFormatSetting);
        var formatDropdownList = formatSetting.InteractiveElement as IUISelectDropDownList;

        epochSettings.Off();

        int timezoneIndex = Array.FindIndex(timeZoneDropDownList.Items, w => w.Value.Equals(timeZoneString));
        timeZoneDropDownList.Select(timezoneIndex);

        int formatIndex = Array.FindIndex(formatDropdownList.Items, w => w.Value.Equals(DateFormat.Seconds));
        formatDropdownList.Select(formatIndex);

        GetGUIElementById<IUINumberInput>(tool.View, DateYearInputNumber).Text(date.Year.ToString());
        GetGUIElementById<IUINumberInput>(tool.View, DateMonthInputNumber).Text(date.Month.ToString());
        GetGUIElementById<IUINumberInput>(tool.View, DateDayInputNumber).Text(date.Day.ToString());
        GetGUIElementById<IUINumberInput>(tool.View, DateHourInputNumber).Text(date.Hour.ToString());
        GetGUIElementById<IUINumberInput>(tool.View, DateMinuteInputNumber).Text(date.Minute.ToString());
        GetGUIElementById<IUINumberInput>(tool.View, DateSecondsInputNumber).Text(date.Second.ToString());
        GetGUIElementById<IUINumberInput>(tool.View, DateMillisecondsInputNumber).Value(0);

        await tool.WorkTask;

        GetGUIElementById<IUINumberInput>(tool.View, NumberInputText).Text.Should().Be(exceptedTimestamp.ToString());
    }

    #endregion

    #region TimestampSeconds

    [Theory(DisplayName = "Convert valid timestamp with custom epoch and Seconds format should return valid dateTime")]
    [InlineData(0, "1870-01-01T00:00:00.0000000Z", "UTC", "1870-01-01T00:00:00.0000000Z")]
    [InlineData(1700683087, "1870-01-01T00:00:00.0000000Z", "UTC", "1923-11-23T19:58:07.0000000Z")]
    [InlineData(-2180836913, "1870-01-01T00:00:00.0000000Z", "UTC", "1800-11-22T19:58:07.0000000Z")]
    [InlineData(0, "1870-01-01T00:00:00.0000000Z", "Pacific Standard Time", "1869-12-31T16:00:00.0000000Z")]
    [InlineData(1700683087, "1870-01-01T00:00:00.0000000Z", "Pacific Standard Time", "1923-11-23T11:58:07.0000000Z")]
    [InlineData(-2180836913, "1870-01-01T00:00:00.0000000Z", "Pacific Standard Time", "1800-11-22T11:58:07.0000000Z")]
    public async Task ConvertValidTimestampWithCustomEpochAndSecondsFormatShouldReturnValidDateTime(
        long timestamp,
        string epochString,
        string timeZoneString,
        string exceptedDateTimeString)
    {
        var expectedDate = DateTimeOffset.Parse(exceptedDateTimeString, CultureInfo.InvariantCulture);
        var epoch = DateTimeOffset.Parse(epochString, CultureInfo.InvariantCulture);

        DateConverterGuiTool tool = GetToolInstance<DateConverterGuiTool>(MefProvider, ToolName);
        IUISwitch epochSettings = GetGUIElementById<IUISwitch>(tool.View, UseCustomEpochSwitch);
        IUISelectDropDownList timeZoneDropDownList = GetGUIElementById<IUISelectDropDownList>(tool.View, SelectTimeZoneList);
        IUISetting formatSetting = GetGUIElementById<IUISetting>(tool.View, DateFormatSetting);
        var formatDropdownList = formatSetting.InteractiveElement as IUISelectDropDownList;

        epochSettings.On();

        int timezoneIndex = Array.FindIndex(timeZoneDropDownList.Items, w => w.Value.Equals(timeZoneString));
        timeZoneDropDownList.Select(timezoneIndex);

        int formatIndex = Array.FindIndex(formatDropdownList.Items, w => w.Value.Equals(DateFormat.Seconds));
        formatDropdownList.Select(formatIndex);

        GetGUIElementById<IUINumberInput>(tool.View, EpochYearInputNumber).Text(epoch.Year.ToString());
        GetGUIElementById<IUINumberInput>(tool.View, EpochMonthInputNumber).Text(epoch.Month.ToString());
        GetGUIElementById<IUINumberInput>(tool.View, EpochDayInputNumber).Text(epoch.Day.ToString());
        GetGUIElementById<IUINumberInput>(tool.View, EpochHourInputNumber).Text(epoch.Hour.ToString());
        GetGUIElementById<IUINumberInput>(tool.View, EpochMinuteInputNumber).Text(epoch.Minute.ToString());
        GetGUIElementById<IUINumberInput>(tool.View, EpochSecondsInputNumber).Text(epoch.Second.ToString());
        GetGUIElementById<IUINumberInput>(tool.View, EpochMillisecondsInputNumber).Text(epoch.Millisecond.ToString());

        GetGUIElementById<IUINumberInput>(tool.View, NumberInputText).Text(timestamp.ToString());

        await tool.WorkTask;

        GetGUIElementById<IUINumberInput>(tool.View, DateYearInputNumber).Text.Should().Be(expectedDate.Year.ToString());
        GetGUIElementById<IUINumberInput>(tool.View, DateMonthInputNumber).Text.Should().Be(expectedDate.Month.ToString());
        GetGUIElementById<IUINumberInput>(tool.View, DateDayInputNumber).Text.Should().Be(expectedDate.Day.ToString());
        GetGUIElementById<IUINumberInput>(tool.View, DateHourInputNumber).Text.Should().Be(expectedDate.Hour.ToString());
        GetGUIElementById<IUINumberInput>(tool.View, DateMinuteInputNumber).Text.Should().Be(expectedDate.Minute.ToString());
        GetGUIElementById<IUINumberInput>(tool.View, DateSecondsInputNumber).Text.Should().Be(expectedDate.Second.ToString());
    }

    [Theory(DisplayName = "Convert valid timestamp with unix epoch and Seconds format should return valid dateTime")]
    [InlineData(0, "UTC", "1970-01-01T00:00:00.0000000Z")]
    [InlineData(1700683087, "UTC", "2023-11-22T19:58:07.0000000Z")]
    [InlineData(-2180836913, "UTC", "1900-11-22T19:58:07.0000000Z")]
    [InlineData(0, "Pacific Standard Time", "1969-12-31T16:00:00.0000000Z")]
    [InlineData(1700683087, "Pacific Standard Time", "2023-11-22T11:58:07.0000000Z")]
    [InlineData(-2180836913, "Pacific Standard Time", "1900-11-22T11:58:07.0000000Z")]
    public async Task ConvertValidTimestampWithUnixEpochAndSecondsFormatShouldReturnValidDateTime(
        long timestamp,
        string timeZoneString,
        string exceptedDateTimeString)
    {
        var expectedDate = DateTimeOffset.Parse(exceptedDateTimeString, CultureInfo.InvariantCulture);

        DateConverterGuiTool tool = GetToolInstance<DateConverterGuiTool>(MefProvider, ToolName);
        IUISwitch epochSettings = GetGUIElementById<IUISwitch>(tool.View, UseCustomEpochSwitch);
        IUISelectDropDownList timeZoneDropDownList = GetGUIElementById<IUISelectDropDownList>(tool.View, SelectTimeZoneList);
        IUISetting formatSetting = GetGUIElementById<IUISetting>(tool.View, DateFormatSetting);
        var formatDropdownList = formatSetting.InteractiveElement as IUISelectDropDownList;

        epochSettings.Off();

        int timezoneIndex = Array.FindIndex(timeZoneDropDownList.Items, w => w.Value.Equals(timeZoneString));
        timeZoneDropDownList.Select(timezoneIndex);

        int formatIndex = Array.FindIndex(formatDropdownList.Items, w => w.Value.Equals(DateFormat.Seconds));
        formatDropdownList.Select(formatIndex);

        GetGUIElementById<IUINumberInput>(tool.View, NumberInputText).Text(timestamp.ToString());

        await tool.WorkTask;

        GetGUIElementById<IUINumberInput>(tool.View, DateYearInputNumber).Text.Should().Be(expectedDate.Year.ToString());
        GetGUIElementById<IUINumberInput>(tool.View, DateMonthInputNumber).Text.Should().Be(expectedDate.Month.ToString());
        GetGUIElementById<IUINumberInput>(tool.View, DateDayInputNumber).Text.Should().Be(expectedDate.Day.ToString());
        GetGUIElementById<IUINumberInput>(tool.View, DateHourInputNumber).Text.Should().Be(expectedDate.Hour.ToString());
        GetGUIElementById<IUINumberInput>(tool.View, DateMinuteInputNumber).Text.Should().Be(expectedDate.Minute.ToString());
        GetGUIElementById<IUINumberInput>(tool.View, DateSecondsInputNumber).Text.Should().Be(expectedDate.Second.ToString());
        GetGUIElementById<IUINumberInput>(tool.View, DateMillisecondsInputNumber).Value.Should().Be(0);
    }

    #endregion

    #region DateTimeMilliseconds

    [Theory(DisplayName = "Convert valid dateTime with custom epoch and Milliseconds format should return valid timestamp to Milliseconds")]
    [InlineData("1870-01-01T00:00:00.0000000Z", "1870-01-01T00:00:00.0000000Z", "UTC", 0)]
    [InlineData("1923-11-23T19:58:07.0000000Z", "1870-01-01T00:00:00.0000000Z", "UTC", 1700683087000)]
    [InlineData("1800-11-22T19:58:07.0000000Z", "1870-01-01T00:00:00.0000000Z", "UTC", -2180836913000)]
    [InlineData("1870-01-01T00:00:00.0000000Z", "1870-01-01T00:00:00.0000000Z", "Pacific Standard Time", 28800000)]
    [InlineData("1923-11-23T19:58:07.0000000Z", "1870-01-01T00:00:00.0000000Z", "Pacific Standard Time", 1700711887000)]
    [InlineData("1800-11-22T19:58:07.0000000Z", "1870-01-01T00:00:00.0000000Z", "Pacific Standard Time", -2180808113000)]
    public async Task ConvertValidDateTimeWithCustomEpochAndMillisecondsFormatShouldReturnValidTimestampInMilliseconds(
        string dateTimeString,
        string epochString,
        string timeZoneString,
        long exceptedTimestamp)
    {
        var date = DateTimeOffset.Parse(dateTimeString, CultureInfo.InvariantCulture);
        var epoch = DateTimeOffset.Parse(epochString, CultureInfo.InvariantCulture);

        DateConverterGuiTool tool = GetToolInstance<DateConverterGuiTool>(MefProvider, ToolName);
        IUISwitch epochSettings = GetGUIElementById<IUISwitch>(tool.View, UseCustomEpochSwitch);
        IUISelectDropDownList timeZoneDropDownList = GetGUIElementById<IUISelectDropDownList>(tool.View, SelectTimeZoneList);
        IUISetting formatSetting = GetGUIElementById<IUISetting>(tool.View, DateFormatSetting);
        var formatDropdownList = formatSetting.InteractiveElement as IUISelectDropDownList;

        epochSettings.On();

        int timezoneIndex = Array.FindIndex(timeZoneDropDownList.Items, w => w.Value.Equals(timeZoneString));
        timeZoneDropDownList.Select(timezoneIndex);

        int formatIndex = Array.FindIndex(formatDropdownList.Items, w => w.Value.Equals(DateFormat.Milliseconds));
        formatDropdownList.Select(formatIndex);

        GetGUIElementById<IUINumberInput>(tool.View, EpochYearInputNumber).Text(epoch.Year.ToString());
        GetGUIElementById<IUINumberInput>(tool.View, EpochMonthInputNumber).Text(epoch.Month.ToString());
        GetGUIElementById<IUINumberInput>(tool.View, EpochDayInputNumber).Text(epoch.Day.ToString());
        GetGUIElementById<IUINumberInput>(tool.View, EpochHourInputNumber).Text(epoch.Hour.ToString());
        GetGUIElementById<IUINumberInput>(tool.View, EpochMinuteInputNumber).Text(epoch.Minute.ToString());
        GetGUIElementById<IUINumberInput>(tool.View, EpochSecondsInputNumber).Text(epoch.Second.ToString());
        GetGUIElementById<IUINumberInput>(tool.View, EpochMillisecondsInputNumber).Text(epoch.Millisecond.ToString());

        GetGUIElementById<IUINumberInput>(tool.View, DateYearInputNumber).Text(date.Year.ToString());
        GetGUIElementById<IUINumberInput>(tool.View, DateMonthInputNumber).Text(date.Month.ToString());
        GetGUIElementById<IUINumberInput>(tool.View, DateDayInputNumber).Text(date.Day.ToString());
        GetGUIElementById<IUINumberInput>(tool.View, DateHourInputNumber).Text(date.Hour.ToString());
        GetGUIElementById<IUINumberInput>(tool.View, DateMinuteInputNumber).Text(date.Minute.ToString());
        GetGUIElementById<IUINumberInput>(tool.View, DateSecondsInputNumber).Text(date.Second.ToString());
        GetGUIElementById<IUINumberInput>(tool.View, DateMillisecondsInputNumber).Text(date.Millisecond.ToString());

        await tool.WorkTask;

        GetGUIElementById<IUINumberInput>(tool.View, NumberInputText).Text.Should().Be(exceptedTimestamp.ToString());
    }

    [Theory(DisplayName = "Convert valid dateTime with unix epoch and Milliseconds format should return valid timestamp to Milliseconds")]
    [InlineData("1970-01-01T00:00:00.0000000Z", "UTC", 0)]
    [InlineData("2023-11-22T19:58:07.0000000Z", "UTC", 1700683087000)]
    [InlineData("1900-11-22T19:58:07.0000000Z", "UTC", -2180836913000)]
    [InlineData("1970-01-01T00:00:00.0000000Z", "Pacific Standard Time", 28800000)]
    [InlineData("2023-11-22T19:58:07.0000000Z", "Pacific Standard Time", 1700711887000)]
    [InlineData("1900-11-22T19:58:07.0000000Z", "Pacific Standard Time", -2180808113000)]
    public async Task ConvertValidDateTimeWithUnixEpochAndMillisecondsFormatShouldReturnValidTimestampInMilliseconds(
        string dateTimeString,
        string timeZoneString,
        long exceptedTimestamp)
    {
        var date = DateTimeOffset.Parse(dateTimeString, CultureInfo.InvariantCulture);

        DateConverterGuiTool tool = GetToolInstance<DateConverterGuiTool>(MefProvider, ToolName);
        IUISwitch epochSettings = GetGUIElementById<IUISwitch>(tool.View, UseCustomEpochSwitch);
        IUISelectDropDownList timeZoneDropDownList = GetGUIElementById<IUISelectDropDownList>(tool.View, SelectTimeZoneList);
        IUISetting formatSetting = GetGUIElementById<IUISetting>(tool.View, DateFormatSetting);
        var formatDropdownList = formatSetting.InteractiveElement as IUISelectDropDownList;

        epochSettings.Off();

        int timezoneIndex = Array.FindIndex(timeZoneDropDownList.Items, w => w.Value.Equals(timeZoneString));
        timeZoneDropDownList.Select(timezoneIndex);

        int formatIndex = Array.FindIndex(formatDropdownList.Items, w => w.Value.Equals(DateFormat.Milliseconds));
        formatDropdownList.Select(formatIndex);

        GetGUIElementById<IUINumberInput>(tool.View, DateYearInputNumber).Text(date.Year.ToString());
        GetGUIElementById<IUINumberInput>(tool.View, DateMonthInputNumber).Text(date.Month.ToString());
        GetGUIElementById<IUINumberInput>(tool.View, DateDayInputNumber).Text(date.Day.ToString());
        GetGUIElementById<IUINumberInput>(tool.View, DateHourInputNumber).Text(date.Hour.ToString());
        GetGUIElementById<IUINumberInput>(tool.View, DateMinuteInputNumber).Text(date.Minute.ToString());
        GetGUIElementById<IUINumberInput>(tool.View, DateSecondsInputNumber).Text(date.Second.ToString());
        GetGUIElementById<IUINumberInput>(tool.View, DateMillisecondsInputNumber).Value(0);

        await tool.WorkTask;

        GetGUIElementById<IUINumberInput>(tool.View, NumberInputText).Text.Should().Be(exceptedTimestamp.ToString());
    }

    #endregion

    #region TimestampMilliseconds

    [Theory(DisplayName = "Convert valid timestamp with custom epoch and Milliseconds format should return valid dateTime")]
    [InlineData(0, "1870-01-01T00:00:00.0000000Z", "UTC", "1870-01-01T00:00:00.0000000Z")]
    [InlineData(1700683087000, "1870-01-01T00:00:00.0000000Z", "UTC", "1923-11-23T19:58:07.0000000Z")]
    [InlineData(-2180836913000, "1870-01-01T00:00:00.0000000Z", "UTC", "1800-11-22T19:58:07.0000000Z")]
    [InlineData(0, "1870-01-01T00:00:00.0000000Z", "Pacific Standard Time", "1869-12-31T16:00:00.0000000Z")]
    [InlineData(1700683087000, "1870-01-01T00:00:00.0000000Z", "Pacific Standard Time", "1923-11-23T11:58:07.0000000Z")]
    [InlineData(-2180836913000, "1870-01-01T00:00:00.0000000Z", "Pacific Standard Time", "1800-11-22T11:58:07.0000000Z")]
    public async Task ConvertValidTimestampWithCustomEpochAndMillisecondsFormatShouldReturnValidDateTime(
        long timestamp,
        string epochString,
        string timeZoneString,
        string exceptedDateTimeString)
    {
        var expectedDate = DateTimeOffset.Parse(exceptedDateTimeString, CultureInfo.InvariantCulture);
        var epoch = DateTimeOffset.Parse(epochString, CultureInfo.InvariantCulture);

        DateConverterGuiTool tool = GetToolInstance<DateConverterGuiTool>(MefProvider, ToolName);
        IUISwitch epochSettings = GetGUIElementById<IUISwitch>(tool.View, UseCustomEpochSwitch);
        IUISelectDropDownList timeZoneDropDownList = GetGUIElementById<IUISelectDropDownList>(tool.View, SelectTimeZoneList);
        IUISetting formatSetting = GetGUIElementById<IUISetting>(tool.View, DateFormatSetting);
        var formatDropdownList = formatSetting.InteractiveElement as IUISelectDropDownList;

        epochSettings.On();

        int timezoneIndex = Array.FindIndex(timeZoneDropDownList.Items, w => w.Value.Equals(timeZoneString));
        timeZoneDropDownList.Select(timezoneIndex);

        int formatIndex = Array.FindIndex(formatDropdownList.Items, w => w.Value.Equals(DateFormat.Milliseconds));
        formatDropdownList.Select(formatIndex);

        GetGUIElementById<IUINumberInput>(tool.View, EpochYearInputNumber).Text(epoch.Year.ToString());
        GetGUIElementById<IUINumberInput>(tool.View, EpochMonthInputNumber).Text(epoch.Month.ToString());
        GetGUIElementById<IUINumberInput>(tool.View, EpochDayInputNumber).Text(epoch.Day.ToString());
        GetGUIElementById<IUINumberInput>(tool.View, EpochHourInputNumber).Text(epoch.Hour.ToString());
        GetGUIElementById<IUINumberInput>(tool.View, EpochMinuteInputNumber).Text(epoch.Minute.ToString());
        GetGUIElementById<IUINumberInput>(tool.View, EpochSecondsInputNumber).Text(epoch.Second.ToString());
        GetGUIElementById<IUINumberInput>(tool.View, EpochMillisecondsInputNumber).Text(epoch.Millisecond.ToString());

        GetGUIElementById<IUINumberInput>(tool.View, NumberInputText).Text(timestamp.ToString());

        await tool.WorkTask;

        GetGUIElementById<IUINumberInput>(tool.View, DateYearInputNumber).Text.Should().Be(expectedDate.Year.ToString());
        GetGUIElementById<IUINumberInput>(tool.View, DateMonthInputNumber).Text.Should().Be(expectedDate.Month.ToString());
        GetGUIElementById<IUINumberInput>(tool.View, DateDayInputNumber).Text.Should().Be(expectedDate.Day.ToString());
        GetGUIElementById<IUINumberInput>(tool.View, DateHourInputNumber).Text.Should().Be(expectedDate.Hour.ToString());
        GetGUIElementById<IUINumberInput>(tool.View, DateMinuteInputNumber).Text.Should().Be(expectedDate.Minute.ToString());
        GetGUIElementById<IUINumberInput>(tool.View, DateSecondsInputNumber).Text.Should().Be(expectedDate.Second.ToString());
        GetGUIElementById<IUINumberInput>(tool.View, DateMillisecondsInputNumber).Text.Should().Be(expectedDate.Millisecond.ToString());
    }

    [Theory(DisplayName = "Convert valid timestamp with unix epoch and Milliseconds format should return valid dateTime")]
    [InlineData(0, "UTC", "1970-01-01T00:00:00.0000000Z")]
    [InlineData(1700683087000, "UTC", "2023-11-22T19:58:07.0000000Z")]
    [InlineData(-2180836913000, "UTC", "1900-11-22T19:58:07.0000000Z")]
    [InlineData(0, "Pacific Standard Time", "1969-12-31T16:00:00.0000000Z")]
    [InlineData(1700683087000, "Pacific Standard Time", "2023-11-22T11:58:07.0000000Z")]
    [InlineData(-2180836913000, "Pacific Standard Time", "1900-11-22T11:58:07.0000000Z")]
    public async Task ConvertValidTimestampWithUnixEpochAndMillisecondsFormatShouldReturnValidDateTime(
        long timestamp,
        string timeZoneString,
        string exceptedDateTimeString)
    {
        var expectedDate = DateTimeOffset.Parse(exceptedDateTimeString, CultureInfo.InvariantCulture);

        DateConverterGuiTool tool = GetToolInstance<DateConverterGuiTool>(MefProvider, ToolName);
        IUISwitch epochSettings = GetGUIElementById<IUISwitch>(tool.View, UseCustomEpochSwitch);
        IUISelectDropDownList timeZoneDropDownList = GetGUIElementById<IUISelectDropDownList>(tool.View, SelectTimeZoneList);
        IUISetting formatSetting = GetGUIElementById<IUISetting>(tool.View, DateFormatSetting);
        var formatDropdownList = formatSetting.InteractiveElement as IUISelectDropDownList;

        epochSettings.Off();

        int timezoneIndex = Array.FindIndex(timeZoneDropDownList.Items, w => w.Value.Equals(timeZoneString));
        timeZoneDropDownList.Select(timezoneIndex);

        int formatIndex = Array.FindIndex(formatDropdownList.Items, w => w.Value.Equals(DateFormat.Milliseconds));
        formatDropdownList.Select(formatIndex);

        GetGUIElementById<IUINumberInput>(tool.View, NumberInputText).Text(timestamp.ToString());

        await tool.WorkTask;

        GetGUIElementById<IUINumberInput>(tool.View, DateYearInputNumber).Text.Should().Be(expectedDate.Year.ToString());
        GetGUIElementById<IUINumberInput>(tool.View, DateMonthInputNumber).Text.Should().Be(expectedDate.Month.ToString());
        GetGUIElementById<IUINumberInput>(tool.View, DateDayInputNumber).Text.Should().Be(expectedDate.Day.ToString());
        GetGUIElementById<IUINumberInput>(tool.View, DateHourInputNumber).Text.Should().Be(expectedDate.Hour.ToString());
        GetGUIElementById<IUINumberInput>(tool.View, DateMinuteInputNumber).Text.Should().Be(expectedDate.Minute.ToString());
        GetGUIElementById<IUINumberInput>(tool.View, DateSecondsInputNumber).Text.Should().Be(expectedDate.Second.ToString());
        GetGUIElementById<IUINumberInput>(tool.View, DateMillisecondsInputNumber).Text.Should().Be(expectedDate.Millisecond.ToString());
    }

    #endregion

    [Theory(DisplayName = "Convert valid dateTime should return valid Ticks")]
    [InlineData("1870-01-01T00:00:00.0000000+00:00", "UTC", 589799232000000000)]
    [InlineData("1923-11-23T19:58:07.0000000+00:00", "UTC", 606806062870000000)]
    [InlineData("1800-11-22T19:58:07.0000000+00:00", "UTC", 567990862870000000)]
    [InlineData("1870-01-01T00:00:00.0000000-08:00", "Pacific Standard Time", 589799520000000000)]
    [InlineData("1870-01-01T00:00:00.0000000+00:00", "Pacific Standard Time", 589799520000000000)]
    [InlineData("1923-11-23T19:58:07.0000000Z", "Pacific Standard Time", 606806350870000000)]
    [InlineData("1800-11-22T19:58:07.0000000Z", "Pacific Standard Time", 567991150870000000)]
    public async Task ConvertValidDateTimeShouldReturnValidTicks(
        string dateTimeString,
        string timeZoneString,
        long exceptedTicks)
    {
        var date = DateTimeOffset.Parse(dateTimeString, CultureInfo.InvariantCulture);

        DateConverterGuiTool tool = GetToolInstance<DateConverterGuiTool>(MefProvider, ToolName);
        IUISwitch epochSettings = GetGUIElementById<IUISwitch>(tool.View, UseCustomEpochSwitch);
        IUISelectDropDownList timeZoneDropDownList = GetGUIElementById<IUISelectDropDownList>(tool.View, SelectTimeZoneList);
        IUISetting formatSetting = GetGUIElementById<IUISetting>(tool.View, DateFormatSetting);
        var formatDropdownList = formatSetting.InteractiveElement as IUISelectDropDownList;

        epochSettings.Off();

        int timezoneIndex = Array.FindIndex(timeZoneDropDownList.Items, w => w.Value.Equals(timeZoneString));
        timeZoneDropDownList.Select(timezoneIndex);

        int formatIndex = Array.FindIndex(formatDropdownList.Items, w => w.Value.Equals(DateFormat.Ticks));
        formatDropdownList.Select(formatIndex);

        GetGUIElementById<IUINumberInput>(tool.View, DateYearInputNumber).Text(date.Year.ToString());
        GetGUIElementById<IUINumberInput>(tool.View, DateMonthInputNumber).Text(date.Month.ToString());
        GetGUIElementById<IUINumberInput>(tool.View, DateDayInputNumber).Text(date.Day.ToString());
        GetGUIElementById<IUINumberInput>(tool.View, DateHourInputNumber).Text(date.Hour.ToString());
        GetGUIElementById<IUINumberInput>(tool.View, DateMinuteInputNumber).Text(date.Minute.ToString());
        GetGUIElementById<IUINumberInput>(tool.View, DateSecondsInputNumber).Text(date.Second.ToString());
        GetGUIElementById<IUINumberInput>(tool.View, DateMillisecondsInputNumber).Value(0);

        await tool.WorkTask;

        GetGUIElementById<IUINumberInput>(tool.View, NumberInputText).Text.Should().Be(exceptedTicks.ToString());
    }

    [Theory(DisplayName = "Convert valid ticks should return valid dateTime")]
    [InlineData(0, "UTC", "0001-01-01T00:00:00.0000000Z")]
    [InlineData(638362798870000000, "UTC", "2023-11-22T19:58:07.0000000Z")]
    [InlineData(599547598870000000, "UTC", "1900-11-22T19:58:07.0000000Z")]
    [InlineData(0, "Pacific Standard Time", "0001-01-01T00:00:00.0000000Z")]
    [InlineData(638362798870000000, "Pacific Standard Time", "2023-11-22T11:58:07.0000000Z")]
    [InlineData(599547598870000000, "Pacific Standard Time", "1900-11-22T11:58:07.0000000Z")]
    public async Task ConvertValidTicksWithTicksFormatShouldReturnValidDateTime(
        long ticks,
        string timeZoneString,
        string exceptedDateTimeString)
    {
        var expectedDate = DateTimeOffset.Parse(exceptedDateTimeString, CultureInfo.InvariantCulture);

        DateConverterGuiTool tool = GetToolInstance<DateConverterGuiTool>(MefProvider, ToolName);
        IUISwitch epochSettings = GetGUIElementById<IUISwitch>(tool.View, UseCustomEpochSwitch);
        IUISelectDropDownList timeZoneDropDownList = GetGUIElementById<IUISelectDropDownList>(tool.View, SelectTimeZoneList);
        IUISetting formatSetting = GetGUIElementById<IUISetting>(tool.View, DateFormatSetting);
        var formatDropdownList = formatSetting.InteractiveElement as IUISelectDropDownList;

        epochSettings.Off();

        int timezoneIndex = Array.FindIndex(timeZoneDropDownList.Items, w => w.Value.Equals(timeZoneString));
        timeZoneDropDownList.Select(timezoneIndex);

        int formatIndex = Array.FindIndex(formatDropdownList.Items, w => w.Value.Equals(DateFormat.Ticks));
        formatDropdownList.Select(formatIndex);

        GetGUIElementById<IUINumberInput>(tool.View, NumberInputText).Text(ticks.ToString());

        await tool.WorkTask;

        GetGUIElementById<IUINumberInput>(tool.View, DateYearInputNumber).Text.Should().Be(expectedDate.Year.ToString());
        GetGUIElementById<IUINumberInput>(tool.View, DateMonthInputNumber).Text.Should().Be(expectedDate.Month.ToString());
        GetGUIElementById<IUINumberInput>(tool.View, DateDayInputNumber).Text.Should().Be(expectedDate.Day.ToString());
        GetGUIElementById<IUINumberInput>(tool.View, DateHourInputNumber).Text.Should().Be(expectedDate.Hour.ToString());
        GetGUIElementById<IUINumberInput>(tool.View, DateMinuteInputNumber).Text.Should().Be(expectedDate.Minute.ToString());
        GetGUIElementById<IUINumberInput>(tool.View, DateSecondsInputNumber).Text.Should().Be(expectedDate.Second.ToString());
        GetGUIElementById<IUINumberInput>(tool.View, DateMillisecondsInputNumber).Text.Should().Be(expectedDate.Millisecond.ToString());
    }

    private static T GetGUIElementById<T>(UIToolView toolView, string name)
        => (T)toolView.GetChildElementById(name);

    private static T GetToolInstance<T>(IMefProvider mefProvider, string name)
        where T : IGuiTool
        => (T)mefProvider.ImportMany<IGuiTool, GuiToolMetadata>()
        .Single(t => t.Metadata.InternalComponentName == name)
        .Value;

}
