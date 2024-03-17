using DevToys.Tools.Models.NumberBase;

namespace DevToys.UnitTests.Tools.Models.NumberBase;

public class RFC4648Base32Tests
{
    [Theory]
    [InlineData(ulong.MinValue, "A")]
    [InlineData(ulong.MaxValue, "P 7777 7777 7777")]
    public void FormatRFC4648Base32(ulong input, string expectedResult)
    {
        RFC4648Base32.Instance.ToFormattedString(input, true).Should().Be(expectedResult);
    }

    [Theory]
    [InlineData("A", ulong.MinValue)]
    [InlineData("P 7777 7777 7777", ulong.MaxValue)]
    public void ParseRFC4648Base32(string input, ulong expectedResult)
    {
        input = input.Replace(" ", string.Empty);
        RFC4648Base32.Instance.Parse(input).Should().Be(expectedResult);
    }
}
