using System.Globalization;
using DevToys.Tools.Helpers;
using DevToys.Tools.Models;

namespace DevToys.UnitTests.Tools.Helpers;

public class DateHelperTests
{
    #region ConvertToLong

    [Theory]
    [InlineData("1970-01-01T00:00:00.000Z", 0)]
    [InlineData("2023-11-22T19:58:07.000Z", 1700683087)]
    [InlineData("1900-11-22T19:58:07.000Z", -2180836913)]
    public void ConvertToLongUtcUsingUnixEpochInSeconds(string dateTimeString, long timestamp)
    {
        var dateTime = DateTimeOffset.Parse(dateTimeString, CultureInfo.InvariantCulture);
        DateTimeOffset epoch = DateTimeOffset.UnixEpoch;

        ResultInfo<long> result = DateHelper.ConvertToLong(dateTime, epoch, DateFormat.Seconds);
        result.Data.Should().Be(timestamp);
    }

    [Theory]
    [InlineData("1970-01-01T00:00:00.000Z", 0)]
    [InlineData("2023-11-22T19:58:07.000Z", 1700683087000)]
    [InlineData("1900-11-22T19:58:07.000Z", -2180836913000)]
    public void ConvertToLongUtcUsingUnixEpochInMilliseconds(string dateTimeString, long timestamp)
    {
        var dateTime = DateTimeOffset.Parse(dateTimeString, CultureInfo.InvariantCulture);
        DateTimeOffset epoch = DateTimeOffset.UnixEpoch;

        ResultInfo<long> result = DateHelper.ConvertToLong(dateTime, epoch, DateFormat.Milliseconds);
        result.Data.Should().Be(timestamp);
    }

    [Theory]
    [InlineData("1970-01-01T00:00:00.000Z", 621355968000000000)]
    [InlineData("2023-11-22T19:58:07.000Z", 638362798870000000)]
    [InlineData("2022-06-15T03:23:50.000Z", 637908602300000000)]
    [InlineData("1900-11-22T19:58:07.000Z", 599547598870000000)]
    public void ConvertToLongUtcUsingUnixEpochInTicks(string dateTimeString, long timestamp)
    {
        var dateTime = DateTimeOffset.Parse(dateTimeString, CultureInfo.InvariantCulture);
        DateTimeOffset epoch = DateTimeOffset.UnixEpoch;

        ResultInfo<long> result = DateHelper.ConvertToLong(dateTime, epoch, DateFormat.Ticks);
        result.Data.Should().Be(timestamp);
    }

    [Theory]
    [InlineData("1970-01-01T00:00:00.000Z", 0)]
    [InlineData("2023-11-22T19:58:07.000Z", 1700683087)]
    [InlineData("1900-11-22T19:58:07.000Z", -2180836913)]
    public void ConvertToLongLocalUsingUnixEpochInSeconds(string dateTimeString, long timestamp)
    {
        var dateTime = DateTimeOffset.Parse(dateTimeString, CultureInfo.CurrentCulture);
        DateTimeOffset epoch = DateTimeOffset.UnixEpoch;

        ResultInfo<long> result = DateHelper.ConvertToLong(dateTime, epoch, DateFormat.Seconds);
        result.Data.Should().Be(timestamp);
    }

    [Theory]
    [InlineData("1970-01-01T00:00:00.000Z", 0)]
    [InlineData("2023-11-22T19:58:07.000Z", 1700683087000)]
    [InlineData("1900-11-22T19:58:07.000Z", -2180836913000)]
    public void ConvertToLongLocalUsingUnixEpochInMilliseconds(string dateTimeString, long timestamp)
    {
        var dateTime = DateTimeOffset.Parse(dateTimeString, CultureInfo.CurrentCulture);
        DateTimeOffset epoch = DateTimeOffset.UnixEpoch;

        ResultInfo<long> result = DateHelper.ConvertToLong(dateTime, epoch, DateFormat.Milliseconds);
        result.Data.Should().Be(timestamp);
    }

