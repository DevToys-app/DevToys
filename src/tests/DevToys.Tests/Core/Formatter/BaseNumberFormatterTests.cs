using System;
using System.Globalization;
using System.Threading;
using DevToys.Core.Formatter;
using DevToys.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevToys.Tests.Core.Formatter
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
        public void OctalToDecimal(string input, long expectedResult)
        {
            Assert.AreEqual(expectedResult, NumberBaseFormatter.StringToBase(input, NumberBaseFormat.Octal));
        }

        [DataTestMethod]
        [DataRow("30653520525&")]
        [DataRow("30 653 520 525 &")]
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
    }
}
