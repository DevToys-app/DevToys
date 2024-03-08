using System.IO;
using System.Threading.Tasks;
using DevToys.CLI;
using DevToys.Tools.Models;
using DevToys.Tools.Tools.Formatters.Xml;
using Microsoft.Extensions.Logging;

namespace DevToys.UnitTests.Tools.Formatters;

[Collection(nameof(TestParallelizationDisabled))]
public sealed class XmlFormatterCommandLineToolTests : MefBasedTest
{
    private readonly StringWriter _consoleWriter = new();
    private readonly StringWriter _consoleErrorWriter = new();
    private readonly XmlFormatterCommandLineTool _tool;
    private readonly Mock<ILogger> _loggerMock;
    private readonly string _baseTestDataDirectory = Path.Combine("Tools", "TestData", nameof(XmlFormatter));

    public XmlFormatterCommandLineToolTests()
        : base(typeof(XmlFormatterCommandLineTool).Assembly)
    {
        _tool = (XmlFormatterCommandLineTool)MefProvider.ImportMany<ICommandLineTool, CommandLineToolMetadata>()
            .Single(t => t.Metadata.InternalComponentName == "XmlFormatter").Value;

        _loggerMock = new Mock<ILogger>();
        Console.SetOut(_consoleWriter);
        Console.SetError(_consoleErrorWriter);
    }

    [Theory(DisplayName = "Format with invalid input should return error exit code")]
    [InlineData("")]
    [InlineData(" ")]
    public async Task FormatXmlInvalidInputShouldReturnErrorExitCode(string input)
    {
        _tool.Input = input;

        int result = await _tool.InvokeAsync(_loggerMock.Object, default);
        result.Should().Be(-1);
        string consoleOutput = _consoleErrorWriter.ToString().Trim();
        consoleOutput.Should().Be("");
    }

    [Fact(DisplayName = "Format xml with invalid xml should output invalid xml error")]
    public async Task FormatXmlInvalidXmlShouldOutputInvalidXmlError()
    {
        _tool.Input = "<root><xml></root > ";

        int result = await _tool.InvokeAsync(_loggerMock.Object, default);
        result.Should().Be(-1);
        string consoleOutput = _consoleErrorWriter.ToString().Trim();
        consoleOutput.Should().Be("The 'xml' start tag on line 1 position 8 does not match the end tag of 'root'. Line 1, position 14.");
    }

    [Theory(DisplayName = "Format xml with valid xml and two spaces indentation should output valid xml")]
    [InlineData(true, "<root>\r\n  <xml\r\n    test=\"true\" />\r\n</root>")]
    [InlineData(false, "<root>\r\n  <xml test=\"true\" />\r\n</root>")]
    public async Task FormatXmlWithValidXmlAndTwoSpacesShouldOutputValidXml(bool newLineOnAttributes, string expectedResult)
    {
        _tool.NewLineOnAttributes = newLineOnAttributes;
        _tool.Input = "<root><xml test=\"true\" /></root>";

        int result = await _tool.InvokeAsync(_loggerMock.Object, default);
        result.Should().Be(0);
        string consoleOutput = _consoleWriter.ToString().Trim();
        consoleOutput.Should().Be(expectedResult);
    }

    [Theory(DisplayName = "Format xml with valid xml and four spaces indentation should output valid xml")]
    [InlineData(true, "<root>\r\n    <xml\r\n        test=\"true\" />\r\n</root>")]
    [InlineData(false, "<root>\r\n    <xml test=\"true\" />\r\n</root>")]
    public async Task FormatXmlWithValidXmlAndFourSpacesShouldOutputValidXml(bool newLineOnAttributes, string expectedResult)
    {
        _tool.IndentationMode = Indentation.FourSpaces;
        _tool.NewLineOnAttributes = newLineOnAttributes;
        _tool.Input = "<root><xml test=\"true\" /></root>";

        int result = await _tool.InvokeAsync(_loggerMock.Object, default);
        result.Should().Be(0);
        string consoleOutput = _consoleWriter.ToString().Trim();
        consoleOutput.Should().Be(expectedResult);
    }

    [Theory(DisplayName = "Format xml with valid xml and one tab indentation should output valid xml")]
    [InlineData(true, "<root>\r\n\t<xml\r\n\t\ttest=\"true\" />\r\n</root>")]
    [InlineData(false, "<root>\r\n\t<xml test=\"true\" />\r\n</root>")]
    public async Task FormatXmlWithValidXmlAndOneTabShouldOutputValidXml(bool newLineOnAttributes, string expectedResult)
    {
        _tool.IndentationMode = Indentation.OneTab;
        _tool.NewLineOnAttributes = newLineOnAttributes;
        _tool.Input = "<root><xml test=\"true\" /></root>";

        int result = await _tool.InvokeAsync(_loggerMock.Object, default);
        result.Should().Be(0);
        string consoleOutput = _consoleWriter.ToString().Trim();
        consoleOutput.Should().Be(expectedResult);
    }

    [Theory(DisplayName = "Format xml with valid xml and minified indentation should output valid xml")]
    [InlineData(true, "<root><xml test=\"true\" /></root>")]
    [InlineData(false, "<root><xml test=\"true\" /></root>")]
    public async Task FormatXmlWithValidXmlAndMinifiedShouldOutputValidXml(bool newLineOnAttributes, string expectedResult)
    {
        _tool.IndentationMode = Indentation.Minified;
        _tool.NewLineOnAttributes = newLineOnAttributes;
        _tool.Input = "<root><xml test=\"true\" /></root>";

        int result = await _tool.InvokeAsync(_loggerMock.Object, default);
        result.Should().Be(0);
        string consoleOutput = _consoleWriter.ToString().Trim();
        consoleOutput.Should().Be(expectedResult);
    }

