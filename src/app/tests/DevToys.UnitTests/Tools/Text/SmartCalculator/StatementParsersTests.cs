using System.Threading.Tasks;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.AbstractSyntaxTree;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Metadata;
using DevToys.Tools.Tools.Text.SmartCalculator.Core;
using DevToys.Tools.Tools.Text.SmartCalculator.Features.StatementParsersAndInterpreters.Comment;
using DevToys.Tools.Tools.Text.SmartCalculator.Features.StatementParsersAndInterpreters.Condition;
using DevToys.Tools.Tools.Text.SmartCalculator.Features.StatementParsersAndInterpreters.Header;
using DevToys.Tools.Tools.Text.SmartCalculator.Features.StatementParsersAndInterpreters.NumericalExpression;

namespace DevToys.UnitTests.Tools.Text.SmartCalculator;

public sealed class StatementParsersTests : MefBaseTest
{
    private readonly ParserAndInterpreter _parserAndInterpreter;
    private readonly TextDocument _textDocument;

    public StatementParsersTests()
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
    public async Task CommentStatement()
    {
        _textDocument.Text = " 132 //comment";
        IReadOnlyList<ParserAndInterpreterResultLine> lineResults = await _parserAndInterpreter.WaitAsync();
        Assert.Equal(2, lineResults[0].StatementsAndData.Count);
        Statement statement = lineResults[0].StatementsAndData[1].ParsedStatement;
        Assert.IsType<CommentStatement>(statement);
        Assert.Equal("[comment_operators] (5, 7): '//'", statement.FirstToken.ToString());
        Assert.Equal("[Word] (7, 14): 'comment'", statement.LastToken.ToString());

        _textDocument.Text = " 132 // comment";
        lineResults = await _parserAndInterpreter.WaitAsync();
        Assert.Equal(2, lineResults[0].StatementsAndData.Count);
        statement = lineResults[0].StatementsAndData[1].ParsedStatement;
        Assert.IsType<CommentStatement>(statement);
        Assert.Equal("[comment_operators] (5, 7): '//'", statement.FirstToken.ToString());
        Assert.Equal("[Word] (8, 15): 'comment'", statement.LastToken.ToString());

        _textDocument.Text = " 132 // comment // comment 2";
        lineResults = await _parserAndInterpreter.WaitAsync();
        Assert.Equal(2, lineResults[0].StatementsAndData.Count);
        statement = lineResults[0].StatementsAndData[1].ParsedStatement;
        Assert.IsType<CommentStatement>(statement);
        Assert.Equal("[comment_operators] (5, 7): '//'", statement.FirstToken.ToString());
        Assert.Equal("[Numeric] (27, 28): '2'", statement.LastToken.ToString());

        _textDocument.Text = " 132 / / comment";
        lineResults = await _parserAndInterpreter.WaitAsync();
        Assert.Single(lineResults[0].StatementsAndData);
        statement = lineResults[0].StatementsAndData[0].ParsedStatement;
        Assert.IsType<NumericalCalculusStatement>(statement);
    }

    [Fact]
    public async Task HeaderStatement()
    {
        _textDocument.Text = "#Header";
        IReadOnlyList<ParserAndInterpreterResultLine> lineResults = await _parserAndInterpreter.WaitAsync();
        Assert.Single(lineResults[0].StatementsAndData);
        Statement statement = lineResults[0].StatementsAndData[0].ParsedStatement;
        Assert.IsType<HeaderStatement>(statement);
        Assert.Equal("[header_operators] (0, 1): '#'", statement.FirstToken.ToString());
        Assert.Equal("[Word] (1, 7): 'Header'", statement.LastToken.ToString());

        _textDocument.Text = " # Header";
        lineResults = await _parserAndInterpreter.WaitAsync();
        Assert.Single(lineResults[0].StatementsAndData);
        statement = lineResults[0].StatementsAndData[0].ParsedStatement;
        Assert.IsType<HeaderStatement>(statement);
        Assert.Equal("[header_operators] (1, 2): '#'", statement.FirstToken.ToString());
        Assert.Equal("[Word] (3, 9): 'Header'", statement.LastToken.ToString());

        _textDocument.Text = " ### Header";
        lineResults = await _parserAndInterpreter.WaitAsync();
        Assert.Single(lineResults[0].StatementsAndData);
        statement = lineResults[0].StatementsAndData[0].ParsedStatement;
        Assert.IsType<HeaderStatement>(statement);
        Assert.Equal("[header_operators] (1, 4): '###'", statement.FirstToken.ToString());
        Assert.Equal("[Word] (5, 11): 'Header'", statement.LastToken.ToString());

        _textDocument.Text = " 123 # Header";
        lineResults = await _parserAndInterpreter.WaitAsync();
        Assert.Single(lineResults[0].StatementsAndData);
        statement = lineResults[0].StatementsAndData[0].ParsedStatement;
        Assert.IsType<NumericalCalculusStatement>(statement);
    }

