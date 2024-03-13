using System.Globalization;
using System.Threading.Tasks;
using DevToys.Core.Tools.Metadata;
using DevToys.Tools.Tools.Formatters.Xml;

namespace DevToys.UnitTests.Tools.Formatters;

public class XmlFormatterGuiToolTests : MefBasedTest
{
    private readonly UIToolView _toolView;
    private readonly XmlFormatterGuiTool _tool;
    private readonly IUIMultiLineTextInput _inputTextArea;
    private readonly IUIMultiLineTextInput _outputTextArea;

    public XmlFormatterGuiToolTests()
        : base(typeof(XmlFormatterGuiTool).Assembly)
    {
        _tool = (XmlFormatterGuiTool)MefProvider.ImportMany<IGuiTool, GuiToolMetadata>()
            .Single(t => t.Metadata.InternalComponentName == "XmlFormatter")
            .Value;

        _toolView = _tool.View;

        _inputTextArea = (IUIMultiLineTextInput)_toolView.GetChildElementById("xml-input-text-area");
        _outputTextArea = (IUIMultiLineTextInput)_toolView.GetChildElementById("xml-output-text-area");
        CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
    }

    [Theory(DisplayName = "Format xml with invalid xml should return empty string")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task FormatXmlWithInvalidXmlShouldReturnXmlError(string input)
    {
        _inputTextArea.Text("abc");
        _inputTextArea.Text(input);
        await _tool.WorkTask;
        _outputTextArea.Text.Should().Be("");
    }

    [Fact(DisplayName = "Format xml with invalid xml should return xml exception message")]
    public async Task FormatXmlWithInvalidXmlShouldReturnXmlExceptionMessage()
    {
        _inputTextArea.Text("<root><xml></root > ");
        await _tool.WorkTask;
        _outputTextArea.Text.Should().Be("The 'xml' start tag on line 1 position 8 does not match the end tag of 'root'. Line 1, position 14.");
    }

    [Fact(DisplayName = "Format xml with valid xml and two spaces indentation should return valid xml")]
    public async Task FormatXmlWithValidXmlAndTwoSpacesShouldReturnValidXml()
    {
        _inputTextArea.Text("<root><xml /></root>");

        var indentationOptions = (IUISelectDropDownList)((IUISetting)_toolView.GetChildElementById("xml-text-indentation-setting")).InteractiveElement;
        indentationOptions.Select(0); // Select TwoSpaces

        await _tool.WorkTask;
        _outputTextArea.Text.Should().Be("<root>\r\n  <xml />\r\n</root>".Replace("\r\n", Environment.NewLine));
    }

    [Fact(DisplayName = "Format xml with valid xml and four spaces indentation should return valid xml")]
    public async Task FormatXmlWithValidXmlAndFourSpacesShouldReturnValidXml()
    {
        _inputTextArea.Text("<root><xml /></root>");

        var indentationOptions = (IUISelectDropDownList)((IUISetting)_toolView.GetChildElementById("xml-text-indentation-setting")).InteractiveElement;
        indentationOptions.Select(1); // Select FourSpaces

        await _tool.WorkTask;
        _outputTextArea.Text.Should().Be("<root>\r\n    <xml />\r\n</root>".Replace("\r\n", Environment.NewLine));
    }

    [Fact(DisplayName = "Format xml with valid xml and one tab indentation should return valid xml")]
    public async Task FormatXmlWithValidXmlAndOneTabShouldReturnValidXml()
    {
        _inputTextArea.Text("<root><xml /></root>");

        var indentationOptions = (IUISelectDropDownList)((IUISetting)_toolView.GetChildElementById("xml-text-indentation-setting")).InteractiveElement;
        indentationOptions.Select(2); // Select OneTab

        await _tool.WorkTask;
        _outputTextArea.Text.Should().Be("<root>\r\n\t<xml />\r\n</root>".Replace("\r\n", Environment.NewLine));
    }

    [Fact(DisplayName = "Format xml with valid xml and minified should return valid xml")]
    public async Task FormatXmlWithValidXmlAndMinifiedShouldReturnValidXml()
    {
        _inputTextArea.Text("<root>\r\n    <xml />\r\n</root>");

        var indentationOptions = (IUISelectDropDownList)((IUISetting)_toolView.GetChildElementById("xml-text-indentation-setting")).InteractiveElement;
        indentationOptions.Select(3); // Select Minified

        await _tool.WorkTask;
        _outputTextArea.Text.Should().Be("<root><xml /></root>");
    }

    [Fact(DisplayName = "Format xml with valid xml and new line on attributes should return valid xml")]
    public async Task FormatXmlWithValidXmlAndNewLineOnAttributesShouldReturnValidXml()
    {
        _inputTextArea.Text("<root><xml test=\"true\" /></root>");
        var indentationOptions = (IUISelectDropDownList)((IUISetting)_toolView.GetChildElementById("xml-text-indentation-setting")).InteractiveElement;
        indentationOptions.Select(0); // Select TwoSpaces

        var newLineOnAttributesOptions = (IUISwitch)((IUISetting)_toolView.GetChildElementById("xml-text-newLineOnAttributes-setting")).InteractiveElement;
        newLineOnAttributesOptions.On();

        await _tool.WorkTask;
        _outputTextArea.Text.Should().Be("<root>\r\n  <xml\r\n    test=\"true\" />\r\n</root>".Replace("\r\n", Environment.NewLine));

        newLineOnAttributesOptions.Off();

        await _tool.WorkTask;
        _outputTextArea.Text.Should().Be("<root>\r\n  <xml test=\"true\" />\r\n</root>".Replace("\r\n", Environment.NewLine));
    }
}
