using DevToys.Tools.Tools.Text.SmartCalculator.Api.Lexer;

namespace DevToys.Tools.Tools.Text.SmartCalculator.Api.AbstractSyntaxTree;

/// <summary>
/// Basic class that represents an expression in a statement.
/// </summary>
public abstract class Expression : AbstractSyntaxTreeBase
{
    protected Expression(LinkedToken firstToken, LinkedToken lastToken)
        : base(firstToken, lastToken)
    {
    }
}
