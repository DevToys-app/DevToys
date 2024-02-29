using DevToys.Tools.Models.NumberBase;

namespace DevToys.UnitTests.Tools.Models.NumberBase;

public class HexadecimalTests
{
    [Theory]
    [InlineData(3333333333, "C6AE A155")]
    [InlineData(33333333, "1FC A055")]
    [InlineData(333333, "5 1615")]
    [InlineData(33333, "8235")]
    [InlineData(long.MinValue, "8000 0000 0000 0000")]
    [InlineData(long.MaxValue, "7FFF FFFF FFFF FFFF")]
    public void FormatHexadecimal(long input, string expectedResult)
    {
        Hexadecimal.Instance.ToFormattedString(input, true).Should().Be(expectedResult);
    }

    [Theory]
    [InlineData("8000 0000 0000 0000", long.MinValue)]
    [InlineData("7FFF FFFF FFFF FFFF", long.MaxValue)]
    public void ParseHexadecimal(string input, long expectedResult)
    {
        input = input.Replace(" ", string.Empty);
        Hexadecimal.Instance.Parse(input).Should().Be(expectedResult);
    }
}
