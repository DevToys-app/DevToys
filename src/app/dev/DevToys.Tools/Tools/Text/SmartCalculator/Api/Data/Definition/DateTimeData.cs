using System.Globalization;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Lexer;

namespace DevToys.Tools.Tools.Text.SmartCalculator.Api.Data.Definition;

[DebuggerDisplay(
    $"Value = {{{nameof(Value)}}}, " +
    $"Type = {{{nameof(Type)}}}, " +
    $"Text = {{{nameof(GetText)}()}}, " +
    $"StartInLine = {{{nameof(StartInLine)}}}")]
public sealed record DateTimeData : Data<DateTime>, INumericData, ISupportMultipleDataTypeForArithmeticOperation
{
    public bool IsNegative => Value.Ticks < 0;

    public double NumericValueInCurrentUnit => Value.Ticks;

    public double NumericValueInStandardUnit => NumericValueInCurrentUnit;

    public override string GetDisplayText(string culture)
    {
        return Value.ToString(new CultureInfo(culture));
    }

    public static DateTimeData CreateFrom(DateTimeData origin, DateTime value)
    {
        return new DateTimeData(
            origin.LineTextIncludingLineBreak,
            origin.StartInLine,
            origin.EndInLine,
            value);
    }

    public DateTimeData(string lineTextIncludingLineBreak, int startInLine, int endInLine, DateTime value)
        : base(
              lineTextIncludingLineBreak,
              startInLine,
              endInLine,
              value,
              PredefinedTokenAndDataTypeNames.Numeric,
              PredefinedTokenAndDataTypeNames.SubDataTypeNames.DateTime)
    {
    }

    public override IData MergeDataLocations(IData otherData)
    {
        return new DateTimeData(
            LineTextIncludingLineBreak,
            Math.Min(StartInLine, otherData.StartInLine),
            Math.Max(EndInLine, otherData.EndInLine),
            Value);
    }

    public INumericData CreateFromStandardUnit(double value)
    {
        return CreateFrom(this, new DateTime((long)value));
    }

    public INumericData CreateFromCurrentUnit(double value)
    {
        return CreateFromStandardUnit(value);
    }

    public INumericData Add(INumericData otherData)
    {
        if (otherData is DurationData otherDuration)
            return CreateFrom(this, Value + otherDuration.Value);

        ThrowIncompatibleUnitsException();
        return null!;
    }

    public INumericData Substract(INumericData otherData)
    {
        if (otherData is DurationData otherDuration)
            return CreateFrom(this, Value - otherDuration.Value);

        ThrowIncompatibleUnitsException();
        return null!;
    }

    public INumericData Multiply(INumericData otherData)
    {
        ThrowUnsupportedArithmeticOperationException();
        return null!;
    }

    public INumericData Divide(INumericData otherData)
    {
        ThrowUnsupportedArithmeticOperationException();
        return null!;
    }

    public override string ToString()
    {
        return base.ToString();
    }
}
