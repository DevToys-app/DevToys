using System;
using System.Globalization;
using System.Threading;
using DevToys.Models;
using DevToys.ViewModels.Tools.Converters.NumberBaseConverter;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevToys.Tests.Providers.Tools
{
    [TestClass]
    public class BaseNumberFormatterTests
    {
        [DataTestMethod]
        [DataRow("123ABC", 1194684)]
        [DataRow("123 ABC", 1194684)]
        [DataRow("123abc", 1194684)]
        [DataRow("123 abc", 1194684)]
        [DataRow("C6AEA155", 3333333333)]
        [DataRow("C6AE A155", 3333333333)]
        [DataRow("C6AE A15 5", 3333333333)]
        [DataRow("FFFF FFFF FFFF FFF6", -10)]
        [DataRow("FFFFFFFFFFFFFFF6", -10)]
        public void HexadecimalToDecimal(string input, long expectedResult)
        {
            Assert.AreEqual(expectedResult, NumberBaseFormatter.StringToBase(input, NumberBaseFormat.Hexadecimal));
        }

        [DataTestMethod]
        [DataRow("123ZER")]
        [DataRow("123&ZER")]
        public void HexadecimalToDecimalShouldThrowInvalidOperationException(string input)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            InvalidOperationException exception = Assert.ThrowsException<InvalidOperationException>(
              () => NumberBaseFormatter.StringToBase(input, NumberBaseFormat.Hexadecimal));
            Assert.AreEqual("The current value isn't a valid Hexadecimal", exception.Message);
        }

        [DataTestMethod]
        [DataRow("30653520525", 3333333333)]
        [DataRow("30 653 520 525", 3333333333)]
        [DataRow("1777777777747124257253", -3333333333)]
        [DataRow("1 777 777 777 747 124 257 253", -3333333333)]
        public void OctalToDecimal(string input, long expectedResult)
        {
            Assert.AreEqual(expectedResult, NumberBaseFormatter.StringToBase(input, NumberBaseFormat.Octal));
        }

        [DataTestMethod]
        [DataRow("30653520525&")]
        [DataRow("30 653 520 525 &")]
        [DataRow("306535205258")]
        [DataRow("30 653 520 525 8")]
        [DataRow("306535295256")]
        [DataRow("30 653 529 525 6")]
        [DataRow("3065352a5256")]
        [DataRow("3065352A5256")]
        public void OctalToDecimalShouldThrowInvalidOperationException(string input)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            InvalidOperationException exception = Assert.ThrowsException<InvalidOperationException>(
              () => NumberBaseFormatter.StringToBase(input, NumberBaseFormat.Octal));
            Assert.AreEqual("The current value isn't a valid Octal", exception.Message);
        }

        [DataTestMethod]
        [DataRow("000101001101", 333)]
        [DataRow("0001 0100 1101", 333)]
        [DataRow("11000110101011101010000101010101", 3333333333)]
        [DataRow("1100 0110 1010 1110 1010 0001 0101 0101", 3333333333)]
        [DataRow("1111111111111111111111111111111100111001010100010101111010101011", -3333333333)]
        [DataRow("1111 1111 1111 1111 1111 1111 1111 1111 0011 1001 0101 0001 0101 1110 1010 1011", -3333333333)]
        public void BinaryToDecimal(string input, long expectedResult)
        {
            Assert.AreEqual(expectedResult, NumberBaseFormatter.StringToBase(input, NumberBaseFormat.Binary));
        }

        [DataTestMethod]
        [DataRow("000101001101&")]
        [DataRow("0001 0100 1101as")]
        public void BinaryToDecimalShouldThrowInvalidOperationException(string input)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            InvalidOperationException exception = Assert.ThrowsException<InvalidOperationException>(
                () => NumberBaseFormatter.StringToBase(input, NumberBaseFormat.Binary));
            Assert.AreEqual("The current value isn't a valid Binary", exception.Message);
        }

        [DataTestMethod]
        [DataRow(18006427676, "43144481C", false)]
        [DataRow(18006427676, "4 3144 481C", true)]
        [DataRow(-18006427676, "FFFFFFFBCEBBB7E4", false)]
        [DataRow(-18006427676, "FFFF FFFB CEBB B7E4", true)]
        public void DecimalToHexadecimal(long input, string expectedResult, bool isFormatted)
        {
            Assert.AreEqual(expectedResult, NumberBaseFormatter.LongToBase(input, NumberBaseFormat.Hexadecimal, isFormatted));
        }

        [DataTestMethod]
        [DataRow(18006427676, "206121044034", false)]
        [DataRow(18006427676, "206 121 044 034", true)]
        [DataRow(-18006427676, "1777777777571656733744", false)]
        [DataRow(-18006427676, "1 777 777 777 571 656 733 744", true)]
        public void DecimalToOctal(long input, string expectedResult, bool isFormatted)
        {
            Assert.AreEqual(expectedResult, NumberBaseFormatter.LongToBase(input, NumberBaseFormat.Octal, isFormatted));
        }

        [DataTestMethod]
        [DataRow(18006427676, "010000110001010001000100100000011100", false)]
        [DataRow(18006427676, "0100 0011 0001 0100 0100 0100 1000 0001 1100", true)]
        [DataRow(-18006427676, "1111111111111111111111111111101111001110101110111011011111100100", false)]
        [DataRow(-18006427676, "1111 1111 1111 1111 1111 1111 1111 1011 1100 1110 1011 1011 1011 0111 1110 0100", true)]
        public void DecimalToBinary(long input, string expectedResult, bool isFormatted)
        {
            Assert.AreEqual(expectedResult, NumberBaseFormatter.LongToBase(input, NumberBaseFormat.Binary, isFormatted));
        }

        [DataTestMethod]
        [DataRow(null, "")]
        [DataRow(" ", "")]
        [DataRow("   C6AE A15 5   ", "C6AEA155")]
        [DataRow("0001 0100 1101", "000101001101")]
        [DataRow("C6AE A15 5", "C6AEA155")]
        public void RemoveFormatting(string input, string expectedResult)
        {
            Assert.AreEqual(expectedResult, NumberBaseFormatter.RemoveFormatting(input));
        }

        [DataTestMethod]
        [DataRow("000101001101", "0001 0100 1101")]
        [DataRow("00 01 01 00 11 01", "0001 0100 1101")]
        public void FormatBinary(string input, string expectedResult)
        {
            Assert.AreEqual(expectedResult, NumberBaseFormatter.FormatNumber(input, NumberBaseFormat.Binary));
        }

        [DataTestMethod]
        [DataRow("2061 210 44  034 ", "206 121 044 034")]
        [DataRow("206121044034", "206 121 044 034")]
        public void FormatOctal(string input, string expectedResult)
        {
            Assert.AreEqual(expectedResult, NumberBaseFormatter.FormatNumber(input, NumberBaseFormat.Octal));
        }

        [DataTestMethod]
        [DataRow("123456", "123,456")]
        [DataRow("12345", "12,345")]
        [DataRow("-123456", "-123,456")]
        [DataRow("-12345", "-12,345")]
        [DataRow("-12,3456", "-123,456")]
        public void FormatDecimal(string input, string expectedResult)
        {
            Assert.AreEqual(expectedResult, NumberBaseFormatter.FormatNumber(input, NumberBaseFormat.Decimal));
        }

        [DataTestMethod]
        [DataRow("C6AEA155", "C6AE A155")]
        [DataRow("C6AE A155", "C6AE A155")]
        [DataRow(" C 6AE A1  5 5", "C6AE A155")]
        public void FormatHexadecimal(string input, string expectedResult)
        {
            Assert.AreEqual(expectedResult, NumberBaseFormatter.FormatNumber(input, NumberBaseFormat.Hexadecimal));
        }
    }
}
