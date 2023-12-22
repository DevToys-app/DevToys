using DevToys.Tools.Tools.Text.SmartCalculator.Api.Grammar;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Lexer;

namespace DevToys.Tools.Tools.Text.SmartCalculator.Api.AbstractSyntaxTree;

public sealed class FunctionExpression : Expression
{
    public FunctionDefinition FunctionDefinition { get; }

    public FunctionExpression(FunctionDefinition functionDefinition, LinkedToken firstToken, LinkedToken lastToken)
        : base(firstToken, lastToken)
    {
        FunctionDefinition = functionDefinition;
    }

    public override string ToString()
    {
        return $"function {FunctionDefinition.FunctionFullName}()";
    }
}
