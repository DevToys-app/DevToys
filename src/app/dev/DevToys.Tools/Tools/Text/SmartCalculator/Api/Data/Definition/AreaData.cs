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
public sealed record AreaData : Data<Area>, INumericData
{
    public bool IsNegative => Value.Value < 0;

    public double NumericValueInCurrentUnit => (double)Value.Value;

    public double NumericValueInStandardUnit { get; }

    public override string GetDisplayText(string culture)
    {
        return ToBestUnitForDisplay(Value).ToString("s4", new CultureInfo(culture));
    }

    public static AreaData CreateFrom(AreaData origin, Area value)
    {
        return new AreaData(
            origin.LineTextIncludingLineBreak,
            origin.StartInLine,
            origin.EndInLine,
            value);
    }

    public AreaData(string lineTextIncludingLineBreak, int startInLine, int endInLine, Area value)
        : base(
              lineTextIncludingLineBreak,
              startInLine,
              endInLine,
              value,
              PredefinedTokenAndDataTypeNames.Numeric,
              PredefinedTokenAndDataTypeNames.SubDataTypeNames.Area)
    {
        NumericValueInStandardUnit = value.ToUnit(Area.BaseUnit).Value;
    }

    public override IData MergeDataLocations(IData otherData)
    {
        return new AreaData(
            LineTextIncludingLineBreak,
            Math.Min(StartInLine, otherData.StartInLine),
            Math.Max(EndInLine, otherData.EndInLine),
            Value);
    }

    public INumericData CreateFromStandardUnit(double value)
    {
        return CreateFrom(this, new Area(value, Area.BaseUnit).ToUnit(Value.Unit));
    }

    public INumericData CreateFromCurrentUnit(double value)
    {
        return CreateFrom(this, new Area(value, Value.Unit));
    }

    public INumericData Add(INumericData otherData)
    {
        return CreateFrom(this, Value + ((AreaData)otherData).Value);
    }

    public INumericData Substract(INumericData otherData)
    {
        return CreateFrom(this, Value - ((AreaData)otherData).Value);
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

    private static Area ToBestUnitForDisplay(Area area)
    {
        if (area.Unit == AreaUnit.SquareMeter && area.SquareKilometers >= 1)
            return area.ToUnit(AreaUnit.SquareKilometer);

        if (area.Unit == AreaUnit.SquareKilometer && area.SquareKilometers < 1 && area.SquareKilometers > 0)
            return area.ToUnit(AreaUnit.SquareMeter);

        return area;
    }
}
