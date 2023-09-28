using System.Globalization;
using System.Threading.Tasks;
using DevToys.Core.Tools.Metadata;
using DevToys.Tools.Tools.Converters.JsonYaml;

namespace DevToys.UnitTests.Tools.Converters;

public class JsonYamlConverterGuiToolTests : MefBasedTest
{
    private readonly UIToolView _toolView;
    private readonly JsonYamlConverterGuiTool _tool;
    private readonly IUIMultiLineTextInput _inputTextArea;
    private readonly IUIMultiLineTextInput _outputTextArea;

    public JsonYamlConverterGuiToolTests()
        : base(typeof(JsonYamlConverterGuiTool).Assembly)
    {
        _tool = (JsonYamlConverterGuiTool)MefProvider.ImportMany<IGuiTool, GuiToolMetadata>()
            .Single(t => t.Metadata.InternalComponentName == "JsonYamlConverter")
            .Value;

        _toolView = _tool.View;

        _inputTextArea = (IUIMultiLineTextInput)_toolView.GetChildElementById("json-to-yaml-input-text-area");
        _outputTextArea = (IUIMultiLineTextInput)_toolView.GetChildElementById("json-to-yaml-output-text-area");
        CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
    }

    [Theory(DisplayName = "Convert json with invalid json should return json error")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task ConvertJsonWithInvalidJsonShouldReturnJsonError(string input)
    {
        _inputTextArea.Text("abc");
        _inputTextArea.Text(input);
        await _tool.WorkTask;
        _outputTextArea.Text.Should().Be(JsonYamlConverter.InvalidJson);
    }

    [Fact(DisplayName = "Convert json with invalid json should return json exception message")]
    public async Task ConvertJsonWithInvalidJsonShouldReturnJsonExceptionMessage()
    {
        _inputTextArea.Text("   bar { \"foo\": 123 }  ");
        await _tool.WorkTask;
        _outputTextArea.Text.Should().Be("'b' is an invalid start of a value. LineNumber: 0 | BytePositionInLine: 3.");
    }

    [Fact(DisplayName = "Convert json with valid json should return valid yaml")]
    public async Task ConvertJsonWithValidJsonShouldReturnValidYaml()
    {
        _inputTextArea.Text("  { \"foo\": \"bar\" }  ");
        await _tool.WorkTask;
        _outputTextArea.Text.Should().Be("foo: bar\r\n");
    }

    [Fact(DisplayName = "Convert json with valid json and two spaces indentation should return valid yaml")]
    public async Task ConvertJsonWithValidJsonAndTwoSpacesShouldReturnValidYaml()
    {
        _inputTextArea.Text("{\r\n  \"foo\": \"bar\",\r\n  \"fizz\": [\r\n     \"wizz\"\r\n  ]\r\n}");

        var indentationOptions = (IUISelectDropDownList)((IUISetting)_toolView.GetChildElementById("json-to-yaml-text-indentation-setting")).InteractiveElement;
        indentationOptions.Select(0); // Select TwoSpaces

        await _tool.WorkTask;
        _outputTextArea.Text.Should().Be("foo: bar\r\nfizz:\r\n  - wizz\r\n");
    }

    [Fact(DisplayName = "Convert json with valid json and four spaces indentation should return valid yaml")]
    public async Task ConvertJsonWithValidJsonAndFourSpacesShouldReturnValidYaml()
    {
        _inputTextArea.Text("{\r\n  \"foo\": \"bar\",\r\n  \"fizz\": [\r\n     \"wizz\"\r\n  ]\r\n}");

        var indentationOptions = (IUISelectDropDownList)((IUISetting)_toolView.GetChildElementById("json-to-yaml-text-indentation-setting")).InteractiveElement;
        indentationOptions.Select(1); // Select FourSpaces

        await _tool.WorkTask;
        _outputTextArea.Text.Should().Be("foo: bar\r\nfizz:\r\n    - wizz\r\n");
    }

    [Theory(DisplayName = "Convert yaml with invalid input should return yaml error")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task ConvertYamlWithInvalidYamlShouldReturnYamlError(string input)
    {
        var conversionSettings = (IUISelectDropDownList)((IUISetting)_toolView.GetChildElementById("json-to-yaml-text-conversion-setting")).InteractiveElement;
        conversionSettings.Select(1); // Select YamlToJson

        _inputTextArea.Text(input);
        await _tool.WorkTask;
        _outputTextArea.Text.Should().Be(JsonYamlConverter.InvalidYaml);
    }

    [Fact(DisplayName = "Convert yaml with invalid yaml should return yaml exception message")]
    public async Task ConvertYamlWithInvalidYamlShouldReturnYamlExceptionMessage()
    {
        var conversionSettings = (IUISelectDropDownList)((IUISetting)_toolView.GetChildElementById("json-to-yaml-text-conversion-setting")).InteractiveElement;
        conversionSettings.Select(1); // Select YamlToJson

        _inputTextArea.Text("'L' is an invalid start of a value. LineNumber: 0 | BytePositionInLine: 0.");
        await _tool.WorkTask;
        _outputTextArea.Text.Should().Be("While parsing a block mapping, did not find expected key.");
    }

    [Fact(DisplayName = "Convert yaml with valid yaml should return valid json")]
    public async Task ConvertYamlWithValidYamlShouldReturnValidJson()
    {
        var conversionSettings = (IUISelectDropDownList)((IUISetting)_toolView.GetChildElementById("json-to-yaml-text-conversion-setting")).InteractiveElement;
        conversionSettings.Select(1); // Select YamlToJson

        var indentationOptions = (IUISelectDropDownList)((IUISetting)_toolView.GetChildElementById("json-to-yaml-text-indentation-setting")).InteractiveElement;
        indentationOptions.Select(0); // Select TwoSpaces

        _inputTextArea.Text("foo: bar\r\nfizz:\r\n - wizz\r\n\r\n");
        await _tool.WorkTask;
        _outputTextArea.Text.Should().Be("{\r\n  \"foo\": \"bar\",\r\n  \"fizz\": [\r\n    \"wizz\"\r\n  ]\r\n}");
    }

    [Fact(DisplayName = "Convert yaml with valid yaml and four spaces should return valid json with four spaces")]
    public async Task ConvertYamlWithValidYamlAndFourSpacesIndentationShouldReturnValidYaml()
    {
        var conversionSettings = (IUISelectDropDownList)((IUISetting)_toolView.GetChildElementById("json-to-yaml-text-conversion-setting")).InteractiveElement;
        conversionSettings.Select(1); // Select YamlToJson

        var indentationOptions = (IUISelectDropDownList)((IUISetting)_toolView.GetChildElementById("json-to-yaml-text-indentation-setting")).InteractiveElement;
        indentationOptions.Select(1); // Select FourSpaces

        _inputTextArea.Text("foo: bar\r\nfizz:\r\n - wizz\r\n\r\n");
        await _tool.WorkTask;
        _outputTextArea.Text.Should().Be("{\r\n    \"foo\": \"bar\",\r\n    \"fizz\": [\r\n        \"wizz\"\r\n    ]\r\n}");
    }
}