    [Fact(DisplayName = "Format xml with unknown xml file should output invalid xml error")]
    public async Task FormatXmlWithUnknownFileInputShouldReturnErrorExitCode()
    {
        string filePath = Path.Combine(_baseTestDataDirectory, "Unknown.xml");
        FileInfo inputFile = TestDataProvider.GetFile(filePath);
        _tool.Input = inputFile;
        _tool.OutputFile = new FileInfo("Dummy.xml");
        ((MockIFileStorage)MefProvider.Import<IFileStorage>()).FileExistsResult = false;

        int result = await _tool.InvokeAsync(_loggerMock.Object, default);
        result.Should().Be(-1);
        string consoleOutput = _consoleErrorWriter.ToString().Trim();
        consoleOutput.Should().Be(XmlFormatter.InputFileNotFound);
    }

    [Fact(DisplayName = "Format xml with valid xml file and two spaces indentation should output valid xml file")]
    public async Task FormatXmlWithValidFileInputAndTwoSpacesShouldOutputValidXmlFile()
    {
        string expectedFilePath = Path.Combine(_baseTestDataDirectory, "TwoSpaces.xml");
        string expectedContent = File.ReadAllText(expectedFilePath);

        string filePath = Path.Combine(_baseTestDataDirectory, "ValidXml.xml");
        FileInfo inputFile = TestDataProvider.GetFile(filePath);
        _tool.Input = inputFile;
        _tool.OutputFile = new FileInfo("OutputTwoSpaces.xml");

        int result = await _tool.InvokeAsync(_loggerMock.Object, default);
        result.Should().Be(0);
        string outputContent = File.ReadAllText(_tool.OutputFile.FullName);
        outputContent.Should().Be(expectedContent);
    }

    [Fact(DisplayName = "Format xml with valid xml file and four spaces indentation should output valid xml file")]
    public async Task FormatXmlWithValidFileInputAndFourSpacesShouldOutputValidXmlFile()
    {
        string expectedFilePath = Path.Combine(_baseTestDataDirectory, "FourSpaces.xml");
        string expectedContent = File.ReadAllText(expectedFilePath);

        string filePath = Path.Combine(_baseTestDataDirectory, "ValidXml.xml");
        FileInfo inputFile = TestDataProvider.GetFile(filePath);
        _tool.Input = inputFile;
        _tool.IndentationMode = Indentation.FourSpaces;
        _tool.OutputFile = new FileInfo("OutputFourSpaces.xml");

        int result = await _tool.InvokeAsync(_loggerMock.Object, default);
        result.Should().Be(0);
        string outputContent = File.ReadAllText(_tool.OutputFile.FullName);
        outputContent.Should().Be(expectedContent);
    }

    [Fact(DisplayName = "Format xml with valid xml file and one tab indentation should output valid xml file")]
    public async Task FormatXmlWithValidFileInputAndOneTabShouldOutputValidXmlFile()
    {
        string expectedFilePath = Path.Combine(_baseTestDataDirectory, "OneTab.xml");
        string expectedContent = File.ReadAllText(expectedFilePath);

        string filePath = Path.Combine(_baseTestDataDirectory, "ValidXml.xml");
        FileInfo inputFile = TestDataProvider.GetFile(filePath);
        _tool.Input = inputFile;
        _tool.IndentationMode = Indentation.OneTab;
        _tool.OutputFile = new FileInfo("OutputOneTab.xml");

        int result = await _tool.InvokeAsync(_loggerMock.Object, default);
        result.Should().Be(0);
        string outputContent = File.ReadAllText(_tool.OutputFile.FullName);
        outputContent.Should().Be(expectedContent);
    }

    [Fact(DisplayName = "Format xml with valid xml file and minified indentation should output valid xml file")]
    public async Task FormatXmlWithValidFileInputAndMinifiedShouldOutputValidXmlFile()
    {
        string expectedFilePath = Path.Combine(_baseTestDataDirectory, "Minified.xml");
        string expectedContent = File.ReadAllText(expectedFilePath);

        string filePath = Path.Combine(_baseTestDataDirectory, "ValidXml.xml");
        FileInfo inputFile = TestDataProvider.GetFile(filePath);
        _tool.Input = inputFile;
        _tool.IndentationMode = Indentation.Minified;
        _tool.OutputFile = new FileInfo("OutputMinified.xml");

        int result = await _tool.InvokeAsync(_loggerMock.Object, default);
        result.Should().Be(0);
        string outputContent = File.ReadAllText(_tool.OutputFile.FullName);
        outputContent.Should().Be(expectedContent);
    }

    [Fact(DisplayName = "Format xml with valid xml file and new line on attributes should output valid xml file")]
    public async Task FormatXmlWithValidFileInputAndNewLineOnAttributesShouldOutputValidXmlFile()
    {
        string expectedFilePath = Path.Combine(_baseTestDataDirectory, "NewLineOnAttributes.xml");
        string expectedContent = File.ReadAllText(expectedFilePath);

        string filePath = Path.Combine(_baseTestDataDirectory, "ValidXml.xml");
        FileInfo inputFile = TestDataProvider.GetFile(filePath);
        _tool.Input = inputFile;
        _tool.NewLineOnAttributes = true;
        _tool.OutputFile = new FileInfo("OutputNewLineOnAttributes.xml");

        int result = await _tool.InvokeAsync(_loggerMock.Object, default);
        result.Should().Be(0);
        string outputContent = File.ReadAllText(_tool.OutputFile.FullName);
        outputContent.Should().Be(expectedContent);
    }
}
