using System.Globalization;
using System.Threading.Tasks;
using DevToys.Core.Tools.Metadata;
using DevToys.Tools.Models;
using DevToys.Tools.Tools.Converters.Date;

namespace DevToys.UnitTests.Tools.Converters;

[Collection(nameof(TestParallelizationDisabled))]
public class DateConverterGuiToolTests : MefBasedTest
{
    private readonly UIToolView _toolView;

    private readonly DateConverterGuiTool _tool;

    private readonly IUIInfoBar _errorInfoBar;

    private readonly IUINumberInput _timeStampInputText;

    private readonly IUISelectDropDownList _timeZoneDropDownList;

    private readonly IUISetting _formatSetting;

    #region EpochUiInputs
    private readonly IUINumberInput _epochYearInputNumber;
    private readonly IUINumberInput _epochMonthInputNumber;
    private readonly IUINumberInput _epochDayInputNumber;
    private readonly IUINumberInput _epochHourInputNumber;
    private readonly IUINumberInput _epochMinuteInputNumber;
    private readonly IUINumberInput _epochSecondsInputNumber;
    private readonly IUINumberInput _epochMillisecondsInputNumber;
    #endregion

    #region DateTimeUiInputs
    private readonly IUINumberInput _timeYearInputNumber;
    private readonly IUINumberInput _timeMonthInputNumber;
    private readonly IUINumberInput _timeDayInputNumber;
    private readonly IUINumberInput _timeHourInputNumber;
    private readonly IUINumberInput _timeMinuteInputNumber;
    private readonly IUINumberInput _timeSecondsInputNumber;
    private readonly IUINumberInput _timeMillisecondsInputNumber;
    #endregion

    public DateConverterGuiToolTests()
        : base(typeof(DateConverterGuiTool).Assembly)
    {
        _tool = (DateConverterGuiTool)MefProvider.ImportMany<IGuiTool, GuiToolMetadata>()
            .Single(t => t.Metadata.InternalComponentName == "DateConverter")
            .Value;

        _toolView = _tool.View;

        _errorInfoBar = (IUIInfoBar)_toolView.GetChildElementById("error-info-bar");

        _timeStampInputText = (IUINumberInput)_toolView.GetChildElementById("timestamp-input-value");
        _formatSetting = (IUISetting)_toolView.GetChildElementById("timestamp-format-setting");
        _timeZoneDropDownList = (IUISelectDropDownList)_toolView.GetChildElementById("timestamp-timezone-dropdown");

        _epochYearInputNumber = (IUINumberInput)_toolView.GetChildElementById("timestamp-epoch-input-year");
        _epochMonthInputNumber = (IUINumberInput)_toolView.GetChildElementById("timestamp-epoch-input-time-month");
        _epochDayInputNumber = (IUINumberInput)_toolView.GetChildElementById("timestamp-epoch-input-time-day");
        _epochHourInputNumber = (IUINumberInput)_toolView.GetChildElementById("timestamp-epoch-input-time-hour");
        _epochMinuteInputNumber = (IUINumberInput)_toolView.GetChildElementById("timestamp-epoch-input-time-minute");
        _epochSecondsInputNumber = (IUINumberInput)_toolView.GetChildElementById("timestamp-epoch-input-time-second");
        _epochMillisecondsInputNumber = (IUINumberInput)_toolView.GetChildElementById("timestamp-epoch-input-time-millisecond");

        _timeYearInputNumber = (IUINumberInput)_toolView.GetChildElementById("timestamp-input-time-year");
        _timeMonthInputNumber = (IUINumberInput)_toolView.GetChildElementById("timestamp-input-time-month");
        _timeDayInputNumber = (IUINumberInput)_toolView.GetChildElementById("timestamp-input-time-day");
        _timeHourInputNumber = (IUINumberInput)_toolView.GetChildElementById("timestamp-input-time-hour");
        _timeMinuteInputNumber = (IUINumberInput)_toolView.GetChildElementById("timestamp-input-time-minute");
        _timeSecondsInputNumber = (IUINumberInput)_toolView.GetChildElementById("timestamp-input-time-second");
        _timeMillisecondsInputNumber = (IUINumberInput)_toolView.GetChildElementById("timestamp-input-time-millisecond");
    }

    #region DateTimeSeconds

