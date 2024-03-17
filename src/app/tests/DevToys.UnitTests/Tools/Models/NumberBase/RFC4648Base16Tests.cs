using DevToys.Tools.Models.NumberBase;

namespace DevToys.UnitTests.Tools.Models.NumberBase;

public class RFC4648Base16Tests
{
    [Theory]
    [InlineData(ulong.MinValue, "0")]
    [InlineData(ulong.MaxValue, "FFFF FFFF FFFF FFFF")]
    public void FormatRFC4648Base16(ulong input, string expectedResult)
    {
        RFC4648Base16.Instance.ToFormattedString(input, true).Should().Be(expectedResult);
    }

    [Theory]
    [InlineData("0", ulong.MinValue)]
    [InlineData("FFFF FFFF FFFF FFFF", ulong.MaxValue)]
    public void ParseRFC4648Base16(string input, ulong expectedResult)
    {
        input = input.Replace(" ", string.Empty);
        RFC4648Base16.Instance.Parse(input).Should().Be(expectedResult);
    }
}
