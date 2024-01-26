using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel.__Internals;
using DevToys.CLI;
using DevToys.Tools.Models;
using DevToys.Tools.Tools.Converters.Date;
using Microsoft.Extensions.Logging;

namespace DevToys.UnitTests.Tools.Converters;

[Collection(nameof(TestParallelizationDisabled))]
public sealed class DateConverterCommandLineToolTests : MefBasedTest
{
    private readonly StringWriter _consoleWriter = new();
    private readonly StringWriter _consoleErrorWriter = new();
    private readonly DateConverterCommandLineTool _tool;
    private readonly Mock<ILogger> _loggerMock;

    public DateConverterCommandLineToolTests()
        : base(typeof(DateConverterCommandLineTool).Assembly)
    {
        _tool = (DateConverterCommandLineTool)MefProvider.ImportMany<ICommandLineTool, CommandLineToolMetadata>()
            .Single(t => t.Metadata.InternalComponentName == "DateConverter").Value;

        _loggerMock = new Mock<ILogger>();
        Console.SetOut(_consoleWriter);
        Console.SetError(_consoleErrorWriter);
    }

    [Theory(DisplayName = "Convert with invalid input should return error exit code")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task ConvertJsonWithInvalidInputShouldReturnErrorExitCode(string input)
    {
        _tool.Input = input;
        _tool.FormatOption = DateFormat.Seconds;

        int result = await _tool.InvokeAsync(_loggerMock.Object, default);
        result.Should().Be(-1);
    }

    #region DateTimeSeconds

    [Theory(DisplayName = "Convert valid dateTime with custom epoch and Seconds format should return valid timestamp to Seconds")]
    [InlineData("1870-01-01T00:00:00.0000000Z", "1870-01-01T00:00:00.0000000Z", "Pacific Standard Time", 0)]
    [InlineData("1923-11-23T19:58:07.0000000Z", "1870-01-01T00:00:00.0000000Z", "Pacific Standard Time", 1700683087)]
    [InlineData("1800-11-22T19:58:07.0000000Z", "1870-01-01T00:00:00.0000000Z", "Pacific Standard Time", -2180836913)]
    [InlineData("1870-01-01T00:00:00.0000000Z", "1870-01-01T00:00:00.0000000Z", "UTC", 0)]
    [InlineData("1923-11-23T19:58:07.0000000Z", "1870-01-01T00:00:00.0000000Z", "UTC", 1700683087)]
    [InlineData("1800-11-22T19:58:07.0000000Z", "1870-01-01T00:00:00.0000000Z", "UTC", -2180836913)]
    public async Task ConvertValidDateTimeWithCustomEpochAndSecondsFormatShouldReturnValidTimestampInSeconds(
        string dateTimeString,
        string epochString,
        string timeZoneString,
        long exceptedTimestamp)
    {
        _tool.Input = dateTimeString;
        _tool.EpochOption = epochString;
        _tool.TimeZoneOption = timeZoneString;
        _tool.FormatOption = DateFormat.Seconds;

        int result = await _tool.InvokeAsync(_loggerMock.Object, default);

        result.Should().Be(0);

        string consoleOutput = _consoleWriter.ToString().Trim();
        consoleOutput.Should().Be(exceptedTimestamp.ToString());
    }

    [Theory(DisplayName = "Convert valid dateTime with unix epoch and Seconds format should return valid timestamp to Seconds")]
    [InlineData("1970-01-01T00:00:00.0000000Z", "Pacific Standard Time", 0)]
    [InlineData("2023-11-22T19:58:07.0000000Z", "Pacific Standard Time", 1700683087)]
    [InlineData("1900-11-22T19:58:07.0000000Z", "Pacific Standard Time", -2180836913)]
    [InlineData("1970-01-01T00:00:00.0000000Z", "UTC", 0)]
    [InlineData("2023-11-22T19:58:07.0000000Z", "UTC", 1700683087)]
    [InlineData("1900-11-22T19:58:07.0000000Z", "UTC", -2180836913)]
    public async Task ConvertValidDateTimeWithUnixEpochAndSecondsFormatShouldReturnValidTimestampInSeconds(
        string dateTimeString,
        string timeZoneString,
        long exceptedTimestamp)
    {
        _tool.Input = dateTimeString;
        _tool.TimeZoneOption = timeZoneString;
        _tool.FormatOption = DateFormat.Seconds;

        int result = await _tool.InvokeAsync(_loggerMock.Object, default);

        result.Should().Be(0);

        string consoleOutput = _consoleWriter.ToString().Trim();
        consoleOutput.Should().Be(exceptedTimestamp.ToString());
    }

    #endregion

    #region TimestampSeconds

    [Theory(DisplayName = "Convert valid timestamp with custom epoch and Seconds format should return valid dateTime")]
    [InlineData(0, "1870-01-01T00:00:00.0000000Z", "UTC", "1870-01-01T00:00:00.0000000+00:00")]
    [InlineData(1700683087, "1870-01-01T00:00:00.0000000Z", "UTC", "1923-11-23T19:58:07.0000000+00:00")]
    [InlineData(-2180836913, "1870-01-01T00:00:00.0000000Z", "UTC", "1800-11-22T19:58:07.0000000+00:00")]
    [InlineData(0, "1870-01-01T00:00:00.0000000Z", "Pacific Standard Time", "1869-12-31T16:00:00.0000000-08:00")]
    [InlineData(1700683087, "1870-01-01T00:00:00.0000000Z", "Pacific Standard Time", "1923-11-23T11:58:07.0000000-08:00")]
    [InlineData(-2180836913, "1870-01-01T00:00:00.0000000Z", "Pacific Standard Time", "1800-11-22T11:58:07.0000000-08:00")]
    public async Task ConvertValidTimestampWithCustomEpochAndSecondsFormatShouldReturnValidDateTime(
        long timestamp,
        string epochString,
        string timeZoneString,
        string exceptedDateTimeString)
    {
        _tool.Input = timestamp;
        _tool.EpochOption = epochString;
        _tool.TimeZoneOption = timeZoneString;
        _tool.FormatOption = DateFormat.Seconds;

        int result = await _tool.InvokeAsync(_loggerMock.Object, default);

        result.Should().Be(0);

        string consoleOutput = _consoleWriter.ToString().Trim();
        consoleOutput.Should().Be(exceptedDateTimeString);
    }

    [Theory(DisplayName = "Convert valid timestamp with unix epoch and Seconds format should return valid dateTime")]
    [InlineData(0, "UTC", "1970-01-01T00:00:00.0000000+00:00")]
    [InlineData(1700683087, "UTC", "2023-11-22T19:58:07.0000000+00:00")]
    [InlineData(-2180836913, "UTC", "1900-11-22T19:58:07.0000000+00:00")]
    [InlineData(0, "Pacific Standard Time", "1969-12-31T16:00:00.0000000-08:00")]
    [InlineData(1700683087, "Pacific Standard Time", "2023-11-22T11:58:07.0000000-08:00")]
    [InlineData(-2180836913, "Pacific Standard Time", "1900-11-22T11:58:07.0000000-08:00")]
    public async Task ConvertValidTimestampWithUnixEpochAndSecondsFormatShouldReturnValidDateTime(
    long timestamp,
    string timeZoneString,
    string exceptedDateTimeString)
    {
        _tool.Input = timestamp;
        _tool.TimeZoneOption = timeZoneString;
        _tool.FormatOption = DateFormat.Seconds;

        int result = await _tool.InvokeAsync(_loggerMock.Object, default);

        result.Should().Be(0);

        string consoleOutput = _consoleWriter.ToString().Trim();
        consoleOutput.Should().Be(exceptedDateTimeString);
    }

    #endregion

    #region DateTimeMilliseconds

    [Theory(DisplayName = "Convert valid dateTime with custom epoch and Milliseconds format should return valid timestamp to Milliseconds")]
    [InlineData("1870-01-01T00:00:00.0000000Z", "1870-01-01T00:00:00.0000000Z", "UTC", 0)]
    [InlineData("1923-11-23T19:58:07.0000000Z", "1870-01-01T00:00:00.0000000Z", "UTC", 1700683087000)]
    [InlineData("1800-11-22T19:58:07.0000000Z", "1870-01-01T00:00:00.0000000Z", "UTC", -2180836913000)]
    [InlineData("1870-01-01T00:00:00.0000000Z", "1870-01-01T00:00:00.0000000Z", "Pacific Standard Time", 0)]
    [InlineData("1923-11-23T19:58:07.0000000Z", "1870-01-01T00:00:00.0000000Z", "Pacific Standard Time", 1700683087000)]
    [InlineData("1800-11-22T19:58:07.0000000Z", "1870-01-01T00:00:00.0000000Z", "Pacific Standard Time", -2180836913000)]
    public async Task ConvertValidDateTimeWithCustomEpochAndMillisecondsFormatShouldReturnValidTimestampInMilliseconds(
        string dateTimeString,
        string epochString,
        string timeZoneString,
        long exceptedTimestamp)
    {
        _tool.Input = dateTimeString;
        _tool.EpochOption = epochString;
        _tool.TimeZoneOption = timeZoneString;
        _tool.FormatOption = DateFormat.Milliseconds;

        int result = await _tool.InvokeAsync(_loggerMock.Object, default);

        result.Should().Be(0);

        string consoleOutput = _consoleWriter.ToString().Trim();
        consoleOutput.Should().Be(exceptedTimestamp.ToString());
    }

    [Theory(DisplayName = "Convert valid dateTime with unix epoch and Milliseconds format should return valid timestamp to Milliseconds")]
    [InlineData("1970-01-01T00:00:00.0000000Z", "UTC", 0)]
    [InlineData("2023-11-22T19:58:07.0000000Z", "UTC", 1700683087000)]
    [InlineData("1900-11-22T19:58:07.0000000Z", "UTC", -2180836913000)]
    [InlineData("1970-01-01T00:00:00.0000000Z", "Pacific Standard Time", 0)]
    [InlineData("2023-11-22T19:58:07.0000000Z", "Pacific Standard Time", 1700683087000)]
    [InlineData("1900-11-22T19:58:07.0000000Z", "Pacific Standard Time", -2180836913000)]
    public async Task ConvertValidDateTimeWithUnixEpochAndMillisecondsFormatShouldReturnValidTimestampInMilliseconds(
        string dateTimeString,
        string timeZoneString,
        long exceptedTimestamp)
    {
        _tool.Input = dateTimeString;
        _tool.TimeZoneOption = timeZoneString;
        _tool.FormatOption = DateFormat.Milliseconds;

        int result = await _tool.InvokeAsync(_loggerMock.Object, default);

        result.Should().Be(0);

        string consoleOutput = _consoleWriter.ToString().Trim();
        consoleOutput.Should().Be(exceptedTimestamp.ToString());
    }

    #endregion

    #region TimestampMilliseconds

    [Theory(DisplayName = "Convert valid timestamp with custom epoch and Milliseconds format should return valid dateTime")]
    [InlineData(0, "1870-01-01T00:00:00.0000000Z", "UTC", "1870-01-01T00:00:00.0000000+00:00")]
    [InlineData(1700683087000, "1870-01-01T00:00:00.0000000Z", "UTC", "1923-11-23T19:58:07.0000000+00:00")]
    [InlineData(-2180836913000, "1870-01-01T00:00:00.0000000Z", "UTC", "1800-11-22T19:58:07.0000000+00:00")]
    [InlineData(0, "1870-01-01T00:00:00.0000000Z", "Pacific Standard Time", "1869-12-31T16:00:00.0000000-08:00")]
    [InlineData(1700683087000, "1870-01-01T00:00:00.0000000Z", "Pacific Standard Time", "1923-11-23T11:58:07.0000000-08:00")]
    [InlineData(-2180836913000, "1870-01-01T00:00:00.0000000Z", "Pacific Standard Time", "1800-11-22T11:58:07.0000000-08:00")]
    public async Task ConvertValidTimestampWithCustomEpochAndMillisecondsFormatShouldReturnValidDateTime(
        long timestamp,
        string epochString,
        string timeZoneString,
        string exceptedDateTimeString)
    {
        _tool.Input = timestamp;
        _tool.EpochOption = epochString;
        _tool.TimeZoneOption = timeZoneString;
        _tool.FormatOption = DateFormat.Milliseconds;

        int result = await _tool.InvokeAsync(_loggerMock.Object, default);

        result.Should().Be(0);

        string consoleOutput = _consoleWriter.ToString().Trim();
        consoleOutput.Should().Be(exceptedDateTimeString);
    }

    [Theory(DisplayName = "Convert valid timestamp with unix epoch and Milliseconds format should return valid dateTime")]
    [InlineData(0, "UTC", "1970-01-01T00:00:00.0000000+00:00")]
    [InlineData(1700683087000, "UTC", "2023-11-22T19:58:07.0000000+00:00")]
    [InlineData(-2180836913000, "UTC", "1900-11-22T19:58:07.0000000+00:00")]
    [InlineData(0, "Pacific Standard Time", "1969-12-31T16:00:00.0000000-08:00")]
    [InlineData(1700683087000, "Pacific Standard Time", "2023-11-22T11:58:07.0000000-08:00")]
    [InlineData(-2180836913000, "Pacific Standard Time", "1900-11-22T11:58:07.0000000-08:00")]
    public async Task ConvertValidTimestampWithUnixEpochAndMillisecondsFormatShouldReturnValidDateTime(
    long timestamp,
    string timeZoneString,
    string exceptedDateTimeString)
    {
        _tool.Input = timestamp;
        _tool.TimeZoneOption = timeZoneString;
        _tool.FormatOption = DateFormat.Milliseconds;

        int result = await _tool.InvokeAsync(_loggerMock.Object, default);

        result.Should().Be(0);

        string consoleOutput = _consoleWriter.ToString().Trim();
        consoleOutput.Should().Be(exceptedDateTimeString);
    }

    #endregion

    [Theory(DisplayName = "Convert valid dateTime should return valid Ticks")]
    [InlineData("1870-01-01T00:00:00.0000000Z", "UTC", 589799232000000000)]
    [InlineData("1923-11-23T19:58:07.0000000Z", "UTC", 606806062870000000)]
    [InlineData("1800-11-22T19:58:07.0000000Z", "UTC", 567990862870000000)]
    [InlineData("1870-01-01T00:00:00.0000000Z", "Pacific Standard Time", 589799232000000000)]
    [InlineData("1923-11-23T19:58:07.0000000Z", "Pacific Standard Time", 606806062870000000)]
    [InlineData("1800-11-22T19:58:07.0000000Z", "Pacific Standard Time", 567990862870000000)]
    public async Task ConvertValidDateTimeShouldReturnValidTicks(
        string dateTimeString,
        string timeZoneString,
        long exceptedTimestamp)
    {
        _tool.Input = dateTimeString;
        _tool.TimeZoneOption = timeZoneString;
        _tool.FormatOption = DateFormat.Ticks;

        int result = await _tool.InvokeAsync(_loggerMock.Object, default);

        result.Should().Be(0);

        string consoleOutput = _consoleWriter.ToString().Trim();
        consoleOutput.Should().Be(exceptedTimestamp.ToString());
    }

    [Theory(DisplayName = "Convert valid ticks should return valid dateTime")]
    [InlineData(0, "UTC", "0001-01-01T00:00:00.0000000+00:00")]
    [InlineData(638362798870000000, "UTC", "2023-11-22T19:58:07.0000000+00:00")]
    [InlineData(599547598870000000, "UTC", "1900-11-22T19:58:07.0000000+00:00")]
    [InlineData(0, "Pacific Standard Time", "0001-01-01T00:00:00.0000000+00:00")]
    [InlineData(638362798870000000, "Pacific Standard Time", "2023-11-22T11:58:07.0000000-08:00")]
    [InlineData(599547598870000000, "Pacific Standard Time", "1900-11-22T11:58:07.0000000-08:00")]
    public async Task ConvertValidTicksWithTicksFormatShouldReturnValidDateTime(
        long timestamp,
        string timeZoneString,
        string exceptedDateTimeString)
    {
        _tool.Input = timestamp;
        _tool.TimeZoneOption = timeZoneString;
        _tool.FormatOption = DateFormat.Ticks;

        int result = await _tool.InvokeAsync(_loggerMock.Object, default);

        result.Should().Be(0);

        string consoleOutput = _consoleWriter.ToString().Trim();
        consoleOutput.Should().Be(exceptedDateTimeString);
    }
}
