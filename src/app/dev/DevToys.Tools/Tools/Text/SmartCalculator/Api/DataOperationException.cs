namespace DevToys.Tools.Tools.Text.SmartCalculator.Api;

public abstract class DataOperationException : Exception
{
    public virtual string GetLocalizedMessage(string culture)
    {
        // TODO: Localize.
        return "Unknown error";
    }
}
