using System.Threading.Tasks;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Metadata;
using DevToys.Tools.Tools.Text.SmartCalculator.Core;
using DevToys.UnitTests.Tools.Text.SmartCalculator;

namespace DevToys.UnitTests.Tools.Text.SmartCalculator;

public sealed class BinaryOperationTests : MefBaseTest
{
    private readonly ParserAndInterpreter _parserAndInterpreter;
    private readonly TextDocument _textDocument;

    public BinaryOperationTests()
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
    [InlineData("4 == 25%", "False")]
    [InlineData("0.4 == 40%", "True")]
    [InlineData("1 == 1.25", "False")]
    [InlineData("1.50 == 1.50", "True")]
    [InlineData("1 == a fifth", "False")]
    [InlineData("0.2 == a fifth", "True")]
    [InlineData("1 == True", "True")]
    [InlineData("1 == False", "False")]
    [InlineData("4 != 25%", "True")]
    [InlineData("0.4 != 40%", "False")]
    [InlineData("1 != 1.25", "True")]
    [InlineData("1.50 != 1.50", "False")]
    [InlineData("1 != a fifth", "True")]
    [InlineData("0.2 != a fifth", "False")]
    [InlineData("1 != True", "False")]
    [InlineData("1 != False", "True")]
    [InlineData("0.1 < 25%", "True")]
    [InlineData("0.4 < 40%", "False")]
    [InlineData("1 < 1.25", "True")]
    [InlineData("1.50 < 1.50", "False")]
    [InlineData("0.1 < a fifth", "True")]
    [InlineData("1 < a fifth", "False")]
    [InlineData("1 < True", "False")]
    [InlineData("0 < True", "True")]
    [InlineData("1 < False", "False")]
    [InlineData("4 <= 25%", "False")]
    [InlineData("0.4 <= 40%", "True")]
    [InlineData("1.25 <= 1", "False")]
    [InlineData("1.50 <= 1.50", "True")]
    [InlineData("1 <= a fifth", "False")]
    [InlineData("0.2 <= a fifth", "True")]
    [InlineData("1 <= True", "True")]
    [InlineData("1 <= False", "False")]
    [InlineData("0.1 > 25%", "False")]
    [InlineData("0.4 > 40%", "False")]
    [InlineData("1 > 1.25", "False")]
    [InlineData("1.50 > 1.50", "False")]
    [InlineData("0.1 > a fifth", "False")]
    [InlineData("1 > a fifth", "True")]
    [InlineData("1 > True", "False")]
    [InlineData("0 > True", "False")]
    [InlineData("1 > False", "True")]
    [InlineData("4 >= 25%", "True")]
    [InlineData("0.4 >= 40%", "True")]
    [InlineData("1.25 >= 1", "True")]
    [InlineData("1.50 >= 1.50", "True")]
    [InlineData("1 >= a fifth", "True")]
    [InlineData("0.2 >= a fifth", "True")]
    [InlineData("1 >= True", "True")]
    [InlineData("1 >= False", "True")]
    public async Task BinaryOperationAsync(string input, string output)
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
