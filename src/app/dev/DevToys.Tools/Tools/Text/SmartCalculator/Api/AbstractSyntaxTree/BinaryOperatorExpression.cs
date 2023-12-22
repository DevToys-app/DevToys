using DevToys.Tools.Tools.Text.SmartCalculator.Api.Core;

namespace DevToys.Tools.Tools.Text.SmartCalculator.Api.AbstractSyntaxTree;

/// <summary>
/// Represents a binary conditional expression
/// </summary>
public sealed class BinaryOperatorExpression : Expression
{
    /// <summary>
    /// Gets or sets the left expression
    /// </summary>
    public Expression LeftExpression { get; }

    /// <summary>
    /// Gets the binary operator
    /// </summary>
    public BinaryOperatorType Operator { get; }

    /// <summary>
    /// Gets or sets the right expression
    /// </summary>
    public Expression RightExpression { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BinaryOperatorExpression"/> class.
    /// </summary>
    /// <param name="leftExpression">The left expression</param>
    /// <param name="conditionalOperator">The binary operator</param>
    /// <param name="rightExpression">The right expression</param>
    public BinaryOperatorExpression(Expression leftExpression, BinaryOperatorType conditionalOperator, Expression rightExpression)
        : base(leftExpression.FirstToken, rightExpression.LastToken)
    {
        Guard.IsNotNull(leftExpression);
        Guard.IsNotNull(rightExpression);
        LeftExpression = leftExpression;
        Operator = conditionalOperator;
        RightExpression = rightExpression;
    }

    public override string ToString()
    {
        return $"({LeftExpression} {Operator.GetDescription()} {RightExpression})";
    }
}
