using DevToys.Tools.Tools.Text.SmartCalculator.Api.AbstractSyntaxTree;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Lexer;

namespace DevToys.Tools.Tools.Text.SmartCalculator.Features.StatementParsersAndInterpreters.Header;

internal sealed class HeaderStatement : Statement
{
    internal HeaderStatement(LinkedToken firstToken, LinkedToken lastToken)
        : base(firstToken, lastToken)
    {
    }

    public override string ToString()
    {
        return "Header";
    }
}
