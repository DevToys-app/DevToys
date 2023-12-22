using DevToys.Tools.Tools.Text.SmartCalculator.Api.AbstractSyntaxTree;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Lexer;

namespace DevToys.Tools.Tools.Text.SmartCalculator.Features.StatementParsersAndInterpreters.NumericalExpression;

internal sealed class NumericalCalculusStatement : Statement
{
    internal Expression NumericalCalculusExpression { get; }

    internal NumericalCalculusStatement(LinkedToken firstToken, LinkedToken lastToken, Expression numericalCalculusExpression)
        : base(firstToken, lastToken)
    {
        NumericalCalculusExpression = numericalCalculusExpression;
    }

    public override string ToString()
    {
        return NumericalCalculusExpression.ToString();
    }
}
