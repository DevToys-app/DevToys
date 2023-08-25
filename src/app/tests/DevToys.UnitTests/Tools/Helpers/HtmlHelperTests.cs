using System.Threading;
using DevToys.Tools.Helpers;
using DevToys.UnitTests.Mocks;

namespace DevToys.UnitTests.Tools.Helpers;

public class HtmlHelperTests
{
    [Theory]
    [InlineData(null, "")]
    [InlineData("<hello world>", "&lt;hello world&gt;")]
    [InlineData("&lt;hello&gt;", "&amp;lt;hello&amp;gt;")]
    internal void EncodeHtml(string input, string expectedResult)
    {
        HtmlHelper.EncodeHtmlData(
            input,
            new MockILogger(),
            CancellationToken.None)
            .Should()
            .Be(expectedResult);
    }

    [Theory]
    [InlineData(null, "")]
    [InlineData("&lt;hello world&gt;", "<hello world>")]
    [InlineData("&amp;lt;hello&amp;gt;", "&lt;hello&gt;")]
    internal void DecodeHtml(string input, string expectedResult)
    {
        HtmlHelper.DecodeHtmlData(
            input,
            new MockILogger(),
            CancellationToken.None)
            .Should()
            .Be(expectedResult);
    }
}