    [Fact]
    public async Task ConditionStatement()
    {
        _textDocument.Text = " if 23 less than or equal to twenty four then ";
        IReadOnlyList<ParserAndInterpreterResultLine> lineResults = await _parserAndInterpreter.WaitAsync();
        Assert.Single(lineResults[0].StatementsAndData);
        var statement = (ConditionStatement)lineResults[0].StatementsAndData[0].ParsedStatement;
        var condition = (BinaryOperatorExpression)statement.Condition;
        Assert.Equal("([Numeric] (4, 6): '23' <= [Numeric] (29, 40): 'twenty four')", condition.ToString());

        _textDocument.Text = "23 == twenty three";
        lineResults = await _parserAndInterpreter.WaitAsync();
        Assert.Single(lineResults[0].StatementsAndData);
        statement = (ConditionStatement)lineResults[0].StatementsAndData[0].ParsedStatement;
        condition = (BinaryOperatorExpression)statement.Condition;
        Assert.Equal("([Numeric] (0, 2): '23' == [Numeric] (6, 18): 'twenty three')", condition.ToString());

        _textDocument.Text = "23 <= twenty three == 23";
        lineResults = await _parserAndInterpreter.WaitAsync();
        Assert.Equal(2, lineResults[0].StatementsAndData.Count);
        statement = (ConditionStatement)lineResults[0].StatementsAndData[0].ParsedStatement;
        condition = (BinaryOperatorExpression)statement.Condition;
        Assert.Equal("([Numeric] (0, 2): '23' <= [Numeric] (6, 18): 'twenty three')", condition.ToString());
        var statement2 = (NumericalCalculusStatement)lineResults[0].StatementsAndData[1].ParsedStatement;
        Expression expression = statement2.NumericalCalculusExpression;
        Assert.Equal("[Numeric] (22, 24): '23'", expression.ToString());

        string input = "if one hundred thousand dollars of income + (30% tax / two people) > 150k then test";

        _textDocument.Text = input;
        lineResults = await _parserAndInterpreter.WaitAsync();
        Assert.Single(lineResults[0].StatementsAndData);
        statement = (ConditionStatement)lineResults[0].StatementsAndData[0].ParsedStatement;
        condition = (BinaryOperatorExpression)statement.Condition;
        Assert.Equal("(([Numeric] (3, 31): 'one hundred thousand dollars' + (([Numeric] (45, 48): '30%' / [Numeric] (55, 58): 'two'))) > [Numeric] (69, 73): '150k')", condition.ToString());
    }

    [Fact]
    public async Task VariableDeclarationAndReference()
    {
        _textDocument.Text =
@"hello There = 2
hello There + 2
   hello There     =    3
well hello There = 5
hello There
Hum well hello There how are you?
hello there";

        IReadOnlyList<ParserAndInterpreterResultLine> lineResults = await _parserAndInterpreter.WaitAsync();

        Assert.Equal(7, lineResults.Count);

        Assert.Single(lineResults[0].StatementsAndData);
        Assert.IsType<VariableDeclarationStatement>(lineResults[0].StatementsAndData[0].ParsedStatement);
        Assert.Equal("Variable $(hello There) = [Numeric] (14, 15): '2'", lineResults[0].StatementsAndData[0].ParsedStatement.ToString());

        Assert.Single(lineResults[1].StatementsAndData);
        Assert.IsType<NumericalCalculusStatement>(lineResults[1].StatementsAndData[0].ParsedStatement);
        Assert.Equal("($(hello There) + [Numeric] (14, 15): '2')", lineResults[1].StatementsAndData[0].ParsedStatement.ToString());

        Assert.Single(lineResults[2].StatementsAndData);
        Assert.IsType<VariableDeclarationStatement>(lineResults[2].StatementsAndData[0].ParsedStatement);
        Assert.Equal("Variable $(hello There) = [Numeric] (24, 25): '3'", lineResults[2].StatementsAndData[0].ParsedStatement.ToString());

        Assert.Single(lineResults[3].StatementsAndData);
        Assert.IsType<VariableDeclarationStatement>(lineResults[3].StatementsAndData[0].ParsedStatement);
        Assert.Equal("Variable $(well hello There) = [Numeric] (19, 20): '5'", lineResults[3].StatementsAndData[0].ParsedStatement.ToString());

        Assert.Single(lineResults[4].StatementsAndData);
        Assert.IsType<NumericalCalculusStatement>(lineResults[4].StatementsAndData[0].ParsedStatement);
        Assert.Equal("$(hello There)", lineResults[4].StatementsAndData[0].ParsedStatement.ToString());

        Assert.Single(lineResults[5].StatementsAndData);
        Assert.IsType<NumericalCalculusStatement>(lineResults[5].StatementsAndData[0].ParsedStatement);
        Assert.Equal("$(well hello There)", lineResults[5].StatementsAndData[0].ParsedStatement.ToString());

        Assert.Empty(lineResults[6].StatementsAndData);
    }
}
