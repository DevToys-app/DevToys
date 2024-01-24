using System.IO;
using System.Threading.Tasks;
using DevToys.Core.Tools.Metadata;
using DevToys.Tools.Tools.Testers.JsonPathTester;

namespace DevToys.UnitTests.Tools.Testers.JsonPathTester;

public sealed class JsonPathTesterGuiToolTests : MefBasedTest
{
    private readonly JsonPathTesterGuiTool _tool;
    private readonly UIToolView _toolView;
    private readonly IUIMultiLineTextInput _inputBox;
    private readonly IUISingleLineTextInput _jsonPathInputBox;
    private readonly IUIMultiLineTextInput _outputBox;
    private readonly string _baseTestDataDirectory = Path.Combine("Tools", "TestData", nameof(JsonPathTester));

    public JsonPathTesterGuiToolTests()
    : base(typeof(JsonPathTesterGuiTool).Assembly)
    {
        _tool = (JsonPathTesterGuiTool)MefProvider.ImportMany<IGuiTool, GuiToolMetadata>()
            .Single(t => t.Metadata.InternalComponentName == "JSONPathTester")
        .Value;

        _toolView = _tool.View;

        _inputBox = (IUIMultiLineTextInput)_toolView.GetChildElementById("input-json-json-path-tester");
        _jsonPathInputBox = (IUISingleLineTextInput)_toolView.GetChildElementById("input-json-path-json-path-tester");
        _outputBox = (IUIMultiLineTextInput)_toolView.GetChildElementById("output-json-path-tester");
    }

    [Fact]
    public async Task TestJsonPathUi()
    {
        string inputJson = await TestDataProvider.GetEmbeddedFileContent("DevToys.UnitTests.Tools.TestData.JsonPathTester.sample.json");

        _inputBox.Text(inputJson);
        _jsonPathInputBox.Text("$.phoneNumbers[:1].type");
        await _tool.WorkTask;

        _outputBox.Text.Should().Be(
            @"[
  ""iPhone""
]");
    }

    [Fact]
    public async Task TestJsonPathUiFailed()
    {
        string inputJson = await TestDataProvider.GetEmbeddedFileContent("DevToys.UnitTests.Tools.TestData.JsonPathTester.sample.json");

        _inputBox.Text(inputJson);
        _jsonPathInputBox.Text("$.TEST");
        await _tool.WorkTask;

        _outputBox.Text.Should().Be("[]");
    }
}
