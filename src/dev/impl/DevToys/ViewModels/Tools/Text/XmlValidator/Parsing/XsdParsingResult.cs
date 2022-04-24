using System.Xml.Schema;

namespace DevToys.ViewModels.Tools.XmlValidator.Parsing
{
    public record XsdParsingResult : ParsingResultBase
    {
        public XmlSchemaSet? SchemaSet { get; set; }
    }
}
