using DevToys.Core.Formatter;
using DevToys.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevToys.Tests.Core.Formatter
{
    [TestClass]
    public class BaseNumberFormatterTests
    {
        [DataTestMethod]
        [DataRow("C6AEA155", 3333333333, false)]
        [DataRow("C6AE A155", 3333333333, true)]
        public void HexadecimalToDecimal(string input, long expectedResult, bool isFormatted)
        {
            Assert.AreEqual(expectedResult, NumberBaseFormatter.StringToBase(input, NumberBaseFormat.Hexadecimal));
        }

        [DataTestMethod]
        [DataRow("30653520525", 3333333333, false)]
        [DataRow("30 653 520 525", 3333333333, true)]
        public void OctalToDecimal(string input, long expectedResult, bool isFormatted)
        {
            Assert.AreEqual(expectedResult, NumberBaseFormatter.StringToBase(input, NumberBaseFormat.Octal));
        }

        [DataTestMethod]
        [DataRow("000101001101", 333, false)]
        [DataRow("0001 0100 1101", 333, true)]
        [DataRow("11000110101011101010000101010101", 3333333333, false)]
        [DataRow("1100 0110 1010 1110 1010 0001 0101 0101", 3333333333, true)]
        public void BinaryToDecimal(string input, long expectedResult, bool isFormatted)
        {
            Assert.AreEqual(expectedResult, NumberBaseFormatter.StringToBase(input, NumberBaseFormat.Binary));
        }

        [DataTestMethod]
        [DataRow(3333333333, "C6AEA155", false)]
        [DataRow(3333333333, "C6AE A155", true)]
        public void DecimalToHexadecimal(long input, string expectedResult, bool isFormatted)
        {
            Assert.AreEqual(expectedResult, NumberBaseFormatter.LongToBase(input, NumberBaseFormat.Hexadecimal, isFormatted));
        }

        [DataTestMethod]
        [DataRow(3333333333, "30653520525", false)]
        [DataRow(3333333333, "30 653 520 525", true)]
        public void DecimalToOctal(long input, string expectedResult, bool isFormatted)
        {
            Assert.AreEqual(expectedResult, NumberBaseFormatter.LongToBase(input, NumberBaseFormat.Octal, isFormatted));
        }

        [DataTestMethod]
        [DataRow(333, "000101001101", false)]
        [DataRow(333, "0001 0100 1101", true)]
        [DataRow(3333333333, "11000110101011101010000101010101", false)]
        [DataRow(3333333333, "1100 0110 1010 1110 1010 0001 0101 0101", true)]
        public void DecimalToBinary(long input, string expectedResult, bool isFormatted)
        {
            Assert.AreEqual(expectedResult, NumberBaseFormatter.LongToBase(input, NumberBaseFormat.Binary, isFormatted));
        }
    }
}
