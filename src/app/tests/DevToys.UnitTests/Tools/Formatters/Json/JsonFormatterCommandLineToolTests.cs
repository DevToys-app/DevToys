using System.IO;
using System.Threading.Tasks;
using DevToys.CLI;
using DevToys.Tools.Models;
using DevToys.Tools.Tools.Formatters.Json;
using Microsoft.Extensions.Logging;

namespace DevToys.UnitTests.Tools.Formatters.Json;

[Collection(nameof(TestParallelizationDisabled))]
public sealed class JsonFormatterCommandLineToolTests : MefBasedTest
{
    private readonly StringWriter _consoleWriter = new();
    private readonly StringWriter _consoleErrorWriter = new();
    private readonly JsonFormatterCommandLineTool _tool;
    private readonly Mock<ILogger> _loggerMock;
    private readonly string _baseTestDataDirectory = Path.Combine("Tools", "TestData", "JsonYamlConverter");

    public JsonFormatterCommandLineToolTests()
        : base(typeof(JsonFormatterCommandLineTool).Assembly)
    {
        _tool = (JsonFormatterCommandLineTool)MefProvider.ImportMany<ICommandLineTool, CommandLineToolMetadata>()
            .Single(t => t.Metadata.InternalComponentName == "JsonFormatter").Value;

        _loggerMock = new Mock<ILogger>();
        Console.SetOut(_consoleWriter);
        Console.SetError(_consoleErrorWriter);
    }

    [Theory(DisplayName = "Format with invalid input should return error exit code")]
    [InlineData("")]
    [InlineData(" ")]
    public async Task FormatJsonInvalidInputShouldReturnErrorExitCode(string input)
    {
        _tool.Input = input;

        int result = await _tool.InvokeAsync(_loggerMock.Object, default);
        result.Should().Be(-1);
        string consoleOutput = _consoleErrorWriter.ToString().Trim();
        consoleOutput.Should().Be("");
    }

    [Fact(DisplayName = "Format json with invalid json should output invalid json error")]
    public async Task FormatJsonInvalidJsonShouldOutputInvalidJsonError()
    {
        _tool.Input = "   bar { \"foo\": 123 }  ";

        int result = await _tool.InvokeAsync(_loggerMock.Object, default);
        result.Should().Be(-1);
        string consoleOutput = _consoleErrorWriter.ToString().Trim();
        consoleOutput.Should().Be("Unexpected character encountered while parsing value: b. Path '', line 1, position 3.");
    }

    [Theory(DisplayName = "Format json with valid json and two spaces indentation should output valid json")]
    [InlineData(true, "{\r\n  \"a\": \"asdf\",\r\n  \"array\": [\r\n    {\r\n      \"a\": \"asdf\",\r\n      \"array\": [],\r\n      \"b\": 33,\r\n      \"c\": 545\r\n    }\r\n  ],\r\n  \"b\": 33,\r\n  \"c\": 545\r\n}")]
    [InlineData(false, "{\r\n  \"a\": \"asdf\",\r\n  \"c\": 545,\r\n  \"b\": 33,\r\n  \"array\": [\r\n    {\r\n      \"a\": \"asdf\",\r\n      \"c\": 545,\r\n      \"b\": 33,\r\n      \"array\": []\r\n    }\r\n  ]\r\n}")]
    public async Task FormatJsonWithValidJsonAndTwoSpacesShouldOutputValidJson(bool sortProperties, string expectedResult)
    {
        expectedResult = expectedResult.Replace("\r\n", Environment.NewLine);
        _tool.SortProperties = sortProperties;
        _tool.Input = "{\"a\": \"asdf\", \"c\" : 545, \"b\": 33, \"array\": [{\"a\": \"asdf\", \"c\" : 545, \"b\": 33, \"array\": []}]}";

        int result = await _tool.InvokeAsync(_loggerMock.Object, default);
        result.Should().Be(0);
        string consoleOutput = _consoleWriter.ToString().Trim();
        consoleOutput.Should().Be(expectedResult);
    }

