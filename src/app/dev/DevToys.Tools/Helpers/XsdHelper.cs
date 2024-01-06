using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.XPath;
using DevToys.Tools.Models;
using DevToys.Tools.Tools.Testers.XMLValidator;
using Microsoft.Extensions.Logging;

namespace DevToys.Tools.Helpers;

internal static class XsdHelper
{
    /// <summary>
    /// Detects whether the given string is a valid XSD or not.
    /// </summary>
    internal static bool IsValid(string? input, ILogger logger)
    {
        try
        {
            using StringReader reader = new(input!);
            var xmlSchema = XmlSchema.Read(reader, null);
            return xmlSchema is not null;
        }
        catch (Exception ex) when (ex is XmlException || ex is XmlSchemaException)
        {
            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Invalid data detected.");
            return false;
        }
    }

    internal static ResultInfo<string, XmlValidatorResultSeverity> ValidateXmlAgainstXsd(string xsd, string xml, ILogger logger, CancellationToken cancellationToken)
    {
        Guard.IsNotNullOrWhiteSpace(xsd);
        Guard.IsNotNullOrWhiteSpace(xml);

        try
        {
            XmlValidatorResultSeverity hasSucceeded = XmlValidatorResultSeverity.Success;
            var errors = new StringBuilder();
            void ValidationErrorCallBack(object? sender, ValidationEventArgs e)
            {
                cancellationToken.ThrowIfCancellationRequested();
                hasSucceeded = e.Severity == XmlSeverityType.Warning ? XmlValidatorResultSeverity.Warning : XmlValidatorResultSeverity.Error;
                errors.AppendLine(e.Message);
            }

            // Load XSD document.
            using StringReader reader = new(xsd!);
            var xmlSchema = XmlSchema.Read(reader, ValidationErrorCallBack);

            if (xmlSchema is not null)
            {
                XmlSchemaSet xmlSchemaSet = new();
                xmlSchemaSet.Add(xmlSchema);

                // Load XML document.
                XDocument xmlDocument;
                try
                {
                    xmlDocument = XDocument.Parse(xml);
                }
                catch (XmlException ex)
                {
                    return new ResultInfo<string, XmlValidatorResultSeverity>($"XML: {ex.Message}", XmlValidatorResultSeverity.Error);
                }

                cancellationToken.ThrowIfCancellationRequested();

                // Validate XML against the XSD.
                xmlDocument.Validate(xmlSchemaSet, ValidationErrorCallBack);

                if (hasSucceeded == XmlValidatorResultSeverity.Success)
                {
                    // XML will always be valid if it doesn't define a namespace. Let's verify that namespaces aren't missing.
                    var xsdDocument = XDocument.Parse(xsd);
                    IEnumerable<XmlNamespace> xsdNamespaces = GetAllNamespaces(xsdDocument);
                    IEnumerable<XmlNamespace> xmlNamespaces = GetAllNamespaces(xmlDocument);

                    cancellationToken.ThrowIfCancellationRequested();

                    IEnumerable<XmlNamespace> namespacesMissingInXsd
                        = GetMissingNamespacesInXsd(xsdNamespaces, xmlSchema.TargetNamespace, xmlNamespaces);

                    bool areAllNamespacesDefinedInXsd = !namespacesMissingInXsd.Any();
                    if (!areAllNamespacesDefinedInXsd)
                    {
                        string missingNamespacesFormatted = FormatNamespaces(namespacesMissingInXsd);
                        return new ResultInfo<string, XmlValidatorResultSeverity>(string.Format(XMLValidator.XsdNamespacesInconsistentMsg, missingNamespacesFormatted), XmlValidatorResultSeverity.Warning);
                    }

                    bool isTargetNamespaceReferenceMissingInXml
                        = DetectMissingTargetNamespaceInXml(xmlSchema.TargetNamespace, xmlNamespaces, out string? missingTargetNamespaceUri);
                    if (isTargetNamespaceReferenceMissingInXml)
                    {
                        return new ResultInfo<string, XmlValidatorResultSeverity>(string.Format(XMLValidator.TargetNamespaceNotDefinedInXml, missingTargetNamespaceUri), XmlValidatorResultSeverity.Warning);
                    }

                    IEnumerable<XmlNamespace> namespacesMissingInXml
                        = GetMissingNamespacesInXml(xsdNamespaces, xmlNamespaces);

                    bool areAllNamespacesDefinedInXml = !namespacesMissingInXml.Any();
                    if (!areAllNamespacesDefinedInXml)
                    {
                        string missingNamespacesFormatted = FormatNamespaces(namespacesMissingInXml);
                        return new ResultInfo<string, XmlValidatorResultSeverity>(string.Format(XMLValidator.XmlNamespacesInconsistentMsg, missingNamespacesFormatted), XmlValidatorResultSeverity.Warning);
                    }
                }
            }

            return new ResultInfo<string, XmlValidatorResultSeverity>(errors.ToString(), hasSucceeded);
        }
        catch (Exception ex) when (ex is XmlException || ex is XmlSchemaException)
        {
            return new ResultInfo<string, XmlValidatorResultSeverity>($"XSD: {ex.Message}", XmlValidatorResultSeverity.Error);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while validation XML against XSD.");
        }

        return new ResultInfo<string, XmlValidatorResultSeverity>(string.Empty, XmlValidatorResultSeverity.Error);
    }