    [Theory]
    [InlineData("1970-01-01T00:00:00.000Z", 621355968000000000)]
    [InlineData("2023-11-22T19:58:07.000Z", 638362798870000000)]
    [InlineData("2022-06-15T03:23:50.000Z", 637908602300000000)]
    [InlineData("1900-11-22T19:58:07.000Z", 599547598870000000)]
    public void ConvertToLongLocalUsingUnixEpochInTicks(string dateTimeString, long timestamp)
    {
        var dateTime = DateTimeOffset.Parse(dateTimeString, CultureInfo.CurrentCulture);
        DateTimeOffset epoch = DateTimeOffset.UnixEpoch;

        ResultInfo<long> result = DateHelper.ConvertToLong(dateTime, epoch, DateFormat.Ticks);
        result.Data.Should().Be(timestamp);
    }

    [Theory]
    [InlineData("1970-01-01T00:00:00.000Z", "1870-01-01T00:00:00.000Z", 3155673600L)]
    [InlineData("2023-11-22T19:58:07.000Z", "1870-01-01T00:00:00.000Z", 4856356687L)]
    [InlineData("1900-11-22T19:58:07.000Z", "1870-01-01T00:00:00.000Z", 974836687L)]
    public void ConvertToLongUtcUsingCustomEpochInSeconds(string dateTimeString, string epochString, long timestamp)
    {
        var dateTime = DateTimeOffset.Parse(dateTimeString, CultureInfo.InvariantCulture);
        var epoch = DateTimeOffset.Parse(epochString, CultureInfo.InvariantCulture);

        ResultInfo<long> result = DateHelper.ConvertToLong(dateTime, epoch, DateFormat.Seconds);
        result.Data.Should().Be(timestamp);
    }

    [Theory]
    [InlineData("1970-01-01T00:00:00.000Z", "1870-01-01T00:00:00.000Z", 3155673600000L)]
    [InlineData("2023-11-22T19:58:07.000Z", "1870-01-01T00:00:00.000Z", 4856356687000L)]
    [InlineData("1900-11-22T19:58:07.000Z", "1870-01-01T00:00:00.000Z", 974836687000L)]
    public void ConvertToLongUtcUsingCustomEpochInMilliseconds(string dateTimeString, string epochString, long timestamp)
    {
        var dateTime = DateTimeOffset.Parse(dateTimeString, CultureInfo.InvariantCulture);
        var epoch = DateTimeOffset.Parse(epochString, CultureInfo.InvariantCulture);

        ResultInfo<long> result = DateHelper.ConvertToLong(dateTime, epoch, DateFormat.Milliseconds);
        result.Data.Should().Be(timestamp);
    }

    [Theory]
    [InlineData("1970-01-01T00:00:00.000Z", "1870-01-01T00:00:00.000Z", 621355968000000000)]
    [InlineData("2023-11-22T19:58:07.000Z", "1870-01-01T00:00:00.000Z", 638362798870000000)]
    [InlineData("2022-06-15T03:23:50.000Z", "1870-01-01T00:00:00.000Z", 637908602300000000)]
    [InlineData("1900-11-22T19:58:07.000Z", "1970-01-01T00:00:00.000Z", 599547598870000000)]
    public void ConvertToLongUtcUsingCustomEpochInTicks(string dateTimeString, string epochString, long timestamp)
    {
        var dateTime = DateTimeOffset.Parse(dateTimeString, CultureInfo.InvariantCulture);
        var epoch = DateTimeOffset.Parse(epochString, CultureInfo.InvariantCulture);

        ResultInfo<long> result = DateHelper.ConvertToLong(dateTime, epoch, DateFormat.Ticks);
        result.Data.Should().Be(timestamp);
    }

