using DevToys.Tools.Models.NumberBase;

namespace DevToys.UnitTests.Tools.Models.NumberBase;

public class OctalTests
{
    [Theory]
    [InlineData(206121044034, "2 777 561 304 102")]
    [InlineData(20612104403, "231 444 740 323")]
    [InlineData(long.MinValue, "1 000 000 000 000 000 000 000")]
    [InlineData(long.MaxValue, "777 777 777 777 777 777 777")]
    public void FormatDecimal(long input, string expectedResult)
    {
        Octal.Instance.ToFormattedString(input, true).Should().Be(expectedResult);
    }

    [Theory]
    [InlineData("1 000 000 000 000 000 000 000", long.MinValue)]
    [InlineData("777 777 777 777 777 777 777", long.MaxValue)]
    public void ParseHexadecimal(string input, long expectedResult)
    {
        input = input.Replace(" ", string.Empty);
        Octal.Instance.Parse(input).Should().Be(expectedResult);
    }
}
