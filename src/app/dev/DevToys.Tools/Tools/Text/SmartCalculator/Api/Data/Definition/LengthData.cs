using System.Globalization;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Lexer;
using UnitsNet;
using UnitsNet.Units;

namespace DevToys.Tools.Tools.Text.SmartCalculator.Api.Data.Definition;

[DebuggerDisplay(
    $"Value = {{{nameof(NumericValueInStandardUnit)}}}, " +
    $"Type = {{{nameof(Type)}}}, " +
    $"Text = {{{nameof(GetText)}()}}, " +
    $"StartInLine = {{{nameof(StartInLine)}}}")]
public sealed record LengthData : Data<Length>, INumericData
{
    public bool IsNegative => Value.Value < 0;

    public double NumericValueInCurrentUnit => Value.Value;

    public double NumericValueInStandardUnit { get; }

    public override string GetDisplayText(string culture)
    {
        return ToBestUnitForDisplay(Value).ToString("s4", new CultureInfo(culture));
    }

    public static LengthData CreateFrom(LengthData origin, Length value)
    {
        return new LengthData(
            origin.LineTextIncludingLineBreak,
            origin.StartInLine,
            origin.EndInLine,
            value);
    }

    public LengthData(string lineTextIncludingLineBreak, int startInLine, int endInLine, Length value)
        : base(
              lineTextIncludingLineBreak,
              startInLine,
              endInLine,
              value,
              PredefinedTokenAndDataTypeNames.Numeric,
              PredefinedTokenAndDataTypeNames.SubDataTypeNames.Length)
    {
        NumericValueInStandardUnit = value.ToUnit(UnitsNet.Length.BaseUnit).Value;
    }

    public override IData MergeDataLocations(IData otherData)
    {
        return new LengthData(
            LineTextIncludingLineBreak,
            Math.Min(StartInLine, otherData.StartInLine),
            Math.Max(EndInLine, otherData.EndInLine),
            Value);
    }

    public INumericData CreateFromStandardUnit(double value)
    {
        return CreateFrom(this, new Length(value, UnitsNet.Length.BaseUnit).ToUnit(Value.Unit));
    }

    public INumericData CreateFromCurrentUnit(double value)
    {
        return CreateFrom(this, new Length(value, Value.Unit));
    }

    public INumericData Add(INumericData otherData)
    {
        return CreateFrom(this, Value + ((LengthData)otherData).Value);
    }

    public INumericData Substract(INumericData otherData)
    {
        return CreateFrom(this, Value - ((LengthData)otherData).Value);
    }

    public INumericData Multiply(INumericData otherData)
    {
        if (otherData is DecimalData)
            return CreateFromCurrentUnit(NumericValueInCurrentUnit * otherData.NumericValueInCurrentUnit);

        var otherLength = (LengthData)otherData;

        return new AreaData(
            LineTextIncludingLineBreak,
            StartInLine,
            EndInLine,
            Value * otherLength.Value);
    }

    public INumericData Divide(INumericData otherData)
    {
        if (otherData is DecimalData)
            return CreateFromCurrentUnit(NumericValueInCurrentUnit / otherData.NumericValueInCurrentUnit);

        var otherLength = (LengthData)otherData;
        return new DecimalData(
            LineTextIncludingLineBreak,
            StartInLine,
            EndInLine,
            Value / otherLength.Value);
    }

    public override string ToString()
    {
        return base.ToString();
    }

    private static Length ToBestUnitForDisplay(Length length)
    {
        if (length.Unit == LengthUnit.Meter && length.Meters >= 1_000)
            return length.ToUnit(LengthUnit.Kilometer);

        if (length.Unit == LengthUnit.Kilometer && length.Meters < 1_000 && length.Meters > 0)
            return length.ToUnit(LengthUnit.Kilometer);

        return length;
    }
}