    [Theory]
    [InlineData("1970-01-01T00:00:00.000Z", "1870-01-01T00:00:00.000Z", 3155673600L)]
    [InlineData("2023-11-22T19:58:07.000Z", "1870-01-01T00:00:00.000Z", 4856356687L)]
    [InlineData("1900-11-22T19:58:07.000Z", "1870-01-01T00:00:00.000Z", 974836687L)]
    public void ConvertToLongLocalUsingCustomEpochInSeconds(string dateTimeString, string epochString, long timestamp)
    {
        var dateTime = DateTimeOffset.Parse(dateTimeString, CultureInfo.CurrentCulture);
        var epoch = DateTimeOffset.Parse(epochString, CultureInfo.InvariantCulture);

        ResultInfo<long> result = DateHelper.ConvertToLong(dateTime, epoch, DateFormat.Seconds);
        result.Data.Should().Be(timestamp);
    }

    [Theory]
    [InlineData("1970-01-01T00:00:00.000Z", "1870-01-01T00:00:00.000Z", 3155673600000L)]
    [InlineData("2023-11-22T19:58:07.000Z", "1870-01-01T00:00:00.000Z", 4856356687000L)]
    [InlineData("1900-11-22T19:58:07.000Z", "1870-01-01T00:00:00.000Z", 974836687000L)]
    public void ConvertToLongLocalUsingCustomEpochInMilliseconds(string dateTimeString, string epochString, long timestamp)
    {
        var dateTime = DateTimeOffset.Parse(dateTimeString, CultureInfo.CurrentCulture);
        var epoch = DateTimeOffset.Parse(epochString, CultureInfo.InvariantCulture);

        ResultInfo<long> result = DateHelper.ConvertToLong(dateTime, epoch, DateFormat.Milliseconds);
        result.Data.Should().Be(timestamp);
    }

    [Theory]
    [InlineData("1970-01-01T00:00:00.000Z", "1870-01-01T00:00:00.000Z", 621355968000000000)]
    [InlineData("2023-11-22T19:58:07.000Z", "1870-01-01T00:00:00.000Z", 638362798870000000)]
    [InlineData("2022-06-15T03:23:50.000Z", "1870-01-01T00:00:00.000Z", 637908602300000000)]
    [InlineData("1900-11-22T19:58:07.000Z", "1970-01-01T00:00:00.000Z", 599547598870000000)]
    public void ConvertToLongLocalUsingCustomEpochInTicks(string dateTimeString, string epochString, long timestamp)
    {
        var dateTime = DateTimeOffset.Parse(dateTimeString, CultureInfo.CurrentCulture);
        var epoch = DateTimeOffset.Parse(epochString, CultureInfo.InvariantCulture);

        ResultInfo<long> result = DateHelper.ConvertToLong(dateTime, epoch, DateFormat.Ticks);
        result.Data.Should().Be(timestamp);
    }

    #endregion

    #region ConvertToDateTime

    [Theory]
    [InlineData("1970-01-01T00:00:00.000Z", 0)]
    [InlineData("2023-11-22T19:58:07.000Z", 1700683087)]
    [InlineData("1900-11-22T19:58:07.000Z", -2180836913)]
    public void ConvertToDateTimeUtcUsingUnixEpochInSeconds(string dateTimeString, long timestamp)
    {
        var dateTime = DateTimeOffset.Parse(dateTimeString, CultureInfo.InvariantCulture);
        DateTimeOffset epoch = DateTimeOffset.UnixEpoch;

        ResultInfo<DateTimeOffset> result = DateHelper.ConvertToDateTimeUtc(timestamp, epoch, DateFormat.Seconds);
        result.Data.Should().Be(dateTime);
    }

    [Theory]
    [InlineData("1970-01-01T00:00:00.000Z", 0)]
    [InlineData("2023-11-22T19:58:07.000Z", 1700683087000)]
    [InlineData("1900-11-22T19:58:07.000Z", -2180836913000)]
    public void ConvertToDateTimeUtcUsingUnixEpochInMilliseconds(string dateTimeString, long timestamp)
    {
        var dateTime = DateTimeOffset.Parse(dateTimeString, CultureInfo.InvariantCulture);
        DateTimeOffset epoch = DateTimeOffset.UnixEpoch;

        ResultInfo<DateTimeOffset> result = DateHelper.ConvertToDateTimeUtc(timestamp, epoch, DateFormat.Milliseconds);
        result.Data.Should().Be(dateTime);
    }

