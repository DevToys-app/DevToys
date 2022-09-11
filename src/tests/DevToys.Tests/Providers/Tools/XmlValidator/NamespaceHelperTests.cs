using System.Collections.Generic;
using System.Threading.Tasks;
using DevToys.ViewModels.Tools.XmlValidator;
using DevToys.ViewModels.Tools.XmlValidator.Parsing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevToys.Tests.Providers.Tools
{
    [TestClass]
    public class NamespaceHelperTests
    {
        [TestMethod]
        public async Task GetMissingNamespaces_NoNamespacesMissing_ReturnsNoNamespacesMissing()
        {
            // Arrange
            string xsdSchemaString = await TestDataProvider.GetFileContent("Xml.ValidXsdWithNamespace.xml");
            XsdParser xsdParser = new XsdParser(xsdSchemaString);
            XsdParsingResult xsdParsingResult = xsdParser.Parse(xsdSchemaString);

            string xmlDataString = await TestDataProvider.GetFileContent("Xml.ValidXmlWithNamespace.xml");
            XmlParser xmlParser = new XmlParser(xmlDataString, xsdParsingResult.SchemaSet);
            XmlParsingResult xmlParsingResult = xmlParser.Parse(xmlDataString);

            // Act
            List<XmlNamespace> nsMissingInXml = NamespaceHelper.GetMissingNamespacesInXml(xsdParsingResult, xmlParsingResult);
            List<XmlNamespace> nsMissingInXsd = NamespaceHelper.GetMissingNamespacesInXsd(xsdParsingResult, xmlParsingResult);

            // Assert
            Assert.IsTrue(nsMissingInXml.Count == 0);
            Assert.IsTrue(nsMissingInXsd.Count == 0);
        }

        [TestMethod]
        public async Task GetMissingNamespaces_XsdTargetNamespaceIsNotPredefinedElsewereInXsdHeader_ReturnsNoNamespacesMissing()
        {
            // Arrange
            string xsdSchemaString = await TestDataProvider.GetFileContent("Xml.ValidXsd.xml");
            XsdParser xsdParser = new XsdParser(xsdSchemaString);
            XsdParsingResult xsdParsingResult = xsdParser.Parse(xsdSchemaString);

            string xmlDataString = await TestDataProvider.GetFileContent("Xml.ValidXml.xml");
            XmlParser xmlParser = new XmlParser(xmlDataString, xsdParsingResult.SchemaSet);
            XmlParsingResult xmlParsingResult = xmlParser.Parse(xmlDataString);

            // Act
            List<XmlNamespace> nsMissingInXml = NamespaceHelper.GetMissingNamespacesInXml(xsdParsingResult, xmlParsingResult);
            List<XmlNamespace> nsMissingInXsd = NamespaceHelper.GetMissingNamespacesInXsd(xsdParsingResult, xmlParsingResult);

            // Assert
            Assert.IsTrue(nsMissingInXml.Count == 0);
            Assert.IsTrue(nsMissingInXsd.Count == 0);
        }

        [TestMethod]
        public async Task GetMissingNamespaces_NamespacesMissingInXml_ReturnsOneNamespaceMissing()
        {
            // Arrange
            string xsdSchemaString = await TestDataProvider.GetFileContent("Xml.ValidXsdWithNamespace.xml");
            XsdParser xsdParser = new XsdParser(xsdSchemaString);
            XsdParsingResult xsdParsingResult = xsdParser.Parse(xsdSchemaString);

            string xmlDataString = await TestDataProvider.GetFileContent("Xml.ValidXmlWithoutNamespace.xml");
            XmlParser xmlParser = new XmlParser(xmlDataString, xsdParsingResult.SchemaSet);
            XmlParsingResult xmlParsingResult = xmlParser.Parse(xmlDataString);

            // Act
            List<XmlNamespace> nsMissingInXml = NamespaceHelper.GetMissingNamespacesInXml(xsdParsingResult, xmlParsingResult);
            List<XmlNamespace> nsMissingInXsd = NamespaceHelper.GetMissingNamespacesInXsd(xsdParsingResult, xmlParsingResult);

            // Assert
            Assert.IsTrue(nsMissingInXml.Count == 1);
            Assert.IsTrue(nsMissingInXsd.Count == 0);
            Assert.IsTrue(xmlParsingResult.IsValid);
        }
        
        [TestMethod]
        public async Task GetMissingNamespaces_NamespacesMissingInXsd_ReturnsOneNamespaceMissing()
        {
            // Arrange
            string xsdSchemaString = await TestDataProvider.GetFileContent("Xml.ValidXsdWithoutNamespace.xml");
            XsdParser xsdParser = new XsdParser(xsdSchemaString);
            XsdParsingResult xsdParsingResult = xsdParser.Parse(xsdSchemaString);

            string xmlDataString = await TestDataProvider.GetFileContent("Xml.ValidXmlWithNamespace.xml");
            XmlParser xmlParser = new XmlParser(xmlDataString, xsdParsingResult.SchemaSet);
            XmlParsingResult xmlParsingResult = xmlParser.Parse(xmlDataString);

            // Act
            List<XmlNamespace> nsMissingInXml = NamespaceHelper.GetMissingNamespacesInXml(xsdParsingResult, xmlParsingResult);
            List<XmlNamespace> nsMissingInXsd = NamespaceHelper.GetMissingNamespacesInXsd(xsdParsingResult, xmlParsingResult);

            // Assert
            Assert.IsTrue(nsMissingInXml.Count == 0);
            Assert.IsTrue(nsMissingInXsd.Count == 1);
            Assert.IsTrue(xmlParsingResult.IsValid);
        }

        [TestMethod]
        public async Task GetMissingTargetNamespace_XmlIsMissingTargetedNamespace_ReturnsMissingTargetNamespace()
        {
            // Arrange
            string xsdSchemaString = await TestDataProvider.GetFileContent("Xml.ValidXsdOnlyWithTargetNamespace.xml");
            XsdParser xsdParser = new XsdParser(xsdSchemaString);
            XsdParsingResult xsdParsingResult = xsdParser.Parse(xsdSchemaString);

            string xmlDataString = await TestDataProvider.GetFileContent("Xml.ValidXmlWithoutNamespace.xml");
            XmlParser xmlParser = new XmlParser(xmlDataString, xsdParsingResult.SchemaSet);
            XmlParsingResult xmlParsingResult = xmlParser.Parse(xmlDataString);

            // Act
            bool isTargetNamespaceMissing  = NamespaceHelper.DetectMissingTargetNamespaceInXml(xsdParsingResult, xmlParsingResult, out string missingTargetNamespace);

            // Assert
            Assert.IsTrue(isTargetNamespaceMissing);
            Assert.IsTrue(string.Equals(missingTargetNamespace, "https://www.w3schools.com"));
        }
    }
}
