using DevToys.Tools.Tools.Text.SmartCalculator.Api.AbstractSyntaxTree;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Lexer;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Metadata;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.ParserAndInterpreter;

namespace DevToys.Tools.Tools.Text.SmartCalculator.Features.StatementParsersAndInterpreters.VariableDeclaration;

[Export(typeof(IStatementParserAndInterpreter))]
[Culture(SupportedCultures.Any)]
[Name(PredefinedStatementParserAndInterpreterNames.VariableStatement)]
[Order(After = PredefinedStatementParserAndInterpreterNames.CommentStatement)]
[Order(After = PredefinedStatementParserAndInterpreterNames.HeaderStatement)]
internal sealed class VariableDeclarationStatementParserAndInterpreter : IStatementParserAndInterpreter
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
        bool foundEqualSymbol
            = currentToken.JumpToNextTokenOfType(
                PredefinedTokenAndDataTypeNames.SymbolOrPunctuation,
                "=",
                out LinkedToken? equalToken);

        if (!foundEqualSymbol || equalToken is null)
            return false;

        LinkedToken variableNameStart;
        LinkedToken? variableNameEnd;

        if (IsAssigneeAnExistingVariable(equalToken))
        {
            variableNameStart = equalToken.Previous!;
            variableNameEnd = equalToken.Previous!;
        }
        else
        {
            LinkedToken? previousToken = equalToken.Previous;
            variableNameStart = equalToken;
            variableNameEnd = previousToken;
            while (previousToken is not null)
            {
                variableNameStart = previousToken;
                if (previousToken.Token.IsNotOfType(PredefinedTokenAndDataTypeNames.Word)
                    && previousToken.Token.IsNotOfType(PredefinedTokenAndDataTypeNames.VariableReference))
                {
                    return false;
                }

                previousToken = previousToken.Previous;
            }
        }

        if (variableNameEnd is not null)
        {
            string variableName = variableNameStart.Token.GetText(variableNameStart.Token.StartInLine, variableNameEnd.Token.EndInLine);
            if (!string.IsNullOrWhiteSpace(variableName))
            {
                ExpressionParserAndInterpreterResult assignedExpressionResult = new();
                bool foundAssignedExpression
                    = await ParserAndInterpreterService.TryParseAndInterpretExpressionAsync(
                        culture,
                        equalToken.Next,
                        variableService,
                        assignedExpressionResult,
                        cancellationToken);

                if (foundAssignedExpression)
                {
                    var statement
                        = new VariableDeclarationStatement(
                            variableNameStart,
                            variableNameEnd,
                            variableName,
                            assignedExpressionResult.ParsedExpression!);
                    variableService.SetVariableValue(statement.VariableName, assignedExpressionResult.ResultedData);

                    result.ParsedStatement = statement;
                    result.ResultedData = assignedExpressionResult.ResultedData;
                    return true;
                }
            }
        }

        return false;
    }

    private static bool IsAssigneeAnExistingVariable(LinkedToken currentToken)
    {
        return currentToken.Previous is not null
            && currentToken.Previous.Token.IsOfType(PredefinedTokenAndDataTypeNames.VariableReference)
            && currentToken.Previous.Previous is null;
    }
}
