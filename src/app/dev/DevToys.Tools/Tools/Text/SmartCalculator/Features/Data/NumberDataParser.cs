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
public sealed class NumberDataParser : IDataParser
{
    private const string Subtype = "subtype";
    private const string Value = "value";

    public IReadOnlyList<IData>? Parse(string culture, TokenizedTextLine tokenizedTextLine, CancellationToken cancellationToken)
    {
        List<ModelResult> modelResults = NumberRecognizer.RecognizeNumber(tokenizedTextLine.LineTextIncludingLineBreak, culture);
        cancellationToken.ThrowIfCancellationRequested();

        var data = new List<IData>();

        for (int i = 0; i < modelResults.Count; i++)
        {
            ModelResult modelResult = modelResults[i];

            if (modelResult.Resolution is not null)
            {
                string valueString = (string)modelResult.Resolution[Value];
                switch (modelResult.Resolution[Subtype])
                {
                    case Constants.INTEGER:
                    case Constants.DECIMAL:
                    case Constants.POWER:
                        data.Add(
                            new DecimalData(
                                tokenizedTextLine.LineTextIncludingLineBreak,
                                modelResult.Start,
                                modelResult.End + 1,
                                double.Parse(valueString)));
                        break;

                    case Constants.FRACTION:
                        data.Add(
                            new FractionData(
                                tokenizedTextLine.LineTextIncludingLineBreak,
                                modelResult.Start,
                                modelResult.End + 1,
                                double.Parse(valueString)));
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
}