    [Theory]
    [InlineData("1970-01-01T00:00:00.000Z", 621355968000000000)]
    [InlineData("2023-11-22T19:58:07.000Z", 638362798870000000)]
    [InlineData("2022-06-15T03:23:50.000Z", 637908602300000000)]
    [InlineData("1900-11-22T19:58:07.000Z", 599547598870000000)]
    public void ConvertToDateTimeUtcUsingUnixEpochInTicks(string dateTimeString, long timestamp)
    {
        var dateTime = DateTimeOffset.Parse(dateTimeString, CultureInfo.InvariantCulture);
        DateTimeOffset epoch = DateTimeOffset.UnixEpoch;

        ResultInfo<DateTimeOffset> result = DateHelper.ConvertToDateTimeUtc(timestamp, epoch, DateFormat.Ticks);
        result.Data.Should().Be(dateTime);
    }

    [Theory]
    [InlineData("1970-01-01T00:00:00.000Z", 0)]
    [InlineData("2023-11-22T19:58:07.000Z", 1700683087)]
    [InlineData("1900-11-22T19:58:07.000Z", -2180836913)]
    public void ConvertToDateTimeLocalUsingUnixEpochInSeconds(string dateTimeString, long timestamp)
    {
        var dateTime = DateTimeOffset.Parse(dateTimeString, CultureInfo.CurrentCulture);
        DateTimeOffset epoch = DateTimeOffset.UnixEpoch;

        ResultInfo<DateTimeOffset> result = DateHelper.ConvertToDateTimeUtc(timestamp, epoch, DateFormat.Seconds);
        result.Data.Should().Be(dateTime);
    }

    [Theory]
    [InlineData("1970-01-01T00:00:00.000Z", 0)]
    [InlineData("2023-11-22T19:58:07.000Z", 1700683087000)]
    [InlineData("1900-11-22T19:58:07.000Z", -2180836913000)]
    public void ConvertToDateTimeLocalUsingUnixEpochInMilliseconds(string dateTimeString, long timestamp)
    {
        var dateTime = DateTimeOffset.Parse(dateTimeString, CultureInfo.CurrentCulture);
        DateTimeOffset epoch = DateTimeOffset.UnixEpoch;

        ResultInfo<DateTimeOffset> result = DateHelper.ConvertToDateTimeUtc(timestamp, epoch, DateFormat.Milliseconds);
        result.Data.Should().Be(dateTime);
    }

    [Theory]
    [InlineData("1970-01-01T00:00:00.000Z", 621355968000000000)]
    [InlineData("2023-11-22T19:58:07.000Z", 638362798870000000)]
    [InlineData("2022-06-15T03:23:50.000Z", 637908602300000000)]
    [InlineData("1900-11-22T19:58:07.000Z", 599547598870000000)]
    public void ConvertToDateTimeLocalUsingUnixEpochInTicks(string dateTimeString, long timestamp)
    {
        var dateTime = DateTimeOffset.Parse(dateTimeString, CultureInfo.CurrentCulture);
        DateTimeOffset epoch = DateTimeOffset.UnixEpoch;

        ResultInfo<DateTimeOffset> result = DateHelper.ConvertToDateTimeUtc(timestamp, epoch, DateFormat.Ticks);
        result.Data.Should().Be(dateTime);
    }

    [Theory]
    [InlineData("1970-01-01T00:00:00.000Z", "1870-01-01T00:00:00.000Z", 3155673600L)]
    [InlineData("2023-11-22T19:58:07.000Z", "1870-01-01T00:00:00.000Z", 4856356687L)]
    [InlineData("1900-11-22T19:58:07.000Z", "1870-01-01T00:00:00.000Z", 974836687L)]
    public void ConvertToDateTimeUtcUsingCustomEpochInSeconds(string dateTimeString, string epochString, long timestamp)
    {
        var dateTime = DateTimeOffset.Parse(dateTimeString, CultureInfo.InvariantCulture);
        var epoch = DateTimeOffset.Parse(epochString, CultureInfo.InvariantCulture);

        ResultInfo<DateTimeOffset> result = DateHelper.ConvertToDateTimeUtc(timestamp, epoch, DateFormat.Seconds);
        result.HasSucceeded.Should().BeTrue();
        result.Data.Should().Be(dateTime);
    }

