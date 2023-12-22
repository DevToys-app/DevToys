using System.Threading.Tasks;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Data;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Data.Definition;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Lexer;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Metadata;
using DevToys.Tools.Tools.Text.SmartCalculator.Core;
using UnitsNet.Units;

namespace DevToys.UnitTests.Tools.Text.SmartCalculator;

public sealed class DataParserTests : MefBaseTest
{
    private readonly ParserAndInterpreter _parserAndInterpreter;
    private readonly TextDocument _textDocument;

    public DataParserTests()
    {
        ParserAndInterpreterFactory parserAndInterpreterFactory = ExportProvider.Import<ParserAndInterpreterFactory>();
        _textDocument = new TextDocument();
        _parserAndInterpreter = parserAndInterpreterFactory.CreateInstance(SupportedCultures.English, _textDocument);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _parserAndInterpreter.Dispose();
    }

    [Theory]
    [InlineData("thirty five thousand", 35000)]
    [InlineData("forty three thousand", 43000)]
    [InlineData("nine hundred and seventy four thousand", 974000)]
    [InlineData("I have nine hundred and seventy four thousand items", 974000)]
    public async Task IntegerParsingAsync(string input, int output)
    {
        _textDocument.Text = input;
        IReadOnlyList<ParserAndInterpreterResultLine> lineResults = await _parserAndInterpreter.WaitAsync();
        Assert.Single(lineResults);
        Assert.Single(lineResults[0].StatementsAndData);
        IData data = lineResults[0].SummarizedResultData;
        Assert.True(data.IsOfType(PredefinedTokenAndDataTypeNames.Numeric));
        Assert.Equal(PredefinedTokenAndDataTypeNames.SubDataTypeNames.Decimal, data.Subtype);
        Assert.Equal(output, ((DecimalData)data).Value);
    }

    [Theory]
    [InlineData("thirty five thousand point one two three", 35000.123)]
    [InlineData("forty three thousand point fifty seven", 43000.57)]
    [InlineData("1.1^+23", 8.95430243255239)]
    public async Task DecimalParsingAsync(string input, double output)
    {
        _textDocument.Text = input;
        IReadOnlyList<ParserAndInterpreterResultLine> lineResults = await _parserAndInterpreter.WaitAsync();
        Assert.Single(lineResults);
        Assert.Single(lineResults[0].StatementsAndData);
        IData data = lineResults[0].SummarizedResultData;
        Assert.True(data.IsOfType(PredefinedTokenAndDataTypeNames.Numeric));
        Assert.Equal(PredefinedTokenAndDataTypeNames.SubDataTypeNames.Decimal, data.Subtype);
        Assert.Equal(output, ((DecimalData)data).Value);
    }

    [Theory]
    [InlineData("a fifth", 0.2)]
    [InlineData("a hundred thousand trillionths", 1E-07)]
    public async Task FractionParsingAsync(string input, double output)
    {
        _textDocument.Text = input;
        IReadOnlyList<ParserAndInterpreterResultLine> lineResults = await _parserAndInterpreter.WaitAsync();
        Assert.Single(lineResults);
        Assert.Single(lineResults[0].StatementsAndData);
        IData data = lineResults[0].SummarizedResultData;
        Assert.True(data.IsOfType(PredefinedTokenAndDataTypeNames.Numeric));
        Assert.Equal(PredefinedTokenAndDataTypeNames.SubDataTypeNames.Fraction, data.Subtype);
        Assert.Equal(output, ((FractionData)data).Value);
    }

    [Theory]
    [InlineData("one hundred percents", 1f)]
    [InlineData("per cent of twenty-two", 0.22)]
    public async Task PercentageParsingAsync(string input, double output)
    {
        _textDocument.Text = input;
        IReadOnlyList<ParserAndInterpreterResultLine> lineResults = await _parserAndInterpreter.WaitAsync();
        Assert.Single(lineResults);
        Assert.Single(lineResults[0].StatementsAndData);
        IData data = lineResults[0].SummarizedResultData;
        Assert.True(data.IsOfType(PredefinedTokenAndDataTypeNames.Numeric));
        Assert.Equal(PredefinedTokenAndDataTypeNames.SubDataTypeNames.Percentage, data.Subtype);
        Assert.Equal(output, ((PercentageData)data).Value);
    }

    [Theory]
    [InlineData("one hundred and fifty thousand dollars", 150000, "Dollar", "", false)]
    [InlineData(" $ -75.3 million USD", -75300000, "United States dollar", "USD", true)]
    public async Task CurrencyParsingAsync(string input, double output, string unit, string iso, bool isNegative)
    {
        _textDocument.Text = input;
        IReadOnlyList<ParserAndInterpreterResultLine> lineResults = await _parserAndInterpreter.WaitAsync();
        Assert.Single(lineResults);
        Assert.Single(lineResults[0].StatementsAndData);
        IData data = lineResults[0].SummarizedResultData;
        Assert.True(data.IsOfType(PredefinedTokenAndDataTypeNames.Numeric));
        var currency = (CurrencyData)data;
        Assert.Equal(isNegative, currency.IsNegative);
        Assert.Equal(output, currency.Value.Value);
        Assert.Equal(unit, currency.Value.Currency);
        Assert.Equal(iso, currency.Value.IsoCurrency);
    }

