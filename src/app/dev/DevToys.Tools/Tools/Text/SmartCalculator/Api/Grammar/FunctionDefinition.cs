using DevToys.Tools.Tools.Text.SmartCalculator.Api.Lexer;

namespace DevToys.Tools.Tools.Text.SmartCalculator.Api.Grammar;

[DebuggerDisplay($"FunctionFullName = {{{nameof(FunctionFullName)}}}")]
public sealed class FunctionDefinition
{
    private readonly Lazy<int> _tokenCount;

    public string FunctionFullName { get; }

    public LinkedToken TokenizedFunctionDefinition { get; }

    public int TokenCount => _tokenCount.Value;

    public FunctionDefinition(string functionFullName, LinkedToken tokenizedFunctionDefinition)
    {
        Guard.IsNotNullOrWhiteSpace(functionFullName);
        Guard.IsNotNull(tokenizedFunctionDefinition);
        FunctionFullName = functionFullName;
        TokenizedFunctionDefinition = tokenizedFunctionDefinition;

        _tokenCount = new Lazy<int>(() =>
        {
            int count = 0;
            LinkedToken? token = TokenizedFunctionDefinition;
            while (token is not null)
            {
                count++;
                token = token.Next;
            }
            return count;
        });
    }

    public override string ToString()
    {
        return FunctionFullName;
    }
}
