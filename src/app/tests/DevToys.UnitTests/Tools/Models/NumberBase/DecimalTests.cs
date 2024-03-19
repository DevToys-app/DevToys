using System.Globalization;
using Decimal = DevToys.Tools.Models.NumberBase.Decimal;

namespace DevToys.UnitTests.Tools.Models.NumberBase;

public class DecimalTests
{
    [Theory]
    [InlineData(123456, "123,456")]
    [InlineData(12345, "12,345")]
    [InlineData(-123456, "-123,456")]
    [InlineData(-12345, "-12,345")]
    [InlineData(long.MinValue, "-9,223,372,036,854,775,808")]
    [InlineData(long.MaxValue, "9,223,372,036,854,775,807")]
    public void FormatDecimal(long input, string expectedResult)
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        Decimal.Instance.ToFormattedString(input, true).Should().Be(expectedResult);
    }

    [Theory]
    [InlineData("-9,223,372,036,854,775,808", long.MinValue)]
    [InlineData("9,223,372,036,854,775,807", long.MaxValue)]
    public void ParseDecimal(string input, long expectedResult)
    {
        input = input.Replace(",", string.Empty);
        Decimal.Instance.Parse(input).Should().Be(expectedResult);
    }
}
