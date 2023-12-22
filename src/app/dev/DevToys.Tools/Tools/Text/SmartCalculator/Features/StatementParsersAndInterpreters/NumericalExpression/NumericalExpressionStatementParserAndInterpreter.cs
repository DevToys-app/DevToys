using DevToys.Tools.Tools.Text.SmartCalculator.Api.AbstractSyntaxTree;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Lexer;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Metadata;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.ParserAndInterpreter;

namespace DevToys.Tools.Tools.Text.SmartCalculator.Features.StatementParsersAndInterpreters.NumericalExpression;

[Export(typeof(IStatementParserAndInterpreter))]
[Culture(SupportedCultures.Any)]
[Name(PredefinedStatementParserAndInterpreterNames.NumericalExpressionStatement)]
[Order(After = PredefinedStatementParserAndInterpreterNames.ConditionExpressionStatement)]
internal sealed class NumericalExpressionStatementParserAndInterpreter : IStatementParserAndInterpreter
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

        bool expressionFound
            = await ParserAndInterpreterService.TryParseAndInterpretExpressionAsync(
                new[] { PredefinedExpressionParserAndInterpreterNames.NumericalExpression },
                culture,
                currentToken,
                variableService,
                expressionResult,
                cancellationToken);

        if (expressionFound
            && expressionResult.ParsedExpression
                is DataExpression
                or VariableReferenceExpression
                or GroupExpression
                or BinaryOperatorExpression)
        {
            if (expressionResult.ParsedExpression is BinaryOperatorExpression binaryOperatorExpression
                && !(binaryOperatorExpression.Operator is BinaryOperatorType.Addition or BinaryOperatorType.Division or BinaryOperatorType.Multiply or BinaryOperatorType.Subtraction))
            {
                return false;
            }

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
