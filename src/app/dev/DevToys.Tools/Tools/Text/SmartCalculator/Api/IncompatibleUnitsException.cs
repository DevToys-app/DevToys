namespace DevToys.Tools.Tools.Text.SmartCalculator.Api;

/// <summary>
/// Exception thrown when data have incompatible units during an arithmetic operation.
/// </summary>
public class IncompatibleUnitsException : DataOperationException
{
    public override string GetLocalizedMessage(string culture)
    {
        // TODO: Localize.
        return "Incompatible units";
    }
}
