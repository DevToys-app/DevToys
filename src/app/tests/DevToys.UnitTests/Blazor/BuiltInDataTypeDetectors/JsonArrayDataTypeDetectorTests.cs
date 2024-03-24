using System.Threading.Tasks;
using DevToys.Blazor.BuiltInDataTypeDetectors;
using Microsoft.Extensions.Logging;

namespace DevToys.UnitTests.Blazor.BuiltInDataTypeDetectors;

public class JsonArrayDataTypeDetectorTests
{
    [Theory]
    [InlineData(null, false)]
    [InlineData("\"foo\"", false)]
    [InlineData("123", false)]
    [InlineData("", false)]
    [InlineData(" ", false)]
    [InlineData("   {  }  ", false)]
    [InlineData("   [  ]  ", false)]
    [InlineData("   { \"foo\": 123 }  ", false)]
    [InlineData("   [{ \"foo\": 123 }]  ", true)]
    [InlineData("   [{ \"foo\": 123 }, { \"bar\": 456 }]  ", true)]
    public async Task TryDetectDataAsync(string input, bool expectedResult)
    {
        LoggingExtensions.LoggerFactory ??= LoggerFactory.Create(builder => { });

        DataDetectionResult dataDetectionResult = new(false, input);
        JsonArrayDataTypeDetector sut = new();
        DataDetectionResult result = await sut.TryDetectDataAsync(
            input,
            dataDetectionResult,
            default);

        result.Success.Should().Be(expectedResult);
    }
}
