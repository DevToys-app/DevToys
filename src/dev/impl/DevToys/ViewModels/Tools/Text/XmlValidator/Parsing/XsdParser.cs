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

        internal override XsdParsingResult Parse(string xsdContent)
        {
            XmlSchema xmlSchema;
            try
            {
                xmlSchema = XmlSchema.Read(new XmlTextReader(new StringReader(xsdContent)), ValidationErrorCallBack);
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
            return new XsdParsingResult {IsValid = true, SchemaSet = schema};
        }

        private static XsdParsingResult GetInvalidParsingResult(string source, int lineNumber, int linePosition,
            string message)
        {
            string formattedErrorMsg = FormatErrorMessage(source, lineNumber, linePosition, message);
            return new XsdParsingResult {IsValid = false, ErrorMessage = formattedErrorMsg};
        }
    }
}
