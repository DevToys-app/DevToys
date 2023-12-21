using System.Globalization;
using DevToys.Tools.Helpers;
using DevToys.Tools.Models;
using Microsoft.Extensions.Logging;

namespace DevToys.Tools.Tools.Converters.Date;

[Export(typeof(ICommandLineTool))]
[Name("DateConverter")]
[CommandName(
    Name = "dateConverter",
    ResourceManagerBaseName = "DevToys.Tools.Tools.Converters.Date.DateConverter",
    DescriptionResourceName = nameof(DateConverter.Description))]
internal sealed class DateConverterCommandLineTool : ICommandLineTool
{
    [CommandLineOption(
        Name = "number",
        Alias = "n",
        DescriptionResourceName = nameof(DateConverter.NumberOptionDescription))]
    internal long? NumberOption { get; set; }

    [CommandLineOption(
        Name = "date",
        Alias = "d",
        DescriptionResourceName = nameof(DateConverter.DateOptionDescription))]
    internal string? DateOption { get; set; }

    [CommandLineOption(
        Name = "epoch",
        Alias = "e",
        DescriptionResourceName = nameof(DateConverter.EpochOptionDescription))]

    internal string? EpochOption { get; set; }

    [CommandLineOption(
        Name = "timezone",
        Alias = "tz",
        IsRequired = true,
        DescriptionResourceName = nameof(DateConverter.TimezoneOptionDescription))]
    internal string? TimeZoneOption { get; set; }

    [CommandLineOption(
        Name = "format",
        Alias = "f",
        IsRequired = true,
        DescriptionResourceName = nameof(DateConverter.FormatOptionDescription))]
    internal DateFormat FormatOption { get; set; } = DateFormat.Seconds;

    public ValueTask<int> InvokeAsync(ILogger logger, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(TimeZoneOption))
        {
            Console.Error.WriteLine(DateConverter.InvalidTimeZoneCommand);
            return ValueTask.FromResult(-1);
        }

        if (!string.IsNullOrWhiteSpace(DateOption))
        {
            return InvokeDateConvertAsync(
                DateOption,
                EpochOption,
                FormatOption,
                cancellationToken);
        }

        if (NumberOption.HasValue)
        {
            return InvokeNumberConvertAsync(
                NumberOption,
                EpochOption,
                TimeZoneOption,
                FormatOption,
                cancellationToken);
        }

        return ValueTask.FromResult(-1);
    }

    private static async ValueTask<int> InvokeDateConvertAsync(
            string dateOption,
            string? epochOption,
            DateFormat formatOption,
            CancellationToken cancellationToken)
    {
        Guard.IsNotNullOrWhiteSpace(dateOption);

        if (TryParseDate(dateOption, out DateTimeOffset dateTimeOffset) != 0)
        {
            Console.Error.WriteLine(DateConverter.InvalidDateTimeCommand);
            return -1;
        }

        DateTimeOffset epoch = DateTime.UnixEpoch;
        if (!string.IsNullOrWhiteSpace(epochOption) && TryParseDate(epochOption, out epoch) != 0)
        {
            Console.Error.WriteLine(DateConverter.InvalidEpochCommand);
            return -1;
        }

        ToolResult<long> result = await DateHelper.ConvertToLongAsync(
            dateTimeOffset,
            epoch,
            formatOption,
            cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();

        if (!result.HasSucceeded)
        {
            Console.Error.WriteLine(result.Data);
            return -1;
        }

        Console.WriteLine(result.Data);
        return 0;
    }

    private static async ValueTask<int> InvokeNumberConvertAsync(
        long? numberOption,
        string? epochOption,
        string timeZoneOption,
        DateFormat formatOption,
        CancellationToken cancellationToken)
    {
        Guard.IsNotNull(numberOption);

        DateTimeOffset epoch = DateTime.UnixEpoch;
        if (!string.IsNullOrWhiteSpace(epochOption) && TryParseDate(epochOption, out epoch) != 0)
        {
            Console.Error.WriteLine(DateConverter.InvalidEpochCommand);
            return -1;
        }

        ToolResult<DateTimeOffset> result = await DateHelper.ConvertToDateTimeUtcAsync(
            numberOption.Value,
            epoch,
            formatOption,
            cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();

        if (!result.HasSucceeded)
        {
            Console.Error.WriteLine(result.Data);
            return -1;
        }

        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneOption);
        DateTimeOffset convertedDateTime = TimeZoneInfo.ConvertTime(result.Data, timeZone);
        Console.WriteLine(convertedDateTime.ToString("O"));
        return 0;
    }

    private static int TryParseDate(string dateTime, out DateTimeOffset dateTimeOffset)
    {
        if (DateTimeOffset.TryParseExact(dateTime, "O", null, DateTimeStyles.None, out dateTimeOffset))
        {
            return 0;
        }
        return -1;
    }
}
