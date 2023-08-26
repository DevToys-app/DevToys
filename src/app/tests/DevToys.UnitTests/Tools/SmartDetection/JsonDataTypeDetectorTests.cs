using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using DevToys.Tools.SmartDetection;
using Microsoft.Extensions.Logging;

namespace DevToys.UnitTests.Tools.SmartDetection;

[ExcludeFromCodeCoverage]
public class JsonDataTypeDetectorTests
{
    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData(" ", false)]
    [InlineData("{ \"title\": \"example glossary\" }", true)]
    [InlineData("[ \"title\", \"example glossary\" ]", true)]
    [InlineData("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.", false)]
    [InlineData("foo :\n  bar :\n    - boo: 1\n    - rab: 2\n    - plop: 3", false)]
    public async Task TryDetectDataAsync(string input, bool expectedResult)
    {
        LoggingExtensions.LoggerFactory ??= LoggerFactory.Create(builder => { });

        DataDetectionResult dataDetectionResult = new(false, input);
        JsonDataTypeDetector sut = new();
        DataDetectionResult result = await sut.TryDetectDataAsync(
            input,
            dataDetectionResult,
            default);

        result.Success.Should().Be(expectedResult);
    }
}
