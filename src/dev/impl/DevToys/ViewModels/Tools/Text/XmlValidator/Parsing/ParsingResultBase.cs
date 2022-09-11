#nullable enable

using System.Collections.Generic;

namespace DevToys.ViewModels.Tools.XmlValidator.Parsing
{
    internal record ParsingResultBase
    {
        internal bool IsValid { get; set; }
        internal string? ErrorMessage { get; set; }
        internal IEnumerable<XmlNamespace> Namespaces { get; set; } = new List<XmlNamespace>();
    }
}
