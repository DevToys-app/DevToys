using System.Text.RegularExpressions;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Data;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Data.Definition;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Lexer;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Metadata;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.Number;
using Constants = Microsoft.Recognizers.Text.Number.Constants;

namespace DevToys.Tools.Tools.Text.SmartCalculator.Features.Data;

[Export(typeof(IDataParser))]
[Culture(SupportedCultures.Any)]
public sealed class PercentageDataParser : IDataParser
{
    private const string Value = "value";
    private const string NullValue = "null";

    public IReadOnlyList<IData>? Parse(string culture, TokenizedTextLine tokenizedTextLine, CancellationToken cancellationToken)
    {
        List<ModelResult> modelResults = NumberRecognizer.RecognizePercentage(tokenizedTextLine.LineTextIncludingLineBreak, culture);
        cancellationToken.ThrowIfCancellationRequested();

        var data = new List<IData>();

        for (int i = 0; i < modelResults.Count; i++)
        {
            ModelResult modelResult = modelResults[i];

            if (modelResult.Resolution is not null)
            {
                switch (modelResult.TypeName)
                {
                    case Constants.MODEL_PERCENTAGE:
                        if (!ShouldBeIgnored(culture, modelResult))
                        {
                            string valueString = (string)modelResult.Resolution[Value];
                            if (!string.Equals(NullValue, valueString, StringComparison.OrdinalIgnoreCase))
                            {
                                valueString = valueString.TrimEnd('%');
                                data.Add(
                                    new PercentageData(
                                        tokenizedTextLine.LineTextIncludingLineBreak,
                                        modelResult.Start,
                                        modelResult.End + 1,
                                        double.Parse(valueString) / 100));
                            }
                        }
                        break;

                    default:
#if DEBUG
                        ThrowHelper.ThrowNotSupportedException();
#endif
                        break;
                }
            }
        }

        return data;
    }

    private static bool ShouldBeIgnored(string culture, ModelResult modelResult)
    {
        if (culture == SupportedCultures.English)
        {
            // starts with "percent of ".
            var regex = new Regex(@"^(percent\s+of\s+)");
            if (regex.Match(modelResult.Text).Success)
                return true;
        }

        return false;
    }
}
