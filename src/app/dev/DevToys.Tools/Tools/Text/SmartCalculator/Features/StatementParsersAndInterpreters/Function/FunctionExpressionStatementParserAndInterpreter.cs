using DevToys.Tools.Tools.Text.SmartCalculator.Api.Lexer;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Metadata;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.ParserAndInterpreter;
using DevToys.Tools.Tools.Text.SmartCalculator.Features.StatementParsersAndInterpreters.NumericalExpression;

namespace DevToys.Tools.Tools.Text.SmartCalculator.Features.StatementParsersAndInterpreters.Function;

[Export(typeof(IStatementParserAndInterpreter))]
[Culture(SupportedCultures.English)]
[Name(PredefinedStatementParserAndInterpreterNames.FunctionExpressionStatement)]
[Order(Before = PredefinedStatementParserAndInterpreterNames.NumericalExpressionStatement)]
internal sealed class FunctionExpressionStatementParserAndInterpreter : IStatementParserAndInterpreter
{
    [Import]
    public IParserAndInterpreterService ParserAndInterpreterService { get; set; } = null!;

    public async Task<bool> TryParseAndInterpretStatementAsync(
        string culture,
        LinkedToken currentToken,
        IVariableService variableService,
        StatementParserAndInterpreterResult result,
        CancellationToken cancellationToken)
    {
        var expressionResult = new ExpressionParserAndInterpreterResult();

        bool functionFound
            = await ParserAndInterpreterService.TryParseAndInterpretExpressionAsync(
                new[] { PredefinedExpressionParserAndInterpreterNames.FunctionExpression },
                culture,
                currentToken,
                variableService,
                expressionResult,
                cancellationToken);

        if (functionFound)
        {
            result.ParsedStatement
                = new NumericalCalculusStatement(
                    expressionResult.ParsedExpression!.FirstToken,
                    expressionResult.ParsedExpression.LastToken,
                    expressionResult.ParsedExpression);
            result.ResultedData = expressionResult.ResultedData;
            return true;
        }

        return false;
    }
}
