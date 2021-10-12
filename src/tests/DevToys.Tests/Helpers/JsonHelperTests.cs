using DevToys.Helpers;
using DevToys.Models.Enumerations;
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
        public void FormatTwoSpaceIndentation(string input, string expectedResult)
        {
            Assert.AreEqual(expectedResult, JsonHelper.Format(input, IndentationEnumeration.TwoSpaceIndentation));
        }

        [DataTestMethod]
        [DataRow(null, "")]
        [DataRow("", "")]
        [DataRow(" ", "")]
        [DataRow("   {  }  ", "{}")]
        [DataRow("   { \"foo\": 123 }  ", "{\r\n    \"foo\": 123\r\n}")]
        public void FormatFourSpaceIndentation(string input, string expectedResult)
        {
            Assert.AreEqual(expectedResult, JsonHelper.Format(input, IndentationEnumeration.FourSpaceIndentation));
        }

        [DataTestMethod]
        [DataRow(null, "")]
        [DataRow("", "")]
        [DataRow(" ", "")]
        [DataRow("   {  }  ", "{}")]
        [DataRow("   { \"foo\": 123 }  ", "{\r\n\t\"foo\": 123\r\n}")]
        public void FormatOneTabIndentation(string input, string expectedResult)
        {
            Assert.AreEqual(expectedResult, JsonHelper.Format(input, IndentationEnumeration.OneTabIndentation));
        }

        [DataTestMethod]
        [DataRow(null, "")]
        [DataRow("", "")]
        [DataRow(" ", "")]
        [DataRow("   {  }  ", "{}")]
        [DataRow("   { \"foo\": 123 }  ", "{\"foo\":123}")]
        public void FormatNoIndentation(string input, string expectedResult)
        {
            Assert.AreEqual(expectedResult, JsonHelper.Format(input, IndentationEnumeration.NoIndentation));
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
