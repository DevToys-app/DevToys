using System.Globalization;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Lexer;
using UnitsNet;

namespace DevToys.Tools.Tools.Text.SmartCalculator.Api.Data.Definition;

[DebuggerDisplay(
    $"Value = {{{nameof(NumericValueInStandardUnit)}}}, " +
    $"Type = {{{nameof(Type)}}}, " +
    $"Text = {{{nameof(GetText)}()}}, " +
    $"StartInLine = {{{nameof(StartInLine)}}}")]
public sealed record SpeedData : Data<Speed>, INumericData
{
    public bool IsNegative => Value.Value < 0;

    public double NumericValueInCurrentUnit => (double)Value.Value;

    public double NumericValueInStandardUnit { get; }

    public override string GetDisplayText(string culture)
    {
        return Value.ToString("s4", new CultureInfo(culture));
    }

    public static SpeedData CreateFrom(SpeedData origin, Speed value)
    {
        return new SpeedData(
            origin.LineTextIncludingLineBreak,
            origin.StartInLine,
            origin.EndInLine,
            value);
    }

    public SpeedData(string lineTextIncludingLineBreak, int startInLine, int endInLine, Speed value)
        : base(
              lineTextIncludingLineBreak,
              startInLine,
              endInLine,
              value,
              PredefinedTokenAndDataTypeNames.Numeric,
              PredefinedTokenAndDataTypeNames.SubDataTypeNames.Speed)
    {
        NumericValueInStandardUnit = value.ToUnit(Speed.BaseUnit).Value;
    }

    public override IData MergeDataLocations(IData otherData)
    {
        return new SpeedData(
            LineTextIncludingLineBreak,
            Math.Min(StartInLine, otherData.StartInLine),
            Math.Max(EndInLine, otherData.EndInLine),
            Value);
    }

    public INumericData CreateFromStandardUnit(double value)
    {
        return CreateFrom(this, new Speed(value, Speed.BaseUnit).ToUnit(Value.Unit));
    }

    public INumericData CreateFromCurrentUnit(double value)
    {
        return CreateFrom(this, new Speed(value, Value.Unit));
    }

    public INumericData Add(INumericData otherData)
    {
        return CreateFrom(this, Value + ((SpeedData)otherData).Value);
    }

    public INumericData Substract(INumericData otherData)
    {
        return CreateFrom(this, Value - ((SpeedData)otherData).Value);
    }

    public INumericData Multiply(INumericData otherData)
    {
        if (otherData is DecimalData)
            return CreateFromCurrentUnit(NumericValueInCurrentUnit * otherData.NumericValueInCurrentUnit);

        ThrowUnsupportedArithmeticOperationException();
        return null!;
    }

    public INumericData Divide(INumericData otherData)
    {
        if (otherData is DecimalData)
            return CreateFromCurrentUnit(NumericValueInCurrentUnit / otherData.NumericValueInCurrentUnit);

        return new DecimalData(
            LineTextIncludingLineBreak,
            StartInLine,
            EndInLine,
            NumericValueInStandardUnit / otherData.NumericValueInStandardUnit);
    }

    public override string ToString()
    {
        return base.ToString();
    }
}
