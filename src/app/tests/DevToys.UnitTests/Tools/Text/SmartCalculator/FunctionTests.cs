using System.Threading.Tasks;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.AbstractSyntaxTree;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Data;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Metadata;
using DevToys.Tools.Tools.Text.SmartCalculator.Core;
using DevToys.Tools.Tools.Text.SmartCalculator.Features.StatementParsersAndInterpreters.NumericalExpression;
using DevToys.UnitTests.Tools.Text.SmartCalculator;

namespace DevToys.UnitTests.Tools.Text.SmartCalculator;

public class FunctionTests : MefBaseTest
{
    private readonly ParserAndInterpreter _parserAndInterpreter;
    private readonly TextDocument _textDocument;

    public FunctionTests()
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
    [InlineData("20% of (60 + (25% of 30))", "13.5")]
    public async Task RecursiveFunction(string input, string output)
    {
        _textDocument.Text = input;
        IReadOnlyList<ParserAndInterpreterResultLine> lineResults = await _parserAndInterpreter.WaitAsync();
        var statement = (NumericalCalculusStatement)lineResults[lineResults.Count - 1].StatementsAndData[0].ParsedStatement;
        Assert.Equal("percentage.percentOf", ((FunctionExpression)statement.NumericalCalculusExpression).FunctionDefinition.FunctionFullName);
        Assert.Equal(output, lineResults[lineResults.Count - 1].SummarizedResultData.GetDataDisplayText());
    }

    [Theory]
    [InlineData("20% off 60 + (25% of 30)            // equivalent to (20% off 60) + (25% of 30)", "55.5")]
    [InlineData("20% off 60 + 25% of 30              // equivalent to (20% off 60) + (25% of 30)", "55.5")]
    [InlineData("(20% off 60) + 25% of 30            // equivalent to (20% off 60) + (25% of 30)", "55.5")]
    [InlineData("(20% off 60) + (25% of 30)          // equivalent to (20% off 60) + (25% of 30)", "55.5")]
    [InlineData("(20% off 60 + 25% of 30)            // equivalent to (20% off 60) + (25% of 30)", "55.5")]
    [InlineData("20% off 60 (25% of 30)              // equivalent to (20% off 60) * (25% of 30)", "360")]
    [InlineData("20% off 60 25% of 30                // equivalent to (20% off 60) + (25% of 30)", "55.5")]
    [InlineData("(20% off 60) 25% of 30              // equivalent to (20% off 60) * (25% of 30)", "360")]
    [InlineData("(20% off 60) (25% of 30)            // equivalent to (20% off 60) * (25% of 30)", "360")]
    [InlineData("(20% off 60 25% of 30)              // equivalent to (20% off 60) + (25% of 30)", "55.5")]
    [InlineData("20% off 25 + 50                     // equivalent to (20% off 25) + 50", "70")]
    [InlineData("20% off 25 dfghdfghf + dfghdfghf 50 // equivalent to (20% off 25) + 50", "70")]
    [InlineData("20% off 25 + (50)                   // equivalent to (20% off 25) + (50)", "70")]
    [InlineData("20% off 25 50                       // equivalent to (20% off 25) + 50", "70")]
    [InlineData("20% off 25 dfghdfghf 50             // equivalent to (20% off 25) + 50", "70")]
    [InlineData("20% off 25 (50)                     // equivalent to (20% off 25) * (50)", "1000")]
    [InlineData("20% off 25 (+50)                    // equivalent to (20% off 25) * (50)", "1000")]
    [InlineData("20% off 25 (+ 50)                   // equivalent to (20% off 25) * (50)", "1000")]
    [InlineData("20% off 25 50 50                    // equivalent to (20% off 25) + (50) + (50)", "120")]
    [InlineData("20% off 25 * 50                     // equivalent to (20% off 25) * (50)", "1000")]
    public async Task ConcatenationOfExpression(string input, string output)
    {
        _textDocument.Text = input;
        IReadOnlyList<ParserAndInterpreterResultLine> lineResults = await _parserAndInterpreter.WaitAsync();
        Assert.Equal(output, lineResults[lineResults.Count - 1].SummarizedResultData.GetDataDisplayText());
    }