    [Theory(DisplayName = "Convert valid dateTime with custom epoch and Seconds format should return valid timestamp to Seconds")]
    [InlineData("1870-01-01T00:00:00.0000000Z", "1870-01-01T00:00:00.0000000Z", "UTC", 0)]
    [InlineData("1923-11-23T19:58:07.0000000Z", "1870-01-01T00:00:00.0000000Z", "UTC", 1700683087)]
    [InlineData("1800-11-22T19:58:07.0000000Z", "1870-01-01T00:00:00.0000000Z", "UTC", -2180836913)]
    [InlineData("1870-01-01T00:00:00.0000000Z", "1870-01-01T00:00:00.0000000Z", "Pacific Standard Time", 0)]
    [InlineData("1923-11-23T19:58:07.0000000Z", "1870-01-01T00:00:00.0000000Z", "Pacific Standard Time", 1700683087)]
    [InlineData("1800-11-22T19:58:07.0000000Z", "1870-01-01T00:00:00.0000000Z", "Pacific Standard Time", -2180836913)]
    public async Task ConvertValidDateTimeWithCustomEpochAndSecondsFormatShouldReturnValidTimestampInSeconds(
        string dateTimeString,
        string epochString,
        string timeZoneString,
        long exceptedTimestamp)
    {
        var dateTime = DateTimeOffset.Parse(dateTimeString, CultureInfo.InvariantCulture);
        var epoch = DateTimeOffset.Parse(epochString, CultureInfo.InvariantCulture);

        var epochSettings = (IUISwitch)_toolView.GetChildElementById("timestamp-use-custom-epoch-switch");
        epochSettings.On();

        int timezoneIndex = Array.FindIndex(_timeZoneDropDownList.Items, w => w.Value.Equals(timeZoneString));
        _timeZoneDropDownList.Select(timezoneIndex);

        var formatDropdownList = _formatSetting.InteractiveElement as IUISelectDropDownList;
        int formatIndex = Array.FindIndex(formatDropdownList.Items, w => w.Value.Equals(DateFormat.Seconds));
        formatDropdownList.Select(formatIndex);

        _epochYearInputNumber.Text(epoch.Year.ToString());
        _epochMonthInputNumber.Text(epoch.Month.ToString());
        _epochDayInputNumber.Text(epoch.Day.ToString());
        _epochHourInputNumber.Text(epoch.Hour.ToString());
        _epochMinuteInputNumber.Text(epoch.Minute.ToString());
        _epochSecondsInputNumber.Text(epoch.Second.ToString());
        _epochMillisecondsInputNumber.Text(epoch.Millisecond.ToString());

        _timeYearInputNumber.Text(dateTime.Year.ToString());
        _timeMonthInputNumber.Text(dateTime.Month.ToString());
        _timeDayInputNumber.Text(dateTime.Day.ToString());
        _timeHourInputNumber.Text(dateTime.Hour.ToString());
        _timeMinuteInputNumber.Text(dateTime.Minute.ToString());
        _timeSecondsInputNumber.Text(dateTime.Second.ToString());
        _timeMillisecondsInputNumber.Text("0");

        await _tool.WorkTask;

        _timeStampInputText.Text.Should().Be(exceptedTimestamp.ToString());
    }