    [Theory(DisplayName = "Format json with valid json and four spaces indentation should output valid json")]
    [InlineData(true, "{\r\n    \"a\": \"asdf\",\r\n    \"array\": [\r\n        {\r\n            \"a\": \"asdf\",\r\n            \"array\": [],\r\n            \"b\": 33,\r\n            \"c\": 545\r\n        }\r\n    ],\r\n    \"b\": 33,\r\n    \"c\": 545\r\n}")]
    [InlineData(false, "{\r\n    \"a\": \"asdf\",\r\n    \"c\": 545,\r\n    \"b\": 33,\r\n    \"array\": [\r\n        {\r\n            \"a\": \"asdf\",\r\n            \"c\": 545,\r\n            \"b\": 33,\r\n            \"array\": []\r\n        }\r\n    ]\r\n}")]
    public async Task FormatJsonWithValidJsonAndFourSpacesShouldOutputValidJson(bool sortProperties, string expectedResult)
    {
        expectedResult = expectedResult.Replace("\r\n", Environment.NewLine);
        _tool.IndentationMode = Indentation.FourSpaces;
        _tool.SortProperties = sortProperties;
        _tool.Input = "{\"a\": \"asdf\", \"c\" : 545, \"b\": 33, \"array\": [{\"a\": \"asdf\", \"c\" : 545, \"b\": 33, \"array\": []}]}";

        int result = await _tool.InvokeAsync(_loggerMock.Object, default);
        result.Should().Be(0);
        string consoleOutput = _consoleWriter.ToString().Trim();
        consoleOutput.Should().Be(expectedResult);
    }

    [Theory(DisplayName = "Format json with valid json and one tab indentation should output valid json")]
    [InlineData(true, "{\r\n\t\"a\": \"asdf\",\r\n\t\"array\": [\r\n\t\t{\r\n\t\t\t\"a\": \"asdf\",\r\n\t\t\t\"array\": [],\r\n\t\t\t\"b\": 33,\r\n\t\t\t\"c\": 545\r\n\t\t}\r\n\t],\r\n\t\"b\": 33,\r\n\t\"c\": 545\r\n}")]
    [InlineData(false, "{\r\n\t\"a\": \"asdf\",\r\n\t\"c\": 545,\r\n\t\"b\": 33,\r\n\t\"array\": [\r\n\t\t{\r\n\t\t\t\"a\": \"asdf\",\r\n\t\t\t\"c\": 545,\r\n\t\t\t\"b\": 33,\r\n\t\t\t\"array\": []\r\n\t\t}\r\n\t]\r\n}")]
    public async Task FormatJsonWithValidJsonAndOneTabShouldOutputValidJson(bool sortProperties, string expectedResult)
    {
        expectedResult = expectedResult.Replace("\r\n", Environment.NewLine);
        _tool.IndentationMode = Indentation.OneTab;
        _tool.SortProperties = sortProperties;
        _tool.Input = "{\"a\": \"asdf\", \"c\" : 545, \"b\": 33, \"array\": [{\"a\": \"asdf\", \"c\" : 545, \"b\": 33, \"array\": []}]}";

        int result = await _tool.InvokeAsync(_loggerMock.Object, default);
        result.Should().Be(0);
        string consoleOutput = _consoleWriter.ToString().Trim();
        consoleOutput.Should().Be(expectedResult);
    }

    [Theory(DisplayName = "Format json with valid json and minified indentation should output valid json")]
    [InlineData(true, "{\"a\":\"asdf\",\"array\":[{\"a\":\"asdf\",\"array\":[],\"b\":33,\"c\":545}],\"b\":33,\"c\":545}")]
    [InlineData(false, "{\"a\":\"asdf\",\"c\":545,\"b\":33,\"array\":[{\"a\":\"asdf\",\"c\":545,\"b\":33,\"array\":[]}]}")]
    public async Task FormatJsonWithValidJsonAndMinifiedShouldOutputValidJson(bool sortProperties, string expectedResult)
    {
        _tool.IndentationMode = Indentation.Minified;
        _tool.SortProperties = sortProperties;
        _tool.Input = "{\"a\": \"asdf\", \"c\" : 545, \"b\": 33, \"array\": [{\"a\": \"asdf\", \"c\" : 545, \"b\": 33, \"array\": []}]}";

        int result = await _tool.InvokeAsync(_loggerMock.Object, default);
        result.Should().Be(0);
        string consoleOutput = _consoleWriter.ToString().Trim();
        consoleOutput.Should().Be(expectedResult);
    }

