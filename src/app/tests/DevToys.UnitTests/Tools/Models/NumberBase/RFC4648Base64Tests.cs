using DevToys.Tools.Models.NumberBase;

namespace DevToys.UnitTests.Tools.Models.NumberBase;

public class RFC4648Base64Tests
{
    [Theory]
    [InlineData(ulong.MinValue, "A")]
    [InlineData(ulong.MaxValue, "P// //// ////")]
    public void FormatRFC4648Base64(ulong input, string expectedResult)
    {
        RFC4648Base64.Instance.ToFormattedString(input, true).Should().Be(expectedResult);
    }

    [Theory]
    [InlineData("A", ulong.MinValue)]
    [InlineData("P// //// ////", ulong.MaxValue)]
    public void ParseRFC4648Base64(string input, ulong expectedResult)
    {
        input = input.Replace(" ", string.Empty);
        RFC4648Base64.Instance.Parse(input).Should().Be(expectedResult);
    }
}
