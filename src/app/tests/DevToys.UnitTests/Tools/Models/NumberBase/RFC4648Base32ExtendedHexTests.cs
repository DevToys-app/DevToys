using DevToys.Tools.Models.NumberBase;

namespace DevToys.UnitTests.Tools.Models.NumberBase;

public class RFC4648Base32ExtendedHexTests
{
    [Theory]
    [InlineData(ulong.MinValue, "0")]
    [InlineData(ulong.MaxValue, "F VVVV VVVV VVVV")]
    public void FormatRFC4648Base32ExtendedHex(ulong input, string expectedResult)
    {
        RFC4648Base32ExtendedHex.Instance.ToFormattedString(input, true).Should().Be(expectedResult);
    }

    [Theory]
    [InlineData("0", ulong.MinValue)]
    [InlineData("F VVVV VVVV VVVV", ulong.MaxValue)]
    public void ParseRFC4648Base32ExtendedHex(string input, ulong expectedResult)
    {
        input = input.Replace(" ", string.Empty);
        RFC4648Base32ExtendedHex.Instance.Parse(input).Should().Be(expectedResult);
    }
}
