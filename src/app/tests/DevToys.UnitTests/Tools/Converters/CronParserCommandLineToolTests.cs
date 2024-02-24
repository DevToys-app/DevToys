using System.IO;
using System.Threading.Tasks;
using DevToys.CLI;
using DevToys.Tools.Tools.Converters.Cron;
using Microsoft.Extensions.Logging;

namespace DevToys.UnitTests.Tools.Converters;

[Collection(nameof(TestParallelizationDisabled))]
public sealed class CronParserCommandLineToolTests : MefBasedTest
{
    private readonly StringWriter _consoleWriter = new();
    private readonly StringWriter _consoleErrorWriter = new();
    private readonly CronParserCommandLineTool _tool;
    private readonly Mock<ILogger> _loggerMock;

    public CronParserCommandLineToolTests()
        : base(typeof(CronParserCommandLineTool).Assembly)
    {
        _tool = (CronParserCommandLineTool)MefProvider.ImportMany<ICommandLineTool, CommandLineToolMetadata>()
            .Single(t => t.Metadata.InternalComponentName == "CronParser").Value;

        _loggerMock = new Mock<ILogger>();
        Console.SetOut(_consoleWriter);
        Console.SetError(_consoleErrorWriter);
    }

    [Theory(DisplayName = "Invalid cron expression")]
    [InlineData("*")]
    [InlineData("* * * * * * * * *")]
    public async Task InvalidCronExpression(string input)
    {
        _tool.CronExpression = input;

        int result = await _tool.InvokeAsync(_loggerMock.Object, default);
        result.Should().Be(-1);
        string consoleOutput = _consoleWriter.ToString().Trim();
        consoleOutput.Should().Be("");
        string consoleErrorOutput = _consoleErrorWriter.ToString().Trim();
        consoleErrorOutput.Should().NotBe("");
    }

    [Theory(DisplayName = "Invalid date format")]
    [InlineData("hello")]
    public async Task InvalidDateFormat(string input)
    {
        _tool.CronExpression = "* * * * *";
        _tool.DateFormat = input;

        int result = await _tool.InvokeAsync(_loggerMock.Object, default);
        result.Should().Be(-1);
        string consoleOutput = _consoleWriter.ToString().Trim();
        consoleOutput.Should().Be("");
        string consoleErrorOutput = _consoleErrorWriter.ToString().Trim();
        consoleErrorOutput.Should().NotBe("");
    }

    [Theory(DisplayName = "Parse cron correctly")]
    [InlineData("* * * * * *", true, "Every second")]
    [InlineData("* * * * *", false, "Every minute")]
    public async Task ParseCron(string input, bool includeSeconds, string expectedResult)
    {
        _tool.CronExpression = input;
        _tool.IncludeSeconds = includeSeconds;

        int result = await _tool.InvokeAsync(_loggerMock.Object, default);
        result.Should().Be(0);
        string consoleOutput = _consoleWriter.ToString().Trim();
        consoleOutput.Should().StartWith(expectedResult);
    }
}
