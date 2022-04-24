using System.Xml.Linq;

namespace DevToys.ViewModels.Tools.XmlValidator.Parsing
{
    public record XmlParsingResult : ParsingResultBase
    {
        public XDocument? XmlDocument { get; set; }
    }
}
