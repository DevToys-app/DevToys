using System.Globalization;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Lexer;

namespace DevToys.Tools.Tools.Text.SmartCalculator.Api.Data.Definition;

[DebuggerDisplay(
    $"Value = {{{nameof(NumericValueInStandardUnit)}}}, " +
    $"Type = {{{nameof(Type)}}}, " +
    $"Text = {{{nameof(GetText)}()}}, " +
    $"StartInLine = {{{nameof(StartInLine)}}}")]
public sealed record PercentageData : Data<double>, INumericData
{
    public bool IsNegative => Value < 0;

    public double NumericValueInCurrentUnit => Value;

    public double NumericValueInStandardUnit => NumericValueInCurrentUnit;

    public override string GetDisplayText(string culture)
    {
        // TODO => Localize
        if (double.IsInfinity(Value) || Value == 0)
            return Value.ToString(new CultureInfo(culture));

        return ((double)decimal.Round((decimal)Value * 100, 10)).ToString(new CultureInfo(culture)) + "%";
    }

    public static PercentageData CreateFrom(PercentageData origin, double value)
    {
        return new PercentageData(
            origin.LineTextIncludingLineBreak,
            origin.StartInLine,
            origin.EndInLine,
            value);
    }

    public PercentageData(string lineTextIncludingLineBreak, int startInLine, int endInLine, double value)
        : base(
              lineTextIncludingLineBreak,
              startInLine,
              endInLine,
              value,
              PredefinedTokenAndDataTypeNames.Numeric,
              PredefinedTokenAndDataTypeNames.SubDataTypeNames.Percentage)
    {
    }

    public override IData MergeDataLocations(IData otherData)
    {
        return new PercentageData(
            LineTextIncludingLineBreak,
            Math.Min(StartInLine, otherData.StartInLine),
            Math.Max(EndInLine, otherData.EndInLine),
            Value);
    }

    public INumericData CreateFromStandardUnit(double value)
    {
        return CreateFrom(this, value);
    }

    public INumericData CreateFromCurrentUnit(double value)
    {
        return CreateFrom(this, value);
    }

    public INumericData Add(INumericData otherData)
    {
        Guard.IsOfType<PercentageData>(otherData);
        return CreateFrom(this, NumericValueInStandardUnit + NumericValueInStandardUnit * otherData.NumericValueInStandardUnit);
    }

    public INumericData Substract(INumericData otherData)
    {
        Guard.IsOfType<PercentageData>(otherData);
        return CreateFrom(this, NumericValueInStandardUnit - NumericValueInStandardUnit * otherData.NumericValueInStandardUnit);
    }

    public INumericData Multiply(INumericData otherData)
    {
        Guard.IsOfType<PercentageData>(otherData);
        return CreateFrom(this, NumericValueInStandardUnit * otherData.NumericValueInStandardUnit);
    }

    public INumericData Divide(INumericData otherData)
    {
        Guard.IsOfType<PercentageData>(otherData);
        return CreateFrom(this, NumericValueInStandardUnit / otherData.NumericValueInStandardUnit);
    }

    public override string ToString()
    {
        return base.ToString();
    }
}
