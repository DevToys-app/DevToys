using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using DevToys.Tools.Helpers;
using DevToys.Tools.Models;

namespace DevToys.UnitTests.Tools.Helpers;

public class DateHelperTests
{
    #region ConvertToLong

    [Theory]
    [InlineData("1970-01-01T00:00:00.000Z", 0, DateFormat.Milliseconds)]
    [InlineData("2023-11-22T19:58:07.000Z", 1700683087000, DateFormat.Milliseconds)]
    [InlineData("1900-11-22T19:58:07.000Z", -2180836913000, DateFormat.Milliseconds)]
    [InlineData("1970-01-01T00:00:00.000Z", 0, DateFormat.Seconds)]
    [InlineData("2023-11-22T19:58:07.000Z", 1700683087, DateFormat.Seconds)]
    [InlineData("1900-11-22T19:58:07.000Z", -2180836913, DateFormat.Seconds)]
    [InlineData("1970-01-01T00:00:00.000Z", 621355968000000000, DateFormat.Ticks)]
    [InlineData("2023-11-22T19:58:07.000Z", 638362798870000000, DateFormat.Ticks)]
    [InlineData("2022-06-15T03:23:50.000Z", 637908602300000000, DateFormat.Ticks)]
    [InlineData("1900-11-22T19:58:07.000Z", 599547598870000000, DateFormat.Ticks)]
    public async Task ConvertToLongUtcUsingUnixEpochAsync(string dateTimeString, long timestamp, DateFormat format)
    {
        var dateTime = DateTimeOffset.Parse(dateTimeString, CultureInfo.InvariantCulture);
        DateTimeOffset epoch = DateTimeOffset.UnixEpoch;

        ToolResult<long> result = await DateHelper.ConvertToLongAsync(dateTime, epoch, format, CancellationToken.None);
        result.Data.Should().Be(timestamp);
    }

    [Theory]
    [InlineData("1970-01-01T00:00:00.000Z", 0, DateFormat.Milliseconds)]
    [InlineData("2023-11-22T19:58:07.000Z", 1700683087000, DateFormat.Milliseconds)]
    [InlineData("1900-11-22T19:58:07.000Z", -2180836913000, DateFormat.Milliseconds)]
    [InlineData("1970-01-01T00:00:00.000Z", 0, DateFormat.Seconds)]
    [InlineData("2023-11-22T19:58:07.000Z", 1700683087, DateFormat.Seconds)]
    [InlineData("1900-11-22T19:58:07.000Z", -2180836913, DateFormat.Seconds)]
    [InlineData("1970-01-01T00:00:00.000Z", 621355968000000000, DateFormat.Ticks)]
    [InlineData("2023-11-22T19:58:07.000Z", 638362798870000000, DateFormat.Ticks)]
    [InlineData("2022-06-15T03:23:50.000Z", 637908602300000000, DateFormat.Ticks)]
    [InlineData("1900-11-22T19:58:07.000Z", 599547598870000000, DateFormat.Ticks)]
    public async Task ConvertToLongLocalUsingUnixEpochAsync(string dateTimeString, long timestamp, DateFormat format)
    {
        var dateTime = DateTimeOffset.Parse(dateTimeString, CultureInfo.CurrentCulture);
        DateTimeOffset epoch = DateTimeOffset.UnixEpoch;

        ToolResult<long> result = await DateHelper.ConvertToLongAsync(dateTime, epoch, format, CancellationToken.None);
        result.Data.Should().Be(timestamp);
    }

