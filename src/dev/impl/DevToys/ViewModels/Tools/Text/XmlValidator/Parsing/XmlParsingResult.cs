#nullable enable

using System.Xml.Linq;

namespace DevToys.ViewModels.Tools.XmlValidator.Parsing
{
    internal record XmlParsingResult : ParsingResultBase
    {
        internal XDocument? XmlDocument { get; set; }
    }
}
