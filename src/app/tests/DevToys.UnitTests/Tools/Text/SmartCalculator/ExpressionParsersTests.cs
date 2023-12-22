using System.Threading.Tasks;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.AbstractSyntaxTree;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Data;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Metadata;
using DevToys.Tools.Tools.Text.SmartCalculator.Core;
using DevToys.Tools.Tools.Text.SmartCalculator.Features.StatementParsersAndInterpreters.NumericalExpression;
using DevToys.UnitTests.Tools.Text.SmartCalculator;

namespace DevToys.UnitTests.Tools.Text.SmartCalculator;

public sealed class ExpressionParsersTests : MefBaseTest
{
    private readonly ParserAndInterpreter _parserAndInterpreter;
    private readonly TextDocument _textDocument;

    public ExpressionParsersTests()
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

    [Fact]
    public async Task NumberDataExpression_Integer()
    {
        _textDocument.Text = " 132 ";
        IReadOnlyList<ParserAndInterpreterResultLine> lineResults = await _parserAndInterpreter.WaitAsync();
        Assert.Single(lineResults[0].StatementsAndData);
        var statement = (NumericalCalculusStatement)lineResults[0].StatementsAndData[0].ParsedStatement;
        IData data = ((DataExpression)statement.NumericalCalculusExpression).Data;
        Assert.Equal("[Numeric] (1, 4): '132'", data.ToString());
    }

    [Fact]
    public async Task NumberDataExpression_Group()
    {
        _textDocument.Text = " (132) ";
        IReadOnlyList<ParserAndInterpreterResultLine> lineResults = await _parserAndInterpreter.WaitAsync();
        Assert.Single(lineResults[0].StatementsAndData);
        var statement = (NumericalCalculusStatement)lineResults[0].StatementsAndData[0].ParsedStatement;
        IData data = ((DataExpression)((GroupExpression)statement.NumericalCalculusExpression).InnerExpression).Data;
        Assert.Equal("[Numeric] (2, 5): '132'", data.ToString());

        _textDocument.Text = " (  132  ) ";
        lineResults = await _parserAndInterpreter.WaitAsync();
        Assert.Single(lineResults[0].StatementsAndData);
        statement = (NumericalCalculusStatement)lineResults[0].StatementsAndData[0].ParsedStatement;
        data = ((DataExpression)((GroupExpression)statement.NumericalCalculusExpression).InnerExpression).Data;
        Assert.Equal("[Numeric] (4, 7): '132'", data.ToString());

        _textDocument.Text = " (  132 horse ) ";
        lineResults = await _parserAndInterpreter.WaitAsync();
        Assert.Single(lineResults[0].StatementsAndData);
        statement = (NumericalCalculusStatement)lineResults[0].StatementsAndData[0].ParsedStatement;
        data = ((DataExpression)((GroupExpression)statement.NumericalCalculusExpression).InnerExpression).Data;
        Assert.Equal("[Numeric] (4, 7): '132'", data.ToString());

        _textDocument.Text = " ( pigeon 132 horse ) ";
        lineResults = await _parserAndInterpreter.WaitAsync();
        Assert.Single(lineResults[0].StatementsAndData);
        statement = (NumericalCalculusStatement)lineResults[0].StatementsAndData[0].ParsedStatement;
        data = ((DataExpression)((GroupExpression)statement.NumericalCalculusExpression).InnerExpression).Data;
        Assert.Equal("[Numeric] (10, 13): '132'", data.ToString());

        _textDocument.Text = " ( ) ";
        lineResults = await _parserAndInterpreter.WaitAsync();
        Assert.Empty(lineResults[0].StatementsAndData);

        _textDocument.Text = "1()2";
        lineResults = await _parserAndInterpreter.WaitAsync();
        Assert.Equal(2, lineResults[0].StatementsAndData.Count);
    }

    [Fact]
    public async Task NumberDataExpression_GroupNested()
    {
        _textDocument.Text = " ((132)) ";
        IReadOnlyList<ParserAndInterpreterResultLine> lineResults = await _parserAndInterpreter.WaitAsync();
        Assert.Single(lineResults[0].StatementsAndData);
        var statement = (NumericalCalculusStatement)lineResults[0].StatementsAndData[0].ParsedStatement;
        if (statement.NumericalCalculusExpression is not GroupExpression groupExpression)
        {
            Assert.Fail("group expected");
            return;
        }

        if (groupExpression.InnerExpression is not GroupExpression groupExpression2)
        {
            Assert.Fail("group expected");
            return;
        }

        if (groupExpression2.InnerExpression is not DataExpression data)
        {
            Assert.Fail("data expected");
            return;
        }

        Assert.Equal("[Numeric] (3, 6): '132'", data.ToString());
    }

    [Fact]
    public async Task NumberDataExpression_MultiplicationAndDivision()
    {
        _textDocument.Text = " 123 horses * 2 trucks x 5 people divided by a farm with 8 fields ";
        IReadOnlyList<ParserAndInterpreterResultLine> lineResults = await _parserAndInterpreter.WaitAsync();
        Assert.Single(lineResults[0].StatementsAndData);
        var statement = (NumericalCalculusStatement)lineResults[0].StatementsAndData[0].ParsedStatement;
        var expression = (BinaryOperatorExpression)statement.NumericalCalculusExpression;
        Assert.Equal("((([Numeric] (1, 4): '123' * [Numeric] (14, 15): '2') * [Numeric] (25, 26): '5') / [Numeric] (57, 58): '8')", expression.ToString());
    }

