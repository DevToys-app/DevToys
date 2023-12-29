using System.Globalization;
using System.Threading.Tasks;
using DevToys.Core.Tools.Metadata;
using DevToys.Tools.Tools.Formatters.Json;

namespace DevToys.UnitTests.Tools.Formatters.Json;

public class JsonFormatterGuiToolTests : MefBasedTest
{
    private readonly UIToolView _toolView;
    private readonly JsonFormatterGuiTool _tool;
    private readonly IUIMultiLineTextInput _inputTextArea;
    private readonly IUIMultiLineTextInput _outputTextArea;

    public JsonFormatterGuiToolTests()
        : base(typeof(JsonFormatterGuiTool).Assembly)
    {
        _tool = (JsonFormatterGuiTool)MefProvider.ImportMany<IGuiTool, GuiToolMetadata>()
            .Single(t => t.Metadata.InternalComponentName == "JsonFormatter")
            .Value;

        _toolView = _tool.View;

        _inputTextArea = (IUIMultiLineTextInput)_toolView.GetChildElementById("json-input-text-area");
        _outputTextArea = (IUIMultiLineTextInput)_toolView.GetChildElementById("json-output-text-area");
        CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
    }

    [Theory(DisplayName = "Format json with invalid json should return empty string")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task FormatJsonWithInvalidJsonShouldReturnJsonError(string input)
    {
        _inputTextArea.Text("abc");
        _inputTextArea.Text(input);
        await _tool.WorkTask;
        _outputTextArea.Text.Should().Be("");
    }

    [Fact(DisplayName = "Format json with invalid json should return json exception message")]
    public async Task FormatJsonWithInvalidJsonShouldReturnJsonExceptionMessage()
    {
        _inputTextArea.Text("   bar { \"foo\": 123 }  ");
        await _tool.WorkTask;
        _outputTextArea.Text.Should().Be("Unexpected character encountered while parsing value: b. Path '', line 1, position 3.");
    }

    [Fact(DisplayName = "Format json with valid json and two spaces indentation should return valid json")]
    public async Task FormatJsonWithValidJsonAndTwoSpacesShouldReturnValidJson()
    {
        _inputTextArea.Text("{ \"foo\": 123 }");

        var indentationOptions = (IUISelectDropDownList)((IUISetting)_toolView.GetChildElementById("json-text-indentation-setting")).InteractiveElement;
        indentationOptions.Select(0); // Select TwoSpaces

        await _tool.WorkTask;
        _outputTextArea.Text.Should().Be("{\r\n  \"foo\": 123\r\n}");
    }

    [Fact(DisplayName = "Format json with valid json and four spaces indentation should return valid json")]
    public async Task FormatJsonWithValidJsonAndFourSpacesShouldReturnValidJson()
    {
        _inputTextArea.Text("{ \"foo\": 123 }");

        var indentationOptions = (IUISelectDropDownList)((IUISetting)_toolView.GetChildElementById("json-text-indentation-setting")).InteractiveElement;
        indentationOptions.Select(1); // Select FourSpaces

        await _tool.WorkTask;
        _outputTextArea.Text.Should().Be("{\r\n    \"foo\": 123\r\n}");
    }

    [Fact(DisplayName = "Format json with valid json and one tab indentation should return valid json")]
    public async Task FormatJsonWithValidJsonAndOneTabShouldReturnValidJson()
    {
        _inputTextArea.Text("{ \"foo\": 123 }");

        var indentationOptions = (IUISelectDropDownList)((IUISetting)_toolView.GetChildElementById("json-text-indentation-setting")).InteractiveElement;
        indentationOptions.Select(2); // Select OneTab

        await _tool.WorkTask;
        _outputTextArea.Text.Should().Be("{\r\n\t\"foo\": 123\r\n}");
    }

    [Fact(DisplayName = "Format json with valid json and minified should return valid json")]
    public async Task FormatJsonWithValidJsonAndMinifiedShouldReturnValidJson()
    {
        _inputTextArea.Text("{ \"foo\": 123 }");

        var indentationOptions = (IUISelectDropDownList)((IUISetting)_toolView.GetChildElementById("json-text-indentation-setting")).InteractiveElement;
        indentationOptions.Select(3); // Select Minified

        await _tool.WorkTask;
        _outputTextArea.Text.Should().Be("{\"foo\":123}");
    }

    [Fact(DisplayName = "Format json with valid json and sort properties should return valid json")]
    public async Task FormatJsonWithValidJsonAndSortPropertiesShouldReturnValidJson()
    {
        _inputTextArea.Text("{\"a\": \"asdf\", \"c\" : 545, \"b\": 33, \"array\": [{\"a\": \"asdf\", \"c\" : 545, \"b\": 33, \"array\": []}]}");
        var indentationOptions = (IUISelectDropDownList)((IUISetting)_toolView.GetChildElementById("json-text-indentation-setting")).InteractiveElement;
        indentationOptions.Select(0); // Select TwoSpaces

        var newLineOnAttributesOptions = (IUISwitch)((IUISetting)_toolView.GetChildElementById("json-text-sortProperties-setting")).InteractiveElement;
        newLineOnAttributesOptions.On();

        await _tool.WorkTask;
        _outputTextArea.Text.Should().Be("{\r\n  \"a\": \"asdf\",\r\n  \"array\": [\r\n    {\r\n      \"a\": \"asdf\",\r\n      \"array\": [],\r\n      \"b\": 33,\r\n      \"c\": 545\r\n    }\r\n  ],\r\n  \"b\": 33,\r\n  \"c\": 545\r\n}");

        newLineOnAttributesOptions.Off();

        await _tool.WorkTask;
        _outputTextArea.Text.Should().Be("{\r\n  \"a\": \"asdf\",\r\n  \"c\": 545,\r\n  \"b\": 33,\r\n  \"array\": [\r\n    {\r\n      \"a\": \"asdf\",\r\n      \"c\": 545,\r\n      \"b\": 33,\r\n      \"array\": []\r\n    }\r\n  ]\r\n}");
    }
}
