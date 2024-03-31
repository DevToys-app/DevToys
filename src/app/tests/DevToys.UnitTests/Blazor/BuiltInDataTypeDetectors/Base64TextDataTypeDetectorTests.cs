using System.Threading.Tasks;
using DevToys.Blazor.BuiltInDataTypeDetectors;
using Microsoft.Extensions.Logging;

namespace DevToys.UnitTests.Blazor.BuiltInDataTypeDetectors;

public class Base64TextDataTypeDetectorTests
{
    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData(" ", false)]
    [InlineData("aGVsbG8gd29ybGQ=", true)]
    [InlineData("aGVsbG8gd2f9ybGQ=", false)]
    [InlineData("SGVsbG8gV29y", true)]
    [InlineData("SGVsbG8gVa29y", false)]
    public async Task TryDetectDataAsync(string input, bool expectedResult)
    {
        LoggingExtensions.LoggerFactory ??= LoggerFactory.Create(builder => { });

        DataDetectionResult dataDetectionResult = new(false, input);
        Base64TextDataTypeDetector sut = new();
        DataDetectionResult result = await sut.TryDetectDataAsync(
            input,
            dataDetectionResult,
            default);

        result.Success.Should().Be(expectedResult);
    }
}
