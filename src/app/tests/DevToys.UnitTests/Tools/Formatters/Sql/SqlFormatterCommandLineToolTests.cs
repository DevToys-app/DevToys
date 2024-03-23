using System.IO;
using System.Threading.Tasks;
using DevToys.CLI;
using DevToys.Tools.Helpers.SqlFormatter;
using DevToys.Tools.Models;
using DevToys.Tools.Tools.Formatters.Sql;
using Microsoft.Extensions.Logging;

namespace DevToys.UnitTests.Tools.Formatters.Sql;

[Collection(nameof(TestParallelizationDisabled))]
public class SqlFormatterCommandLineToolTests : MefBasedTest
{
    private readonly StringWriter _consoleWriter = new();
    private readonly StringWriter _consoleErrorWriter = new();
    private readonly SqlFormatterCommandLineTool _tool;
    private readonly Mock<ILogger> _loggerMock;
    private readonly string _baseTestDataDirectory = Path.Combine("Tools", "TestData", "Sql");

    public SqlFormatterCommandLineToolTests()
        : base(typeof(SqlFormatterCommandLineTool).Assembly)
    {
        _tool = (SqlFormatterCommandLineTool)MefProvider.ImportMany<ICommandLineTool, CommandLineToolMetadata>()
            .Single(t => t.Metadata.InternalComponentName == "SqlFormatter").Value;

        _loggerMock = new Mock<ILogger>();
        Console.SetOut(_consoleWriter);
        Console.SetError(_consoleErrorWriter);
    }

    [Theory(DisplayName = "Format with invalid input should return error exit code")]
    [InlineData("")]
    [InlineData(" ")]
    public async Task FormatSqlInvalidInputShouldReturnErrorExitCode(string input)
    {
        _tool.Input = input;

        int result = await _tool.InvokeAsync(_loggerMock.Object, default);
        result.Should().Be(-1);
        string consoleOutput = _consoleErrorWriter.ToString().Trim();
        consoleOutput.Should().Be("");
    }

    [Theory(DisplayName = "Format sql with valid standard sql should output valid sql")]
    [InlineData(SqlLanguage.Sql, "SELECT * FETCH FIRST 2 ROWS ONLY;", @"SELECT
  *
FETCH FIRST
  2 ROWS ONLY;")]
    [InlineData(SqlLanguage.N1ql, "SELECT order_lines[0].productId FROM orders;", @"SELECT
  order_lines[0].productId
FROM
  orders;")]
    public async Task FormatSqlWithValidSqlAndTwoSpacesShouldOutputValidSql(SqlLanguage language, string input, string expectedResult)
    {
        _tool.Input = input;
        _tool.SqlLanguage = language;

        int result = await _tool.InvokeAsync(_loggerMock.Object, default);
        result.Should().Be(0);
        string consoleOutput = _consoleWriter.ToString().Trim();
        consoleOutput.Should().Be(expectedResult);
    }

    [Theory(DisplayName = "Format sql with valid standard sql should output valid sql")]
    [InlineData(SqlLanguage.Sql, "SELECT * FETCH FIRST 2 ROWS ONLY;", @"SELECT
    *
FETCH FIRST
    2 ROWS ONLY;")]
    [InlineData(SqlLanguage.Tsql, "INSERT Customers (ID, MoneyBalance, Address, City) VALUES (12,-123.4, 'Skagen 2111','Stv');", @"INSERT
    Customers (ID, MoneyBalance, Address, City)
VALUES
    (12, -123.4, 'Skagen 2111', 'Stv');")]
    public async Task FormatSqlWithValidSqlAndFourSpacesShouldOutputValidSql(SqlLanguage language, string input, string expectedResult)
    {
        _tool.Input = input;
        _tool.SqlLanguage = language;
        _tool.IndentationMode = Indentation.FourSpaces;

        int result = await _tool.InvokeAsync(_loggerMock.Object, default);
        result.Should().Be(0);
        string consoleOutput = _consoleWriter.ToString().Trim();
        consoleOutput.Should().Be(expectedResult);
    }

    [Theory(DisplayName = "Format sql with valid standard sql should output valid sql")]
    [InlineData(SqlLanguage.Sql, "SELECT * FETCH FIRST 2 ROWS ONLY;", "SELECT\r\n\t*\r\nFETCH FIRST\r\n\t2 ROWS ONLY;")]
    [InlineData(SqlLanguage.Db2, "SELECT col1 FROM tbl ORDER BY col2 DESC FETCH FIRST 20 ROWS ONLY;", "SELECT\r\n\tcol1\r\nFROM\r\n\ttbl\r\nORDER BY\r\n\tcol2 DESC\r\nFETCH FIRST\r\n\t20 ROWS ONLY;")]
    public async Task FormatSqlWithValidSqlAndTabShouldOutputValidSql(SqlLanguage language, string input, string expectedResult)
    {
        expectedResult = expectedResult.Replace("\r\n", Environment.NewLine);
        _tool.Input = input;
        _tool.SqlLanguage = language;
        _tool.IndentationMode = Indentation.OneTab;

        int result = await _tool.InvokeAsync(_loggerMock.Object, default);
        result.Should().Be(0);
        string consoleOutput = _consoleWriter.ToString().Trim();
        consoleOutput.Should().Be(expectedResult);
    }

    [Theory(DisplayName = "Format sql with valid sql and use leading comma should return valid sql")]
    [InlineData("SELECT column1, column2 FROM table", """
SELECT
  column1
  , column2
FROM
  TABLE
""")]
    public async Task FormatSqlWithValidSqlAndUseLeadingCommaShouldReturnValidSql(string input, string expectedResult)
    {
        _tool.Input = input;
        _tool.UseLeadingComma = true;

        int result = await _tool.InvokeAsync(_loggerMock.Object, default);

        result.Should().Be(0);
        string consoleOutput = _consoleWriter.ToString().Trim();
        consoleOutput.Should().Be(expectedResult);
    }

    [Theory(DisplayName = "Format sql with valid sql and without leading comma should return valid sql")]
    [InlineData("SELECT column1, column2 FROM table", """
SELECT
  column1,
  column2
FROM
  TABLE
""")]
    public async Task FormatSqlWithValidSqlAndWithoutLeadingCommaShouldReturnValidSql(string input, string expectedResult)
    {
        _tool.Input = input;
        _tool.UseLeadingComma = false;

        int result = await _tool.InvokeAsync(_loggerMock.Object, default);

        result.Should().Be(0);
        string consoleOutput = _consoleWriter.ToString().Trim();
        consoleOutput.Should().Be(expectedResult);
    }


}
