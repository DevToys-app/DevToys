using DevToys.Tools.Tools.Text.SmartCalculator.Api.Data;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Data.Definition;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Lexer;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Metadata;
using Microsoft.Recognizers.Text;

namespace DevToys.Tools.Tools.Text.SmartCalculator.Features.Data;

[Export(typeof(IDataParser))]
[Culture(SupportedCultures.Any)]
public sealed class BooleanDataParser : IDataParser
{
    public IReadOnlyList<IData>? Parse(string culture, TokenizedTextLine tokenizedTextLine, CancellationToken cancellationToken)
    {
        var results = new List<IData>();
        LinkedToken? currentToken = tokenizedTextLine.Tokens;
        while (currentToken is not null)
        {
            if (currentToken.Token.IsOfType(PredefinedTokenAndDataTypeNames.TrueIdentifier))
            {
                results.Add(
                    new BooleanData(
                        tokenizedTextLine.LineTextIncludingLineBreak,
                        currentToken.Token.StartInLine,
                        currentToken.Token.EndInLine,
                        true));
            }
            else if (currentToken.Token.IsOfType(PredefinedTokenAndDataTypeNames.FalseIdentifier))
            {
                results.Add(
                    new BooleanData(
                        tokenizedTextLine.LineTextIncludingLineBreak,
                        currentToken.Token.StartInLine,
                        currentToken.Token.EndInLine,
                        false));
            }

            currentToken = currentToken.Next;
        }

        return results;
    }
}
