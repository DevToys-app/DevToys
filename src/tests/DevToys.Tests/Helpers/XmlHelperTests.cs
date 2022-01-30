
using DevToys.Helpers;
using DevToys.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevToys.Tests.Helpers
{
    [TestClass]
    public class XmlHelperTests
    {
        [DataTestMethod]
        [DataRow(null, false)]
        [DataRow("", false)]
        [DataRow(" ", false)]
        [DataRow("<>", false)]
        [DataRow("</>", false)]
        [DataRow("<xml />", true)]
        [DataRow("<root><xml /></root>", true)]
        [DataRow("<root><xml test=\"true\" /></root>", true)]
        public void IsValid(string input, bool expectedResult)
        {
            Assert.AreEqual(expectedResult, XmlHelper.IsValid(input));
        }

        [DataTestMethod]
        [DataRow(null, "")]
        [DataRow("", "")]
        [DataRow(" ", "")]
        [DataRow("<xml />", "<xml />")]
        [DataRow("<root><xml /></root>", "<root>\r\n  <xml />\r\n</root>")]
        [DataRow("<root><xml test=\"true\" /></root>", "<root>\r\n  <xml test=\"true\" />\r\n</root>", false)]
        [DataRow("<root><xml test=\"true\" /></root>", "<root>\r\n  <xml\r\n    test=\"true\" />\r\n</root>", true)]
        public void FormatTwoSpaces(string input, string expectedResult, bool newLineOnAttributes = false)
        {
            Assert.AreEqual(expectedResult, XmlHelper.Format(input, Indentation.TwoSpaces, newLineOnAttributes));
        }

        [DataTestMethod]
        [DataRow(null, "")]
        [DataRow("", "")]
        [DataRow(" ", "")]
        [DataRow("<xml />", "<xml />")]
        [DataRow("<root><xml /></root>", "<root>\r\n    <xml />\r\n</root>")]
        [DataRow("<root><xml test=\"true\" /></root>", "<root>\r\n    <xml test=\"true\" />\r\n</root>", false)]
        [DataRow("<root><xml test=\"true\" /></root>", "<root>\r\n    <xml\r\n        test=\"true\" />\r\n</root>", true)]
        public void FormatFourSpaces(string input, string expectedResult, bool newLineOnAttributes = false)
        {
            Assert.AreEqual(expectedResult, XmlHelper.Format(input, Indentation.FourSpaces, newLineOnAttributes));
        }

        [DataTestMethod]
        [DataRow(null, "")]
        [DataRow("", "")]
        [DataRow(" ", "")]
        [DataRow("<xml />", "<xml />")]
        [DataRow("<root><xml /></root>", "<root>\r\n\t<xml />\r\n</root>")]
        [DataRow("<root><xml test=\"true\" /></root>", "<root>\r\n\t<xml test=\"true\" />\r\n</root>", false)]
        [DataRow("<root><xml test=\"true\" /></root>", "<root>\r\n\t<xml\r\n\t\ttest=\"true\" />\r\n</root>", true)]
        public void FormatOneTab(string input, string expectedResult, bool newLineOnAttributes = false)
        {
            Assert.AreEqual(expectedResult, XmlHelper.Format(input, Indentation.OneTab, newLineOnAttributes));
        }

        [DataTestMethod]
        [DataRow(null, "")]
        [DataRow("", "")]
        [DataRow(" ", "")]
        [DataRow("<xml />", "<xml />")]
        [DataRow("<root>\r\n    <xml />\r\n</root>", "<root><xml /></root>")]
        [DataRow("<root>\r\n      <xml />\r\n</root>", "<root><xml /></root>")]
        [DataRow("<root>\r\n    <xml test=\"true\" />\r\n</root>", "<root><xml test=\"true\" /></root>", false)]
        [DataRow("<root>\r\n    <xml\r\n        test=\"true\" />\r\n</root>", "<root><xml test=\"true\" /></root>",  true)]
        [DataRow("<root>\r\n\t<xml />\r\n</root>", "<root><xml /></root>")]
        [DataRow("<root>\r\n\t<xml test=\"true\" />\r\n</root>", "<root><xml test=\"true\" /></root>", false)]
        [DataRow("<root>\r\n\t<xml\r\n\t\ttest=\"true\" />\r\n</root>", "<root><xml test=\"true\" /></root>", true)]
        public void FormatMinified(string input, string expectedResult, bool newLineOnAttributes = false)
        {
            Assert.AreEqual(expectedResult, XmlHelper.Format(input, Indentation.Minified, newLineOnAttributes));
        }
    }
}
