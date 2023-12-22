using DevToys.Tools.Tools.Text.SmartCalculator.Api.Lexer;

namespace DevToys.Tools.Tools.Text.SmartCalculator.Api.AbstractSyntaxTree;

public abstract class AbstractSyntaxTreeBase
{
    public LinkedToken FirstToken { get; }

    public LinkedToken LastToken { get; }

    protected AbstractSyntaxTreeBase(LinkedToken firstToken, LinkedToken lastToken)
    {
        Guard.IsNotNull(firstToken);
        Guard.IsNotNull(lastToken);
        FirstToken = firstToken;
        LastToken = lastToken;
    }

    /// <summary>
    /// Gets a string representation of the expression or statement.
    /// </summary>
    /// <returns>String that reprensents the expression or statement</returns>
    public abstract override string ToString();
}
