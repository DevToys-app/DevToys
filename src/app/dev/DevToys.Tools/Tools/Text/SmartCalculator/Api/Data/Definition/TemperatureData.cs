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
public sealed record TemperatureData : Data<Temperature>, INumericData
{
    public bool IsNegative => Value.Value < 0;

    public double NumericValueInCurrentUnit => (double)Value.Value;

    public double NumericValueInStandardUnit { get; }

    public override string GetDisplayText(string culture)
    {
        return Value.ToString("s4", new CultureInfo(culture));
    }

    public static TemperatureData CreateFrom(TemperatureData origin, Temperature value)
    {
        return new TemperatureData(
            origin.LineTextIncludingLineBreak,
            origin.StartInLine,
            origin.EndInLine,
            value);
    }

    public TemperatureData(string lineTextIncludingLineBreak, int startInLine, int endInLine, Temperature value)
        : base(
              lineTextIncludingLineBreak,
              startInLine,
              endInLine,
              value,
              PredefinedTokenAndDataTypeNames.Numeric,
              PredefinedTokenAndDataTypeNames.SubDataTypeNames.Temperature)
    {
        NumericValueInStandardUnit = value.ToUnit(Temperature.BaseUnit).Value;
    }

    public override IData MergeDataLocations(IData otherData)
    {
        return new TemperatureData(
            LineTextIncludingLineBreak,
            Math.Min(StartInLine, otherData.StartInLine),
            Math.Max(EndInLine, otherData.EndInLine),
            Value);
    }

    public INumericData CreateFromStandardUnit(double value)
    {
        return CreateFrom(this, new Temperature(value, Temperature.BaseUnit).ToUnit(Value.Unit));
    }

    public INumericData CreateFromCurrentUnit(double value)
    {
        return CreateFrom(this, new Temperature(value, Value.Unit));
    }

    public INumericData Add(INumericData otherData)
    {
        var otherTemperature = (TemperatureData)otherData;
        Temperature newTemperature = Value + TemperatureDelta.From(otherTemperature.Value.Value, (TemperatureDeltaUnit)otherTemperature.Value.Unit);
        return CreateFrom(this, newTemperature.ToUnit(otherTemperature.Value.Unit));
    }

    public INumericData Substract(INumericData otherData)
    {
        var otherTemperature = (TemperatureData)otherData;
        Temperature newTemperature = Value - TemperatureDelta.From(otherTemperature.Value.Value, (TemperatureDeltaUnit)otherTemperature.Value.Unit);
        return CreateFrom(this, newTemperature.ToUnit(otherTemperature.Value.Unit));
    }

    public INumericData Multiply(INumericData otherData)
    {
        if (otherData is DecimalData)
        {
            Temperature newTemperature = Value.Multiply(otherData.NumericValueInCurrentUnit, Value.Unit);
            return CreateFrom(this, newTemperature.ToUnit(Value.Unit));
        }

        ThrowUnsupportedArithmeticOperationException();
        return null!;
    }

    public INumericData Divide(INumericData otherData)
    {
        if (otherData is DecimalData)
        {
            Temperature newTemperature = Value.Divide(otherData.NumericValueInCurrentUnit, Value.Unit);
            return CreateFrom(this, newTemperature.ToUnit(Value.Unit));
        }

        ThrowUnsupportedArithmeticOperationException();
        return null!;
    }

    public override string ToString()
    {
        return base.ToString();
    }
}
