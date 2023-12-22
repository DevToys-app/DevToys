using DevToys.Tools.Tools.Text.SmartCalculator.Api.Lexer;

namespace DevToys.Tools.Tools.Text.SmartCalculator.Api.Data.Definition;

[DebuggerDisplay(
    $"Value = {{{nameof(Value)}}}, " +
    $"Type = {{{nameof(Type)}}}, " +
    $"Text = {{{nameof(GetText)}()}}, " +
    $"StartInLine = {{{nameof(StartInLine)}}}")]
public sealed record DurationData : Data<TimeSpan>, INumericData, ISupportMultipleDataTypeForArithmeticOperation
{
    public bool IsNegative => Value.Ticks < 0;

    public double NumericValueInCurrentUnit => Value.Ticks;

    public double NumericValueInStandardUnit => NumericValueInCurrentUnit;

    public override string GetDisplayText(string culture)
    {
        // TODO: Localize
        return Value.ToString();
    }

    public static DurationData CreateFrom(DurationData origin, TimeSpan value)
    {
        return new DurationData(
            origin.LineTextIncludingLineBreak,
            origin.StartInLine,
            origin.EndInLine,
            value);
    }

    public DurationData(string lineTextIncludingLineBreak, int startInLine, int endInLine, TimeSpan value)
        : base(
              lineTextIncludingLineBreak,
              startInLine,
              endInLine,
              value,
              PredefinedTokenAndDataTypeNames.Numeric,
              PredefinedTokenAndDataTypeNames.SubDataTypeNames.Duration)
    {
    }

    public override IData MergeDataLocations(IData otherData)
    {
        return new DurationData(
            LineTextIncludingLineBreak,
            Math.Min(StartInLine, otherData.StartInLine),
            Math.Max(EndInLine, otherData.EndInLine),
            Value);
    }

    public INumericData CreateFromStandardUnit(double value)
    {
        return CreateFrom(this, new TimeSpan((long)value));
    }

    public INumericData CreateFromCurrentUnit(double value)
    {
        return CreateFromStandardUnit(value);
    }

    public INumericData Add(INumericData otherData)
    {
        if (otherData is DurationData otherDuration)
            return CreateFrom(this, Value + otherDuration.Value);
        else if (otherData is DateTimeData otherDateTime)
        {
            return new DateTimeData(
                LineTextIncludingLineBreak,
                StartInLine,
                EndInLine,
                otherDateTime.Value + Value);
        }

        ThrowIncompatibleUnitsException();
        return null!;
    }

    public INumericData Substract(INumericData otherData)
    {
        if (otherData is DurationData otherDuration)
            return CreateFrom(this, Value - otherDuration.Value);
        else if (otherData is DateTimeData otherDateTime)
        {
            return new DateTimeData(
                LineTextIncludingLineBreak,
                StartInLine,
                EndInLine,
                otherDateTime.Value - Value);
        }

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