    [Theory]
    [InlineData("1970-01-01T00:00:00.000Z", "1870-01-01T00:00:00.000Z", 3155673600000L, DateFormat.Milliseconds)]
    [InlineData("2023-11-22T19:58:07.000Z", "1870-01-01T00:00:00.000Z", 4856356687000L, DateFormat.Milliseconds)]
    [InlineData("1900-11-22T19:58:07.000Z", "1870-01-01T00:00:00.000Z", 974836687000L, DateFormat.Milliseconds)]
    [InlineData("1970-01-01T00:00:00.000Z", "1870-01-01T00:00:00.000Z", 3155673600L, DateFormat.Seconds)]
    [InlineData("2023-11-22T19:58:07.000Z", "1870-01-01T00:00:00.000Z", 4856356687L, DateFormat.Seconds)]
    [InlineData("1900-11-22T19:58:07.000Z", "1870-01-01T00:00:00.000Z", 974836687L, DateFormat.Seconds)]
    [InlineData("1970-01-01T00:00:00.000Z", "1870-01-01T00:00:00.000Z", 621355968000000000, DateFormat.Ticks)]
    [InlineData("2023-11-22T19:58:07.000Z", "1870-01-01T00:00:00.000Z", 638362798870000000, DateFormat.Ticks)]
    [InlineData("2022-06-15T03:23:50.000Z", "1870-01-01T00:00:00.000Z", 637908602300000000, DateFormat.Ticks)]
    [InlineData("1900-11-22T19:58:07.000Z", "1970-01-01T00:00:00.000Z", 599547598870000000, DateFormat.Ticks)]
    public async Task ConvertToLongUtcUsingCustomEpochAsync(string dateTimeString, string epochString, long timestamp, DateFormat format)
    {
        var dateTime = DateTimeOffset.Parse(dateTimeString, CultureInfo.InvariantCulture);
        var epoch = DateTimeOffset.Parse(epochString, CultureInfo.InvariantCulture);

        ToolResult<long> result = await DateHelper.ConvertToLongAsync(dateTime, epoch, format, CancellationToken.None);
        result.Data.Should().Be(timestamp);
    }

    [Theory]
    [InlineData("1970-01-01T00:00:00.000Z", "1870-01-01T00:00:00.000Z", 3155673600000L, DateFormat.Milliseconds)]
    [InlineData("2023-11-22T19:58:07.000Z", "1870-01-01T00:00:00.000Z", 4856356687000L, DateFormat.Milliseconds)]
    [InlineData("1900-11-22T19:58:07.000Z", "1870-01-01T00:00:00.000Z", 974836687000L, DateFormat.Milliseconds)]
    [InlineData("1970-01-01T00:00:00.000Z", "1870-01-01T00:00:00.000Z", 3155673600L, DateFormat.Seconds)]
    [InlineData("2023-11-22T19:58:07.000Z", "1870-01-01T00:00:00.000Z", 4856356687L, DateFormat.Seconds)]
    [InlineData("1900-11-22T19:58:07.000Z", "1870-01-01T00:00:00.000Z", 974836687L, DateFormat.Seconds)]
    [InlineData("1970-01-01T00:00:00.000Z", "1870-01-01T00:00:00.000Z", 621355968000000000, DateFormat.Ticks)]
    [InlineData("2023-11-22T19:58:07.000Z", "1870-01-01T00:00:00.000Z", 638362798870000000, DateFormat.Ticks)]
    [InlineData("2022-06-15T03:23:50.000Z", "1870-01-01T00:00:00.000Z", 637908602300000000, DateFormat.Ticks)]
    [InlineData("1900-11-22T19:58:07.000Z", "1970-01-01T00:00:00.000Z", 599547598870000000, DateFormat.Ticks)]
    public async Task ConvertToLongLocalUsingCustomEpochAsync(string dateTimeString, string epochString, long timestamp, DateFormat format)
    {
        var dateTime = DateTimeOffset.Parse(dateTimeString, CultureInfo.CurrentCulture);
        var epoch = DateTimeOffset.Parse(epochString, CultureInfo.InvariantCulture);

        ToolResult<long> result = await DateHelper.ConvertToLongAsync(dateTime, epoch, format, CancellationToken.None);
        result.Data.Should().Be(timestamp);
    }

    #endregion

    #region ConvertToDateTime

    [Theory]
    [InlineData("1970-01-01T00:00:00.000Z", 0, DateFormat.Milliseconds)]
    [InlineData("2023-11-22T19:58:07.000Z", 1700683087000, DateFormat.Milliseconds)]
    [InlineData("1900-11-22T19:58:07.000Z", -2180836913000, DateFormat.Milliseconds)]
    [InlineData("1970-01-01T00:00:00.000Z", 0, DateFormat.Seconds)]
    [InlineData("2023-11-22T19:58:07.000Z", 1700683087, DateFormat.Seconds)]
    [InlineData("1900-11-22T19:58:07.000Z", -2180836913, DateFormat.Seconds)]
    [InlineData("1970-01-01T00:00:00.000Z", 621355968000000000, DateFormat.Ticks)]
    [InlineData("2023-11-22T19:58:07.000Z", 638362798870000000, DateFormat.Ticks)]
    [InlineData("2022-06-15T03:23:50.000Z", 637908602300000000, DateFormat.Ticks)]
    [InlineData("1900-11-22T19:58:07.000Z", 599547598870000000, DateFormat.Ticks)]
    public async Task ConvertToDateTimeUtcUsingUnixEpoch(string dateTimeString, long timestamp, DateFormat format)
    {
        var dateTime = DateTimeOffset.Parse(dateTimeString, CultureInfo.InvariantCulture);
        DateTimeOffset epoch = DateTimeOffset.UnixEpoch;

        ToolResult<DateTimeOffset> result = await DateHelper.ConvertToDateTimeUtcAsync(timestamp, epoch, format, CancellationToken.None);
        result.Data.Should().Be(dateTime);
    }

