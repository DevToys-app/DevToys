namespace DevToys.Tools.Tools.Text.SmartCalculator.Api.Data.Definition;

public struct CurrencyValue
{
    public CurrencyValue(double value, string currency, string isoCurrency)
    {
        Value = value;
        Currency = currency;
        IsoCurrency = isoCurrency;
    }

    public double Value { get; }

    public string Currency { get; }

    public string IsoCurrency { get; }
}
