using DevToys.Tools.Tools.Text.SmartCalculator.Api.AbstractSyntaxTree;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Lexer;

namespace DevToys.Tools.Tools.Text.SmartCalculator.Features.StatementParsersAndInterpreters.Comment;

internal sealed class CommentStatement : Statement
{
    internal CommentStatement(LinkedToken firstToken, LinkedToken lastToken)
        : base(firstToken, lastToken)
    {
    }

    public override string ToString()
    {
        return "Comment";
    }
}