    [Fact(DisplayName = "Format json with unknown json file should output invalid json error")]
    public async Task FormatJsonWithUnknownFileInputShouldReturnErrorExitCode()
    {
        string filePath = Path.Combine(_baseTestDataDirectory, "Unknown.json");
        FileInfo inputFile = TestDataProvider.GetFile(filePath);
        _tool.Input = inputFile;
        _tool.OutputFile = new FileInfo("Dummy.json");
        ((MockIFileStorage)MefProvider.Import<IFileStorage>()).FileExistsResult = false;

        int result = await _tool.InvokeAsync(_loggerMock.Object, default);
        result.Should().Be(-1);
        string consoleOutput = _consoleErrorWriter.ToString().Trim();
        consoleOutput.Should().Be(JsonFormatter.InputFileNotFound);
    }

    [Fact(DisplayName = "Format json with valid json file and two spaces indentation should output valid json file")]
    public async Task FormatJsonWithValidFileInputAndTwoSpacesShouldOutputValidJsonFile()
    {
        string expectedFilePath = Path.Combine(_baseTestDataDirectory, "TwoSpaces.json");
        string expectedContent = File.ReadAllText(expectedFilePath);

        string filePath = Path.Combine(_baseTestDataDirectory, "FourSpaces.json");
        FileInfo inputFile = TestDataProvider.GetFile(filePath);
        _tool.Input = inputFile;
        _tool.OutputFile = new FileInfo("OutputTwoSpaces.json");

        int result = await _tool.InvokeAsync(_loggerMock.Object, default);
        result.Should().Be(0);
        string outputContent = File.ReadAllText(_tool.OutputFile.FullName);
        outputContent.Should().Be(expectedContent);
    }

    [Fact(DisplayName = "Format json with valid json file and four spaces indentation should output valid json file")]
    public async Task FormatJsonWithValidFileInputAndFourSpacesShouldOutputValidJsonFile()
    {
        string expectedFilePath = Path.Combine(_baseTestDataDirectory, "FourSpaces.json");
        string expectedContent = File.ReadAllText(expectedFilePath);

        string filePath = Path.Combine(_baseTestDataDirectory, "TwoSpaces.json");
        FileInfo inputFile = TestDataProvider.GetFile(filePath);
        _tool.Input = inputFile;
        _tool.IndentationMode = Indentation.FourSpaces;
        _tool.OutputFile = new FileInfo("OutputFourSpaces.json");

        int result = await _tool.InvokeAsync(_loggerMock.Object, default);
        result.Should().Be(0);
        string outputContent = File.ReadAllText(_tool.OutputFile.FullName);
        outputContent.Should().Be(expectedContent);
    }

    [Fact(DisplayName = "Format json with json file containing invalid json should output invalid json error")]
    public async Task FormatJsonWithFileInputWithInvalidJsonShouldOutputInvalidJsonError()
    {
        string filePath = Path.Combine(_baseTestDataDirectory, "Invalid.json");
        FileInfo inputFile = TestDataProvider.GetFile(filePath);
        _tool.Input = inputFile;
        _tool.IndentationMode = Indentation.OneTab;
        _tool.OutputFile = new FileInfo("OutputOneTab.json");

        int result = await _tool.InvokeAsync(_loggerMock.Object, default);
        result.Should().Be(-1);
        string consoleOutput = _consoleErrorWriter.ToString().Trim();
        consoleOutput.Should().Be("Unexpected character encountered while parsing value: b. Path '', line 1, position 3.");
    }
}
