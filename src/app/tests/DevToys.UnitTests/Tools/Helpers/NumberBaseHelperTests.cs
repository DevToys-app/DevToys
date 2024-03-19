using System.Globalization;
using DevToys.Tools.Helpers;
using DevToys.Tools.Models.NumberBase;
using Decimal = DevToys.Tools.Models.NumberBase.Decimal;

namespace DevToys.UnitTests.Tools.Helpers;

public class NumberBaseHelperTests
{
    [Theory]
    [InlineData(
        "123ABC",
        true,
        "12 3ABC",
        "1,194,684",
        "4 435 274",
        "0001 0010 0011 1010 1011 1100",
        true)]
    [InlineData(
        "123 ABC",
        true,
        "12 3ABC",
        "1,194,684",
        "4 435 274",
        "0001 0010 0011 1010 1011 1100",
        true)]
    [InlineData(
        "123abc",
        true,
        "12 3ABC",
        "1,194,684",
        "4 435 274",
        "0001 0010 0011 1010 1011 1100",
        true)]
    [InlineData(
        "123 abc",
        true,
        "12 3ABC",
        "1,194,684",
        "4 435 274",
        "0001 0010 0011 1010 1011 1100",
        true)]
    [InlineData(
        "C6AEA155",
        true,
        "C6AE A155",
        "3,333,333,333",
        "30 653 520 525",
        "1100 0110 1010 1110 1010 0001 0101 0101",
        true)]
    [InlineData(
        "C6AE A155",
        true,
        "C6AE A155",
        "3,333,333,333",
        "30 653 520 525",
        "1100 0110 1010 1110 1010 0001 0101 0101",
        true)]
    [InlineData(
        "C6AE A15 5",
        true,
        "C6AE A155",
        "3,333,333,333",
        "30 653 520 525",
        "1100 0110 1010 1110 1010 0001 0101 0101",
        true)]
    [InlineData(
        "FFFF FFFF FFFF FFF6",
        true,
        "FFFF FFFF FFFF FFF6",
        "-10",
        "1 777 777 777 777 777 777 766",
        "1111 1111 1111 1111 1111 1111 1111 1111 1111 1111 1111 1111 1111 1111 1111 0110",
        true)]
    [InlineData(
        "FFFFFFFFFFFFFFF6",
        true,
        "FFFF FFFF FFFF FFF6",
        "-10",
        "1 777 777 777 777 777 777 766",
        "1111 1111 1111 1111 1111 1111 1111 1111 1111 1111 1111 1111 1111 1111 1111 0110",
        true)]
    [InlineData(
        "123ABC",
        false,
        "123ABC",
        "1194684",
        "4435274",
        "000100100011101010111100",
        true)]
    [InlineData(
        "123abc",
        false,
        "123ABC",
        "1194684",
        "4435274",
        "000100100011101010111100",
        true)]
    [InlineData(
        "C6AEA155",
        false,
        "C6AEA155",
        "3333333333",
        "30653520525",
        "11000110101011101010000101010101",
        true)]
    [InlineData(
        "FFFFFFFFFFFFFFF6",
        false,
        "FFFFFFFFFFFFFFF6",
        "-10",
        "1777777777777777777766",
        "1111111111111111111111111111111111111111111111111111111111110110",
        true)]
    [InlineData(
        "123ZER",
        true,
        "",
        "",
        "",
        "",
        false)]
    [InlineData(
        "123&ER",
        true,
        "",
        "",
        "",
        "",
        false)]
    [InlineData(
        "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",
        true,
        "",
        "",
        "",
        "",
        false)]
    public void ConvertFromHexadecimalWithFormatting(
        string input,
        bool format,
        string expectedHexadecimal,
        string expectedDecimal,
        string expectedOctal,
        string expectedBinary,
        bool expectedSuccess)
    {
        ConvertTest(
            Hexadecimal.Instance,
            format,
            input,
            expectedHexadecimal,
            expectedDecimal,
            expectedOctal,
            expectedBinary,
            expectedSuccess);
    }

    [Theory]
    [InlineData(
        "18006427676",
        true,
        "4 3144 481C",
        "18,006,427,676",
        "206 121 044 034",
        "0100 0011 0001 0100 0100 0100 1000 0001 1100",
        true)]
    [InlineData(
        "18,006,427,676",
        false,
        "43144481C",
        "18006427676",
        "206121044034",
        "010000110001010001000100100000011100",
        true)]
    [InlineData(
        "-18006427676",
        true,
        "FFFF FFFB CEBB B7E4",
        "-18,006,427,676",
        "1 777 777 777 571 656 733 744",
        "1111 1111 1111 1111 1111 1111 1111 1011 1100 1110 1011 1011 1011 0111 1110 0100",
        true)]
    [InlineData(
        "-18,006,427,676",
        false,
        "FFFFFFFBCEBBB7E4",
        "-18006427676",
        "1777777777571656733744",
        "1111111111111111111111111111101111001110101110111011011111100100",
        true)]
    [InlineData(
        "-18,006,427,676,999,999,999,999,999,999",
        false,
        "",
        "",
        "",
        "",
        false)]
    [InlineData(
        "abc",
        false,
        "",
        "",
        "",
        "",
        false)]
    public void ConvertFromDecimalWithFormatting(
        string input,
        bool format,
        string expectedHexadecimal,
        string expectedDecimal,
        string expectedOctal,
        string expectedBinary,
        bool expectedSuccess)
    {
        ConvertTest(
            Decimal.Instance,
            format,
            input,
            expectedHexadecimal,
            expectedDecimal,
            expectedOctal,
            expectedBinary,
            expectedSuccess);
    }

