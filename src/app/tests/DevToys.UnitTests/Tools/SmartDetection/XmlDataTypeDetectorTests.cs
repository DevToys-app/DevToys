﻿using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using DevToys.Tools.SmartDetection;
using Microsoft.Extensions.Logging;

namespace DevToys.UnitTests.Tools.SmartDetection;

[ExcludeFromCodeCoverage]
public class XmlDataTypeDetectorTests
{
    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData(" ", false)]
    [InlineData("<xml />", true)]
    [InlineData("<root><xml /></root>", true)]
    [InlineData("<root><xml test=\"true\" /></root>", true)]
    [InlineData("   <root>\r\n\t<xml test=\"true\" />\r\n</root>", true)]
    public async Task TryDetectDataAsync(string input, bool expectedResult)
    {
        LoggingExtensions.LoggerFactory ??= LoggerFactory.Create(builder => { });

        DataDetectionResult dataDetectionResult = new(false, input);
        XmlDataTypeDetector sut = new();
        DataDetectionResult result = await sut.TryDetectDataAsync(
            input,
            dataDetectionResult,
            default);

        result.Success.Should().Be(expectedResult);
    }
}