    [Theory]
    [InlineData("20% of 60", "12")]
    [InlineData("20% of 60km", "12 km")]
    [InlineData("20% of (25 + 50)", "15")]
    [InlineData("tax = 40% \r\n tax of 75$", "30 Dollar")]
    [InlineData("20% of 20%", "4%")]
    [InlineData("20% of 1 CAD", "0.2 CAD")]
    public async Task Percentage_PercentOf(string input, string output)
    {
        _textDocument.Text = input;
        IReadOnlyList<ParserAndInterpreterResultLine> lineResults = await _parserAndInterpreter.WaitAsync();
        Assert.Single(lineResults[lineResults.Count - 1].StatementsAndData);
        var statement = (NumericalCalculusStatement)lineResults[lineResults.Count - 1].StatementsAndData[0].ParsedStatement;
        Assert.Equal("percentage.percentOf", ((FunctionExpression)statement.NumericalCalculusExpression).FunctionDefinition.FunctionFullName);
        Assert.Equal(output, lineResults[lineResults.Count - 1].SummarizedResultData.GetDataDisplayText());
    }

    [Theory]
    [InlineData("20% off 60", "48")]
    [InlineData("20% off 25", "20")]
    [InlineData("20% off 60km", "48 km")]
    [InlineData("tax = 40% \r\n tax off 75$", "45 Dollar")]
    [InlineData("20% off 20%", "16%")]
    [InlineData("20% off 2 CAD", "1.6 CAD")]
    public async Task Percentage_PercentOff(string input, string output)
    {
        _textDocument.Text = input;
        IReadOnlyList<ParserAndInterpreterResultLine> lineResults = await _parserAndInterpreter.WaitAsync();
        Assert.Single(lineResults[lineResults.Count - 1].StatementsAndData);
        var statement = (NumericalCalculusStatement)lineResults[lineResults.Count - 1].StatementsAndData[0].ParsedStatement;
        Assert.Equal("percentage.percentOff", ((FunctionExpression)statement.NumericalCalculusExpression).FunctionDefinition.FunctionFullName);
        Assert.Equal(output, lineResults[lineResults.Count - 1].SummarizedResultData.GetDataDisplayText());
    }

    [Theory]
    [InlineData("20% on 60", "72")]
    [InlineData("20% on 60km", "72 km")]
    [InlineData("tax = 40% \r\n tax on 75$", "105 Dollar")]
    [InlineData("20% on 20%", "24%")]
    [InlineData("20% on 2 CAD", "2.4 CAD")]
    public async Task Percentage_PercentOn(string input, string output)
    {
        _textDocument.Text = input;
        IReadOnlyList<ParserAndInterpreterResultLine> lineResults = await _parserAndInterpreter.WaitAsync();
        Assert.Single(lineResults[lineResults.Count - 1].StatementsAndData);
        var statement = (NumericalCalculusStatement)lineResults[lineResults.Count - 1].StatementsAndData[0].ParsedStatement;
        Assert.Equal("percentage.percentOn", ((FunctionExpression)statement.NumericalCalculusExpression).FunctionDefinition.FunctionFullName);
        Assert.Equal(output, lineResults[lineResults.Count - 1].SummarizedResultData.GetDataDisplayText());
    }

    [Theory]
    [InlineData("1km is what % of 10,000m", "10%")]
    [InlineData("1km is what percent of 10,000m", "10%")]
    [InlineData("1km is what percentage of 10,000m", "10%")]
    [InlineData("1km represents what % of 10,000m", "10%")]
    [InlineData("1km represents what percent of 10,000m", "10%")]
    [InlineData("1km represents what percentage of 10,000m", "10%")]
    [InlineData("1km off 10,000m is what %", "10%")]
    [InlineData("1km off 10,000m is what percent", "10%")]
    [InlineData("1km off 10,000m is what percentage", "10%")]
    [InlineData("1km as a % of 10,000m", "10%")]
    [InlineData("1km as % of 10,000m", "10%")]
    [InlineData("1km as a percent of 10,000m", "10%")]
    [InlineData("1km as percent of 10,000m", "10%")]
    [InlineData("1km as a percentage of 10,000m", "10%")]
    [InlineData("1km as percentage of 10,000m", "10%")]
    [InlineData("1,000 CAD is what percent of 10,000 CAD", "10%")]
    [InlineData("1,000 USD is what percent of 10,000 USD", "10%")]
    [InlineData("1,000 CAD is what percent of 10,000 USD", "7.6335877863%")]
    [InlineData("1,000 USD is what percent of 10,000 CAD", "13.1%")]
    public async Task Percentage_IsWhatPercentOf(string input, string output)
    {
        _textDocument.Text = input;
        IReadOnlyList<ParserAndInterpreterResultLine> lineResults = await _parserAndInterpreter.WaitAsync();
        Assert.Single(lineResults[lineResults.Count - 1].StatementsAndData);
        var statement = (NumericalCalculusStatement)lineResults[lineResults.Count - 1].StatementsAndData[0].ParsedStatement;
        Assert.Equal("percentage.isWhatPercentOf", ((FunctionExpression)statement.NumericalCalculusExpression).FunctionDefinition.FunctionFullName);
        Assert.Equal(output, lineResults[lineResults.Count - 1].SummarizedResultData.GetDataDisplayText());
    }

