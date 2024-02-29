using DevToys.Tools.Models.NumberBase;

namespace DevToys.UnitTests.Tools.Models.NumberBase;

public class RFC4648Base64UrlEncodeTests
{
    [Theory]
    [InlineData(ulong.MinValue, "A")]
    [InlineData(ulong.MaxValue, "P__ ____ ____")]
    public void FormatRFC4648Base64UrlEncode(ulong input, string expectedResult)
    {
        RFC4648Base64UrlEncode.Instance.ToFormattedString(input, true).Should().Be(expectedResult);
    }

    [Theory]
    [InlineData("A", ulong.MinValue)]
    [InlineData("P__ ____ ____", ulong.MaxValue)]
    public void ParseRFC4648Base64UrlEncode(string input, ulong expectedResult)
    {
        input = input.Replace(" ", string.Empty);
        RFC4648Base64UrlEncode.Instance.Parse(input).Should().Be(expectedResult);
    }
}
