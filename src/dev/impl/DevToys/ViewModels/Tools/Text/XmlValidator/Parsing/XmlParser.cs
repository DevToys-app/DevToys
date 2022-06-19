using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace DevToys.ViewModels.Tools.XmlValidator.Parsing
{
    internal class XmlParser : ParserBase<XmlParsingResult>
    {
        private readonly XmlSchemaSet _xsdSchema;

        internal XmlParser(string source, XmlSchemaSet xsdSchema) : base(source)
        {
            _xsdSchema = xsdSchema;
        }

        internal override XmlParsingResult Parse(string content)
        {
            XDocument xmlFile = new();
            try
            {
                xmlFile = XDocument.Parse(content, LoadOptions.SetLineInfo);
            }
            catch (XmlException ex)
            {
                string formattedErrorMsg = FormatErrorMessage(Source, ex.LineNumber, ex.LinePosition, ex.Message);
                return new XmlParsingResult {IsValid = false, ErrorMessage = formattedErrorMsg};
            }


            xmlFile.Validate(_xsdSchema, ValidationErrorCallBack);
            if (HasValidationSucceeded is false)
            {
                return new XmlParsingResult {IsValid = false, ErrorMessage = GetValidationMessages()};
            }

            return new XmlParsingResult {IsValid = true, XmlDocument = xmlFile};
        }
    }
}
