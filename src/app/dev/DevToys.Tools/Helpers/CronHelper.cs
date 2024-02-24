using System.Globalization;
using CronExpressionDescriptor;
using Cronos;
using DevToys.Tools.Tools.Converters.Cron;

namespace DevToys.Tools.Helpers;

internal static class CronHelper
{
    internal static bool TryGenerateNextSchedulesAndDescription(
        string cronExpression,
        string dateFormat,
        bool includeSeconds,
        int maxScheduleCount,
        out string schedule,
        out string description,
        out string error)
    {
        if (string.IsNullOrEmpty(cronExpression))
        {
            schedule = string.Empty;
            description = string.Empty;
            error = string.Empty;
            return true;
        }

        // Validate date format
        if (!ValidateDateTimeFormat(dateFormat))
        {
            schedule = string.Empty;
            description = string.Empty;
            error = CronParser.OutputFormatErrorMessage;
            return false;
        }

        try
        {
            // Attempt to generate a description
            description = ExpressionDescriptor.GetDescription(cronExpression);

            var expression
                = CronExpression.Parse(
                    cronExpression,
                    includeSeconds ? CronFormat.IncludeSeconds : CronFormat.Standard);

            Guard.IsNotNull(expression, nameof(expression));

            // Attempt to generate next schedules
            DateTimeOffset? nextOccurrence = expression.GetNextOccurrence(DateTimeOffset.Now, TimeZoneInfo.Local, true);
            var output = new List<string>();
            int i = 0;
            while (i < maxScheduleCount && nextOccurrence.HasValue)
            {
                output.Add(nextOccurrence.Value.ToString(dateFormat));
                nextOccurrence = expression.GetNextOccurrence(nextOccurrence.Value, TimeZoneInfo.Local, false);
                i++;
            }

            schedule = string.Join(Environment.NewLine, output);
            error = string.Empty;
            return true;
        }
        catch
        {
            schedule = string.Empty;
            description = string.Empty;
            error = CronParser.CronErrorMessage;
            return false;
        }
    }

    private static bool ValidateDateTimeFormat(string dateFormat)
    {
        try
        {
            string s = DateTime.Now.ToString(dateFormat, CultureInfo.InvariantCulture);
            DateTime.Parse(s, CultureInfo.InvariantCulture);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
