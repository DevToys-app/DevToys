#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.XPath;

namespace DevToys.ViewModels.Tools.XmlValidator.Parsing
{
    internal abstract class ParserBase<TResult> where TResult : ParsingResultBase
    {
        protected readonly string Source;
        private readonly List<string> _validationWarnings;
        private readonly List<string> _validationErrors;

        protected bool HasValidationSucceeded { get; private set; }

        protected ParserBase(string source)
        {
            HasValidationSucceeded = true;

            Source = source;
            _validationWarnings = new List<string>();
            _validationErrors = new List<string>();
        }

        internal TResult Parse(string content)
        {
            TResult result = ParsingOperation(content);

            XDocument document;
            try
            {
                document = XDocument.Parse(content);
            }
            catch
            {
                document = new XDocument();
            }

            result.Namespaces = GetAllNamespaces(document);
            return result;
        }

        protected abstract TResult ParsingOperation(string content);
        
        internal void ValidationErrorCallBack(object sender, ValidationEventArgs e)
        {
            string information =
                FormatErrorMessage(Source, e.Exception.LineNumber, e.Exception.LinePosition, e.Message);

            switch (e.Severity)
            {
                case XmlSeverityType.Warning:
                    _validationWarnings.Add("Warning " + information);
                    break;
                case XmlSeverityType.Error:
                    _validationErrors.Add("Error " + information);
                    HasValidationSucceeded = false;
                    break;
            }
        }

        internal static string FormatErrorMessage(string source, int lineNumber, int linePosition, string message)
        {
            string information =
                $"[Source: {source}, line: {lineNumber}, position: {linePosition}]: {message}";
            return information;
        }

        protected string GetValidationMessages()
        {
            string messages = string.Join(Environment.NewLine, _validationErrors);
            messages += string.Join(Environment.NewLine, _validationWarnings);
            return messages;
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

            IDictionary<string, string> namespaces =  navigator.GetNamespacesInScope(XmlNamespaceScope.ExcludeXml) ?? new Dictionary<string, string>();
            return namespaces.Select(pair => new XmlNamespace(pair.Key, pair.Value)).ToList();
        }
    }
}
