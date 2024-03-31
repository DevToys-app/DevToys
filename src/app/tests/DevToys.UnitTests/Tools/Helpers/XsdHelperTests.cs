using System.Threading;
using System.Threading.Tasks;
using DevToys.Tools.Helpers;
using DevToys.Tools.Models;
using DevToys.UnitTests.Mocks;

namespace DevToys.UnitTests.Tools.Helpers;

public class XsdHelperTests
{
    [Fact]
    public async Task ValidateXml_InvalidXmlFile()
    {
        string xsdSchemaString = await TestDataProvider.GetEmbeddedFileContent("DevToys.UnitTests.Tools.TestData.XMLTester.ValidXsd.xml");
        string xmlDataString = await TestDataProvider.GetEmbeddedFileContent("DevToys.UnitTests.Tools.TestData.XMLTester.InvalidXml.xml");

        ResultInfo<string> result = XsdHelper.ValidateXmlAgainstXsd(xsdSchemaString, xmlDataString, new MockILogger(), CancellationToken.None);

        result.Severity.Should().Be(ResultInfoSeverity.Error);
    }

    [Fact]
    public async Task ValidateXml_ValidXmlSchema()
    {
        string xsdSchemaString = await TestDataProvider.GetEmbeddedFileContent("DevToys.UnitTests.Tools.TestData.XMLTester.ValidXsd.xml");
        string xmlDataString = await TestDataProvider.GetEmbeddedFileContent("DevToys.UnitTests.Tools.TestData.XMLTester.ValidXml.xml");

        ResultInfo<string> result = XsdHelper.ValidateXmlAgainstXsd(xsdSchemaString, xmlDataString, new MockILogger(), CancellationToken.None);

        result.Severity.Should().Be(ResultInfoSeverity.Success);
    }

    [Fact]
    public async Task ValidateXml_InvalidXsdSchema()
    {
        string xsdSchemaString = await TestDataProvider.GetEmbeddedFileContent("DevToys.UnitTests.Tools.TestData.XMLTester.InvalidXsd.xml");
        string xmlDataString = await TestDataProvider.GetEmbeddedFileContent("DevToys.UnitTests.Tools.TestData.XMLTester.ValidXml.xml");

        ResultInfo<string> result = XsdHelper.ValidateXmlAgainstXsd(xsdSchemaString, xmlDataString, new MockILogger(), CancellationToken.None);

        result.Severity.Should().Be(ResultInfoSeverity.Error);
    }

    [Fact]
    public async Task ValidateXml_NoNamespacesMissing()
    {
        string xsdSchemaString = await TestDataProvider.GetEmbeddedFileContent("DevToys.UnitTests.Tools.TestData.XMLTester.ValidXsdWithNamespace.xml");
        string xmlDataString = await TestDataProvider.GetEmbeddedFileContent("DevToys.UnitTests.Tools.TestData.XMLTester.ValidXmlWithNamespace.xml");

        ResultInfo<string> result = XsdHelper.ValidateXmlAgainstXsd(xsdSchemaString, xmlDataString, new MockILogger(), CancellationToken.None);

        result.Severity.Should().Be(ResultInfoSeverity.Success);
    }

    [Fact]
    public async Task ValidateXml_XsdTargetNamespaceIsNotPredefinedElsewhereInXsdHeader()
    {
        string xsdSchemaString = await TestDataProvider.GetEmbeddedFileContent("DevToys.UnitTests.Tools.TestData.XMLTester.ValidXsd.xml");
        string xmlDataString = await TestDataProvider.GetEmbeddedFileContent("DevToys.UnitTests.Tools.TestData.XMLTester.ValidXml.xml");

        ResultInfo<string> result = XsdHelper.ValidateXmlAgainstXsd(xsdSchemaString, xmlDataString, new MockILogger(), CancellationToken.None);

        result.Severity.Should().Be(ResultInfoSeverity.Success);
    }

    [Fact]
    public async Task ValidateXml_NamespacesMissingInXml()
    {
        string xsdSchemaString = await TestDataProvider.GetEmbeddedFileContent("DevToys.UnitTests.Tools.TestData.XMLTester.ValidXsdWithNamespace.xml");
        string xmlDataString = await TestDataProvider.GetEmbeddedFileContent("DevToys.UnitTests.Tools.TestData.XMLTester.ValidXmlWithoutNamespace.xml");

        ResultInfo<string> result = XsdHelper.ValidateXmlAgainstXsd(xsdSchemaString, xmlDataString, new MockILogger(), CancellationToken.None);

        result.Severity.Should().Be(ResultInfoSeverity.Warning);
    }

    [Fact]
    public async Task ValidateXml_NamespacesMissingInXsd()
    {
        string xsdSchemaString = await TestDataProvider.GetEmbeddedFileContent("DevToys.UnitTests.Tools.TestData.XMLTester.ValidXsdWithoutNamespace.xml");
        string xmlDataString = await TestDataProvider.GetEmbeddedFileContent("DevToys.UnitTests.Tools.TestData.XMLTester.ValidXmlWithNamespace.xml");

        ResultInfo<string> result = XsdHelper.ValidateXmlAgainstXsd(xsdSchemaString, xmlDataString, new MockILogger(), CancellationToken.None);

        result.Severity.Should().Be(ResultInfoSeverity.Warning);
    }

    [Fact]
    public async Task ValidateXml_XmlIsMissingTargetedNamespace()
    {
        string xsdSchemaString = await TestDataProvider.GetEmbeddedFileContent("DevToys.UnitTests.Tools.TestData.XMLTester.ValidXsdOnlyWithTargetNamespace.xml");
        string xmlDataString = await TestDataProvider.GetEmbeddedFileContent("DevToys.UnitTests.Tools.TestData.XMLTester.ValidXmlWithoutNamespace.xml");

        ResultInfo<string> result = XsdHelper.ValidateXmlAgainstXsd(xsdSchemaString, xmlDataString, new MockILogger(), CancellationToken.None);

        result.Severity.Should().Be(ResultInfoSeverity.Warning);
    }
}
