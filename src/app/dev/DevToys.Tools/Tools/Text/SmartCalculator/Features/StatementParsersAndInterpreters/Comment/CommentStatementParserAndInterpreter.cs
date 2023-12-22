using DevToys.Tools.Tools.Text.SmartCalculator.Api.Lexer;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Metadata;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.ParserAndInterpreter;

namespace DevToys.Tools.Tools.Text.SmartCalculator.Features.StatementParsersAndInterpreters.Comment;

[Export(typeof(IStatementParserAndInterpreter))]
[Culture(SupportedCultures.Any)]
[Name(PredefinedStatementParserAndInterpreterNames.CommentStatement)]
internal sealed class CommentStatementParserAndInterpreter : IStatementParserAndInterpreter
{
    public IParserAndInterpreterService ParserAndInterpreterService => throw new NotImplementedException();

    public Task<bool> TryParseAndInterpretStatementAsync(
        string culture,
        LinkedToken currentToken,
        IVariableService variableService,
        StatementParserAndInterpreterResult result,
        CancellationToken cancellationToken)
    {
        if (currentToken.Token.IsOfType(PredefinedTokenAndDataTypeNames.CommentOperator))
        {
            LinkedToken lastTokenInLine = currentToken;
            LinkedToken? nextToken = currentToken.Next;
            while (nextToken is not null)
            {
                lastTokenInLine = nextToken;
                nextToken = nextToken.Next;
            }

            result.ParsedStatement = new CommentStatement(currentToken.SkipNextWordTokens()!, lastTokenInLine);
            return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }
}
