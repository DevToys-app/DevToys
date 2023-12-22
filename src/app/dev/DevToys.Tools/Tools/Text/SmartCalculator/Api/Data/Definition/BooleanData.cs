using System.Globalization;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Lexer;

namespace DevToys.Tools.Tools.Text.SmartCalculator.Api.Data.Definition;

[DebuggerDisplay(
    $"Value = {{{nameof(Value)}}}, " +
    $"Type = {{{nameof(Type)}}}, " +
    $"Text = {{{nameof(GetText)}()}}, " +
    $"StartInLine = {{{nameof(StartInLine)}}}")]
public sealed record BooleanData : Data<bool>, INumericData, IDecimal
{
    public bool IsNegative => false;

    public double NumericValueInCurrentUnit => Convert.ToInt32(Value);

    public double NumericValueInStandardUnit => NumericValueInCurrentUnit;

    public override string GetDisplayText(string culture)
    {
        return Value.ToString(new CultureInfo(culture));
        // TODO => Localize. For example, in french, double separator is `,` instead of `.`
    }

    public static BooleanData CreateFrom(BooleanData origin, bool value)
    {
        return new BooleanData(
            origin.LineTextIncludingLineBreak,
            origin.StartInLine,
            origin.EndInLine,
            value);
    }

    public BooleanData(string lineTextIncludingLineBreak, int startInLine, int endInLine, bool value)
        : base(
              lineTextIncludingLineBreak,
              startInLine,
              endInLine,
              value,
              PredefinedTokenAndDataTypeNames.Numeric,
              PredefinedTokenAndDataTypeNames.SubDataTypeNames.Decimal)
    {
    }

    public override IData MergeDataLocations(IData otherData)
    {
        return new BooleanData(
            LineTextIncludingLineBreak,
            Math.Min(StartInLine, otherData.StartInLine),
            Math.Max(EndInLine, otherData.EndInLine),
            Value);
    }

    public INumericData CreateFromStandardUnit(double value)
    {
        return CreateBooleanValue(value);
    }

    public INumericData CreateFromCurrentUnit(double value)
    {
        return CreateFromStandardUnit(value);
    }

    public INumericData Add(INumericData otherData)
    {
        return CreateBooleanValue(NumericValueInStandardUnit + otherData.NumericValueInStandardUnit);
    }

    public INumericData Substract(INumericData otherData)
    {
        return CreateBooleanValue(NumericValueInStandardUnit - otherData.NumericValueInStandardUnit);
    }

    public INumericData Multiply(INumericData otherData)
    {
        return CreateBooleanValue(NumericValueInStandardUnit * otherData.NumericValueInStandardUnit);
    }

    public INumericData Divide(INumericData otherData)
    {
        return CreateBooleanValue(NumericValueInStandardUnit / otherData.NumericValueInStandardUnit);
    }

    public override string ToString()
    {
        return base.ToString();
    }

    private INumericData CreateBooleanValue(double value)
    {
        if (value == 0 || value == 1)
            return CreateFrom(this, value == 1);

        return new DecimalData(
            LineTextIncludingLineBreak,
            StartInLine,
            EndInLine,
            value);
    }
}
