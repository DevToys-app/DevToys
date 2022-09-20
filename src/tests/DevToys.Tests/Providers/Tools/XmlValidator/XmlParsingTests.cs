using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using DevToys.ViewModels.Tools.XmlValidator;
using DevToys.ViewModels.Tools.XmlValidator.Parsing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevToys.Tests.Providers.Tools
{
    [TestClass]
    public class XmlParsingTests
    {
        [TestMethod]
        public async Task Parse_EmptyXmlFile_Fails()
        {
            // Arrange
            string xsdSchemaString = await TestDataProvider.GetFileContent("Xml.ValidXsd.xml");
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
            string xsdSchemaString = await TestDataProvider.GetFileContent("Xml.ValidXsd.xml");
            XsdParser xsdParser = new XsdParser(xsdSchemaString);
            XsdParsingResult xsdParsingResult = xsdParser.Parse(xsdSchemaString);

            string xmlDataString = await TestDataProvider.GetFileContent("Xml.InvalidXml.xml");
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
            string xsdSchemaString = await TestDataProvider.GetFileContent("Xml.ValidXsd.xml");
            XsdParser xsdParser = new XsdParser(xsdSchemaString);
            XsdParsingResult xsdParsingResult = xsdParser.Parse(xsdSchemaString);

            string xmlDataString = await TestDataProvider.GetFileContent("Xml.ValidXml.xml");
            XmlParser xmlParser = new XmlParser(xmlDataString, xsdParsingResult.SchemaSet);

            // Act
            XmlParsingResult result = xmlParser.Parse(xmlDataString);

            // Assert
            Assert.IsTrue(result.IsValid);
            Assert.IsTrue(string.IsNullOrEmpty(result.ErrorMessage));
            Assert.IsInstanceOfType(result.XmlDocument, typeof(XDocument));
        }
        
        [TestMethod]
        public async Task Parse_ValidXmlSchema_SucceedsAndExtractsNamespacesCorrectly()
        {
            // Arrange
            string xsdSchemaString = await TestDataProvider.GetFileContent("Xml.ValidXsd.xml");
            XsdParser xsdParser = new XsdParser(xsdSchemaString);
            XsdParsingResult xsdParsingResult = xsdParser.Parse(xsdSchemaString);

            string xmlDataString = await TestDataProvider.GetFileContent("Xml.ValidXml.xml");
            XmlParser xmlParser = new XmlParser(xmlDataString, xsdParsingResult.SchemaSet);

            // Act
            XmlParsingResult result = xmlParser.Parse(xmlDataString);

            // Assert
            Assert.IsTrue(result.IsValid);

            IEnumerable<XmlNamespace> extractedNamespace = result.Namespaces;
            Assert.IsTrue(extractedNamespace.Count() == 1);
            
            (string namespacePrefix, string namespacePath) = extractedNamespace.First();
            (string expectedNamespacePrefix, string expectedNamespacePath) = ("", @"http://www.contoso.com/books");

            Assert.IsTrue(string.Equals(namespacePrefix, expectedNamespacePrefix));
            Assert.IsTrue(string.Equals(namespacePath, expectedNamespacePath));
        }
    }
}
