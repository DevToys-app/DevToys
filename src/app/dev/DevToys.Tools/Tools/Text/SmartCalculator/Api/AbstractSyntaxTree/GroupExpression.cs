using DevToys.Tools.Tools.Text.SmartCalculator.Api.Lexer;

namespace DevToys.Tools.Tools.Text.SmartCalculator.Api.AbstractSyntaxTree;

/// <summary>
/// Represents an expression between parenthesis.
/// </summary>
public sealed class GroupExpression : Expression
{
    /// <summary>
    /// Gets or sets the expression in the group
    /// </summary>
    public Expression InnerExpression { get; }

    public GroupExpression(LinkedToken firstToken, LinkedToken lastToken, Expression innerExpression)
        : base(firstToken, lastToken)
    {
        Guard.IsNotNull(innerExpression);
        InnerExpression = innerExpression;
    }

    public override string ToString()
    {
        return $"({InnerExpression})";
    }
}
