using DevToys.Tools.Tools.Text.SmartCalculator.Api.AbstractSyntaxTree;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Lexer;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Metadata;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.ParserAndInterpreter;

namespace DevToys.Tools.Tools.Text.SmartCalculator.Features.ExpressionParsersAndInterpreters.Conditional;

[Export(typeof(IExpressionParserAndInterpreter))]
[Name(PredefinedExpressionParserAndInterpreterNames.ConditionalExpression)]
[Culture(SupportedCultures.Any)]
internal sealed class ConditionalExpressionParserAndInterpreter : IExpressionParserAndInterpreter
{
    [Import]
    public IParserAndInterpreterService ParserAndInterpreterService { get; set; } = null!;

    public Task<bool> TryParseAndInterpretExpressionAsync(
        string culture,
        LinkedToken currentToken,
        IVariableService variableService,
        ExpressionParserAndInterpreterResult result,
        CancellationToken cancellationToken)
    {
        return
            ParseEqualityAndRelationalExpression(
                culture,
                currentToken,
                variableService,
                result,
                cancellationToken);
    }

    /// <summary>
    /// Parse expression that contains equality symbols.
    /// 
    /// Corresponding grammar :
    ///     NumericalCalculus_Expression (('==' | '!=' | '<' | '>' | '<=' | '>=') NumericalCalculus_Expression)
    /// </summary>
    private async Task<bool> ParseEqualityAndRelationalExpression(
        string culture,
        LinkedToken? currentToken,
        IVariableService variableService,
        ExpressionParserAndInterpreterResult result,
        CancellationToken cancellationToken)
    {
        bool foundLeftExpression
            = await ParserAndInterpreterService.TryParseAndInterpretExpressionAsync(
                new[] { PredefinedExpressionParserAndInterpreterNames.NumericalExpression },
                culture,
                currentToken,
                variableService,
                result,
                cancellationToken);

        if (foundLeftExpression)
        {
            LinkedToken? operatorToken = result.NextTokenToContinueWith.SkipNextWordTokens();

            if (operatorToken is not null)
            {
                BinaryOperatorType binaryOperator;
                if (operatorToken.Token.IsOfType(PredefinedTokenAndDataTypeNames.IsEqualToOperator))
                    binaryOperator = BinaryOperatorType.Equality;
                else if (operatorToken.Token.IsOfType(PredefinedTokenAndDataTypeNames.IsNotEqualToOperator))
                {
                    binaryOperator = BinaryOperatorType.NoEquality;
                }
                else if (operatorToken.Token.IsOfType(PredefinedTokenAndDataTypeNames.LessThanOrEqualToOperator))
                {
                    binaryOperator = BinaryOperatorType.LessThanOrEqualTo;
                }
                else if (operatorToken.Token.IsOfType(PredefinedTokenAndDataTypeNames.LessThanOperator))
                {
                    binaryOperator = BinaryOperatorType.LessThan;
                }
                else if (operatorToken.Token.IsOfType(PredefinedTokenAndDataTypeNames.GreaterThanOrEqualToOperator))
                {
                    binaryOperator = BinaryOperatorType.GreaterThanOrEqualTo;
                }
                else if (operatorToken.Token.IsOfType(PredefinedTokenAndDataTypeNames.GreaterThanOperator))
                {
                    binaryOperator = BinaryOperatorType.GreaterThan;
                }
                else
                {
                    return foundLeftExpression;
                }

                ExpressionParserAndInterpreterResult leftExpressionResult = result;
                ExpressionParserAndInterpreterResult rightExpressionResult = new();

                bool foundRightExpression
                    = await ParserAndInterpreterService.TryParseAndInterpretExpressionAsync(
                        new[] { PredefinedExpressionParserAndInterpreterNames.NumericalExpression },
                        culture,
                        operatorToken.Next,
                        variableService,
                        rightExpressionResult,
                        cancellationToken);

                if (foundRightExpression)
                {
                    result.NextTokenToContinueWith = rightExpressionResult.NextTokenToContinueWith;

                    result.ParsedExpression
                        = new BinaryOperatorExpression(
                            leftExpressionResult.ParsedExpression!,
                            binaryOperator,
                            rightExpressionResult.ParsedExpression!);

                    result.ResultedData
                        = ParserAndInterpreterService.ArithmeticAndRelationOperationService.PerformBinaryOperation(
                            leftExpressionResult.ResultedData,
                            binaryOperator,
                            rightExpressionResult.ResultedData);
                }
                else
                {
                    return foundLeftExpression;
                }
            }
        }

        return foundLeftExpression;
    }
}
