using System.Runtime.CompilerServices;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Data;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Data.Definition;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Lexer;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Metadata;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;
using Microsoft.Recognizers.Text.DateTime;
using Constants = Microsoft.Recognizers.Text.DateTime.Constants;

namespace DevToys.Tools.Tools.Text.SmartCalculator.Features.Data;

[Export(typeof(IDataParser))]
[Culture(SupportedCultures.Any)]
public sealed class DateTimeDataParser : IDataParser
{
    private const string Value = "value";
    private const string Values = "values";
    private const string Start = "start";
    private const string End = "end";
    private const string Type = "type";

    public IReadOnlyList<IData>? Parse(string culture, TokenizedTextLine tokenizedTextLine, CancellationToken cancellationToken)
    {
        List<ModelResult> modelResults = DateTimeRecognizer.RecognizeDateTime(tokenizedTextLine.LineTextIncludingLineBreak, culture);
        cancellationToken.ThrowIfCancellationRequested();

        var data = new List<IData>();

        if (modelResults.Count > 0)
        {
            for (int i = 0; i < modelResults.Count; i++)
            {
                ModelResult modelResult = modelResults[i];

                if (modelResult.Resolution is not null)
                {
                    var values = modelResult.Resolution[Values] as List<Dictionary<string, string>>;
                    if (values is not null)
                    {
                        switch (values[0][Type])
                        {
                            case Constants.SYS_DATETIME_DURATION:
                                ParseDuration(data, tokenizedTextLine, modelResult, values[0]);
                                break;

                            case Constants.SYS_DATETIME_DATE:
                            case Constants.SYS_DATETIME_TIME:
                            case Constants.SYS_DATETIME_DATETIME:
                                ParseDateTime(data, tokenizedTextLine, modelResult, values[0]);
                                break;

                            case Constants.SYS_DATETIME_DATEPERIOD:
                            case Constants.SYS_DATETIME_DATETIMEPERIOD:
                                ParseDateTimeRange(data, tokenizedTextLine, modelResult, values[0]);
                                break;

                            case Constants.SYS_DATETIME_TIMEPERIOD:
                                ParseTimeRange(data, tokenizedTextLine, modelResult, values[0]);
                                break;

                            default:
                                ThrowHelper.ThrowNotSupportedException();
                                return null;
                        }
                    }
                }
            }
        }

        return data;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ParseDuration(List<IData> data, TokenizedTextLine tokenizedTextLine, ModelResult modelResult, Dictionary<string, string> values)
    {
        string valueString = values[Value];
        var duration = TimeSpan.FromSeconds(double.Parse(valueString));

        data.Add(
            new DurationData(
                tokenizedTextLine.LineTextIncludingLineBreak,
                modelResult.Start,
                modelResult.End + 1,
                duration));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ParseDateTime(List<IData> data, TokenizedTextLine tokenizedTextLine, ModelResult modelResult, Dictionary<string, string> values)
    {
        if (!ShouldBeIgnored(modelResult))
        {
            string valueString = values[Value];
            var dateTime = DateTime.Parse(valueString);

            data.Add(
                new DateTimeData(
                    tokenizedTextLine.LineTextIncludingLineBreak,
                    modelResult.Start,
                    modelResult.End + 1,
                    dateTime));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ParseDateTimeRange(List<IData> data, TokenizedTextLine tokenizedTextLine, ModelResult modelResult, Dictionary<string, string> values)
    {
        if (values.ContainsKey(Start) && values.ContainsKey(End))
        {
            string startString = values[Start];
            var startDateTime = DateTime.Parse(startString);
            string endString = values[End];
            var endDateTime = DateTime.Parse(endString);

            TimeSpan duration = endDateTime - startDateTime;

            data.Add(
                new DurationData(
                    tokenizedTextLine.LineTextIncludingLineBreak,
                    modelResult.Start,
                    modelResult.End + 1,
                    duration));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ParseTimeRange(List<IData> data, TokenizedTextLine tokenizedTextLine, ModelResult modelResult, Dictionary<string, string> values)
    {
        var timex = new TimexProperty(values["timex"]);

        int hours = (int)timex.Hours.GetValueOrDefault(0);
        int minutes = (int)timex.Minutes.GetValueOrDefault(0);
        int seconds = (int)timex.Seconds.GetValueOrDefault(0);

        var duration = new TimeSpan(hours, minutes, seconds);

        data.Add(
            new DurationData(
                tokenizedTextLine.LineTextIncludingLineBreak,
                modelResult.Start,
                modelResult.End + 1,
                duration));
    }

    private static bool ShouldBeIgnored(ModelResult modelResult)
    {
        // Should ignore if the extracted text is just an integer without other indication it may be a date.
        return int.TryParse(modelResult.Text, out _);
    }
}
