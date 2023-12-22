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
public sealed record InformationData : Data<Information>, INumericData
{
    public bool IsNegative => Value.Value < 0;

    public double NumericValueInCurrentUnit => (double)Value.Value;

    public double NumericValueInStandardUnit { get; }

    public override string GetDisplayText(string culture)
    {
        return ToBestUnitForDisplay(Value).ToString("s4", new CultureInfo(culture));
    }

    public static InformationData CreateFrom(InformationData origin, Information value)
    {
        return new InformationData(
            origin.LineTextIncludingLineBreak,
            origin.StartInLine,
            origin.EndInLine,
            value);
    }

    public InformationData(string lineTextIncludingLineBreak, int startInLine, int endInLine, Information value)
        : base(
              lineTextIncludingLineBreak,
              startInLine,
              endInLine,
              value,
              PredefinedTokenAndDataTypeNames.Numeric,
              PredefinedTokenAndDataTypeNames.SubDataTypeNames.Information)
    {
        NumericValueInStandardUnit = (double)value.ToUnit(Information.BaseUnit).Value;
    }

    public override IData MergeDataLocations(IData otherData)
    {
        return new InformationData(
            LineTextIncludingLineBreak,
            Math.Min(StartInLine, otherData.StartInLine),
            Math.Max(EndInLine, otherData.EndInLine),
            Value);
    }

    public INumericData CreateFromStandardUnit(double value)
    {
        return CreateFrom(this, new Information((decimal)value, Information.BaseUnit).ToUnit(Value.Unit));
    }

    public INumericData CreateFromCurrentUnit(double value)
    {
        return CreateFrom(this, new Information((decimal)value, Value.Unit));
    }

    public INumericData Add(INumericData otherData)
    {
        return CreateFrom(this, Value + ((InformationData)otherData).Value);
    }

    public INumericData Substract(INumericData otherData)
    {
        return CreateFrom(this, Value - ((InformationData)otherData).Value);
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

    private static Information ToBestUnitForDisplay(Information info)
    {
        if (info.Unit == InformationUnit.Terabyte && info.Bytes < 1_000_000_000_000)
            return info.ToUnit(InformationUnit.Gigabyte);

        if (info.Unit == InformationUnit.Gigabyte && info.Bytes < 1_000_000_000)
            return info.ToUnit(InformationUnit.Megabyte);

        if (info.Unit == InformationUnit.Megabyte && info.Bytes < 1_000_000)
            return info.ToUnit(InformationUnit.Kilobyte);

        if (info.Unit == InformationUnit.Kilobyte && info.Bytes < 1_000)
            return info.ToUnit(InformationUnit.Byte);

        return info;
    }
}
