namespace DevToys.Tools.Tools.Text.SmartCalculator.Api;

/// <summary>
/// Exception thrown when an arithmetic operation isn't supported.
/// </summary>
public class UnsupportedArithmeticOperationException : DataOperationException
{
    public override string GetLocalizedMessage(string culture)
    {
        // TODO: Localize.
        return "Unsupported arithmetic operation";
    }
}