    [Fact]
    public async Task NumberDataExpression_AdditionAndSubstraction()
    {
        _textDocument.Text = " I got 123 dogs plus 1 cat and two goldfish, minus 1 death ";
        IReadOnlyList<ParserAndInterpreterResultLine> lineResults = await _parserAndInterpreter.WaitAsync();
        Assert.Equal(2, lineResults[0].StatementsAndData.Count);
        var statement = (NumericalCalculusStatement)lineResults[0].StatementsAndData[0].ParsedStatement;
        var expression1 = (BinaryOperatorExpression)statement.NumericalCalculusExpression;
        statement = (NumericalCalculusStatement)lineResults[0].StatementsAndData[1].ParsedStatement;
        var expression2 = (DataExpression)statement.NumericalCalculusExpression;
        Assert.Equal("(([Numeric] (7, 10): '123' + [Numeric] (21, 22): '1') + [Numeric] (31, 34): 'two')", expression1.ToString());
        Assert.Equal("[Numeric] (45, 52): 'minus 1'", expression2.ToString());
    }

    [Fact]
    public async Task NumberDataExpression_ComplexScenario()
    {
        _textDocument.Text = "(123+(1 +2)) * -3";
        IReadOnlyList<ParserAndInterpreterResultLine> lineResults = await _parserAndInterpreter.WaitAsync();
        Assert.Single(lineResults[0].StatementsAndData);
        var statement = (NumericalCalculusStatement)lineResults[0].StatementsAndData[0].ParsedStatement;
        var expression = (BinaryOperatorExpression)statement.NumericalCalculusExpression;
        Assert.Equal("((([Numeric] (1, 4): '123' + (([Numeric] (6, 7): '1' + [Numeric] (9, 10): '2')))) * [Numeric] (15, 17): '-3')", expression.ToString());
    }

    [Fact]
    public async Task NumberDataExpression_ImplicitOperator()
    {
        _textDocument.Text = "1+(2)(3)";
        IReadOnlyList<ParserAndInterpreterResultLine> lineResults = await _parserAndInterpreter.WaitAsync();
        Assert.Single(lineResults[0].StatementsAndData);
        var statement = (NumericalCalculusStatement)lineResults[0].StatementsAndData[0].ParsedStatement;
        var expression = (BinaryOperatorExpression)statement.NumericalCalculusExpression;
        Assert.Equal("([Numeric] (0, 1): '1' + (([Numeric] (3, 4): '2') * ([Numeric] (6, 7): '3')))", expression.ToString());

        /*
         How do we solve (12)3+(1 +2)(3(2))(1 +2)-3 step by step?
            Multiple: 12 * 3 = 36
            Add: 1 + 2 = 3
            Multiple: 3 * 2 = 6
            Multiple: the result of step No. 2 * the result of step No. 3 = 3 * 6 = 18
            Add: 1 + 2 = 3
            Multiple: the result of step No. 4 * the result of step No. 5 = 18 * 3 = 54
            Add: the result of step No. 1 + the result of step No. 6 = 36 + 54 = 90
            Subtract: the result of step No. 7 - 3 = 90 - 3 = 87
         */
        _textDocument.Text = "(12)3+(1 +2)(3(2))(1 +2)-3";
        lineResults = await _parserAndInterpreter.WaitAsync();
        Assert.Single(lineResults[0].StatementsAndData);
        statement = (NumericalCalculusStatement)lineResults[0].StatementsAndData[0].ParsedStatement;
        expression = (BinaryOperatorExpression)statement.NumericalCalculusExpression;
        Assert.Equal("((([Numeric] (1, 3): '12') * [Numeric] (4, 5): '3') + ((((([Numeric] (7, 8): '1' + [Numeric] (10, 11): '2')) * (([Numeric] (13, 14): '3' * ([Numeric] (15, 16): '2')))) * (([Numeric] (19, 20): '1' + [Numeric] (22, 23): '2'))) + [Numeric] (24, 26): '-3'))", expression.ToString());
    }

    [Theory]
    [InlineData("2m 2km", "2.002 km")]
    [InlineData("2m (2km)", "4,000 m²")]
    [InlineData("25 50", "75")]
    [InlineData("25 (50)", "1250")]
    [InlineData("25 50 50", "125")]
    [InlineData("25 (50) (50)", "62500")]
    [InlineData("20% off 25 50", "70")]
    [InlineData("20% off 25 (50)", "1000")]
    [InlineData("-25 (50)", "-1250")]
    [InlineData("(25) 50", "1250")]
    [InlineData("(25) -50", "-25")]
    [InlineData("-25 -50", "-75")]
    [InlineData("(-25) (-50)", "1250")]
    [InlineData("-25 (-50)", "1250")]
    public async Task NumberDataExpression_ImplicitOperator2(string input, string output)
    {
        _textDocument.Text = input;
        IReadOnlyList<ParserAndInterpreterResultLine> lineResults = await _parserAndInterpreter.WaitAsync();
        Assert.Single(lineResults);
        Assert.Single(lineResults[0].StatementsAndData);
        Assert.Equal(output, lineResults[0].SummarizedResultData.GetDataDisplayText());
    }
}
