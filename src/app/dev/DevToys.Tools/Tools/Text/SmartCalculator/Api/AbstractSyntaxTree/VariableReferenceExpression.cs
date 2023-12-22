using DevToys.Tools.Tools.Text.SmartCalculator.Api.Lexer;

namespace DevToys.Tools.Tools.Text.SmartCalculator.Api.AbstractSyntaxTree;

public sealed class VariableReferenceExpression : ReferenceExpression
{
    public string VariableName { get; }

    public LinkedToken VariableToken { get; }

    public VariableReferenceExpression(LinkedToken token)
        : base(token, token)
    {
        Guard.IsNotNull(token);
        VariableToken = token;
        VariableName = token.Token.GetText();
    }

    public override string ToString()
    {
        return $"$({VariableName})";
    }
}
