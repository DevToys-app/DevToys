using System.Threading.Tasks;
using DevToys.Tools.Tools.Text.SmartCalculator.Api;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.AbstractSyntaxTree;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Data;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Metadata;
using DevToys.Tools.Tools.Text.SmartCalculator.Core;

namespace DevToys.UnitTests.Tools.Text.SmartCalculator;

public sealed class OperationHelperTests : MefBaseTest
{
    private readonly ParserAndInterpreter _parserAndInterpreter;
    private readonly TextDocument _textDocument;
    private readonly IArithmeticAndRelationOperationService _arithmeticAndRelationOperationService;

    public OperationHelperTests()
    {
        _arithmeticAndRelationOperationService = ExportProvider.Import<IArithmeticAndRelationOperationService>();
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
    [InlineData("1", BinaryOperatorType.Addition, "2", "3")]
    [InlineData("2km", BinaryOperatorType.Addition, "25%", "2.5 km")]
    [InlineData("25%", BinaryOperatorType.Addition, "2km", "2.5 km")]
    [InlineData("1km", BinaryOperatorType.Addition, "1 meter", "1.001 km")]
    [InlineData("1 meter", BinaryOperatorType.Addition, "1km", "1.001 km")]
    [InlineData("1km", BinaryOperatorType.Addition, "1 minute", "Incompatible units")]
    [InlineData("1km", BinaryOperatorType.Addition, "1", "2 km")]
    [InlineData("1", BinaryOperatorType.Addition, "1km", "2 km")]
    [InlineData("25%", BinaryOperatorType.Addition, "25%", "31.25%")]
    [InlineData("1", BinaryOperatorType.Subtraction, "2", "-1")]
    [InlineData("1km", BinaryOperatorType.Subtraction, "3km", "-2 km")]
    [InlineData("1", BinaryOperatorType.Subtraction, "3km", "-2 km")]
    [InlineData("3km", BinaryOperatorType.Subtraction, "25%", "2.25 km")]
    [InlineData("25%", BinaryOperatorType.Subtraction, "3km", "-3.75 km")]
    [InlineData("25%", BinaryOperatorType.Subtraction, "25%", "18.75%")]
    [InlineData("2", BinaryOperatorType.Multiply, "3", "6")]
    [InlineData("2km", BinaryOperatorType.Multiply, "3km", "6 km²")]
    [InlineData("1km", BinaryOperatorType.Multiply, "3km", "3 km\u00b2")]
    [InlineData("1kg", BinaryOperatorType.Multiply, "1kg", "Unsupported arithmetic operation")]
    [InlineData("2kg", BinaryOperatorType.Multiply, "3", "6 kg")]
    [InlineData("2km", BinaryOperatorType.Multiply, "3", "6 km")]
    [InlineData("2kg", BinaryOperatorType.Multiply, "25%", "0.5 kg")]
    [InlineData("2km", BinaryOperatorType.Multiply, "25%", "0.5 km")]
    [InlineData("25%", BinaryOperatorType.Multiply, "2km", "0.5 km")]
    [InlineData("25%", BinaryOperatorType.Multiply, "25%", "6.25%")]
    [InlineData("6", BinaryOperatorType.Division, "2", "3")]
    [InlineData("1km", BinaryOperatorType.Division, "0", "∞")]
    [InlineData("1km", BinaryOperatorType.Division, "3", "0.3333 km")]
    [InlineData("1km", BinaryOperatorType.Division, "3 meter", "333.3333333333")]
    [InlineData("1km", BinaryOperatorType.Division, "3km", "0.3333333333")]
    [InlineData("25%", BinaryOperatorType.Division, "25%", "100%")]
    [InlineData("1km", BinaryOperatorType.LessThan, "3km", "True")]
    [InlineData("1km", BinaryOperatorType.Equality, "3km", "False")]
    [InlineData("1km", BinaryOperatorType.LessThan, "3kg", "Incompatible units")]
    [InlineData("10 rad", BinaryOperatorType.Division, "20 rad", "0.5")]
    [InlineData("10 rad", BinaryOperatorType.Multiply, "20 rad", "Unsupported arithmetic operation")]
    [InlineData("10 rad", BinaryOperatorType.Addition, "20 rad", "30 rad")]
    [InlineData("10 rad", BinaryOperatorType.Subtraction, "20 rad", "-10 rad")]
    [InlineData("10 km2", BinaryOperatorType.Division, "20", "500,000 m²")]
    [InlineData("10 km2", BinaryOperatorType.Division, "20 km2", "0.5")]
    [InlineData("10 km2", BinaryOperatorType.Multiply, "20 km2", "Unsupported arithmetic operation")]
    [InlineData("10 km2", BinaryOperatorType.Multiply, "20", "200 km²")]
    [InlineData("10 km2", BinaryOperatorType.Addition, "20 km2", "30 km²")]
    [InlineData("10 km2", BinaryOperatorType.Subtraction, "20 km2", "-10 km²")]
    [InlineData("10F", BinaryOperatorType.Division, "20F", "Unsupported arithmetic operation")]
    [InlineData("10F", BinaryOperatorType.Multiply, "20F", "Unsupported arithmetic operation")]
    [InlineData("10F", BinaryOperatorType.Addition, "20F", "30 °F")]
    [InlineData("10F", BinaryOperatorType.Addition, "20", "30 °F")]
    [InlineData("32F", BinaryOperatorType.Addition, "20C", "20 °C")] // This may be better than what Soulver and Wolfram Alpha is doing
    [InlineData("10F", BinaryOperatorType.Subtraction, "20F", "-10 °F")]
    [InlineData("2 m3 ", BinaryOperatorType.Addition, "3m3", "5 m³")]
    [InlineData("2 m3 ", BinaryOperatorType.Subtraction, "3m3", "-1 m³")]
    [InlineData("2 m3 ", BinaryOperatorType.Multiply, "3m3", "Unsupported arithmetic operation")]
    [InlineData("2 m3 ", BinaryOperatorType.Division, "3m3", "0.6666666667")]
    [InlineData("3 m/s", BinaryOperatorType.Addition, "2 m/s", "5 m/s")]
    [InlineData("3 m/s", BinaryOperatorType.Subtraction, "2 m/s", "1 m/s")]
    [InlineData("3 m/s", BinaryOperatorType.Multiply, "2 m/s", "Unsupported arithmetic operation")]
    [InlineData("3 m/s", BinaryOperatorType.Division, "2 m/s", "1.5")]
    [InlineData("3 kg", BinaryOperatorType.Addition, "2 g", "3.002 kg")]
    [InlineData("3 kg", BinaryOperatorType.Subtraction, "2 g", "2.998 kg")]
    [InlineData("3 kg", BinaryOperatorType.Multiply, "2 g", "Unsupported arithmetic operation")]
    [InlineData("3 kg", BinaryOperatorType.Division, "2 g", "1500")]
    [InlineData("3 MB", BinaryOperatorType.Addition, "2 KB", "3.002 MB")]
    [InlineData("3 MB", BinaryOperatorType.Subtraction, "2 KB", "2.998 MB")]
    [InlineData("1 MB", BinaryOperatorType.Subtraction, "2 KB", "998 kB")]
    [InlineData("3 MB", BinaryOperatorType.Multiply, "2 KB", "Unsupported arithmetic operation")]
    [InlineData("3 MB", BinaryOperatorType.Division, "2 KB", "1500")]
    [InlineData("3", BinaryOperatorType.Addition, "a fifth", "3.6")]
    [InlineData("3", BinaryOperatorType.Subtraction, "a fifth", "2.4")]
    [InlineData("3", BinaryOperatorType.Multiply, "a fifth", "1.8")]
    [InlineData("3", BinaryOperatorType.Division, "a fifth", "5")]
    [InlineData("1000 m2", BinaryOperatorType.Addition, "10 m2", "1,010 m²")]
    [InlineData("1000 m2", BinaryOperatorType.Subtraction, "10 m2", "990 m²")]
    [InlineData("1000 m2", BinaryOperatorType.Multiply, "10 m2", "Unsupported arithmetic operation")]
    [InlineData("1000 m2", BinaryOperatorType.Division, "10 m2", "100")]
    [InlineData("2 CAD", BinaryOperatorType.Addition, "2 USD", "4.62 CAD")]
    [InlineData("2 CAD", BinaryOperatorType.Subtraction, "2 USD", "-0.62 CAD")]
    [InlineData("2 CAD", BinaryOperatorType.Multiply, "2 USD", "5.24 CAD")]
    [InlineData("2 CAD", BinaryOperatorType.Division, "2 USD", "0.76 CAD")]
    [InlineData("2 USD", BinaryOperatorType.Addition, "2 CAD", "3.53 USD")]
    [InlineData("2 USD", BinaryOperatorType.Subtraction, "2 CAD", "0.47 USD")]
    [InlineData("2 USD", BinaryOperatorType.Multiply, "2 CAD", "3.05 USD")]
    [InlineData("2 USD", BinaryOperatorType.Division, "2 CAD", "1.31 USD")]
    [InlineData("2 USD", BinaryOperatorType.Addition, "2 USD", "4 USD")]
    [InlineData("2 USD", BinaryOperatorType.Subtraction, "2 USD", "0 USD")]
    [InlineData("2 USD", BinaryOperatorType.Multiply, "2 USD", "4 USD")]
    [InlineData("2 USD", BinaryOperatorType.Division, "2 USD", "1 USD")]
    [InlineData("2 CAD", BinaryOperatorType.Addition, "2 CAD", "4 CAD")]
    [InlineData("2 CAD", BinaryOperatorType.Subtraction, "2 CAD", "0 CAD")]
    [InlineData("2 CAD", BinaryOperatorType.Multiply, "2 CAD", "4 CAD")]
    [InlineData("2 CAD", BinaryOperatorType.Division, "2 CAD", "1 CAD")]
    public async Task Operation(string inputLeft, BinaryOperatorType binaryOperatorType, string inputRight, string output)
    {
        _textDocument.Text = inputLeft;
        IReadOnlyList<ParserAndInterpreterResultLine> parsingResult = await _parserAndInterpreter.WaitAsync();
        var leftData = (IData)parsingResult[0].TokenizedTextLine.Tokens.Token;

        _textDocument.Text = inputRight.PadLeft(inputLeft.Length + 1 + inputRight.Length /* for operator */);
        parsingResult = await _parserAndInterpreter.WaitAsync();
        var rightData = (IData)parsingResult[0].TokenizedTextLine.Tokens.Token;

        try
        {
            IData result = _arithmeticAndRelationOperationService.PerformOperation(leftData, binaryOperatorType, rightData);
            Assert.Equal(0, result.StartInLine);
            Assert.Equal(inputLeft.Length + 1 + inputRight.Length, result.Length);
            Assert.Equal(output, result.GetDataDisplayText());
        }
        catch (DataOperationException ex)
        {
            Assert.Equal(output, ex.GetLocalizedMessage(SupportedCultures.English));
        }
    }
}
