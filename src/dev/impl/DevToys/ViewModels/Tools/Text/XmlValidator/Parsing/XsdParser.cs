#nullable enable

using System.IO;
using System.Xml;
using System.Xml.Schema;

namespace DevToys.ViewModels.Tools.XmlValidator.Parsing
{
    internal class XsdParser : ParserBase<XsdParsingResult>
    {
        public XsdParser(string source) : base(source)
        {
        }

        protected override XsdParsingResult ParsingOperation(string xsdContent)
        {
            XmlSchema xmlSchema;
            string? targetNamespace;
            try
            {
                using StringReader reader = new(xsdContent);
                xmlSchema = XmlSchema.Read(reader, ValidationErrorCallBack);
                targetNamespace = string.Equals(xmlSchema.TargetNamespace, string.Empty) ? null : xmlSchema.TargetNamespace;
            }
            catch (XmlException ex)
            {
                return GetInvalidParsingResult(Source, ex.LineNumber, ex.LinePosition, ex.Message);
            }
            catch (XmlSchemaException ex)
            {
                return GetInvalidParsingResult(Source, ex.LineNumber, ex.LinePosition, ex.Message);
            }

            if (HasValidationSucceeded is false)
            {
                return new XsdParsingResult {IsValid = false, ErrorMessage = GetValidationMessages()};
            }

            XmlSchemaSet schema = new();
            schema.Add(xmlSchema);
            schema.Compile();

            return new XsdParsingResult {IsValid = true, SchemaSet = schema, TargetNamespace = targetNamespace};
        }

        private static XsdParsingResult GetInvalidParsingResult(string source, int lineNumber, int linePosition,
            string message)
        {
            string formattedErrorMsg = FormatErrorMessage(source, lineNumber, linePosition, message);
            return new XsdParsingResult {IsValid = false, ErrorMessage = formattedErrorMsg};
        }
    }
}
