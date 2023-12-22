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
public sealed class OrdinalDataParser : IDataParser
{
    private const string Value = "value";
    private const string NullValue = "null";

    public IReadOnlyList<IData>? Parse(string culture, TokenizedTextLine tokenizedTextLine, CancellationToken cancellationToken)
    {
        List<ModelResult> modelResults = NumberRecognizer.RecognizeOrdinal(tokenizedTextLine.LineTextIncludingLineBreak, culture);
        cancellationToken.ThrowIfCancellationRequested();

        var data = new List<IData>();

        for (int i = 0; i < modelResults.Count; i++)
        {
            ModelResult modelResult = modelResults[i];

            if (modelResult.Resolution is not null)
            {
                string valueString = (string)modelResult.Resolution[Value];
                switch (modelResult.TypeName)
                {
                    case Constants.MODEL_ORDINAL:
                        if (!string.Equals(NullValue, valueString, StringComparison.OrdinalIgnoreCase))
                        {
                            data.Add(
                            new OrdinalData(
                                tokenizedTextLine.LineTextIncludingLineBreak,
                                modelResult.Start,
                                modelResult.End + 1,
                                long.Parse(valueString)));
                        }
                        break;

                    case Constants.MODEL_ORDINAL_RELATIVE:
                        // TODO
                        ThrowHelper.ThrowNotSupportedException();
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