    [Theory]
    [InlineData(
        "30653520525",
        true,
        "C6AE A155",
        "3,333,333,333",
        "30 653 520 525",
        "1100 0110 1010 1110 1010 0001 0101 0101",
        true)]
    [InlineData(
        "30 653 520 525",
        false,
        "C6AEA155",
        "3333333333",
        "30653520525",
        "11000110101011101010000101010101",
        true)]
    [InlineData(
        "1777777777747124257253",
        true,
        "FFFF FFFF 3951 5EAB",
        "-3,333,333,333",
        "1 777 777 777 747 124 257 253",
        "1111 1111 1111 1111 1111 1111 1111 1111 0011 1001 0101 0001 0101 1110 1010 1011",
        true)]
    [InlineData(
        "1 777 777 777 747 124 257 253",
        false,
        "FFFFFFFF39515EAB",
        "-3333333333",
        "1777777777747124257253",
        "1111111111111111111111111111111100111001010100010101111010101011",
        true)]
    [InlineData(
        "30653520525&",
        false,
        "",
        "",
        "",
        "",
        false)]
    [InlineData(
        "30 653 520 525 &",
        false,
        "",
        "",
        "",
        "",
        false)]
    [InlineData(
        "306535205258",
        false,
        "",
        "",
        "",
        "",
        false)]
    [InlineData(
        "30 653 520 525 8",
        false,
        "",
        "",
        "",
        "",
        false)]
    [InlineData(
        "306535295256",
        false,
        "",
        "",
        "",
        "",
        false)]
    [InlineData(
        "30 653 529 525 6",
        false,
        "",
        "",
        "",
        "",
        false)]
    [InlineData(
        "3065352a5256",
        false,
        "",
        "",
        "",
        "",
        false)]
    [InlineData(
        "3065352A5256",
        false,
        "",
        "",
        "",
        "",
        false)]
    public void ConvertFromOctalWithFormatting(
        string input,
        bool format,
        string expectedHexadecimal,
        string expectedDecimal,
        string expectedOctal,
        string expectedBinary,
        bool expectedSuccess)
    {
        ConvertTest(
            Octal.Instance,
            format,
            input,
            expectedHexadecimal,
            expectedDecimal,
            expectedOctal,
            expectedBinary,
            expectedSuccess);
    }

    [Theory]
    [InlineData(
        "000101001101",
        false,
        "14D",
        "333",
        "515",
        "000101001101",
        true)]
    [InlineData(
        "11000110101011101010000101010101",
        false,
        "C6AEA155",
        "3333333333",
        "30653520525",
        "11000110101011101010000101010101",
        true)]
    [InlineData(
        "111111111 111111111111111111111 1100111001010100010 101111010101011",
        false,
        "FFFFFFFF39515EAB",
        "-3333333333",
        "1777777777747124257253",
        "1111111111111111111111111111111100111001010100010101111010101011",
        true)]
    [InlineData(
        "0001 0100 1101as",
        false,
        "",
        "",
        "",
        "",
        false)]
    [InlineData(
        "000101001101&",
        false,
        "",
        "",
        "",
        "",
        false)]
    public void ConvertFromBinaryWithFormatting(
        string input,
        bool format,
        string expectedHexadecimal,
        string expectedDecimal,
        string expectedOctal,
        string expectedBinary,
        bool expectedSuccess)
    {
        ConvertTest(
            Binary.Instance,
            format,
            input,
            expectedHexadecimal,
            expectedDecimal,
            expectedOctal,
            expectedBinary,
            expectedSuccess);
    }

    [Theory]
    [InlineData("0", ".", true)]
    [InlineData("FFFF FFFF FFFF FFFF", "?}: )>)) )')? ; _(", true)]
    public void ConvertFromRFC4648Base16ToCustomFormatWithFormatting(
        string rfc4648Base16Input,
        string expectedResult,
        bool expectedSuccess)
    {
        NumberBaseHelper.TryConvertNumberBase(
            rfc4648Base16Input,
            RFC4648Base16.Instance,
            new CustomUnsigned("./,<>?;':\"[]{ }()-_=+"),
            format: true,
            out string result,
            out string _)
            .Should().Be(expectedSuccess);

        result.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData("0", ".", true)]
    [InlineData("FFFF FFFF FFFF FFFF", ",,/> ,,.< .<// >>.. >,>/ ,//, ,><.", true)]
    public void ConvertFromRFC4648Base16ToSmallCustomFormatWithFormatting(
        string rfc4648Base16Input,
        string expectedResult,
        bool expectedSuccess)
    {
        NumberBaseHelper.TryConvertNumberBase(
            rfc4648Base16Input,
            RFC4648Base16.Instance,
            new CustomUnsigned("./,<>"),
            format: true,
            out string result,
            out string _)
            .Should().Be(expectedSuccess);

        result.Should().Be(expectedResult);
    }

    private static void ConvertTest(
        SignedLongNumberBaseDefinition numberBaseDefinition,
        bool format,
        string input,
        string expectedHexadecimal,
        string expectedDecimal,
        string expectedOctal,
        string expectedBinary,
        bool expectedSuccess)
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

        NumberBaseHelper.TryConvertNumberBase(
            input,
            numberBaseDefinition,
            format,
            out string hexadecimal,
            out string @decimal,
            out string octal,
            out string binary,
            out string _)
            .Should().Be(expectedSuccess);

        hexadecimal.Should().Be(expectedHexadecimal);
        @decimal.Should().Be(expectedDecimal);
        octal.Should().Be(expectedOctal);
        binary.Should().Be(expectedBinary);
    }
}
