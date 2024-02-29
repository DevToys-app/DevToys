using DevToys.Tools.Models.NumberBase;

namespace DevToys.UnitTests.Tools.Models.NumberBase;

public class BinaryTests
{
    [Theory]
    [InlineData(333, "0001 0100 1101")]
    [InlineData(1869, "0111 0100 1101")]
    [InlineData(3917, "1111 0100 1101")]
    [InlineData(long.MinValue, "1000 0000 0000 0000 0000 0000 0000 0000 0000 0000 0000 0000 0000 0000 0000 0000")]
    [InlineData(long.MaxValue, "0111 1111 1111 1111 1111 1111 1111 1111 1111 1111 1111 1111 1111 1111 1111 1111")]
    public void FormatBinary(long input, string expectedResult)
    {
        Binary.Instance.ToFormattedString(input, true).Should().Be(expectedResult);
    }

    [Theory]
    [InlineData("1000 0000 0000 0000 0000 0000 0000 0000 0000 0000 0000 0000 0000 0000 0000 0000", long.MinValue)]
    [InlineData("0111 1111 1111 1111 1111 1111 1111 1111 1111 1111 1111 1111 1111 1111 1111 1111", long.MaxValue)]
    public void ParseBinary(string input, long expectedResult)
    {
        input = input.Replace(" ", string.Empty);
        Binary.Instance.Parse(input).Should().Be(expectedResult);
    }
}