    [Theory]
    [InlineData("1970-01-01T00:00:00.000Z", 0, DateFormat.Milliseconds)]
    [InlineData("2023-11-22T19:58:07.000Z", 1700683087000, DateFormat.Milliseconds)]
    [InlineData("1900-11-22T19:58:07.000Z", -2180836913000, DateFormat.Milliseconds)]
    [InlineData("1970-01-01T00:00:00.000Z", 0, DateFormat.Seconds)]
    [InlineData("2023-11-22T19:58:07.000Z", 1700683087, DateFormat.Seconds)]
    [InlineData("1900-11-22T19:58:07.000Z", -2180836913, DateFormat.Seconds)]
    [InlineData("1970-01-01T00:00:00.000Z", 621355968000000000, DateFormat.Ticks)]
    [InlineData("2023-11-22T19:58:07.000Z", 638362798870000000, DateFormat.Ticks)]
    [InlineData("2022-06-15T03:23:50.000Z", 637908602300000000, DateFormat.Ticks)]
    [InlineData("1900-11-22T19:58:07.000Z", 599547598870000000, DateFormat.Ticks)]
    public async Task ConvertToDateTimeLocalUsingUnixEpoch(string dateTimeString, long timestamp, DateFormat format)
    {
        var dateTime = DateTimeOffset.Parse(dateTimeString, CultureInfo.CurrentCulture);
        DateTimeOffset epoch = DateTimeOffset.UnixEpoch;

        ToolResult<DateTimeOffset> result = await DateHelper.ConvertToDateTimeUtcAsync(timestamp, epoch, format, CancellationToken.None);
        result.Data.Should().Be(dateTime);
    }

    [Theory]
    [InlineData("1970-01-01T00:00:00.000Z", "1870-01-01T00:00:00.000Z", 3155673600000L, DateFormat.Milliseconds)]
    [InlineData("2023-11-22T19:58:07.000Z", "1870-01-01T00:00:00.000Z", 4856356687000L, DateFormat.Milliseconds)]
    [InlineData("1900-11-22T19:58:07.000Z", "1870-01-01T00:00:00.000Z", 974836687000L, DateFormat.Milliseconds)]
    [InlineData("1970-01-01T00:00:00.000Z", "1870-01-01T00:00:00.000Z", 3155673600L, DateFormat.Seconds)]
    [InlineData("2023-11-22T19:58:07.000Z", "1870-01-01T00:00:00.000Z", 4856356687L, DateFormat.Seconds)]
    [InlineData("1900-11-22T19:58:07.000Z", "1870-01-01T00:00:00.000Z", 974836687L, DateFormat.Seconds)]
    [InlineData("1970-01-01T00:00:00.000Z", "1870-01-01T00:00:00.000Z", 621355968000000000, DateFormat.Ticks)]
    [InlineData("2023-11-22T19:58:07.000Z", "1870-01-01T00:00:00.000Z", 638362798870000000, DateFormat.Ticks)]
    [InlineData("2022-06-15T03:23:50.000Z", "1870-01-01T00:00:00.000Z", 637908602300000000, DateFormat.Ticks)]
    [InlineData("1900-11-22T19:58:07.000Z", "1970-01-01T00:00:00.000Z", 599547598870000000, DateFormat.Ticks)]
    public async Task ConvertToDateTimeUtcUsingCustomEpoch(string dateTimeString, string epochString, long timestamp, DateFormat format)
    {
        var dateTime = DateTimeOffset.Parse(dateTimeString, CultureInfo.InvariantCulture);
        var epoch = DateTimeOffset.Parse(epochString, CultureInfo.InvariantCulture);

        ToolResult<DateTimeOffset> result = await DateHelper.ConvertToDateTimeUtcAsync(timestamp, epoch, format, CancellationToken.None);
        result.HasSucceeded.Should().BeTrue();
        result.Data.Should().Be(dateTime);
    }