    [Theory]
    [InlineData("1970-01-01T00:00:00.000Z", "1870-01-01T00:00:00.000Z", 3155673600000L)]
    [InlineData("2023-11-22T19:58:07.000Z", "1870-01-01T00:00:00.000Z", 4856356687000L)]
    [InlineData("1900-11-22T19:58:07.000Z", "1870-01-01T00:00:00.000Z", 974836687000L)]
    public void ConvertToDateTimeUtcUsingCustomEpochInMilliseconds(string dateTimeString, string epochString, long timestamp)
    {
        var dateTime = DateTimeOffset.Parse(dateTimeString, CultureInfo.InvariantCulture);
        var epoch = DateTimeOffset.Parse(epochString, CultureInfo.InvariantCulture);

        ResultInfo<DateTimeOffset> result = DateHelper.ConvertToDateTimeUtc(timestamp, epoch, DateFormat.Milliseconds);
        result.HasSucceeded.Should().BeTrue();
        result.Data.Should().Be(dateTime);
    }

    [Theory]
    [InlineData("1970-01-01T00:00:00.000Z", "1870-01-01T00:00:00.000Z", 621355968000000000)]
    [InlineData("2023-11-22T19:58:07.000Z", "1870-01-01T00:00:00.000Z", 638362798870000000)]
    [InlineData("2022-06-15T03:23:50.000Z", "1870-01-01T00:00:00.000Z", 637908602300000000)]
    [InlineData("1900-11-22T19:58:07.000Z", "1970-01-01T00:00:00.000Z", 599547598870000000)]
    public void ConvertToDateTimeUtcUsingCustomEpochInTicks(string dateTimeString, string epochString, long timestamp)
    {
        var dateTime = DateTimeOffset.Parse(dateTimeString, CultureInfo.InvariantCulture);
        var epoch = DateTimeOffset.Parse(epochString, CultureInfo.InvariantCulture);

        ResultInfo<DateTimeOffset> result = DateHelper.ConvertToDateTimeUtc(timestamp, epoch, DateFormat.Ticks);
        result.HasSucceeded.Should().BeTrue();
        result.Data.Should().Be(dateTime);
    }

    [Theory]
    [InlineData("1970-01-01T00:00:00.000Z", "1870-01-01T00:00:00.000Z", 3155673600L)]
    [InlineData("2023-11-22T19:58:07.000Z", "1870-01-01T00:00:00.000Z", 4856356687L)]
    [InlineData("1900-11-22T19:58:07.000Z", "1870-01-01T00:00:00.000Z", 974836687L)]
    public void ConvertToDateTimeLocalUsingCustomEpochInSeconds(string dateTimeString, string epochString, long timestamp)
    {
        var dateTime = DateTimeOffset.Parse(dateTimeString, CultureInfo.CurrentCulture);
        var epoch = DateTimeOffset.Parse(epochString, CultureInfo.InvariantCulture);

        ResultInfo<DateTimeOffset> result = DateHelper.ConvertToDateTimeUtc(timestamp, epoch, DateFormat.Seconds);
        result.HasSucceeded.Should().BeTrue();
        result.Data.Should().Be(dateTime);
    }

    [Theory]
    [InlineData("1970-01-01T00:00:00.000Z", "1870-01-01T00:00:00.000Z", 3155673600000L)]
    [InlineData("2023-11-22T19:58:07.000Z", "1870-01-01T00:00:00.000Z", 4856356687000L)]
    [InlineData("1900-11-22T19:58:07.000Z", "1870-01-01T00:00:00.000Z", 974836687000L)]
    public void ConvertToDateTimeLocalUsingCustomEpochInMilliseconds(string dateTimeString, string epochString, long timestamp)
    {
        var dateTime = DateTimeOffset.Parse(dateTimeString, CultureInfo.CurrentCulture);
        var epoch = DateTimeOffset.Parse(epochString, CultureInfo.InvariantCulture);

        ResultInfo<DateTimeOffset> result = DateHelper.ConvertToDateTimeUtc(timestamp, epoch, DateFormat.Milliseconds);
        result.HasSucceeded.Should().BeTrue();
        result.Data.Should().Be(dateTime);
    }

