using System.Threading.Tasks;
using DevToys.Blazor.BuiltInDataTypeDetectors;
using Microsoft.Extensions.Logging;

namespace DevToys.UnitTests.Blazor.BuiltInDataTypeDetectors;

public class GZipDataTypeDetectorTests
{
    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData(" ", false)]
    [InlineData("          ", false)]
    [InlineData("H4sIAAAAAAAAC/NIzcnJBwCCidH3BQAAAA==", true)]
    [InlineData("  H4sIAAAAAAAAC/NIzcnJBwCCidH3BQAAAA==", true)]
    [InlineData("              H4sIAAAAAAAAC/NIzcnJBwCCidH3BQAAAA==", true)]
    [InlineData("H4sI", true)]
    [InlineData("H4sIA", true)]
    [InlineData("H4s", false)]
    public async Task TryDetectDataAsync(string input, bool expectedResult)
    {
        LoggingExtensions.LoggerFactory ??= LoggerFactory.Create(builder => { });

        DataDetectionResult dataDetectionResult = new(false, input);
        GZipTextDataTypeDetector sut = new();
        DataDetectionResult result = await sut.TryDetectDataAsync(
            input,
            dataDetectionResult,
            default);

        result.Success.Should().Be(expectedResult);
    }
}
