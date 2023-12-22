using DevToys.Tools.Tools.Text.SmartCalculator.Api.AbstractSyntaxTree;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Lexer;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Metadata;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.ParserAndInterpreter;
using DevToys.Tools.Tools.Text.SmartCalculator.Features.StatementParsersAndInterpreters.NumericalExpression;

namespace DevToys.Tools.Tools.Text.SmartCalculator.Features.StatementParsersAndInterpreters.Condition;

[Export(typeof(IStatementParserAndInterpreter))]
[Culture(SupportedCultures.Any)]
[Name(PredefinedStatementParserAndInterpreterNames.ConditionExpressionStatement)]
[Order(After = PredefinedStatementParserAndInterpreterNames.ConditionStatement)]
internal sealed class ConditionalExpressionStatementParserAndInterpreter : IStatementParserAndInterpreter
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
                new[] { PredefinedExpressionParserAndInterpreterNames.ConditionalExpression },
                culture,
                currentToken,
                variableService,
                expressionResult,
                cancellationToken);

        result.Error = expressionResult.Error;

        if (expressionFound)
        {
            if (expressionResult.ParsedExpression is BinaryOperatorExpression binaryOperatorExpression
                && binaryOperatorExpression.Operator
                    is BinaryOperatorType.NoEquality
                    or BinaryOperatorType.Equality
                    or BinaryOperatorType.LessThanOrEqualTo
                    or BinaryOperatorType.LessThan
                    or BinaryOperatorType.GreaterThanOrEqualTo
                    or BinaryOperatorType.GreaterThan)
            {
                result.ParsedStatement
                    = new ConditionStatement(
                        expressionResult.ParsedExpression!.FirstToken,
                        expressionResult.ParsedExpression.LastToken,
                        expressionResult.ParsedExpression,
                        expressionResult.ResultedData);
                result.ResultedData = expressionResult.ResultedData;
                return true;
            }

            // Fast path. We found an algebric operation. Since it's not a comparison, we can directly
            // return a numerical calculus statement instead of throwing away this finding and letting
            // NumericalExpressionStatementParserAndInterpreter redoing the same work and find the same result.
            if (expressionFound
                && expressionResult.ParsedExpression
                    is DataExpression
                    or VariableReferenceExpression
                    or GroupExpression
                    or BinaryOperatorExpression)
            {
                if (expressionResult.ParsedExpression is BinaryOperatorExpression binaryOperatorExpression2
                    && !(binaryOperatorExpression2.Operator
                        is BinaryOperatorType.Addition
                        or BinaryOperatorType.Division
                        or BinaryOperatorType.Multiply
                        or BinaryOperatorType.Subtraction))
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
        }

        return false;
    }
}