    [Theory(DisplayName = "Convert valid dateTime with unix epoch and Seconds format should return valid timestamp to Seconds")]
    [InlineData("1970-01-01T00:00:00.0000000Z", "UTC", 0)]
    [InlineData("2023-11-22T19:58:07.0000000Z", "UTC", 1700683087)]
    [InlineData("1900-11-22T19:58:07.0000000Z", "UTC", -2180836912)]
    [InlineData("1970-01-01T00:00:00.0000000Z", "Pacific Standard Time", 0)]
    [InlineData("2023-11-22T19:58:07.0000000Z", "Pacific Standard Time", 1700683087)]
    [InlineData("1900-11-22T19:58:07.0000000Z", "Pacific Standard Time", -2180836912)]
    public async Task ConvertValidDateTimeWithUnixEpochAndSecondsFormatShouldReturnValidTimestampInSeconds(
        string dateTimeString,
        string timeZoneString,
        long exceptedTimestamp)
    {
        var epochSettings = (IUISwitch)_toolView.GetChildElementById("timestamp-use-custom-epoch-switch");
        epochSettings.Off();

        int timezoneIndex = Array.FindIndex(_timeZoneDropDownList.Items, w => w.Value.Equals(timeZoneString));
        _timeZoneDropDownList.Select(timezoneIndex);

        var formatDropdownList = _formatSetting.InteractiveElement as IUISelectDropDownList;
        int formatIndex = Array.FindIndex(formatDropdownList.Items, w => w.Value.Equals(DateFormat.Seconds));
        formatDropdownList.Select(formatIndex);

        var dateTime = DateTimeOffset.Parse(dateTimeString);
        _timeYearInputNumber.Text(dateTime.Year.ToString());
        _timeMonthInputNumber.Text(dateTime.Month.ToString());
        _timeDayInputNumber.Text(dateTime.Day.ToString());
        _timeHourInputNumber.Text(dateTime.Hour.ToString());
        _timeMinuteInputNumber.Text(dateTime.Minute.ToString());
        _timeSecondsInputNumber.Text(dateTime.Second.ToString());
        _timeMillisecondsInputNumber.Text("0");

        await _tool.WorkTask;

        _timeStampInputText.Text.Should().Be(exceptedTimestamp.ToString());
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
        DateTimeOffset.TryParseExact(exceptedDateTimeString, "O", null, DateTimeStyles.None, out DateTimeOffset expectedDateTime);
        DateTimeOffset.TryParseExact(epochString, "O", null, DateTimeStyles.None, out DateTimeOffset epoch);

        var epochSettings = (IUISwitch)_toolView.GetChildElementById("timestamp-use-custom-epoch-switch");
        epochSettings.On();

        int timezoneIndex = Array.FindIndex(_timeZoneDropDownList.Items, w => w.Value.Equals(timeZoneString));
        _timeZoneDropDownList.Select(timezoneIndex);

        var formatDropdownList = _formatSetting.InteractiveElement as IUISelectDropDownList;
        int formatIndex = Array.FindIndex(formatDropdownList.Items, w => w.Value.Equals(DateFormat.Seconds));
        formatDropdownList.Select(formatIndex);

        _epochYearInputNumber.Text(epoch.Year.ToString());
        _epochMonthInputNumber.Text(epoch.Month.ToString());
        _epochDayInputNumber.Text(epoch.Day.ToString());
        _epochHourInputNumber.Text(epoch.Hour.ToString());
        _epochMinuteInputNumber.Text(epoch.Minute.ToString());
        _epochSecondsInputNumber.Text(epoch.Second.ToString());
        _epochMillisecondsInputNumber.Text(epoch.Millisecond.ToString());

        _timeStampInputText.Text(timestamp.ToString());

        await _tool.WorkTask;

        _timeYearInputNumber.Text.Should().Be(expectedDateTime.Year.ToString());
        _timeMonthInputNumber.Text.Should().Be(expectedDateTime.Month.ToString());
        _timeDayInputNumber.Text.Should().Be(expectedDateTime.Day.ToString());
        _timeHourInputNumber.Text.Should().Be(expectedDateTime.Hour.ToString());
        _timeMinuteInputNumber.Text.Should().Be(expectedDateTime.Minute.ToString());
        _timeSecondsInputNumber.Text.Should().Be(expectedDateTime.Second.ToString());
        _timeMillisecondsInputNumber.Text.Should().Be("0");
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
        DateTimeOffset.TryParseExact(exceptedDateTimeString, "O", null, DateTimeStyles.None, out DateTimeOffset expectedDateTime);
        var epochSettings = (IUISwitch)_toolView.GetChildElementById("timestamp-use-custom-epoch-switch");
        epochSettings.Off();

        int timezoneIndex = Array.FindIndex(_timeZoneDropDownList.Items, w => w.Value.Equals(timeZoneString));
        _timeZoneDropDownList.Select(timezoneIndex);

        var formatDropdownList = _formatSetting.InteractiveElement as IUISelectDropDownList;
        int formatIndex = Array.FindIndex(formatDropdownList.Items, w => w.Value.Equals(DateFormat.Seconds));
        formatDropdownList.Select(formatIndex);

        _timeStampInputText.Text(timestamp.ToString());

        await _tool.WorkTask;

        _timeYearInputNumber.Text.Should().Be(expectedDateTime.Year.ToString());
        _timeMonthInputNumber.Text.Should().Be(expectedDateTime.Month.ToString());
        _timeDayInputNumber.Text.Should().Be(expectedDateTime.Day.ToString());
        _timeHourInputNumber.Text.Should().Be(expectedDateTime.Hour.ToString());
        _timeMinuteInputNumber.Text.Should().Be(expectedDateTime.Minute.ToString());
        _timeSecondsInputNumber.Text.Should().Be(expectedDateTime.Second.ToString());
        _timeMillisecondsInputNumber.Text.Should().Be("0");
    }

