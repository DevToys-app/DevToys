using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Schema;
using DevToys.ViewModels.Tools.XmlValidator;
using DevToys.ViewModels.Tools.XmlValidator.Parsing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevToys.Tests.Providers.Tools
{
    [TestClass]
    public class XsdParsingTests
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
            string xsdSchemaString = await TestDataProvider.GetFileContent("Xml.InvalidXsd.xml");
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
            string xsdSchemaString = await TestDataProvider.GetFileContent("Xml.ValidXsd.xml");
            XsdParser parser = new XsdParser(xsdSchemaString);

            // Act
            XsdParsingResult result = parser.Parse(xsdSchemaString);

            // Assert
            Assert.IsTrue(result.IsValid);
            Assert.IsTrue(string.IsNullOrEmpty(result.ErrorMessage));
            Assert.IsInstanceOfType(result.SchemaSet, typeof(XmlSchemaSet));
        }
        
        [TestMethod]
        public async Task Parse_ValidXsdSchema_SucceedsAndExtractsNamespacesCorrectly()
        {
            // Arrange 
            string xsdSchemaString = await TestDataProvider.GetFileContent("Xml.ValidXsd.xml");
            XsdParser parser = new XsdParser(xsdSchemaString);

            // Act
            XsdParsingResult result = parser.Parse(xsdSchemaString);

            // Assert
            Assert.IsTrue(result.IsValid);

            IEnumerable<XmlNamespace> extractedNamespace = result.Namespaces;
            Assert.IsTrue(extractedNamespace.Count() == 1);
            
            (string namespacePrefix, string namespacePath) = extractedNamespace.First();
            (string expectedNamespacePrefix, string expectedNamespacePath) = ("xs", @"http://www.w3.org/2001/XMLSchema");

            Assert.IsTrue(string.Equals(namespacePrefix, expectedNamespacePrefix));
            Assert.IsTrue(string.Equals(namespacePath, expectedNamespacePath));
        }
    }
}
