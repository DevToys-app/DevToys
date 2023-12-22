using System.Runtime.CompilerServices;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Data;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Data.Definition;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Lexer;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Metadata;
using DevToys.Tools.Tools.Text.SmartCalculator.Features.Grammars;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.NumberWithUnit;
using UnitsNet;
using UnitsNet.Units;
using Constants = Microsoft.Recognizers.Text.NumberWithUnit.Constants;

namespace DevToys.Tools.Tools.Text.SmartCalculator.Features.Data;

[Export(typeof(IDataParser))]
[Culture(SupportedCultures.Any)]
public sealed class UnitDataParser : IDataParser
{
    private const string Value = "value";
    private const string Unit = "unit";
    private const string Subtype = "subtype";

    private readonly UnitMapProvider _unitMapProvider;

    [ImportingConstructor]
    public UnitDataParser(UnitMapProvider unitMapProvider)
    {
        _unitMapProvider = unitMapProvider;
    }

    public IReadOnlyList<IData>? Parse(string culture, TokenizedTextLine tokenizedTextLine, CancellationToken cancellationToken)
    {
        List<ModelResult> modelResults = NumberWithUnitRecognizer.RecognizeDimension(tokenizedTextLine.LineTextIncludingLineBreak, culture);
        cancellationToken.ThrowIfCancellationRequested();

        var data = new List<IData>();

        if (modelResults.Count > 0)
        {
            UnitMap unitMap = _unitMapProvider.LoadUnitMap(culture);

            for (int i = 0; i < modelResults.Count; i++)
            {
                ModelResult modelResult = modelResults[i];

                if (modelResult.Resolution is not null)
                {
                    string valueString = (string)modelResult.Resolution[Value];
                    string unit = (string)modelResult.Resolution[Unit];

                    switch (modelResult.Resolution[Subtype])
                    {
                        case Constants.LENGTH:
                            ParseLength(unitMap, data, tokenizedTextLine, modelResult, unit, valueString);
                            break;

                        case Constants.INFORMATION:
                            ParseInformation(unitMap, data, tokenizedTextLine, modelResult, unit, valueString);
                            break;

                        case Constants.AREA:
                            ParseArea(unitMap, data, tokenizedTextLine, modelResult, unit, valueString);
                            break;

                        case Constants.SPEED:
                            ParseSpeed(unitMap, data, tokenizedTextLine, modelResult, unit, valueString);
                            break;

                        case Constants.VOLUME:
                            ParseVolume(unitMap, data, tokenizedTextLine, modelResult, unit, valueString);
                            break;

                        case Constants.WEIGHT:
                            ParseMass(unitMap, data, tokenizedTextLine, modelResult, unit, valueString);
                            break;

                        case Constants.ANGLE:
                            ParseAngle(unitMap, data, tokenizedTextLine, modelResult, unit, valueString);
                            break;

                        default:
                            ThrowHelper.ThrowNotSupportedException();
                            return null;
                    }
                }
            }
        }

        return data;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ParseLength(UnitMap unitMap, List<IData> data, TokenizedTextLine tokenizedTextLine, ModelResult modelResult, string unit, string valueString)
    {
        if (unitMap.Length.TryGetValue(unit, out LengthUnit lengthUnit))
        {
            data.Add(
                new LengthData(
                    tokenizedTextLine.LineTextIncludingLineBreak,
                    modelResult.Start,
                    modelResult.End + 1,
                    new Length(double.Parse(valueString), lengthUnit)));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ParseInformation(UnitMap unitMap, List<IData> data, TokenizedTextLine tokenizedTextLine, ModelResult modelResult, string unit, string valueString)
    {
        if (unitMap.Information.TryGetValue(unit, out InformationUnit informationUnit))
        {
            data.Add(
                new InformationData(
                    tokenizedTextLine.LineTextIncludingLineBreak,
                    modelResult.Start,
                    modelResult.End + 1,
                    new Information(decimal.Parse(valueString), informationUnit)));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ParseArea(UnitMap unitMap, List<IData> data, TokenizedTextLine tokenizedTextLine, ModelResult modelResult, string unit, string valueString)
    {
        if (unitMap.Area.TryGetValue(unit, out AreaUnit areaUnit))
        {
            data.Add(
                new AreaData(
                    tokenizedTextLine.LineTextIncludingLineBreak,
                    modelResult.Start,
                    modelResult.End + 1,
                    new Area(double.Parse(valueString), areaUnit)));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ParseSpeed(UnitMap unitMap, List<IData> data, TokenizedTextLine tokenizedTextLine, ModelResult modelResult, string unit, string valueString)
    {
        if (unitMap.Speed.TryGetValue(unit, out SpeedUnit speedUnit))
        {
            data.Add(
                new SpeedData(
                    tokenizedTextLine.LineTextIncludingLineBreak,
                    modelResult.Start,
                    modelResult.End + 1,
                    new Speed(double.Parse(valueString), speedUnit)));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ParseVolume(UnitMap unitMap, List<IData> data, TokenizedTextLine tokenizedTextLine, ModelResult modelResult, string unit, string valueString)
    {
        if (unitMap.Volume.TryGetValue(unit, out VolumeUnit volumeUnit))
        {
            data.Add(
                new VolumeData(
                    tokenizedTextLine.LineTextIncludingLineBreak,
                    modelResult.Start,
                    modelResult.End + 1,
                    new Volume(double.Parse(valueString), volumeUnit)));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ParseMass(UnitMap unitMap, List<IData> data, TokenizedTextLine tokenizedTextLine, ModelResult modelResult, string unit, string valueString)
    {
        if (unitMap.Mass.TryGetValue(unit, out MassUnit massUnit))
        {
            data.Add(
                new MassData(
                    tokenizedTextLine.LineTextIncludingLineBreak,
                    modelResult.Start,
                    modelResult.End + 1,
                    new Mass(double.Parse(valueString), massUnit)));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ParseAngle(UnitMap unitMap, List<IData> data, TokenizedTextLine tokenizedTextLine, ModelResult modelResult, string unit, string valueString)
    {
        if (unitMap.Angle.TryGetValue(unit, out AngleUnit angleUnit))
        {
            data.Add(
                new AngleData(
                    tokenizedTextLine.LineTextIncludingLineBreak,
                    modelResult.Start,
                    modelResult.End + 1,
                    new Angle(double.Parse(valueString), angleUnit)));
        }
    }
}
