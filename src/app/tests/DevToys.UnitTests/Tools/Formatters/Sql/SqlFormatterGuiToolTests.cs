using System.Globalization;
using System.Threading.Tasks;
using DevToys.Core.Tools.Metadata;
using DevToys.Tools.Tools.Formatters.Sql;

namespace DevToys.UnitTests.Tools.Formatters.Sql;

public class SqlFormatterGuiToolTests : MefBasedTest
{
    private readonly UIToolView _toolView;
    private readonly SqlFormatterGuiTool _tool;
    private readonly IUIMultiLineTextInput _inputTextArea;
    private readonly IUIMultiLineTextInput _outputTextArea;

    public SqlFormatterGuiToolTests()
        : base(typeof(SqlFormatterGuiTool).Assembly)
    {
        _tool = (SqlFormatterGuiTool)MefProvider.ImportMany<IGuiTool, GuiToolMetadata>()
            .Single(t => t.Metadata.InternalComponentName == "SqlFormatter")
            .Value;

        _toolView = _tool.View;

        _inputTextArea = (IUIMultiLineTextInput)_toolView.GetChildElementById("sql-input-text-area");
        _outputTextArea = (IUIMultiLineTextInput)_toolView.GetChildElementById("sql-output-text-area");
        CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
    }

    [Theory(DisplayName = "Format sql with invalid sql should return empty string")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task FormatSqlWithInvalidSqlShouldReturnSqlError(string input)
    {
        _inputTextArea.Text("abc");
        _inputTextArea.Text(input);
        await _tool.WorkTask;
        _outputTextArea.Text.Should().Be("");
    }

    [Fact(DisplayName = "Format sql with valid sql and two spaces indentation should return valid sql")]
    public async Task FormatSqlWithValidSqlAndTwoSpacesShouldReturnValidSql()
    {
        _inputTextArea.Text("SELECT * FETCH FIRST 2 ROWS ONLY;");

        var indentationOptions = (IUISelectDropDownList)((IUISetting)_toolView.GetChildElementById("sql-text-indentation-setting")).InteractiveElement;
        indentationOptions.Select(0); // Select TwoSpaces

        await _tool.WorkTask;
        _outputTextArea.Text.Should().Be(@"SELECT
  *
FETCH FIRST
  2 ROWS ONLY;");
    }

    [Fact(DisplayName = "Format sql with valid sql and four spaces indentation should return valid sql")]
    public async Task FormatSqlWithValidSqlAndFourSpacesShouldReturnValidSql()
    {
        _inputTextArea.Text("SELECT * FETCH FIRST 2 ROWS ONLY;");

        var indentationOptions = (IUISelectDropDownList)((IUISetting)_toolView.GetChildElementById("sql-text-indentation-setting")).InteractiveElement;
        indentationOptions.Select(1); // Select FourSpaces

        await _tool.WorkTask;
        _outputTextArea.Text.Should().Be(@"SELECT
    *
FETCH FIRST
    2 ROWS ONLY;");
    }

    [Fact(DisplayName = "Format sql with valid sql and one tab indentation should return valid sql")]
    public async Task FormatSqlWithValidSqlAndOneTabShouldReturnValidSql()
    {
        _inputTextArea.Text("SELECT * FETCH FIRST 2 ROWS ONLY;");

        var indentationOptions = (IUISelectDropDownList)((IUISetting)_toolView.GetChildElementById("sql-text-indentation-setting")).InteractiveElement;
        indentationOptions.Select(2); // Select OneTab

        await _tool.WorkTask;
        _outputTextArea.Text.Should().Be("SELECT\r\n\t*\r\nFETCH FIRST\r\n\t2 ROWS ONLY;".Replace("\r\n", Environment.NewLine));
    }

    [Fact(DisplayName = "Format sql with valid sql and use leading comma should return valid sql")]
    public async Task FormatSqlWithValidSqlAndUseLeadingCommaShouldReturnValidSql()
    {
        _inputTextArea.Text("SELECT column1, column2 FROM table");

        var leadingCommaSetting = (IUISwitch)((IUISetting)_toolView.GetChildElementById("sql-leading-comma-setting")).InteractiveElement;
        leadingCommaSetting.On();

        await _tool.WorkTask;
        _outputTextArea.Text.Should().Be("""
SELECT
  column1
  , column2
FROM
  TABLE
""");
    }

    [Fact(DisplayName = "Format sql with valid sql and without leading comma should return valid sql")]
    public async Task FormatSqlWithValidSqlAndWithoutLeadingCommaShouldReturnValidSql()
    {
        _inputTextArea.Text("SELECT column1, column2 FROM table");

        var leadingCommaSetting = (IUISwitch)((IUISetting)_toolView.GetChildElementById("sql-leading-comma-setting")).InteractiveElement;
        leadingCommaSetting.Off();

        await _tool.WorkTask;
        _outputTextArea.Text.Should().Be("""
SELECT
  column1,
  column2
FROM
  TABLE
""");
    }
}