    /// <summary>
    /// Identifies namespaces used within the XML data but not defined in the XSD schema.
    /// </summary>
    /// <param name="xsdParsingResult">XSD parsing result</param>
    /// <param name="xmlParsingResult">XML parsing result</param>
    /// <returns>The missing namespaces.</returns>
    private static IEnumerable<XmlNamespace> GetMissingNamespacesInXsd(
        IEnumerable<XmlNamespace> xsdNamespaces,
        string? targetNamespace,
        IEnumerable<XmlNamespace> xmlNamespaces)
    {
        IEnumerable<XmlNamespace> clearedXdsNamespaces = GetNamespacesFromXsd(xsdNamespaces);

        // filter targetNamespace
        if (targetNamespace is not null)
        {
            xmlNamespaces = xmlNamespaces.Where(xns => !string.Equals(xns.Uri, targetNamespace, StringComparison.InvariantCultureIgnoreCase));
        }

        return xmlNamespaces.Where(ns => clearedXdsNamespaces.Contains(ns) == false);
    }

    /// <summary>
    /// Identifies namespaces defined within the XSD schema but not used in the XML data.
    /// </summary>
    /// <param name="xsdParsingResult">XSD parsing result</param>
    /// <param name="xmlParsingResult">XML parsing result</param>
    /// <returns>The missing namespaces</returns>
    private static IEnumerable<XmlNamespace> GetMissingNamespacesInXml(IEnumerable<XmlNamespace> xsdNamespaces, IEnumerable<XmlNamespace> xmlNamespaces)
    {
        IEnumerable<XmlNamespace> clearedXdsNamespaces = GetNamespacesFromXsd(xsdNamespaces);

        return clearedXdsNamespaces.Where(ns => xmlNamespaces.Contains(ns) == false);
    }

    private static bool DetectMissingTargetNamespaceInXml(string? targetNamespace, IEnumerable<XmlNamespace> xmlNamespaces, out string? missingTargetNamespaceUri)
    {
        bool isTargetNamespaceDefinedInXml = xmlNamespaces.Select(ns => ns.Uri).Any(uri => string.Equals(uri, targetNamespace));
        if (isTargetNamespaceDefinedInXml)
        {
            missingTargetNamespaceUri = null;
            return false;
        }

        missingTargetNamespaceUri = targetNamespace;
        return true;
    }

    private static IEnumerable<XmlNamespace> GetNamespacesFromXsd(IEnumerable<XmlNamespace> xsdNamespaces)
    {
        static bool IsDefaultXsdNamespace(XmlNamespace xmlNamespace) => xmlNamespace.Prefix.StartsWith("xs", StringComparison.InvariantCultureIgnoreCase);

        // return namespaces without default XSD namespace (xmlns:xs="http://www.w3.org/2001/XMLSchema")
        return xsdNamespaces.Where(xns => !IsDefaultXsdNamespace(xns));
    }

    /// <summary>
    /// Extracts all <c>xmlns</c> namespaces from a given <paramref name="xmlDocument"/>.
    /// </summary>
    /// <param name="xmlDocument">XML document containing  namespaces.</param>
    /// <returns></returns>
    private static IEnumerable<XmlNamespace> GetAllNamespaces(XDocument xmlDocument)
    {
        XPathNavigator? navigator = xmlDocument.CreateNavigator();
        navigator.MoveToFollowing(XPathNodeType.Element);

        IDictionary<string, string> namespaces = navigator.GetNamespacesInScope(XmlNamespaceScope.ExcludeXml) ?? new Dictionary<string, string>();
        return namespaces.Select(pair => new XmlNamespace(pair.Key, pair.Value)).ToList();
    }

    private static string FormatNamespaces(IEnumerable<XmlNamespace> namespaces)
    {
        List<string> missingNamespaces = new();
        foreach (XmlNamespace ns in namespaces)
        {
            string formattedPrefix;
            if (string.Equals(ns.Prefix, string.Empty))
            {
                formattedPrefix = "xmlns";
            }
            else
            {
                formattedPrefix = $"xmlns:{ns.Prefix}";
            }

            missingNamespaces.Add(formattedPrefix + $"=\"{ns.Uri}\"");
        }

        return string.Join(", ", missingNamespaces);
    }
}