    [Theory]
    [InlineData("three kilometers", 3, "Length", LengthUnit.Kilometer, false)]
    public async Task LengthParsingAsync(string input, double value, string subType, LengthUnit unit, bool isNegative)
    {
        _textDocument.Text = input;
        IReadOnlyList<ParserAndInterpreterResultLine> lineResults = await _parserAndInterpreter.WaitAsync();
        Assert.Single(lineResults);
        Assert.Single(lineResults[0].StatementsAndData);
        IData data = lineResults[0].SummarizedResultData;
        Assert.True(data.IsOfType(PredefinedTokenAndDataTypeNames.Numeric));
        var length = (LengthData)data;
        Assert.Equal(subType, data.Subtype);
        Assert.Equal(isNegative, length.IsNegative);
        Assert.Equal(value, length.Value.Value);
        Assert.Equal(unit, length.Value.Unit);
    }

    [Theory]
    [InlineData("three megabyte", 3, "Information", InformationUnit.Megabyte, false)]
    public async Task InformationParsingAsync(string input, decimal value, string subType, InformationUnit unit, bool isNegative)
    {
        _textDocument.Text = input;
        IReadOnlyList<ParserAndInterpreterResultLine> lineResults = await _parserAndInterpreter.WaitAsync();
        Assert.Single(lineResults);
        Assert.Single(lineResults[0].StatementsAndData);
        IData data = lineResults[0].SummarizedResultData;
        Assert.True(data.IsOfType(PredefinedTokenAndDataTypeNames.Numeric));
        var informationData = (InformationData)data;
        Assert.Equal(subType, data.Subtype);
        Assert.Equal(isNegative, informationData.IsNegative);
        Assert.Equal(value, informationData.Value.Value);
        Assert.Equal(unit, informationData.Value.Unit);
    }

    [Theory]
    [InlineData("three sq km", 3, "Area", AreaUnit.SquareKilometer, false)]
    public async Task AreaParsingAsync(string input, double value, string subType, AreaUnit unit, bool isNegative)
    {
        _textDocument.Text = input;
        IReadOnlyList<ParserAndInterpreterResultLine> lineResults = await _parserAndInterpreter.WaitAsync();
        Assert.Single(lineResults);
        Assert.Single(lineResults[0].StatementsAndData);
        IData data = lineResults[0].SummarizedResultData;
        Assert.True(data.IsOfType(PredefinedTokenAndDataTypeNames.Numeric));
        var areaData = (AreaData)data;
        Assert.Equal(subType, data.Subtype);
        Assert.Equal(isNegative, areaData.IsNegative);
        Assert.Equal(value, areaData.Value.Value);
        Assert.Equal(unit, areaData.Value.Unit);
    }

    [Theory]
    [InlineData("three mph", 3, "Speed", SpeedUnit.MilePerHour, false)]
    public async Task SpeedParsingAsync(string input, double value, string subType, SpeedUnit unit, bool isNegative)
    {
        _textDocument.Text = input;
        IReadOnlyList<ParserAndInterpreterResultLine> lineResults = await _parserAndInterpreter.WaitAsync();
        Assert.Single(lineResults);
        Assert.Single(lineResults[0].StatementsAndData);
        IData data = lineResults[0].SummarizedResultData;
        Assert.True(data.IsOfType(PredefinedTokenAndDataTypeNames.Numeric));
        var speedData = (SpeedData)data;
        Assert.Equal(subType, data.Subtype);
        Assert.Equal(isNegative, speedData.IsNegative);
        Assert.Equal(value, speedData.Value.Value);
        Assert.Equal(unit, speedData.Value.Unit);
    }

    [Theory]
    [InlineData("three liter", 3, "Volume", VolumeUnit.Liter, false)]
    public async Task VolumeParsingAsync(string input, double value, string subType, VolumeUnit unit, bool isNegative)
    {
        _textDocument.Text = input;
        IReadOnlyList<ParserAndInterpreterResultLine> lineResults = await _parserAndInterpreter.WaitAsync();
        Assert.Single(lineResults);
        Assert.Single(lineResults[0].StatementsAndData);
        IData data = lineResults[0].SummarizedResultData;
        Assert.True(data.IsOfType(PredefinedTokenAndDataTypeNames.Numeric));
        var volumeData = (VolumeData)data;
        Assert.Equal(subType, data.Subtype);
        Assert.Equal(isNegative, volumeData.IsNegative);
        Assert.Equal(value, volumeData.Value.Value);
        Assert.Equal(unit, volumeData.Value.Unit);
    }

