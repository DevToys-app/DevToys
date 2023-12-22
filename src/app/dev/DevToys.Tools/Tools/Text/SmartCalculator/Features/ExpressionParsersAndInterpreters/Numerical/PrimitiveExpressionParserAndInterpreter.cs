using System.Runtime.CompilerServices;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.AbstractSyntaxTree;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Data;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Lexer;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Metadata;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.ParserAndInterpreter;

namespace DevToys.Tools.Tools.Text.SmartCalculator.Features.ExpressionParsersAndInterpreters.Numerical;

[Export(typeof(IExpressionParserAndInterpreter))]
[Name(PredefinedExpressionParserAndInterpreterNames.PrimitiveExpression)]
[Culture(SupportedCultures.Any)]
[Order(After = PredefinedExpressionParserAndInterpreterNames.NumericalExpression)]
internal sealed class PrimitiveExpressionParserAndInterpreter : IExpressionParserAndInterpreter
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
        return ParserPrimaryExpressionAsync(
            culture,
            currentToken,
            variableService,
            result,
            cancellationToken);
    }

    /// <summary>
    /// Parse an expression that can be either a primitive data, a variable reference or an expression between parenthesis.
    /// 
    /// Corresponding grammar :
    ///     Primitive_Value
    ///     | Variable Name
    ///     | '(' Expression ')'
    /// </summary>
    private async Task<bool> ParserPrimaryExpressionAsync(
        string culture,
        LinkedToken? currentToken,
        IVariableService variableService,
        ExpressionParserAndInterpreterResult result,
        CancellationToken cancellationToken)
    {
        currentToken = currentToken.SkipNextWordTokens();
        if (currentToken is not null)
        {
            // Detect Numbers, Percentage, Dates...etc.
            if (currentToken.Previous is null
                && currentToken.SkipToken(PredefinedTokenAndDataTypeNames.AdditionOperator, skipWordsToken: false, out LinkedToken? nextToken)
                && nextToken is not null)
            {
                if (nextToken.Token is IData data)
                {
                    var expression = new DataExpression(nextToken, nextToken, data);
                    result.NextTokenToContinueWith = nextToken.Next;
                    result.ParsedExpression = expression;
                    result.ResultedData = expression.Data;
                    return true;
                }
            }
            else if (currentToken.Token is IData data)
            {
                var expression = new DataExpression(currentToken, currentToken, data);
                result.NextTokenToContinueWith = currentToken.Next;
                result.ParsedExpression = expression;
                result.ResultedData = expression.Data;
                return true;
            }

            // Detect variable reference
            if (currentToken.Token.IsOfType(PredefinedTokenAndDataTypeNames.VariableReference))
            {
                var expression = new VariableReferenceExpression(currentToken);
                result.NextTokenToContinueWith = currentToken.Next;
                result.ParsedExpression = expression;
                result.ResultedData = variableService.GetVariableValue(expression.VariableName);
                return true;
            }

            // Detect expression between parenthesis.
            LinkedToken leftParenthToken = currentToken;
            if (DiscardLeftParenth(leftParenthToken, out nextToken))
            {
                bool foundExpression
                     = await ParserAndInterpreterService.TryParseAndInterpretExpressionAsync(
                         culture,
                         nextToken,
                         PredefinedTokenAndDataTypeNames.RightParenth,
                         string.Empty,
                         nestedTokenType: PredefinedTokenAndDataTypeNames.LeftParenth,
                         variableService,
                         result,
                         cancellationToken);

                LinkedToken? rightParenthToken = result.NextTokenToContinueWith?.SkipNextWordTokens();
                if (foundExpression
                    && DiscardRightParenth(rightParenthToken, out nextToken)
                    && rightParenthToken is not null)
                {
                    result.NextTokenToContinueWith = nextToken;
                    result.ParsedExpression = new GroupExpression(leftParenthToken, rightParenthToken, result.ParsedExpression!);
                    // no need to update the data in the `result`. It should already be set by `ParserAndInterpreterService`.
                    return true;
                }
            }
        }

        result.NextTokenToContinueWith = null;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool DiscardLeftParenth(LinkedToken? currentToken, out LinkedToken? nextToken)
    {
        return currentToken.SkipToken(
            PredefinedTokenAndDataTypeNames.LeftParenth,
            skipWordsToken: true,
            out nextToken);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool DiscardRightParenth(LinkedToken? currentToken, out LinkedToken? nextToken)
    {
        return currentToken.SkipToken(
            PredefinedTokenAndDataTypeNames.RightParenth,
            skipWordsToken: true,
            out nextToken);
    }
}
