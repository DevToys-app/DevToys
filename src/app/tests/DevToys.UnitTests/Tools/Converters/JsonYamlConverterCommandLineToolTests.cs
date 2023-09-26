using System.IO;
using System.Threading.Tasks;
using DevToys.CLI;
using DevToys.Tools.Models;
using DevToys.Tools.Tools.Converters.JsonYaml;
using Microsoft.Extensions.Logging;

namespace DevToys.UnitTests.Tools.Converters;

public sealed class JsonYamlConverterCommandLineToolTests : MefBasedTest
{
    private readonly StringWriter _consoleWriter = new();
    private readonly StringWriter _consoleErrorWriter = new();
    private readonly JsonYamlConverterCommandLineTool _tool;
    private readonly Mock<ILogger> _loggerMock;

    private static string _baseTestDataDirectory
        => Path.Combine("Tools", "TestData", nameof(JsonYamlConverter));

    public JsonYamlConverterCommandLineToolTests()
        : base(typeof(JsonYamlConverterCommandLineTool).Assembly)
    {
        _tool = (JsonYamlConverterCommandLineTool)MefProvider.ImportMany<ICommandLineTool, CommandLineToolMetadata>()
            .Single(t => t.Metadata.InternalComponentName == "JsonYamlConverter").Value;

        _loggerMock = new Mock<ILogger>();
        Console.SetOut(_consoleWriter);
        Console.SetError(_consoleErrorWriter);
    }

