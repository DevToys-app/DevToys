using DevToys.Tools.Tools.Text.SmartCalculator.Api.AbstractSyntaxTree;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Data;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Lexer;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Metadata;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.ParserAndInterpreter;

namespace DevToys.Tools.Tools.Text.SmartCalculator.Features.ExpressionParsersAndInterpreters.Numerical;

[Export(typeof(IExpressionParserAndInterpreter))]
[Name(PredefinedExpressionParserAndInterpreterNames.NumericalExpression)]
[Culture(SupportedCultures.Any)]
[Order(After = PredefinedExpressionParserAndInterpreterNames.ConditionalExpression)]
internal sealed class NumericalExpressionParserAndInterpreter : IExpressionParserAndInterpreter
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
            ParseAdditiveExpressionAsync(
                culture,
                currentToken,
                variableService,
                result,
                cancellationToken);
    }

    /// <summary>
    /// Parse expression that contains multiply, division and modulus symbols.
    /// 
    /// Corresponding grammar :
    ///     Multiplicative_Expression (('+' | '-') Multiplicative_Expression)*
    /// </summary>
    private async Task<bool> ParseAdditiveExpressionAsync(
        string culture,
        LinkedToken currentToken,
        IVariableService variableService,
        ExpressionParserAndInterpreterResult result,
        CancellationToken cancellationToken)
    {
        bool foundLeftExpression
            = await ParseMultiplicativeExpressionAsync(
                culture,
                currentToken,
                variableService,
                result,
                cancellationToken);

        if (foundLeftExpression)
        {
            LinkedToken? operatorToken = result.NextTokenToContinueWith.SkipNextWordTokens();
            while (operatorToken is not null)
            {
                BinaryOperatorType binaryOperator;
                if (operatorToken.Token.IsOfType(PredefinedTokenAndDataTypeNames.AdditionOperator))
                    binaryOperator = BinaryOperatorType.Addition;
                else if (operatorToken.Token.IsOfType(PredefinedTokenAndDataTypeNames.SubstractionOperator))
                {
                    binaryOperator = BinaryOperatorType.Subtraction;
                }
                else
                {
                    return foundLeftExpression;
                }

                ExpressionParserAndInterpreterResult leftExpressionResult = result;
                ExpressionParserAndInterpreterResult rightExpressionResult = new();

                bool foundRightExpression
                     = await ParseMultiplicativeExpressionAsync(
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
                        = ParserAndInterpreterService.ArithmeticAndRelationOperationService.PerformAlgebraOperation(
                            leftExpressionResult.ResultedData,
                            binaryOperator,
                            rightExpressionResult.ResultedData);

                    operatorToken = rightExpressionResult.NextTokenToContinueWith.SkipNextWordTokens();
                }
                else
                {
                    return foundLeftExpression;
                }
            }
        }

        return foundLeftExpression;
    }

    /// <summary>
    /// Parse expression that contains multiply, division and modulus symbols.
    /// 
    /// Corresponding grammar :
    ///     Primary_Expression (('*' | '/') Primary_Expression)*
    /// </summary>
    private async Task<bool> ParseMultiplicativeExpressionAsync(
        string culture,
        LinkedToken? currentToken,
        IVariableService variableService,
        ExpressionParserAndInterpreterResult result,
        CancellationToken cancellationToken)
    {
        bool foundLeftExpression
            = await ParserAndInterpreterService.TryParseAndInterpretExpressionAsync(
                new[] { PredefinedExpressionParserAndInterpreterNames.PrimitiveExpression, PredefinedExpressionParserAndInterpreterNames.FunctionExpression },
                culture,
                currentToken,
                variableService,
                result,
                cancellationToken);

        if (foundLeftExpression)
        {
            LinkedToken? operatorToken = result.NextTokenToContinueWith.SkipNextWordTokens();
            while (operatorToken is not null)
            {
                BinaryOperatorType binaryOperator;
                LinkedToken? expressionStartToken = operatorToken.Next;
                if (operatorToken.Token.IsOfType(PredefinedTokenAndDataTypeNames.MultiplicationOperator))
                    binaryOperator = BinaryOperatorType.Multiply;
                else if (operatorToken.Token.IsOfType(PredefinedTokenAndDataTypeNames.DivisionOperator))
                {
                    binaryOperator = BinaryOperatorType.Division;
                }
                else if (operatorToken.Token.IsOfType(PredefinedTokenAndDataTypeNames.LeftParenth))
                {
                    // When we have something like `Primary_Expression (Primary_Expression)`, implicitely
                    // assume it is a multiplication.
                    binaryOperator = BinaryOperatorType.Multiply;
                    expressionStartToken = operatorToken;
                }
                else if (operatorToken.Token.IsOfType(PredefinedTokenAndDataTypeNames.Numeric))
                {
                    // When we have 2 consecutive expression with no parenthesis or operator,
                    // for example `I have 2 dogs, 3 cats`, let's assume we should do an addition.
                    // When there is no operator and that the previous token is a closing parenthesis,
                    // for example `(Primary_Left_Expression) Primary_Right_Expression`, implicitely assume we should
                    // do a multiplication, unless the right Primary_Right_Expression is a negative value,
                    // for example `(123) -10`.
                    if (operatorToken.Previous is not null
                        && operatorToken.Previous.Token.IsOfType(PredefinedTokenAndDataTypeNames.RightParenth))
                    {
                        if (operatorToken.Token is INumericData numericData && numericData.IsNegative)
                            binaryOperator = BinaryOperatorType.Addition;
                        else
                        {
                            binaryOperator = BinaryOperatorType.Multiply;
                        }
                    }
                    else
                    {
                        binaryOperator = BinaryOperatorType.Addition;
                    }

                    expressionStartToken = operatorToken;
                }
                else
                {
                    return foundLeftExpression;
                }

                ExpressionParserAndInterpreterResult leftExpressionResult = result;
                ExpressionParserAndInterpreterResult rightExpressionResult = new();

                bool foundRightExpression
                    = await ParserAndInterpreterService.TryParseAndInterpretExpressionAsync(
                        new[] { PredefinedExpressionParserAndInterpreterNames.PrimitiveExpression, PredefinedExpressionParserAndInterpreterNames.FunctionExpression },
                        culture,
                        expressionStartToken,
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
                        = ParserAndInterpreterService.ArithmeticAndRelationOperationService.PerformAlgebraOperation(
                            leftExpressionResult.ResultedData,
                            binaryOperator,
                            rightExpressionResult.ResultedData);

                    operatorToken = rightExpressionResult.NextTokenToContinueWith.SkipNextWordTokens();
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
