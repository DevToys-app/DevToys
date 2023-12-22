using System.Globalization;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Lexer;

namespace DevToys.Tools.Tools.Text.SmartCalculator.Api.Data.Definition;

[DebuggerDisplay(
    $"Value = {{{nameof(NumericValueInStandardUnit)}}}, " +
    $"Type = {{{nameof(Type)}}}, " +
    $"Text = {{{nameof(GetText)}()}}, " +
    $"StartInLine = {{{nameof(StartInLine)}}}")]
public sealed record OrdinalData : Data<long>, INumericData, IDecimal
{
    public bool IsNegative => Value < 0;

    public double NumericValueInCurrentUnit => Value;

    public double NumericValueInStandardUnit => NumericValueInCurrentUnit;

    public override string GetDisplayText(string culture)
    {
        // TODO: Show "th", "st", "rd"...
        return Value.ToString(new CultureInfo(culture));
    }

    public static OrdinalData CreateFrom(OrdinalData origin, long value)
    {
        return new OrdinalData(
            origin.LineTextIncludingLineBreak,
            origin.StartInLine,
            origin.EndInLine,
            value);
    }

    public OrdinalData(string lineTextIncludingLineBreak, int startInLine, int endInLine, long value)
        : base(
              lineTextIncludingLineBreak,
              startInLine,
              endInLine,
              value,
              PredefinedTokenAndDataTypeNames.Numeric,
              PredefinedTokenAndDataTypeNames.SubDataTypeNames.Ordinal)
    {
    }

    public override IData MergeDataLocations(IData otherData)
    {
        return new OrdinalData(
            LineTextIncludingLineBreak,
            Math.Min(StartInLine, otherData.StartInLine),
            Math.Max(EndInLine, otherData.EndInLine),
            Value);
    }

    public INumericData CreateFromStandardUnit(double value)
    {
        return CreateFrom(this, (long)value);
    }

    public INumericData CreateFromCurrentUnit(double value)
    {
        return CreateFromStandardUnit(value);
    }

    public INumericData Add(INumericData otherData)
    {
        // TODO: Unit tests & Implement
        throw new NotImplementedException();
    }

    public INumericData Substract(INumericData otherData)
    {
        throw new NotImplementedException();
    }

    public INumericData Multiply(INumericData otherData)
    {
        throw new NotImplementedException();
    }

    public INumericData Divide(INumericData otherData)
    {
        throw new NotImplementedException();
    }

    public override string ToString()
    {
        return base.ToString();
    }
}
