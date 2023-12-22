using DevToys.Tools.Tools.Text.SmartCalculator.Api.Lexer;

namespace DevToys.Tools.Tools.Text.SmartCalculator.Api.AbstractSyntaxTree;

/// <summary>
/// Basic class for a reference to something.
/// </summary>
public abstract class ReferenceExpression : Expression
{
    protected ReferenceExpression(LinkedToken firstToken, LinkedToken lastToken)
        : base(firstToken, lastToken)
    {
    }
}
