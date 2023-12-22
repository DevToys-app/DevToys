using System.Globalization;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Lexer;

namespace DevToys.Tools.Tools.Text.SmartCalculator.Api.Data.Definition;

[DebuggerDisplay(
    $"Value = {{{nameof(NumericValueInStandardUnit)}}}, " +
    $"Type = {{{nameof(Type)}}}, " +
    $"Text = {{{nameof(GetText)}()}}, " +
    $"StartInLine = {{{nameof(StartInLine)}}}")]
public sealed record CurrencyData : Data<CurrencyValue>, INumericData
{
    private readonly ICurrencyService _currencyService;
    private readonly AsyncLazy<double> _numericValueInStandardUnit;

    public bool IsNegative => Value.Value < 0;

    public double NumericValueInCurrentUnit => Value.Value;

    public double NumericValueInStandardUnit => _numericValueInStandardUnit.GetValueAsync().CompleteOnCurrentThread();

    public override string GetDisplayText(string culture)
    {
        // TODO => Localize.

        string valueString = ((double)decimal.Round((decimal)Value.Value, 2)).ToString(new CultureInfo(culture));
        if (!string.IsNullOrWhiteSpace(Value.IsoCurrency))
            return $"{valueString} {Value.IsoCurrency}";

        return $"{valueString} {Value.Currency}";
    }

    public static CurrencyData CreateFrom(ICurrencyService currencyService, CurrencyData origin, CurrencyValue value)
    {
        return new CurrencyData(
            currencyService,
            origin.LineTextIncludingLineBreak,
            origin.StartInLine,
            origin.EndInLine,
            value);
    }

    public CurrencyData(
        ICurrencyService currencyService,
        string lineTextIncludingLineBreak,
        int startInLine,
        int endInLine,
        CurrencyValue value)
        : base(
              lineTextIncludingLineBreak,
              startInLine,
              endInLine,
              value,
              PredefinedTokenAndDataTypeNames.Numeric,
              PredefinedTokenAndDataTypeNames.SubDataTypeNames.Currency)
    {
        Guard.IsNotNull(currencyService);
        _currencyService = currencyService;

        _numericValueInStandardUnit = new AsyncLazy<double>(() => ConvertAsync(value.IsoCurrency, value.Value, "USD"));
    }

    public override IData MergeDataLocations(IData otherData)
    {
        return new CurrencyData(
            _currencyService,
            LineTextIncludingLineBreak,
            Math.Min(StartInLine, otherData.StartInLine),
            Math.Max(EndInLine, otherData.EndInLine),
            Value);
    }

    public INumericData CreateFromStandardUnit(double value)
    {
        double newValue = ConvertAsync("USD", value, Value.IsoCurrency).CompleteOnCurrentThread();
        return CreateFromCurrentUnit(newValue);
    }

    public INumericData CreateFromCurrentUnit(double value)
    {
        return CreateFrom(_currencyService, this, new CurrencyValue(value, Value.Currency, Value.IsoCurrency));
    }

    public INumericData Add(INumericData otherData)
    {
        if (otherData is CurrencyData otherCurrencyData)
        {
            if (Value.IsoCurrency == otherCurrencyData.Value.IsoCurrency)
                return CreateFromCurrentUnit(NumericValueInCurrentUnit + otherCurrencyData.NumericValueInCurrentUnit);

            return CreateFromCurrentUnit(NumericValueInCurrentUnit + otherCurrencyData.ConvertAsync(Value.IsoCurrency).CompleteOnCurrentThread());
        }

        return CreateFromStandardUnit(NumericValueInCurrentUnit + otherData.NumericValueInStandardUnit);
    }

    public INumericData Substract(INumericData otherData)
    {
        if (otherData is CurrencyData otherCurrencyData)
        {
            if (Value.IsoCurrency == otherCurrencyData.Value.IsoCurrency)
                return CreateFromCurrentUnit(NumericValueInCurrentUnit - otherCurrencyData.NumericValueInCurrentUnit);

            return CreateFromCurrentUnit(NumericValueInCurrentUnit - otherCurrencyData.ConvertAsync(Value.IsoCurrency).CompleteOnCurrentThread());
        }

        return CreateFromStandardUnit(NumericValueInCurrentUnit - otherData.NumericValueInStandardUnit);
    }

    public INumericData Multiply(INumericData otherData)
    {
        if (otherData is CurrencyData otherCurrencyData)
        {
            if (Value.IsoCurrency == otherCurrencyData.Value.IsoCurrency)
                return CreateFromCurrentUnit(NumericValueInCurrentUnit * otherCurrencyData.NumericValueInCurrentUnit);

            return CreateFromCurrentUnit(NumericValueInCurrentUnit * otherCurrencyData.ConvertAsync(Value.IsoCurrency).CompleteOnCurrentThread());
        }

        return CreateFromCurrentUnit(NumericValueInCurrentUnit * otherData.NumericValueInStandardUnit);
    }

    public INumericData Divide(INumericData otherData)
    {
        if (otherData is CurrencyData otherCurrencyData)
        {
            if (Value.IsoCurrency == otherCurrencyData.Value.IsoCurrency)
                return CreateFromCurrentUnit(NumericValueInCurrentUnit / otherCurrencyData.NumericValueInCurrentUnit);

            return CreateFromCurrentUnit(NumericValueInCurrentUnit / otherCurrencyData.ConvertAsync(Value.IsoCurrency).CompleteOnCurrentThread());
        }

        return CreateFromCurrentUnit(NumericValueInCurrentUnit / otherData.NumericValueInStandardUnit);
    }

    public override string ToString()
    {
        return base.ToString();
    }

    internal Task<double> ConvertAsync(string to)
    {
        return ConvertAsync(Value.IsoCurrency, Value.Value, to);
    }

    private async Task<double> ConvertAsync(string from, double value, string to)
    {
        double? usdValue = await _currencyService.ConvertCurrencyAsync(from, value, to);
        if (usdValue.HasValue)
            return usdValue.Value;

        throw new ConversionFailedExpression();
    }

    /// <summary>
    /// Exception thrown when a currency conversion failed.
    /// </summary>
    private class ConversionFailedExpression : DataOperationException
    {
        public override string GetLocalizedMessage(string culture)
        {
            // TODO: Localize.
            return "Unable to retrieve exchange rates.";
        }
    }
}