    [Theory]
    [InlineData("1970-01-01T00:00:00.000Z", "1870-01-01T00:00:00.000Z", 621355968000000000)]
    [InlineData("2023-11-22T19:58:07.000Z", "1870-01-01T00:00:00.000Z", 638362798870000000)]
    [InlineData("2022-06-15T03:23:50.000Z", "1870-01-01T00:00:00.000Z", 637908602300000000)]
    [InlineData("1900-11-22T19:58:07.000Z", "1970-01-01T00:00:00.000Z", 599547598870000000)]
    public void ConvertToDateTimeLocalUsingCustomEpochInTicks(string dateTimeString, string epochString, long timestamp)
    {
        var dateTime = DateTimeOffset.Parse(dateTimeString, CultureInfo.CurrentCulture);
        var epoch = DateTimeOffset.Parse(epochString, CultureInfo.InvariantCulture);

        ResultInfo<DateTimeOffset> result = DateHelper.ConvertToDateTimeUtc(timestamp, epoch, DateFormat.Ticks);
        result.HasSucceeded.Should().BeTrue();
        result.Data.Should().Be(dateTime);
    }

    #endregion

    [Fact]
    public void ChangeDateTimeWithInvalidValueShouldReturnDateTimeOffsetNow()
    {
        ResultInfo<DateTimeOffset> result = DateHelper.ChangeDateTime(
            -1,
            DateTime.Parse("1970-01-01T00:00:00.000Z"),
            TimeZoneInfo.Utc,
            DateValueType.Year);
        result.HasSucceeded.Should().BeTrue();
        result.Data.Should().BeCloseTo(DateTimeOffset.Now, TimeSpan.FromSeconds(10));
    }

    [Theory]
    [InlineData("1970-01-01T00:00:00.000Z", 1971, "1971-01-01T00:00:00.000Z")]
    [InlineData("2023-02-28T00:00:00.000Z", 2024, "2024-02-28T00:00:00.000Z")]
    [InlineData("2024-02-29T00:00:00.000Z", 2023, "2023-02-28T00:00:00.000Z")]
    public void ChangeDateTimeWithInvalidValueShouldReturnDateTimeOffsetChangedInYear(
        string dateTimeString, int value, string expectedDateTimeString)
    {
        var dateTime = DateTimeOffset.Parse(dateTimeString);
        var expectedDateTime = DateTimeOffset.Parse(expectedDateTimeString);
        TimeZoneInfo timeZone = TimeZoneInfo.Utc;

        ResultInfo<DateTimeOffset> result = DateHelper.ChangeDateTime(value, dateTime, timeZone, DateValueType.Year);
        result.HasSucceeded.Should().BeTrue();
        result.Data.Should().Be(expectedDateTime);
    }

    [Theory]
    [InlineData("2023-12-31T23:59:59.000Z", 1000, "2024-01-01T00:00:00.000Z")]
    public void ChangeDateTimeWithInvalidValueShouldReturnDateTimeOffsetChangedInMillisecond(
        string dateTimeString, int value, string expectedDateTimeString)
    {
        var dateTime = DateTimeOffset.Parse(dateTimeString);
        var expectedDateTime = DateTimeOffset.Parse(expectedDateTimeString);
        TimeZoneInfo timeZone = TimeZoneInfo.Utc;

        ResultInfo<DateTimeOffset> result = DateHelper.ChangeDateTime(value, dateTime, timeZone, DateValueType.Millisecond);
        result.HasSucceeded.Should().BeTrue();
        result.Data.Should().Be(expectedDateTime);
    }
}
