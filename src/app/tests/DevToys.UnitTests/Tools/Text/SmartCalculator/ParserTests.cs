using System.Threading.Tasks;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Metadata;
using DevToys.Tools.Tools.Text.SmartCalculator.Core;
using DevToys.Tools.Tools.Text.SmartCalculator.Features.StatementParsersAndInterpreters.Comment;
using DevToys.Tools.Tools.Text.SmartCalculator.Features.StatementParsersAndInterpreters.Header;
using DevToys.Tools.Tools.Text.SmartCalculator.Features.StatementParsersAndInterpreters.NumericalExpression;
using DevToys.UnitTests.Tools.Text.SmartCalculator;

namespace DevToys.UnitTests.Tools.Text.SmartCalculator;

public class ParserTests : MefBaseTest
{
    private readonly ParserAndInterpreter _parserAndInterpreter;
    private readonly TextDocument _textDocument;

    public ParserTests()
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
    public async Task SimpleAsync()
    {
        _textDocument.Text =
@" # This is a header. 123. By the way I have 456% chance to get it to work.


I got -123 dollars in my pocket. // this is a comment.";

        IReadOnlyList<ParserAndInterpreterResultLine> lineResults = await _parserAndInterpreter.WaitAsync();
        Assert.Equal(4, lineResults.Count);
        Assert.Equal(1, lineResults[0].StatementsAndData.Count);
        Assert.Equal(0, lineResults[1].StatementsAndData.Count);
        Assert.Equal(0, lineResults[2].StatementsAndData.Count);
        Assert.Equal(2, lineResults[3].StatementsAndData.Count);

        Assert.IsType<HeaderStatement>(lineResults[0].StatementsAndData[0].ParsedStatement);
        Assert.IsType<NumericalCalculusStatement>(lineResults[3].StatementsAndData[0].ParsedStatement);
        Assert.IsType<CommentStatement>(lineResults[3].StatementsAndData[1].ParsedStatement);
    }

    [Theory]
    [InlineData("20/3", "6.66666666666667")]
    [InlineData("7/1900", "0.00368421052631579")]
    [InlineData("1/7/1900", "1/7/1900 12:00:00 AM")]
    public async Task ConflictingDataAsync(string input, string output)
    {
        _textDocument.Text = input;
        IReadOnlyList<ParserAndInterpreterResultLine> lineResults = await _parserAndInterpreter.WaitAsync();
        Assert.Single(lineResults);
        Assert.Single(lineResults[0].StatementsAndData);
        Assert.Equal(0, lineResults[0].SummarizedResultData.StartInLine);
        Assert.Equal(input.Length, lineResults[0].SummarizedResultData.Length);
        Assert.Equal(output, lineResults[0].SummarizedResultData.GetDataDisplayText());
    }
}
