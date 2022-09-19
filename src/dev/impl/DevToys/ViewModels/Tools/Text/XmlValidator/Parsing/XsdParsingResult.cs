#nullable enable

using System.Xml.Schema;

namespace DevToys.ViewModels.Tools.XmlValidator.Parsing
{
    internal record XsdParsingResult : ParsingResultBase
    {
        internal string? TargetNamespace { get; set; }
        internal XmlSchemaSet? SchemaSet { get; set; }
    }
}