    [Theory(DisplayName = "Convert with invalid input should return error exit code")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task ConvertJsonWithInvalidInputShouldReturnErrorExitCode(string input)
    {
        _tool.ConversionMode = JsonToYamlConversion.JsonToYaml;
        _tool.Input = input;

        int result = await _tool.InvokeAsync(_loggerMock.Object, default);
        result.Should().Be(-1);
    }

    #region JsonToYaml

    [Fact(DisplayName = "Convert json with invalid json should output invalid json error")]
    public async Task ConvertJsonWithInvalidJsonShouldOutputInvalidJsonError()
    {
        _tool.ConversionMode = JsonToYamlConversion.JsonToYaml;
        _tool.Input = "   bar { \"foo\": 123 }  ";

        int result = await _tool.InvokeAsync(_loggerMock.Object, default);
        result.Should().Be(-1);
        string consoleOutput = _consoleErrorWriter.ToString().Trim();
        consoleOutput.Should().Be("'b' is an invalid start of a value. LineNumber: 0 | BytePositionInLine: 3.");
    }

    [Fact(DisplayName = "Convert json with valid json should output valid yaml")]
    public async Task ConvertJsonWithValidJsonAndTwoSpacesShouldOutputValidYaml()
    {
        _tool.ConversionMode = JsonToYamlConversion.JsonToYaml;
        _tool.IndentationMode = Indentation.TwoSpaces;
        _tool.Input = "{\r\n  \"foo\": \"bar\",\r\n  \"fizz\": [\r\n     \"wizz\"\r\n  ]\r\n}";

        int result = await _tool.InvokeAsync(_loggerMock.Object, default);
        result.Should().Be(0);
        string consoleOutput = _consoleWriter.ToString().Trim();
        consoleOutput.Should().Be("foo: bar\r\nfizz:\r\n  - wizz");
    }

    [Fact(DisplayName = "Convert json with valid json and four spaces indentation should output valid yaml")]
    public async Task ConvertJsonWithValidJsonAndFourSpacesShouldOutputValidYaml()
    {
        _tool.ConversionMode = JsonToYamlConversion.JsonToYaml;
        _tool.IndentationMode = Indentation.FourSpaces;
        _tool.Input = "{\r\n  \"foo\": \"bar\",\r\n  \"fizz\": [\r\n     \"wizz\"\r\n  ]\r\n}";

        int result = await _tool.InvokeAsync(_loggerMock.Object, default);
        result.Should().Be(0);
        string consoleOutput = _consoleWriter.ToString().Trim();
        consoleOutput.Should().Be("foo: bar\r\nfizz:\r\n    - wizz");
    }

    [Fact(DisplayName = "Convert json with unknown json file should output invalid json error")]
    public async Task ConvertJsonWithUnknownFileInputShouldReturnErrorExitCode()
    {
        string filePath = Path.Combine(_baseTestDataDirectory, "Unknown.json");
        FileInfo inputFile = TestDataProvider.GetFile(filePath);
        _tool.ConversionMode = JsonToYamlConversion.JsonToYaml;
        _tool.InputFile = inputFile;
        _tool.OutputFile = new FileInfo("Dummy.yaml");

        int result = await _tool.InvokeAsync(_loggerMock.Object, default);
        result.Should().Be(-1);
        string consoleOutput = _consoleErrorWriter.ToString().Trim();
        consoleOutput.Should().Be(JsonYamlConverter.InputFileNotFound);
    }

    [Fact(DisplayName = "Convert json with invalid json file should output invalid json error")]
    public async Task ConvertJsonWithInvalidFileInputShouldReturnErrorExitCode()
    {
        string filePath = Path.Combine(_baseTestDataDirectory, "Invalid.json");
        FileInfo inputFile = TestDataProvider.GetFile(filePath);
        _tool.ConversionMode = JsonToYamlConversion.JsonToYaml;
        _tool.InputFile = inputFile;
        _tool.OutputFile = new FileInfo("Dummy.yaml");

        int result = await _tool.InvokeAsync(_loggerMock.Object, default);
        result.Should().Be(-1);
        string consoleOutput = _consoleErrorWriter.ToString().Trim();
        consoleOutput.Should().Be("'b' is an invalid start of a value. LineNumber: 0 | BytePositionInLine: 3.");
    }

    [Fact(DisplayName = "Convert json with valid json file should output valid yaml file")]
    public async Task ConvertJsonWithValidFileInputAndTwoSpacesShouldOutputValidYamlFile()
    {
        string filePath = Path.Combine(_baseTestDataDirectory, "TwoSpaces.json");
        FileInfo inputFile = TestDataProvider.GetFile(filePath);
        _tool.ConversionMode = JsonToYamlConversion.JsonToYaml;
        _tool.InputFile = inputFile;
        _tool.OutputFile = new FileInfo("TwoSpaces.yaml");

        int result = await _tool.InvokeAsync(_loggerMock.Object, default);
        result.Should().Be(0);
        string outputContent = File.ReadAllText(_tool.OutputFile.FullName);
        outputContent.Should().Be("foo: bar\r\nfizz:\r\n  - wizz\r\n");
    }

    [Fact(DisplayName = "Convert json with valid json file and four spaces should output valid yaml file")]
    public async Task ConvertJsonWithValidFileInputAndFourSpacesShouldOutputValidYamlFile()
    {
        string filePath = Path.Combine(_baseTestDataDirectory, "FourSpaces.json");
        FileInfo inputFile = TestDataProvider.GetFile(filePath);
        _tool.ConversionMode = JsonToYamlConversion.JsonToYaml;
        _tool.IndentationMode = Indentation.FourSpaces;
        _tool.InputFile = inputFile;
        _tool.OutputFile = new FileInfo("FourSpaces.yaml");

        int result = await _tool.InvokeAsync(_loggerMock.Object, default);
        result.Should().Be(0);
        string outputContent = File.ReadAllText(_tool.OutputFile.FullName);
        outputContent.Should().Be("foo: bar\r\nfizz:\r\n    - wizz\r\n");
    }

    #endregion

    #region YamlToJson

    [Fact(DisplayName = "Convert yaml with invalid yaml should output yaml exception message")]
    public async Task ConvertYamlWithInvalidYamlShouldOutputYamlExceptionMessage()
    {
        _tool.ConversionMode = JsonToYamlConversion.YamlToJson;
        _tool.Input = "'L' is an invalid start of a value. LineNumber: 0 | BytePositionInLine: 0";

        int result = await _tool.InvokeAsync(_loggerMock.Object, default);
        result.Should().Be(-1);
        string consoleOutput = _consoleErrorWriter.ToString().Trim();
        consoleOutput.Should().Be("While parsing a block mapping, did not find expected key.");
    }

    [Fact(DisplayName = "Convert yaml with valid yaml should output valid json")]
    public async Task ConvertYamlWithValidYamlShouldOutputValidJson()
    {
        _tool.ConversionMode = JsonToYamlConversion.YamlToJson;
        _tool.Input = "foo: bar\r\nfizz:\r\n - wizz";

        int result = await _tool.InvokeAsync(_loggerMock.Object, default);
        result.Should().Be(0);
        string consoleOutput = _consoleWriter.ToString().Trim();
        consoleOutput.Should().Be("{\r\n  \"foo\": \"bar\",\r\n  \"fizz\": [\r\n    \"wizz\"\r\n  ]\r\n}");
    }

    [Fact(DisplayName = "Convert yaml with valid yaml and four spaces should output valid json with four spaces")]
    public async Task ConvertYamlWithValidYamlAndFourSpacesIndentationShouldOutputValidYaml()
    {
        _tool.ConversionMode = JsonToYamlConversion.YamlToJson;
        _tool.IndentationMode = Indentation.FourSpaces;
        _tool.Input = "foo: bar\r\nfizz:\r\n - wizz";

        int result = await _tool.InvokeAsync(_loggerMock.Object, default);
        result.Should().Be(0);
        string consoleOutput = _consoleWriter.ToString().Trim();
        consoleOutput.Should().Be("{\r\n    \"foo\": \"bar\",\r\n    \"fizz\": [\r\n        \"wizz\"\r\n    ]\r\n}");
    }

    [Fact(DisplayName = "Convert yaml with invalid yaml file should output invalid yaml error")]
    public async Task ConvertYamlWithInvalidFileInputShouldReturnErrorExitCode()
    {
        string filePath = Path.Combine(_baseTestDataDirectory, "Invalid.yaml");
        FileInfo inputFile = TestDataProvider.GetFile(filePath);
        _tool.ConversionMode = JsonToYamlConversion.YamlToJson;
        _tool.InputFile = inputFile;
        _tool.OutputFile = new FileInfo("Dummy.json");

        int result = await _tool.InvokeAsync(_loggerMock.Object, default);
        result.Should().Be(-1);
        string consoleOutput = _consoleErrorWriter.ToString().Trim();
        consoleOutput.Should().Be("While parsing a block mapping, did not find expected key.");
    }

    [Fact(DisplayName = "Convert yaml with valid yaml file should output valid json file")]
    public async Task ConvertYamlWithValidFileInputAndTwoSpacesShouldOutputValidJsonFile()
    {
        string filePath = Path.Combine(_baseTestDataDirectory, "TwoSpaces.yaml");
        FileInfo inputFile = TestDataProvider.GetFile(filePath);
        _tool.ConversionMode = JsonToYamlConversion.YamlToJson;
        _tool.InputFile = inputFile;
        _tool.OutputFile = new FileInfo("TwoSpaces.json");

        int result = await _tool.InvokeAsync(_loggerMock.Object, default);
        result.Should().Be(0);
        string outputContent = File.ReadAllText(_tool.OutputFile.FullName);
        outputContent.Should().Be("{\r\n  \"foo\": \"bar\",\r\n  \"fizz\": [\r\n    \"wizz\"\r\n  ]\r\n}");
    }

    [Fact(DisplayName = "Convert yaml with valid yaml file and four spaces should output valid json file")]
    public async Task ConvertYamlWithValidFileInputAndFourSpacesShouldOutputValidJsonFile()
    {
        string filePath = Path.Combine(_baseTestDataDirectory, "FourSpaces.yaml");
        FileInfo inputFile = TestDataProvider.GetFile(filePath);
        _tool.ConversionMode = JsonToYamlConversion.YamlToJson;
        _tool.IndentationMode = Indentation.FourSpaces;
        _tool.InputFile = inputFile;
        _tool.OutputFile = new FileInfo("FourSpaces.json");

        int result = await _tool.InvokeAsync(_loggerMock.Object, default);
        result.Should().Be(0);
        string outputContent = File.ReadAllText(_tool.OutputFile.FullName);
        outputContent.Should().Be("{\r\n    \"foo\": \"bar\",\r\n    \"fizz\": [\r\n        \"wizz\"\r\n    ]\r\n}");
    }

    #endregion
}
