#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using DevToys.ViewModels.Tools.XmlValidator.Parsing;

namespace DevToys.ViewModels.Tools.XmlValidator
{
    internal static class NamespaceHelper
    {
        /// <summary>
        /// Identifies namespaces used within the XML data but not defined in the XSD schema.
        /// </summary>
        /// <param name="xsdParsingResult">XSD parsing result</param>
        /// <param name="xmlParsingResult">XML parsing result</param>
        /// <returns>The missing namespaces.</returns>
        public static IEnumerable<XmlNamespace> GetMissingNamespacesInXsd(XsdParsingResult xsdParsingResult, XmlParsingResult xmlParsingResult)
        {
            IEnumerable<XmlNamespace> clearedXdsNamespaces = GetNamespacesFromXsd(xsdParsingResult);
            IEnumerable<XmlNamespace> xmlNamespaces = xmlParsingResult.Namespaces;

            // filter targetNamespace
            if (xsdParsingResult.TargetNamespace is not null)
            {
                xmlNamespaces = xmlNamespaces.Where(xns => !string.Equals(xns.Uri, xsdParsingResult.TargetNamespace, StringComparison.InvariantCultureIgnoreCase));
            }

            return xmlNamespaces.Where(ns => clearedXdsNamespaces.Contains(ns) == false);
        }
        
        /// <summary>
        /// Identifies namespaces defined within the XSD schema but not used in the XML data.
        /// </summary>
        /// <param name="xsdParsingResult">XSD parsing result</param>
        /// <param name="xmlParsingResult">XML parsing result</param>
        /// <returns>The missing namespaces</returns>
        public static IEnumerable<XmlNamespace> GetMissingNamespacesInXml(XsdParsingResult xsdParsingResult, XmlParsingResult xmlParsingResult)
        {
            IEnumerable<XmlNamespace> clearedXdsNamespaces = GetNamespacesFromXsd(xsdParsingResult);
            IEnumerable<XmlNamespace> xmlNamespaces = xmlParsingResult.Namespaces;

            return clearedXdsNamespaces.Where(ns => xmlNamespaces.Contains(ns) == false);
        }

        public static bool DetectMissingTargetNamespaceInXml(XsdParsingResult xsdParsingResult, XmlParsingResult xmlParsingResult, out string? missingTargetNamespaceUri)
        {
            bool isTargetNamespaceDefinedInXml = xmlParsingResult.Namespaces.Select(ns => ns.Uri).Any(uri => string.Equals(uri, xsdParsingResult.TargetNamespace));
            if (isTargetNamespaceDefinedInXml)
            {
                missingTargetNamespaceUri = null;
                return false;
            }

            missingTargetNamespaceUri = xsdParsingResult.TargetNamespace;
            return true;
        }

        private static IEnumerable<XmlNamespace> GetNamespacesFromXsd(XsdParsingResult xsdParsingResult)
        {
            static bool IsDefaultXsdNamespace(XmlNamespace xmlNamespace) => xmlNamespace.Prefix.StartsWith("xs", StringComparison.InvariantCultureIgnoreCase);

            // return namespaces without default XSD namespace (xmlns:xs="http://www.w3.org/2001/XMLSchema")
            return xsdParsingResult.Namespaces.Where(xns => !IsDefaultXsdNamespace(xns));
        }
    }
}