    #endregion

    #region DateTimeMilliseconds

    [Theory(DisplayName = "Convert valid dateTime with custom epoch and Milliseconds format should return valid timestamp to Milliseconds")]
    [InlineData("1870-01-01T00:00:00.0000000Z", "1870-01-01T00:00:00.0000000Z", "UTC", 0)]
    [InlineData("1923-11-23T19:58:07.0000000Z", "1870-01-01T00:00:00.0000000Z", "UTC", 1700683087000)]
    [InlineData("1800-11-22T19:58:07.0000000Z", "1870-01-01T00:00:00.0000000Z", "UTC", -2180836912999)]
    [InlineData("1870-01-01T00:00:00.0000000Z", "1870-01-01T00:00:00.0000000Z", "Pacific Standard Time", 0)]
    [InlineData("1923-11-23T19:58:07.0000000Z", "1870-01-01T00:00:00.0000000Z", "Pacific Standard Time", 1700683087000)]
    [InlineData("1800-11-22T19:58:07.0000000Z", "1870-01-01T00:00:00.0000000Z", "Pacific Standard Time", -2180836912999)]
    public async Task ConvertValidDateTimeWithCustomEpochAndMillisecondsFormatShouldReturnValidTimestampInMilliseconds(
        string dateTimeString,
        string epochString,
        string timeZoneString,
        long exceptedTimestamp)
    {
        var dateTime = DateTimeOffset.Parse(dateTimeString, CultureInfo.InvariantCulture);
        var epoch = DateTimeOffset.Parse(epochString, CultureInfo.InvariantCulture);

        var epochSettings = (IUISwitch)_toolView.GetChildElementById("timestamp-use-custom-epoch-switch");
        epochSettings.On();

        int timezoneIndex = Array.FindIndex(_timeZoneDropDownList.Items, w => w.Value.Equals(timeZoneString));
        _timeZoneDropDownList.Select(timezoneIndex);

        var formatDropdownList = _formatSetting.InteractiveElement as IUISelectDropDownList;
        int formatIndex = Array.FindIndex(formatDropdownList.Items, w => w.Value.Equals(DateFormat.Milliseconds));
        formatDropdownList.Select(formatIndex);

        _epochYearInputNumber.Text(epoch.Year.ToString());
        _epochMonthInputNumber.Text(epoch.Month.ToString());
        _epochDayInputNumber.Text(epoch.Day.ToString());
        _epochHourInputNumber.Text(epoch.Hour.ToString());
        _epochMinuteInputNumber.Text(epoch.Minute.ToString());
        _epochSecondsInputNumber.Text(epoch.Second.ToString());
        _epochMillisecondsInputNumber.Text(epoch.Millisecond.ToString());

        _timeYearInputNumber.Text(dateTime.Year.ToString());
        _timeMonthInputNumber.Text(dateTime.Month.ToString());
        _timeDayInputNumber.Text(dateTime.Day.ToString());
        _timeHourInputNumber.Text(dateTime.Hour.ToString());
        _timeMinuteInputNumber.Text(dateTime.Minute.ToString());
        _timeSecondsInputNumber.Text(dateTime.Second.ToString());
        _timeMillisecondsInputNumber.Text(dateTime.Millisecond.ToString());

        await _tool.WorkTask;

        _timeStampInputText.Text.Should().Be(exceptedTimestamp.ToString());
    }