    [Theory]
    [InlineData("250km is what % on 62,500m", "300%")]
    [InlineData("1,250m is what percent on 1km", "25%")]
    [InlineData("10,000m is what percentage on 1km", "900%")]
    [InlineData("10,000m as a % on 1km", "900%")]
    [InlineData("10,000m as % on 1km", "900%")]
    [InlineData("10,000m as a percent on 1km", "900%")]
    [InlineData("10,000m as percent on 1km", "900%")]
    [InlineData("10,000m as a percentage on 1km", "900%")]
    [InlineData("10,000m as percentage on 1km", "900%")]
    [InlineData("10,000 CAD is what percent on 1,000 CAD", "900%")]
    [InlineData("10,000 USD is what percent on 1,000 USD", "900%")]
    [InlineData("10,000 CAD is what percent on 1,000 USD", "663.358778626%")]
    [InlineData("10,000 USD is what percent on 1,000 CAD", "1210%")]
    public async Task Percentage_IsWhatPercentOn(string input, string output)
    {
        _textDocument.Text = input;
        IReadOnlyList<ParserAndInterpreterResultLine> lineResults = await _parserAndInterpreter.WaitAsync();
        Assert.Single(lineResults[lineResults.Count - 1].StatementsAndData);
        var statement = (NumericalCalculusStatement)lineResults[lineResults.Count - 1].StatementsAndData[0].ParsedStatement;
        Assert.Equal("percentage.isWhatPercentOn", ((FunctionExpression)statement.NumericalCalculusExpression).FunctionDefinition.FunctionFullName);
        Assert.Equal(output, lineResults[lineResults.Count - 1].SummarizedResultData.GetDataDisplayText());
    }

    [Theory]
    [InlineData("62.5km is what % off 250,000m", "75%")]
    [InlineData("1km is what percent off 10,000m", "90%")]
    [InlineData("1km is what percentage off 10,000m", "90%")]
    [InlineData("1km as a % off 10,000m", "90%")]
    [InlineData("1km as % off 10,000m", "90%")]
    [InlineData("1km as a percent off 10,000m", "90%")]
    [InlineData("1km as percent off 10,000m", "90%")]
    [InlineData("1km as a percentage off 10,000m", "90%")]
    [InlineData("1km as percentage off 10,000m", "90%")]
    [InlineData("1,000 CAD is what percent off 10,000 CAD", "90%")]
    [InlineData("1,000 USD is what percent off 10,000 USD", "90%")]
    [InlineData("1,000 CAD is what percent off 10,000 USD", "92.3664122137%")]
    [InlineData("1,000 USD is what percent off 10,000 CAD", "86.9%")]
    public async Task Percentage_IsWhatPercentOff(string input, string output)
    {
        _textDocument.Text = input;
        IReadOnlyList<ParserAndInterpreterResultLine> lineResults = await _parserAndInterpreter.WaitAsync();
        Assert.Single(lineResults[lineResults.Count - 1].StatementsAndData);
        var statement = (NumericalCalculusStatement)lineResults[lineResults.Count - 1].StatementsAndData[0].ParsedStatement;
        Assert.Equal("percentage.isWhatPercentOff", ((FunctionExpression)statement.NumericalCalculusExpression).FunctionDefinition.FunctionFullName);
        Assert.Equal(output, lineResults[lineResults.Count - 1].SummarizedResultData.GetDataDisplayText());
    }

