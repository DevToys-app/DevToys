using DevToys.Helpers.JsonYaml;
using DevToys.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevToys.Tests.Helpers
{
    [TestClass]
    public class JsonHelperTests
    {
        [DataTestMethod]
        [DataRow(null, true)]
        [DataRow("\"foo\"", true)]
        [DataRow("123", false)]
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
        [DataRow("{ \"Date\": \"2012-04-21T18:25:43-05:00\" }", "{\"Date\":\"2012-04-21T18:25:43-05:00\"}")]
        public void FormatDoesNotAlterateDateTimes(string input, string expectedResult)
        {
            Assert.AreEqual(expectedResult, JsonHelper.Format(input, Indentation.Minified));
        }

        [DataTestMethod]
        [DataRow(null, "")]
        [DataRow("", "")]
        [DataRow(" ", "")]
        public void ConvertFromYamlShouldReturnEmptyString(string input, string expected)
        {
            // prepare & act
            string actual = JsonHelper.ConvertFromYaml(input, Indentation.FourSpaces);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod("Convert From Yaml with unsupported Indentation")]
        public void ConvertFromYamlWithUnsupportedIndentationShouldReturnEmptyString()
        {
            // prepare 
            string input = "- key: value";
            string expected = string.Empty;

            // act
            string actual = JsonHelper.ConvertFromYaml(input, Indentation.Minified);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod("Convert From Yaml with unsupported yaml")]
        public void ConvertFromYamlWithUnsupportedYamlShouldExceptionMessage()
        {
            // prepare 
            string input = "[";
            string expected = "(Line: 2, Col: 1, Idx: 1) - (Line: 2, Col: 1, Idx: 1): While parsing a node, did not find expected node content.";

            // act
            string actual = JsonHelper.ConvertFromYaml(input, Indentation.Minified);

            Assert.AreEqual(expected, actual);
        }

        [DataTestMethod]
        [DataRow("   - key: value  ", "[\r\n  {\r\n    \"key\": \"value\"\r\n  }\r\n]")]
        [DataRow("   - key: value\r\n     key2: 1", "[\r\n  {\r\n    \"key\": \"value\",\r\n    \"key2\": 1\r\n  }\r\n]")]
        public void ConvertFromYamlWithTwoSpaces(string input, string expectedResult)
        {
            Assert.AreEqual(expectedResult, JsonHelper.ConvertFromYaml(input, Indentation.TwoSpaces));
        }

        [DataTestMethod]
        [DataRow("   - key: value  ", "[\r\n    {\r\n        \"key\": \"value\"\r\n    }\r\n]")]
        [DataRow("   - key: value\r\n     key2: 1", "[\r\n    {\r\n        \"key\": \"value\",\r\n        \"key2\": 1\r\n    }\r\n]")]
        public void ConvertFromYamlWithFourSpaces(string input, string expectedResult)
        {
            Assert.AreEqual(expectedResult, JsonHelper.ConvertFromYaml(input, Indentation.FourSpaces));
        }
    }
}
