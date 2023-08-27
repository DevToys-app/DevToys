using System.Threading;
using DevToys.Tools.Helpers;
using DevToys.UnitTests.Mocks;

namespace DevToys.UnitTests.Tools.Helpers;

public class UrlHelperTests
{
    [Theory]
    [InlineData(null, "")]
    [InlineData("<hello world>", "%3Chello%20world%3E")]
    [InlineData("%3Chello%20world%3E", "%253Chello%2520world%253E")]
    internal void EncodeUrl(string input, string expectedResult)
    {
        UrlHelper.EncodeUrlData(
            input,
            new MockILogger(),
            CancellationToken.None)
            .Should()
            .Be(expectedResult);
    }

    [Theory]
    [InlineData(null, "")]
    [InlineData("%3Chello%20world%3E", "<hello world>")]
    [InlineData("%253Chello%2520world%253E", "%3Chello%20world%3E")]
    internal void DecodeUrl(string input, string expectedResult)
    {
        UrlHelper.DecodeUrlData(
            input,
            new MockILogger(),
            CancellationToken.None)
            .Should()
            .Be(expectedResult);
    }
}
