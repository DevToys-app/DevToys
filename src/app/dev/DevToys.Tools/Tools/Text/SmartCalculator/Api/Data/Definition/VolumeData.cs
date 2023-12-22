using System.Globalization;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Lexer;
using UnitsNet;

namespace DevToys.Tools.Tools.Text.SmartCalculator.Api.Data.Definition;

[DebuggerDisplay(
    $"Value = {{{nameof(NumericValueInStandardUnit)}}}, " +
    $"Type = {{{nameof(Type)}}}, " +
    $"Text = {{{nameof(GetText)}()}}, " +
    $"StartInLine = {{{nameof(StartInLine)}}}")]
public sealed record VolumeData : Data<Volume>, INumericData
{
    public bool IsNegative => Value.Value < 0;

    public double NumericValueInCurrentUnit => (double)Value.Value;

    public double NumericValueInStandardUnit { get; }

    public override string GetDisplayText(string culture)
    {
        return Value.ToString("s4", new CultureInfo(culture));
    }

    public static VolumeData CreateFrom(VolumeData origin, Volume value)
    {
        return new VolumeData(
            origin.LineTextIncludingLineBreak,
            origin.StartInLine,
            origin.EndInLine,
            value);
    }

    public VolumeData(string lineTextIncludingLineBreak, int startInLine, int endInLine, Volume value)
        : base(
              lineTextIncludingLineBreak,
              startInLine,
              endInLine,
              value,
              PredefinedTokenAndDataTypeNames.Numeric,
              PredefinedTokenAndDataTypeNames.SubDataTypeNames.Volume)
    {
        NumericValueInStandardUnit = value.ToUnit(Volume.BaseUnit).Value;
    }

    public override IData MergeDataLocations(IData otherData)
    {
        return new VolumeData(
            LineTextIncludingLineBreak,
            Math.Min(StartInLine, otherData.StartInLine),
            Math.Max(EndInLine, otherData.EndInLine),
            Value);
    }

    public INumericData CreateFromStandardUnit(double value)
    {
        return CreateFrom(this, new Volume(value, Volume.BaseUnit).ToUnit(Value.Unit));
    }

    public INumericData CreateFromCurrentUnit(double value)
    {
        return CreateFrom(this, new Volume(value, Value.Unit));
    }

    public INumericData Add(INumericData otherData)
    {
        return CreateFrom(this, Value + ((VolumeData)otherData).Value);
    }

    public INumericData Substract(INumericData otherData)
    {
        return CreateFrom(this, Value - ((VolumeData)otherData).Value);
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
