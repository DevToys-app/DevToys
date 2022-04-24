using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Schema;
using DevToys.ViewModels.Tools.XmlValidator.Parsing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevToys.Tests.Providers.Tools
{
    [TestClass]
    public class XmlValidatorTests
    {
        [TestMethod]
        public void Parse_EmptyXsdFile_Fails()
        {
            // Arrange 
            string xsdSchemaString = string.Empty;
            XsdParser parser = new XsdParser(xsdSchemaString);

            // Act
            XsdParsingResult result = parser.Parse(xsdSchemaString);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsFalse(string.IsNullOrEmpty(result.ErrorMessage));
            Assert.AreEqual(null, result.SchemaSet);
        }

        [TestMethod]
        public async Task Parse_InvalidXsdSchema_Fails()
        {
            // Arrange 
            string xsdSchemaString = await TestDataProvider.GetFileContent("InvalidXsd.xml");
            XsdParser parser = new XsdParser(xsdSchemaString);

            // Act
            XsdParsingResult result = parser.Parse(xsdSchemaString);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsFalse(string.IsNullOrEmpty(result.ErrorMessage));
            Assert.AreEqual(null, result.SchemaSet);
        }

        [TestMethod]
        public async Task Parse_ValidXsdSchema_Succeeds()
        {
            // Arrange 
            string xsdSchemaString = await TestDataProvider.GetFileContent("ValidXsd.xml");
            XsdParser parser = new XsdParser(xsdSchemaString);

            // Act
            XsdParsingResult result = parser.Parse(xsdSchemaString);

            // Assert
            Assert.IsTrue(result.IsValid);
            Assert.IsTrue(string.IsNullOrEmpty(result.ErrorMessage));
            Assert.IsInstanceOfType(result.SchemaSet, typeof(XmlSchemaSet));
        }

        [TestMethod]
        public async Task Parse_EmptyXmlFile_Fails()
        {
            // Arrange
            string xsdSchemaString = await TestDataProvider.GetFileContent("ValidXsd.xml");
            XsdParser xsdParser = new XsdParser(xsdSchemaString);
            XsdParsingResult xsdParsingResult = xsdParser.Parse(xsdSchemaString);

            string xmlDataString = string.Empty;
            XmlParser xmlParser = new XmlParser(xmlDataString, xsdParsingResult.SchemaSet);

            // Act
            XmlParsingResult result = xmlParser.Parse(xmlDataString);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsFalse(string.IsNullOrEmpty(result.ErrorMessage));
            Assert.AreEqual(null, result.XmlDocument);
        }

        [TestMethod]
        public async Task Parse_InvalidXmlFile_Fails()
        {
            // Arrange
            string xsdSchemaString = await TestDataProvider.GetFileContent("ValidXsd.xml");
            XsdParser xsdParser = new XsdParser(xsdSchemaString);
            XsdParsingResult xsdParsingResult = xsdParser.Parse(xsdSchemaString);

            string xmlDataString = await TestDataProvider.GetFileContent("InvalidXml.xml");
            XmlParser xmlParser = new XmlParser(xmlDataString, xsdParsingResult.SchemaSet);

            // Act
            XmlParsingResult result = xmlParser.Parse(xmlDataString);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsFalse(string.IsNullOrEmpty(result.ErrorMessage));
            Assert.AreEqual(null, result.XmlDocument);
        }


        [TestMethod]
        public async Task Parse_ValidXmlSchema_Succeeds()
        {
            // Arrange
            string xsdSchemaString = await TestDataProvider.GetFileContent("ValidXsd.xml");
            XsdParser xsdParser = new XsdParser(xsdSchemaString);
            XsdParsingResult xsdParsingResult = xsdParser.Parse(xsdSchemaString);

            string xmlDataString = await TestDataProvider.GetFileContent("ValidXml.xml");
            XmlParser xmlParser = new XmlParser(xmlDataString, xsdParsingResult.SchemaSet);

            // Act
            XmlParsingResult result = xmlParser.Parse(xmlDataString);

            // Assert
            Assert.IsTrue(result.IsValid);
            Assert.IsTrue(string.IsNullOrEmpty(result.ErrorMessage));
            Assert.IsInstanceOfType(result.XmlDocument, typeof(XDocument));
        }
    }
}