    [Theory]
    [InlineData("20 percent of what is 70", "350")]
    [InlineData("20 percent of what number is 70", "350")]
    [InlineData("70 is 20% of what", "350")]
    [InlineData("70 is 20% of what number", "350")]
    [InlineData("20 percent off what is 70", "350")]
    [InlineData("20 percent off what number is 70", "350")]
    [InlineData("70 is 20% off what", "350")]
    [InlineData("70 is 20% off what number", "350")]
    [InlineData("70 CAD is 20% off what", "350 CAD")]
    public async Task Percentage_IsPercentOfWhat(string input, string output)
    {
        _textDocument.Text = input;
        IReadOnlyList<ParserAndInterpreterResultLine> lineResults = await _parserAndInterpreter.WaitAsync();
        Assert.Single(lineResults[lineResults.Count - 1].StatementsAndData);
        var statement = (NumericalCalculusStatement)lineResults[lineResults.Count - 1].StatementsAndData[0].ParsedStatement;
        Assert.Equal("percentage.isPercentOfWhat", ((FunctionExpression)statement.NumericalCalculusExpression).FunctionDefinition.FunctionFullName);
        Assert.Equal(output, lineResults[lineResults.Count - 1].SummarizedResultData.GetDataDisplayText());
    }

    [Theory]
    [InlineData("62.5 is 25% on what", "50")]
    [InlineData("62.5 is 25% on what number", "50")]
    [InlineData("62.5 is what plus 25%", "50")]
    [InlineData("25% on what is 62.5", "50")]
    [InlineData("25% on what number is 62.5", "50")]
    [InlineData("25% on what number is 62.5 CAD", "50 CAD")]
    public async Task Percentage_IsPercentOnWhat(string input, string output)
    {
        _textDocument.Text = input;
        IReadOnlyList<ParserAndInterpreterResultLine> lineResults = await _parserAndInterpreter.WaitAsync();
        Assert.Single(lineResults[lineResults.Count - 1].StatementsAndData);
        var statement = (NumericalCalculusStatement)lineResults[lineResults.Count - 1].StatementsAndData[0].ParsedStatement;
        Assert.Equal("percentage.isPercentOnWhat", ((FunctionExpression)statement.NumericalCalculusExpression).FunctionDefinition.FunctionFullName);
        Assert.Equal(output, lineResults[lineResults.Count - 1].SummarizedResultData.GetDataDisplayText());
    }

    [Fact]
    public async Task General_Random()
    {
        _textDocument.Text = "random number between 0 and 1";
        IReadOnlyList<ParserAndInterpreterResultLine> lineResults = await _parserAndInterpreter.WaitAsync();
        Assert.Single(lineResults[lineResults.Count - 1].StatementsAndData);
        var statement = (NumericalCalculusStatement)lineResults[lineResults.Count - 1].StatementsAndData[0].ParsedStatement;
        Assert.Equal("general.random", ((FunctionExpression)statement.NumericalCalculusExpression).FunctionDefinition.FunctionFullName);
        var number = (INumericData)lineResults[lineResults.Count - 1].SummarizedResultData;
        Assert.InRange(number.NumericValueInStandardUnit, 0d, 1d);

        _textDocument.Text = "random between 10km and 100000m";
        lineResults = await _parserAndInterpreter.WaitAsync();
        Assert.Single(lineResults[lineResults.Count - 1].StatementsAndData);
        statement = (NumericalCalculusStatement)lineResults[lineResults.Count - 1].StatementsAndData[0].ParsedStatement;
        Assert.Equal("general.random", ((FunctionExpression)statement.NumericalCalculusExpression).FunctionDefinition.FunctionFullName);
        number = (INumericData)lineResults[lineResults.Count - 1].SummarizedResultData;
        Assert.InRange(number.NumericValueInStandardUnit, 10_000, 100_000);
    }

    [Theory]
    [InlineData("midpoint between 70 and 46", "58")]
    [InlineData("midpoint between 0 and 10", "5")]
    [InlineData("average between 70kg and 46kg", "58 kg")]
    [InlineData("average between 0 and 10", "5")]
    public async Task General_Midpoint(string input, string output)
    {
        _textDocument.Text = input;
        IReadOnlyList<ParserAndInterpreterResultLine> lineResults = await _parserAndInterpreter.WaitAsync();
        Assert.Single(lineResults[lineResults.Count - 1].StatementsAndData);
        var statement = (NumericalCalculusStatement)lineResults[lineResults.Count - 1].StatementsAndData[0].ParsedStatement;
        Assert.Equal("general.midpoint", ((FunctionExpression)statement.NumericalCalculusExpression).FunctionDefinition.FunctionFullName);
        Assert.Equal(output, lineResults[lineResults.Count - 1].SummarizedResultData.GetDataDisplayText());
    }
}