    [Theory(DisplayName = "Convert valid dateTime with unix epoch and Milliseconds format should return valid timestamp to Milliseconds")]
    [InlineData("1970-01-01T00:00:00.0000000Z", "UTC", 0)]
    [InlineData("2023-11-22T19:58:07.0000000Z", "UTC", 1700683087000)]
    [InlineData("1900-11-22T19:58:07.0000000Z", "UTC", -2180836912999)]
    [InlineData("1970-01-01T00:00:00.0000000Z", "Pacific Standard Time", 0)]
    [InlineData("2023-11-22T19:58:07.0000000Z", "Pacific Standard Time", 1700683087000)]
    [InlineData("1900-11-22T19:58:07.0000000Z", "Pacific Standard Time", -2180836912999)]
    public async Task ConvertValidDateTimeWithUnixEpochAndMillisecondsFormatShouldReturnValidTimestampInMilliseconds(
        string dateTimeString,
        string timeZoneString,
        long exceptedTimestamp)
    {
        var epochSettings = (IUISwitch)_toolView.GetChildElementById("timestamp-use-custom-epoch-switch");
        epochSettings.Off();

        int timezoneIndex = Array.FindIndex(_timeZoneDropDownList.Items, w => w.Value.Equals(timeZoneString));
        _timeZoneDropDownList.Select(timezoneIndex);

        var formatDropdownList = _formatSetting.InteractiveElement as IUISelectDropDownList;
        int formatIndex = Array.FindIndex(formatDropdownList.Items, w => w.Value.Equals(DateFormat.Milliseconds));
        formatDropdownList.Select(formatIndex);

        var dateTime = DateTimeOffset.Parse(dateTimeString);
        _timeYearInputNumber.Text(dateTime.Year.ToString());
        _timeMonthInputNumber.Text(dateTime.Month.ToString());
        _timeDayInputNumber.Text(dateTime.Day.ToString());
        _timeHourInputNumber.Text(dateTime.Hour.ToString());
        _timeMinuteInputNumber.Text(dateTime.Minute.ToString());
        _timeSecondsInputNumber.Text(dateTime.Second.ToString());
        _timeMillisecondsInputNumber.Text(dateTime.Millisecond.ToString());

        await _tool.WorkTask;

        _timeStampInputText.Value.Should().Be(exceptedTimestamp);
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
        var expectedDateTime = DateTimeOffset.Parse(exceptedDateTimeString, CultureInfo.InvariantCulture);
        var epoch = DateTimeOffset.Parse(epochString, CultureInfo.InvariantCulture);

        var epochSettings = (IUISwitch)_toolView.GetChildElementById("timestamp-use-custom-epoch-switch");
        epochSettings.On();

        int timezoneIndex = Array.FindIndex(_timeZoneDropDownList.Items, w => w.Value.Equals(timeZoneString));
        _timeZoneDropDownList.Select(timezoneIndex);

        var formatDropdownList = _formatSetting.InteractiveElement as IUISelectDropDownList;
        int formatIndex = Array.FindIndex(formatDropdownList.Items, w => w.Value.Equals(DateFormat.Milliseconds));
        formatDropdownList.Select(formatIndex);

        _epochYearInputNumber.Text(epoch.Year.ToString());
        _epochMonthInputNumber.Text(epoch.Month.ToString());
        _epochDayInputNumber.Text(epoch.Day.ToString());
        _epochHourInputNumber.Text(epoch.Hour.ToString());
        _epochMinuteInputNumber.Text(epoch.Minute.ToString());
        _epochSecondsInputNumber.Text(epoch.Second.ToString());
        _epochMillisecondsInputNumber.Text(epoch.Millisecond.ToString());

        _timeStampInputText.Text(timestamp.ToString());

        await _tool.WorkTask;

        _timeYearInputNumber.Text.Should().Be(expectedDateTime.Year.ToString());
        _timeMonthInputNumber.Text.Should().Be(expectedDateTime.Month.ToString());
        _timeDayInputNumber.Text.Should().Be(expectedDateTime.Day.ToString());
        _timeHourInputNumber.Text.Should().Be(expectedDateTime.Hour.ToString());
        _timeMinuteInputNumber.Text.Should().Be(expectedDateTime.Minute.ToString());
        _timeSecondsInputNumber.Text.Should().Be(expectedDateTime.Second.ToString());
        _timeMillisecondsInputNumber.Text.Should().Be("0");
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
        var expectedDateTime = DateTimeOffset.Parse(exceptedDateTimeString);

        int timezoneIndex = Array.FindIndex(_timeZoneDropDownList.Items, w => w.Value.Equals(timeZoneString));
        _timeZoneDropDownList.Select(timezoneIndex);

        var formatDropdownList = _formatSetting.InteractiveElement as IUISelectDropDownList;
        int formatIndex = Array.FindIndex(formatDropdownList.Items, w => w.Value.Equals(DateFormat.Milliseconds));
        formatDropdownList.Select(formatIndex);

        _timeStampInputText.Text(timestamp.ToString());

        await _tool.WorkTask;

        _timeYearInputNumber.Text.Should().Be(expectedDateTime.Year.ToString());
        _timeMonthInputNumber.Text.Should().Be(expectedDateTime.Month.ToString());
        _timeDayInputNumber.Text.Should().Be(expectedDateTime.Day.ToString());
        _timeHourInputNumber.Text.Should().Be(expectedDateTime.Hour.ToString());
        _timeMinuteInputNumber.Text.Should().Be(expectedDateTime.Minute.ToString());
        _timeSecondsInputNumber.Text.Should().Be(expectedDateTime.Second.ToString());
        _timeMillisecondsInputNumber.Text.Should().Be("0");
    }