    [Theory]
    [InlineData("1970-01-01T00:00:00.000Z", "1870-01-01T00:00:00.000Z", 3155673600000L, DateFormat.Milliseconds)]
    [InlineData("2023-11-22T19:58:07.000Z", "1870-01-01T00:00:00.000Z", 4856356687000L, DateFormat.Milliseconds)]
    [InlineData("1900-11-22T19:58:07.000Z", "1870-01-01T00:00:00.000Z", 974836687000L, DateFormat.Milliseconds)]
    [InlineData("1970-01-01T00:00:00.000Z", "1870-01-01T00:00:00.000Z", 3155673600L, DateFormat.Seconds)]
    [InlineData("2023-11-22T19:58:07.000Z", "1870-01-01T00:00:00.000Z", 4856356687L, DateFormat.Seconds)]
    [InlineData("1900-11-22T19:58:07.000Z", "1870-01-01T00:00:00.000Z", 974836687L, DateFormat.Seconds)]
    [InlineData("1970-01-01T00:00:00.000Z", "1870-01-01T00:00:00.000Z", 621355968000000000, DateFormat.Ticks)]
    [InlineData("2023-11-22T19:58:07.000Z", "1870-01-01T00:00:00.000Z", 638362798870000000, DateFormat.Ticks)]
    [InlineData("2022-06-15T03:23:50.000Z", "1870-01-01T00:00:00.000Z", 637908602300000000, DateFormat.Ticks)]
    [InlineData("1900-11-22T19:58:07.000Z", "1970-01-01T00:00:00.000Z", 599547598870000000, DateFormat.Ticks)]
    public async Task ConvertToDateTimeLocalUsingCustomEpoch(string dateTimeString, string epochString, long timestamp, DateFormat format)
    {
        var dateTime = DateTimeOffset.Parse(dateTimeString, CultureInfo.CurrentCulture);
        var epoch = DateTimeOffset.Parse(epochString, CultureInfo.InvariantCulture);

        ToolResult<DateTimeOffset> result = await DateHelper.ConvertToDateTimeUtcAsync(timestamp, epoch, format, CancellationToken.None);
        result.HasSucceeded.Should().BeTrue();
        result.Data.Should().Be(dateTime);
    }

    #endregion

    [Fact]
    public void ChangeDateTimeWithInvalidValueShouldReturnDateTimeOffsetNow()
    {
        ToolResult<DateTimeOffset> result = DateHelper.ChangeDateTime(
            -1,
            DateTime.Parse("1970-01-01T00:00:00.000Z"),
            TimeZoneInfo.Utc,
            DateValueType.Year);
        result.HasSucceeded.Should().BeTrue();
        result.Data.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromSeconds(10));
    }

    [Theory]
    [InlineData("1970-01-01T00:00:00.000Z", 1971, "1971-01-01T00:00:00.000Z", DateValueType.Year)]
    [InlineData("2023-02-28T00:00:00.000Z", 2024, "2024-02-28T00:00:00.000Z", DateValueType.Year)]
    [InlineData("2024-02-29T00:00:00.000Z", 2023, "2023-02-28T00:00:00.000Z", DateValueType.Year)]
    [InlineData("2023-02-28T00:00:00.000Z", 04, "2023-04-28T00:00:00.000Z", DateValueType.Month)]
    [InlineData("2023-02-28T00:00:00.000Z", 05, "2023-02-05T00:00:00.000Z", DateValueType.Day)]
    [InlineData("2023-02-28T00:00:00.000Z", 15, "2023-02-28T15:00:00.000Z", DateValueType.Hour)]
    [InlineData("2023-02-28T15:00:00.000Z", 50, "2023-02-28T15:50:00.000Z", DateValueType.Minute)]
    [InlineData("2023-02-28T15:50:00.000Z", 80, "2023-02-28T15:51:20.000Z", DateValueType.Second)]
    [InlineData("2023-02-28T15:51:20.000Z", 1000, "2023-02-28T15:51:21.000Z", DateValueType.Millisecond)]
    [InlineData("2023-12-31T23:59:59.000Z", 1000, "2024-01-01T00:00:00.000Z", DateValueType.Millisecond)]
    public void ChangeDateTimeWithInvalidValueShouldReturnDateTimeOffsetChanged(
        string dateTimeString, int value, string expectedDateTimeString, DateValueType valueChanged)
    {
        var dateTime = DateTimeOffset.Parse(dateTimeString);
        var expectedDateTime = DateTimeOffset.Parse(expectedDateTimeString);
        TimeZoneInfo timeZone = TimeZoneInfo.Utc;

        ToolResult<DateTimeOffset> result = DateHelper.ChangeDateTime(value, dateTime, timeZone, valueChanged);
        result.HasSucceeded.Should().BeTrue();
        result.Data.Should().Be(expectedDateTime);
    }
}
