using DevToys.Tools.Helpers;
using Microsoft.Extensions.Logging;

namespace DevToys.Tools.Tools.Converters.Cron;

[Export(typeof(ICommandLineTool))]
[Name("CronParser")]
[CommandName(
    Name = "cronparser",
    Alias = "cron",
    ResourceManagerBaseName = "DevToys.Tools.Tools.Converters.Cron.CronParser",
    DescriptionResourceName = nameof(CronParser.Description))]
internal sealed class CronParserCommandLineTool : ICommandLineTool
{
    [CommandLineOption(
        Name = "seconds",
        Alias = "s",
        DescriptionResourceName = nameof(CronParser.IncludeSecondsOptionDescription))]
    internal bool IncludeSeconds { get; set; } = true;

    [CommandLineOption(
        Name = "scheduleCount",
        Alias = "c",
        DescriptionResourceName = nameof(CronParser.MaxScheduleCountOptionDescription))]
    internal int MaxScheduleCount { get; set; } = 5;

    [CommandLineOption(
        Name = "dateFormat",
        Alias = "d",
        DescriptionResourceName = nameof(CronParser.DateFormatOptionDescription))]
    internal string DateFormat { get; set; } = "yyyy-MM-dd ddd HH:mm:ss";

    [CommandLineOption(
        Name = "expression",
        Alias = "e",
        IsRequired = true,
        DescriptionResourceName = nameof(CronParser.CronExpressionOptionDescription))]
    internal string CronExpression { get; set; } = string.Empty;

    public ValueTask<int> InvokeAsync(ILogger logger, CancellationToken cancellationToken)
    {
        bool succeeded
            = CronHelper.TryGenerateNextSchedulesAndDescription(
                CronExpression,
                DateFormat,
                IncludeSeconds,
                MaxScheduleCount,
                out string schedule,
                out string description,
                out string error);

        if (!string.IsNullOrEmpty(description))
        {
            Console.WriteLine(description);
        }

        if (!string.IsNullOrEmpty(schedule))
        {
            Console.WriteLine(schedule);
        }

        if (succeeded)
        {
            return ValueTask.FromResult(0);
        }
        else
        {
            Console.Error.WriteLine(error);
            return ValueTask.FromResult(-1);
        }
    }
}
