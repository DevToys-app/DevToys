using DevToys.Helpers;
using DevToys.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevToys.Tests.Helpers
{
    [TestClass]
    public class JsonHelperTests
    {
        [DataTestMethod]
        [DataRow(null, false)]
        [DataRow("", false)]
        [DataRow(" ", false)]
        [DataRow("   {  }  ", true)]
        [DataRow("   { \"foo\": 123 }  ", true)]
        [DataRow("   bar { \"foo\": 123 }  ", false)]
        public void IsValid(string input, bool expectedResult)
        {
            Assert.AreEqual(expectedResult, JsonHelper.IsValid(input));
        }

        [DataTestMethod]
        [DataRow(null, "")]
        [DataRow("", "")]
        [DataRow(" ", "")]
        [DataRow("   {  }  ", "{}")]
        [DataRow("   { \"foo\": 123 }  ", "{\r\n  \"foo\": 123\r\n}")]
        public void FormatTwoSpaces(string input, string expectedResult)
        {
            Assert.AreEqual(expectedResult, JsonHelper.Format(input, Indentation.TwoSpaces));
        }

        [DataTestMethod]
        [DataRow(null, "")]
        [DataRow("", "")]
        [DataRow(" ", "")]
        [DataRow("   {  }  ", "{}")]
        [DataRow("   { \"foo\": 123 }  ", "{\r\n    \"foo\": 123\r\n}")]
        public void FormatFourSpaces(string input, string expectedResult)
        {
            Assert.AreEqual(expectedResult, JsonHelper.Format(input, Indentation.FourSpaces));
        }

        [DataTestMethod]
        [DataRow(null, "")]
        [DataRow("", "")]
        [DataRow(" ", "")]
        [DataRow("   {  }  ", "{}")]
        [DataRow("   { \"foo\": 123 }  ", "{\r\n\t\"foo\": 123\r\n}")]
        public void FormatOneTab(string input, string expectedResult)
        {
            Assert.AreEqual(expectedResult, JsonHelper.Format(input, Indentation.OneTab));
        }

        [DataTestMethod]
        [DataRow(null, "")]
        [DataRow("", "")]
        [DataRow(" ", "")]
        [DataRow("   {  }  ", "{}")]
        [DataRow("   { \"foo\": 123 }  ", "{\"foo\":123}")]
        public void FormatMinified(string input, string expectedResult)
        {
            Assert.AreEqual(expectedResult, JsonHelper.Format(input, Indentation.Minified));
        }

        [DataTestMethod]
        [DataRow(null, "")]
        [DataRow("", "")]
        [DataRow(" ", "")]
        [DataRow("   {  }  ", "")]
        [DataRow("   { \"foo\": 123 }  ", "")]
        public void FormatNullIndentation(string input, string expectedResult)
        {
            Assert.AreEqual(expectedResult, JsonHelper.Format(input, null));
        }
    }
}
