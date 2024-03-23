using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using DevToys.Tools.SmartDetection;
using Microsoft.Extensions.Logging;

namespace DevToys.UnitTests.Tools.SmartDetection;

public class EscapedCharacterDetectorTests
{
    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData(" ", false)]
    [InlineData("hello world", false)]
    [InlineData("hello\rworld", false)]
    [InlineData("hello\\rworld", true)]
    [InlineData("hello\\nworld", true)]
    [InlineData("hello\\\\world", true)]
    [InlineData("hello\\\"world", true)]
    [InlineData("hello\\tworld", true)]
    [InlineData("hello\\fworld", true)]
    [InlineData("hello\\bworld", true)]
    public async Task TryDetectDataAsync(string input, bool expectedResult)
    {
        LoggingExtensions.LoggerFactory ??= LoggerFactory.Create(builder => { });

        DataDetectionResult dataDetectionResult = new(false, input);
        EscapedCharactersDetector detector = new();
        DataDetectionResult result = await detector.TryDetectDataAsync(
            input,
            dataDetectionResult,
            default);

        result.Success.Should().Be(expectedResult);
    }
}