    [Theory]
    [InlineData("three kilograms", 3, "Mass", MassUnit.Kilogram, false)]
    public async Task MassParsingAsync(string input, double value, string subType, MassUnit unit, bool isNegative)
    {
        _textDocument.Text = input;
        IReadOnlyList<ParserAndInterpreterResultLine> lineResults = await _parserAndInterpreter.WaitAsync();
        Assert.Single(lineResults);
        Assert.Single(lineResults[0].StatementsAndData);
        IData data = lineResults[0].SummarizedResultData;
        Assert.True(data.IsOfType(PredefinedTokenAndDataTypeNames.Numeric));
        var massData = (MassData)data;
        Assert.Equal(subType, data.Subtype);
        Assert.Equal(isNegative, massData.IsNegative);
        Assert.Equal(value, massData.Value.Value);
        Assert.Equal(unit, massData.Value.Unit);
    }

    [Theory]
    [InlineData("three rad", 3, "Angle", AngleUnit.Radian, false)]
    public async Task AngleParsingAsync(string input, double value, string subType, AngleUnit unit, bool isNegative)
    {
        _textDocument.Text = input;
        IReadOnlyList<ParserAndInterpreterResultLine> lineResults = await _parserAndInterpreter.WaitAsync();
        Assert.Single(lineResults);
        Assert.Single(lineResults[0].StatementsAndData);
        IData data = lineResults[0].SummarizedResultData;
        Assert.True(data.IsOfType(PredefinedTokenAndDataTypeNames.Numeric));
        var angleData = (AngleData)data;
        Assert.Equal(subType, data.Subtype);
        Assert.Equal(isNegative, angleData.IsNegative);
        Assert.Equal(value, angleData.Value.Value);
        Assert.Equal(unit, angleData.Value.Unit);
    }

    [Theory]
    [InlineData("three °f", 3, "Temperature", TemperatureUnit.DegreeFahrenheit, false)]
    public async Task TemperatureParsingAsync(string input, double value, string subType, TemperatureUnit unit, bool isNegative)
    {
        _textDocument.Text = input;
        IReadOnlyList<ParserAndInterpreterResultLine> lineResults = await _parserAndInterpreter.WaitAsync();
        Assert.Single(lineResults);
        Assert.Single(lineResults[0].StatementsAndData);
        IData data = lineResults[0].SummarizedResultData;
        Assert.True(data.IsOfType(PredefinedTokenAndDataTypeNames.Numeric));
        var temperatureData = (TemperatureData)data;
        Assert.Equal(subType, data.Subtype);
        Assert.Equal(isNegative, temperatureData.IsNegative);
        Assert.Equal(value, temperatureData.Value.Value);
        Assert.Equal(unit, temperatureData.Value.Unit);
    }

    [Theory]
    [InlineData("1h", 3600, "Duration", false)]
    [InlineData("8pm to 5pm", 75600, "Duration", false)]
    [InlineData("2018 to 2022", 126230400, "Duration", false)]
    public async Task DurationParsingAsync(string input, double value, string subType, bool isNegative)
    {
        _textDocument.Text = input;
        IReadOnlyList<ParserAndInterpreterResultLine> lineResults = await _parserAndInterpreter.WaitAsync();
        Assert.Single(lineResults);
        Assert.Single(lineResults[0].StatementsAndData);
        IData data = lineResults[0].SummarizedResultData;
        Assert.True(data.IsOfType(PredefinedTokenAndDataTypeNames.Numeric));
        var durationData = (DurationData)data;
        Assert.Equal(subType, data.Subtype);
        Assert.Equal(isNegative, durationData.IsNegative);
        Assert.Equal(value, durationData.Value.TotalSeconds);
    }

    [Theory]
    [InlineData("June 23 2022 at 4pm", 637915968000000000, "DateTime", false)]
    public async Task DateTimeParsingAsync(string input, long value, string subType, bool isNegative)
    {
        _textDocument.Text = input;
        IReadOnlyList<ParserAndInterpreterResultLine> lineResults = await _parserAndInterpreter.WaitAsync();
        Assert.Single(lineResults);
        Assert.Single(lineResults[0].StatementsAndData);
        IData data = lineResults[0].SummarizedResultData;
        Assert.True(data.IsOfType(PredefinedTokenAndDataTypeNames.Numeric));
        var dateTimeData = (DateTimeData)data;
        Assert.Equal(subType, data.Subtype);
        Assert.Equal(isNegative, dateTimeData.IsNegative);
        Assert.Equal(value, dateTimeData.Value.Ticks);
    }

    [Theory]
    [InlineData("1km + 1m", "1.001 km")]
    [InlineData("1h + 1m", "30.01:00:00")]
    public async Task ConflictingDataParsingAsync(string input, string output)
    {
        _textDocument.Text = input;
        IReadOnlyList<ParserAndInterpreterResultLine> lineResults = await _parserAndInterpreter.WaitAsync();
        Assert.Single(lineResults);
        Assert.Single(lineResults[0].StatementsAndData);
        IData data = lineResults[0].SummarizedResultData;
        Assert.Equal(output, data.GetDataDisplayText());
    }
}
