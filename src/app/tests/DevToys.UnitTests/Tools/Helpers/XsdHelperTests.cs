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
        string xsdSchemaString = await TestDataProvider.GetFileContent("DevToys.UnitTests.Tools.TestData.XmlValidator.ValidXsd.xml");
        string xmlDataString = await TestDataProvider.GetFileContent("DevToys.UnitTests.Tools.TestData.XmlValidator.InvalidXml.xml");

        ResultInfo<string, XmlValidatorResultSeverity> result = XsdHelper.ValidateXmlAgainstXsd(xsdSchemaString, xmlDataString, new MockILogger(), CancellationToken.None);

        result.Severity.Should().Be(XmlValidatorResultSeverity.Error);
    }

    [Fact]
    public async Task ValidateXml_ValidXmlSchema()
    {
        string xsdSchemaString = await TestDataProvider.GetFileContent("DevToys.UnitTests.Tools.TestData.XmlValidator.ValidXsd.xml");
        string xmlDataString = await TestDataProvider.GetFileContent("DevToys.UnitTests.Tools.TestData.XmlValidator.ValidXml.xml");

        ResultInfo<string, XmlValidatorResultSeverity> result = XsdHelper.ValidateXmlAgainstXsd(xsdSchemaString, xmlDataString, new MockILogger(), CancellationToken.None);

        result.Severity.Should().Be(XmlValidatorResultSeverity.Success);
    }

    [Fact]
    public async Task ValidateXml_InvalidXsdSchema()
    {
        string xsdSchemaString = await TestDataProvider.GetFileContent("DevToys.UnitTests.Tools.TestData.XmlValidator.InvalidXsd.xml");
        string xmlDataString = await TestDataProvider.GetFileContent("DevToys.UnitTests.Tools.TestData.XmlValidator.ValidXml.xml");

        ResultInfo<string, XmlValidatorResultSeverity> result = XsdHelper.ValidateXmlAgainstXsd(xsdSchemaString, xmlDataString, new MockILogger(), CancellationToken.None);

        result.Severity.Should().Be(XmlValidatorResultSeverity.Error);
    }

    [Fact]
    public async Task ValidateXml_NoNamespacesMissing()
    {
        string xsdSchemaString = await TestDataProvider.GetFileContent("DevToys.UnitTests.Tools.TestData.XmlValidator.ValidXsdWithNamespace.xml");
        string xmlDataString = await TestDataProvider.GetFileContent("DevToys.UnitTests.Tools.TestData.XmlValidator.ValidXmlWithNamespace.xml");

        ResultInfo<string, XmlValidatorResultSeverity> result = XsdHelper.ValidateXmlAgainstXsd(xsdSchemaString, xmlDataString, new MockILogger(), CancellationToken.None);

        result.Severity.Should().Be(XmlValidatorResultSeverity.Success);
    }

    [Fact]
    public async Task ValidateXml_XsdTargetNamespaceIsNotPredefinedElsewhereInXsdHeader()
    {
        string xsdSchemaString = await TestDataProvider.GetFileContent("DevToys.UnitTests.Tools.TestData.XmlValidator.ValidXsd.xml");
        string xmlDataString = await TestDataProvider.GetFileContent("DevToys.UnitTests.Tools.TestData.XmlValidator.ValidXml.xml");

        ResultInfo<string, XmlValidatorResultSeverity> result = XsdHelper.ValidateXmlAgainstXsd(xsdSchemaString, xmlDataString, new MockILogger(), CancellationToken.None);

        result.Severity.Should().Be(XmlValidatorResultSeverity.Success);
    }

    [Fact]
    public async Task ValidateXml_NamespacesMissingInXml()
    {
        string xsdSchemaString = await TestDataProvider.GetFileContent("DevToys.UnitTests.Tools.TestData.XmlValidator.ValidXsdWithNamespace.xml");
        string xmlDataString = await TestDataProvider.GetFileContent("DevToys.UnitTests.Tools.TestData.XmlValidator.ValidXmlWithoutNamespace.xml");

        ResultInfo<string, XmlValidatorResultSeverity> result = XsdHelper.ValidateXmlAgainstXsd(xsdSchemaString, xmlDataString, new MockILogger(), CancellationToken.None);

        result.Severity.Should().Be(XmlValidatorResultSeverity.Warning);
    }

    [Fact]
    public async Task ValidateXml_NamespacesMissingInXsd()
    {
        string xsdSchemaString = await TestDataProvider.GetFileContent("DevToys.UnitTests.Tools.TestData.XmlValidator.ValidXsdWithoutNamespace.xml");
        string xmlDataString = await TestDataProvider.GetFileContent("DevToys.UnitTests.Tools.TestData.XmlValidator.ValidXmlWithNamespace.xml");

        ResultInfo<string, XmlValidatorResultSeverity> result = XsdHelper.ValidateXmlAgainstXsd(xsdSchemaString, xmlDataString, new MockILogger(), CancellationToken.None);

        result.Severity.Should().Be(XmlValidatorResultSeverity.Warning);
    }

    [Fact]
    public async Task ValidateXml_XmlIsMissingTargetedNamespace()
    {
        string xsdSchemaString = await TestDataProvider.GetFileContent("DevToys.UnitTests.Tools.TestData.XmlValidator.ValidXsdOnlyWithTargetNamespace.xml");
        string xmlDataString = await TestDataProvider.GetFileContent("DevToys.UnitTests.Tools.TestData.XmlValidator.ValidXmlWithoutNamespace.xml");

        ResultInfo<string, XmlValidatorResultSeverity> result = XsdHelper.ValidateXmlAgainstXsd(xsdSchemaString, xmlDataString, new MockILogger(), CancellationToken.None);

        result.Severity.Should().Be(XmlValidatorResultSeverity.Warning);
    }
}
