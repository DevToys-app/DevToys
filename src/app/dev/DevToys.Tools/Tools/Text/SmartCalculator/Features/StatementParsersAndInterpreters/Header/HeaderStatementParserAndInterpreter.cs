using DevToys.Tools.Tools.Text.SmartCalculator.Api.Lexer;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Metadata;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.ParserAndInterpreter;

namespace DevToys.Tools.Tools.Text.SmartCalculator.Features.StatementParsersAndInterpreters.Header;

[Export(typeof(IStatementParserAndInterpreter))]
[Culture(SupportedCultures.Any)]
[Name(PredefinedStatementParserAndInterpreterNames.HeaderStatement)]
internal sealed class HeaderStatementParserAndInterpreter : IStatementParserAndInterpreter
{
    public IParserAndInterpreterService ParserAndInterpreterService => throw new NotImplementedException();

    public Task<bool> TryParseAndInterpretStatementAsync(
        string culture,
        LinkedToken currentToken,
        IVariableService variableService,
        StatementParserAndInterpreterResult result,
        CancellationToken cancellationToken)
    {
        if (currentToken.Token.IsOfType(PredefinedTokenAndDataTypeNames.HeaderOperator))
        {
            LinkedToken? previousToken = currentToken.Previous;
            LinkedToken firstTokenInLine = currentToken;
            while (previousToken is not null)
            {
                firstTokenInLine = previousToken;
                if (previousToken.Token.IsNotOfType(PredefinedTokenAndDataTypeNames.Whitespace))
                    return Task.FromResult(false);

                previousToken = previousToken.Previous;
            }

            LinkedToken? nextToken = currentToken.Next;
            LinkedToken lastTokenInLine = currentToken;
            while (nextToken is not null)
            {
                lastTokenInLine = nextToken;
                nextToken = nextToken.Next;
            }

            result.ParsedStatement = new HeaderStatement(firstTokenInLine, lastTokenInLine);
            return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }
}
