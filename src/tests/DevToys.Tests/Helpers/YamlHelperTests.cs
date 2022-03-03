using DevToys.Helpers.JsonYaml;
using DevToys.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevToys.Tests.Helpers
{
    [TestClass]
    public class YamlHelperTests
    {
        [DataTestMethod]
        [DataRow(null, false)]
        [DataRow("", false)]
        [DataRow(" ", false)]
        [DataRow("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.", false)]
        [DataRow("foo :\n  bar :\n    - boo: 1\n    - rab: 2\n    - plop: 3", true)]
        public void IsValidYaml(string input, bool expectedResult)
        {
            Assert.AreEqual(expectedResult, YamlHelper.IsValidYaml(input));
        }

        [DataTestMethod]
        [DataRow(null, "")]
        [DataRow("", "")]
        [DataRow(" ", "")]
        public void ConvertFromJsonShouldReturnEmptyString(string input, string expected)
        {
            // prepare & act
            string actual = YamlHelper.ConvertFromJson(input, Indentation.FourSpaces);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod("Convert From Json with unsupported Indentation")]
        public void ConvertFromJsonWithUnsupportedIndentationShouldReturnEmptyString()
        {
            // prepare 
            string input = "{}";
            string expected = string.Empty;

            // act
            string actual = YamlHelper.ConvertFromJson(input, Indentation.Minified);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod("Convert From Json with unsupported Json")]
        public void ConvertFromJsonWithUnsupportedJsonShouldExceptionMessage()
        {
            // prepare 
            string input = "-";
            string expected = "Input string '-' is not a valid number. Path '', line 1, position 1.";

            // act
            string actual = YamlHelper.ConvertFromJson(input, Indentation.Minified);

            Assert.AreEqual(expected, actual);
        }

        [DataTestMethod]
        [DataRow("{\r\n    \"key\": \"value\"\r\n  }", "key: value\r\n")]
        [DataRow("{\r\n    \"key\": \"value\",\r\n    \"key2\": 1\r\n  }", "key: value\r\nkey2: 1\r\n")]
        public void ConvertFromJsonWithTwoSpaces(string input, string expectedResult)
        {
            Assert.AreEqual(expectedResult, YamlHelper.ConvertFromJson(input, Indentation.TwoSpaces));
        }

        [DataTestMethod]
        [DataRow("{\r\n    \"key\": \"value\"\r\n  }", "key: value\r\n")]
        [DataRow("{\r\n    \"key\": \"value\",\r\n    \"key2\": 1\r\n  }", "key: value\r\nkey2: 1\r\n")]
        public void ConvertFromJsonWithFourSpaces(string input, string expectedResult)
        {
            Assert.AreEqual(expectedResult, YamlHelper.ConvertFromJson(input, Indentation.FourSpaces));
        }

        [DataTestMethod]
        [DataRow("[\r\n  {\r\n    \"key\": \"value\"\r\n  }\r\n]", "- key: value\r\n")]
        [DataRow("[\r\n  {\r\n    \"key\": \"value\",\r\n    \"key2\": 1\r\n  }\r\n]", "- key: value\r\n  key2: 1\r\n")]
        public void ConvertFromJsonWithJsonRootArrayWithTwoSpaces(string input, string expectedResult)
        {
            Assert.AreEqual(expectedResult, YamlHelper.ConvertFromJson(input, Indentation.TwoSpaces));
        }

        [DataTestMethod]
        [DataRow("[\r\n  {\r\n    \"key\": \"value\"\r\n  }\r\n]", "-   key: value\r\n")]
        [DataRow("[\r\n  {\r\n    \"key\": \"value\",\r\n    \"key2\": 1\r\n  }\r\n]", "-   key: value\r\n    key2: 1\r\n")]
        public void ConvertFromJsonWithJsonRootArrayWithFourSpaces(string input, string expectedResult)
        {
            Assert.AreEqual(expectedResult, YamlHelper.ConvertFromJson(input, Indentation.FourSpaces));
        }

    }
}