    #endregion

    [Theory(DisplayName = "Convert valid dateTime should return valid Ticks")]
    [InlineData("1870-01-01T00:00:00.0000000+00:00", "UTC", 589799232000000000)]
    [InlineData("1923-11-23T19:58:07.0000000+00:00", "UTC", 606806062870000000)]
    [InlineData("1800-11-22T19:58:07.0000000+00:00", "UTC", 567990862870000000)]
    [InlineData("1870-01-01T00:00:00.0000000-08:00", "Pacific Standard Time", 589799520000000582)]
    [InlineData("1870-01-01T00:00:00.0000000+00:00", "Pacific Standard Time", 589799232000000000)]
    [InlineData("1923-11-23T19:58:07.0000000Z", "Pacific Standard Time", 606806062870000000)]
    [InlineData("1800-11-22T19:58:07.0000000Z", "Pacific Standard Time", 567990574870000000)]
    public async Task ConvertValidDateTimeShouldReturnValidTicks(
        string dateTimeString,
        string timeZoneString,
        long exceptedTicks)
    {
        DateTimeOffset.TryParseExact(dateTimeString, "O", null, DateTimeStyles.None, out DateTimeOffset dateTimeOffset);

        var epochSettings = (IUISwitch)_toolView.GetChildElementById("timestamp-use-custom-epoch-switch");
        epochSettings.Off();

        int timezoneIndex = Array.FindIndex(_timeZoneDropDownList.Items, w => w.Value.Equals(timeZoneString));
        _timeZoneDropDownList.Select(timezoneIndex);

        var formatDropdownList = _formatSetting.InteractiveElement as IUISelectDropDownList;
        int formatIndex = Array.FindIndex(formatDropdownList.Items, w => w.Value.Equals(DateFormat.Ticks));
        formatDropdownList.Select(formatIndex);

        _timeYearInputNumber.Value(dateTimeOffset.Year);
        _timeMonthInputNumber.Value(dateTimeOffset.Month);
        _timeDayInputNumber.Value(dateTimeOffset.Day);
        _timeHourInputNumber.Value(dateTimeOffset.Hour);
        _timeMinuteInputNumber.Value(dateTimeOffset.Minute);
        _timeSecondsInputNumber.Value(dateTimeOffset.Second);
        _timeMillisecondsInputNumber.Value(dateTimeOffset.Millisecond);

        await _tool.WorkTask;

        _timeStampInputText.Text.Should().Be(exceptedTicks.ToString());
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
        var expectedDateTime = DateTimeOffset.Parse(exceptedDateTimeString, CultureInfo.InvariantCulture);

        var epochSettings = (IUISwitch)_toolView.GetChildElementById("timestamp-use-custom-epoch-switch");
        epochSettings.Off();

        int timezoneIndex = Array.FindIndex(_timeZoneDropDownList.Items, w => w.Value.Equals(timeZoneString));
        _timeZoneDropDownList.Select(timezoneIndex);

        var formatDropdownList = _formatSetting.InteractiveElement as IUISelectDropDownList;
        int formatIndex = Array.FindIndex(formatDropdownList.Items, w => w.Value.Equals(DateFormat.Ticks));
        formatDropdownList.Select(formatIndex);

        _timeStampInputText.Text(ticks.ToString());

        await _tool.WorkTask;

        _timeYearInputNumber.Text.Should().Be(expectedDateTime.Year.ToString());
        _timeMonthInputNumber.Text.Should().Be(expectedDateTime.Month.ToString());
        _timeDayInputNumber.Text.Should().Be(expectedDateTime.Day.ToString());
        _timeHourInputNumber.Text.Should().Be(expectedDateTime.Hour.ToString());
        _timeMinuteInputNumber.Text.Should().Be(expectedDateTime.Minute.ToString());
        _timeSecondsInputNumber.Text.Should().Be(expectedDateTime.Second.ToString());
        _timeMillisecondsInputNumber.Text.Should().Be("0");
    }

}
