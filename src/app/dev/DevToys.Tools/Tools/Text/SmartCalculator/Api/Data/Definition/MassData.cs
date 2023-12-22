using System.Globalization;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Lexer;
using UnitsNet;

namespace DevToys.Tools.Tools.Text.SmartCalculator.Api.Data.Definition;

[DebuggerDisplay(
    $"Value = {{{nameof(NumericValueInStandardUnit)}}}, " +
    $"Type = {{{nameof(Type)}}}, " +
    $"Text = {{{nameof(GetText)}()}}, " +
    $"StartInLine = {{{nameof(StartInLine)}}}")]
public sealed record MassData : Data<Mass>, INumericData
{
    public bool IsNegative => Value.Value < 0;

    public double NumericValueInCurrentUnit => (double)Value.Value;

    public double NumericValueInStandardUnit { get; }

    public override string GetDisplayText(string culture)
    {
        return Value.ToString("s4", new CultureInfo(culture));
    }

    public static MassData CreateFrom(MassData origin, Mass value)
    {
        return new MassData(
            origin.LineTextIncludingLineBreak,
            origin.StartInLine,
            origin.EndInLine,
            value);
    }

    public MassData(string lineTextIncludingLineBreak, int startInLine, int endInLine, Mass value)
        : base(
              lineTextIncludingLineBreak,
              startInLine,
              endInLine,
              value,
              PredefinedTokenAndDataTypeNames.Numeric,
              PredefinedTokenAndDataTypeNames.SubDataTypeNames.Mass)
    {
        NumericValueInStandardUnit = value.ToUnit(Mass.BaseUnit).Value;
    }

    public override IData MergeDataLocations(IData otherData)
    {
        return new MassData(
            LineTextIncludingLineBreak,
            Math.Min(StartInLine, otherData.StartInLine),
            Math.Max(EndInLine, otherData.EndInLine),
            Value);
    }

    public INumericData CreateFromStandardUnit(double value)
    {
        return CreateFrom(this, new Mass(value, Mass.BaseUnit).ToUnit(Value.Unit));
    }

    public INumericData CreateFromCurrentUnit(double value)
    {
        return CreateFrom(this, new Mass(value, Value.Unit));
    }

    public INumericData Add(INumericData otherData)
    {
        return CreateFrom(this, Value + ((MassData)otherData).Value);
    }

    public INumericData Substract(INumericData otherData)
    {
        return CreateFrom(this, Value - ((MassData)otherData).Value);
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
