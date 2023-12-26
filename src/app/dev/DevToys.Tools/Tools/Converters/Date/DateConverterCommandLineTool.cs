using System.Globalization;
using DevToys.Tools.Helpers;
using DevToys.Tools.Models;
using Microsoft.Extensions.Logging;
using OneOf;

namespace DevToys.Tools.Tools.Converters.Date;

[Export(typeof(ICommandLineTool))]
[Name("DateConverter")]
[CommandName(
    Name = "date",
    ResourceManagerBaseName = "DevToys.Tools.Tools.Converters.Date.DateConverter",
    DescriptionResourceName = nameof(DateConverter.Description))]
internal sealed class DateConverterCommandLineTool : ICommandLineTool
{
    [CommandLineOption(
        Name = "input",
        Alias = "i",
        IsRequired = true,
        DescriptionResourceName = nameof(DateConverter.InputDescription))]
    internal OneOf<long, string>? Input { get; set; }

    [CommandLineOption(
        Name = "epoch",
        Alias = "e",
        DescriptionResourceName = nameof(DateConverter.EpochOptionDescription))]

    internal string? EpochOption { get; set; }

    [CommandLineOption(
        Name = "timezone",
        Alias = "tz",
        DescriptionResourceName = nameof(DateConverter.TimezoneOptionDescription))]
    internal string? TimeZoneOption { get; set; }

    [CommandLineOption(
        Name = "format",
        Alias = "f",
        DescriptionResourceName = nameof(DateConverter.FormatOptionDescription))]
    internal DateFormat FormatOption { get; set; } = DateFormat.Seconds;

    public ValueTask<int> InvokeAsync(ILogger logger, CancellationToken cancellationToken)
    {
        if (!Input.HasValue)
        {
            Console.Error.WriteLine(DateConverter.InvalidValue);
            return ValueTask.FromResult(-1);
        }

        return Input.Value.Match(
            inputNumber => InvokeNumberConvert(
                inputNumber,
                EpochOption,
                FormatOption,
                cancellationToken),
            inputDate => InvokeDateConvert(
                inputDate,
                EpochOption,
                FormatOption,
                cancellationToken)
        );
    }

    private static ValueTask<int> InvokeDateConvert(
            string dateOption,
            string? epochOption,
            DateFormat formatOption,
            CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dateOption))
        {
            Console.Error.WriteLine(DateConverter.InvalidValue);
            return ValueTask.FromResult(-1);
        }

        if (TryParseDate(dateOption, out DateTimeOffset dateTimeOffset) != 0)
        {
            Console.Error.WriteLine(DateConverter.InvalidDateTimeCommand);
            return ValueTask.FromResult(-1);
        }

        DateTimeOffset epoch = DateTime.UnixEpoch;
        if (!string.IsNullOrWhiteSpace(epochOption) && TryParseDate(epochOption, out epoch) != 0)
        {
            Console.Error.WriteLine(DateConverter.InvalidEpochCommand);
            return ValueTask.FromResult(-1);
        }

        ResultInfo<long> result = DateHelper.ConvertToLong(
            dateTimeOffset,
            epoch,
            formatOption);

        cancellationToken.ThrowIfCancellationRequested();

        if (!result.HasSucceeded)
        {
            Console.Error.WriteLine(result.Data);
            return ValueTask.FromResult(-1);
        }

        Console.WriteLine(result.Data);
        return ValueTask.FromResult(0);
    }

    private ValueTask<int> InvokeNumberConvert(
        long numberOption,
        string? epochOption,
        DateFormat formatOption,
        CancellationToken cancellationToken)
    {
        DateTimeOffset epoch = DateTime.UnixEpoch;
        if (!string.IsNullOrWhiteSpace(epochOption) && TryParseDate(epochOption, out epoch) != 0)
        {
            Console.Error.WriteLine(DateConverter.InvalidEpochCommand);
            return ValueTask.FromResult(-1);
        }

        ResultInfo<DateTimeOffset> result = DateHelper.ConvertToDateTimeUtc(
            numberOption,
            epoch,
            formatOption);

        cancellationToken.ThrowIfCancellationRequested();

        if (!result.HasSucceeded)
        {
            Console.Error.WriteLine(result.Data);
            return ValueTask.FromResult(-1);
        }

        TimeZoneInfo timeZone = TimeZoneInfo.Local;
        if (!string.IsNullOrWhiteSpace(TimeZoneOption))
        {
            try
            {
                timeZone = TimeZoneInfo.FindSystemTimeZoneById(TimeZoneOption);
            }
            catch (TimeZoneNotFoundException)
            {
                Console.Error.WriteLine(DateConverter.InvalidTimeZoneCommand);
                return ValueTask.FromResult(-1);
            }
        }

        DateTimeOffset convertedDateTime = TimeZoneInfo.ConvertTime(result.Data, timeZone);
        Console.WriteLine(convertedDateTime.ToString("O"));
        return ValueTask.FromResult(0);
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
