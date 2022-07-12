#nullable enable

namespace DevToys.ViewModels.Tools.XmlValidator.Parsing
{
    internal record ParsingResultBase
    {
        internal bool IsValid { get; set; }
        internal string? ErrorMessage { get; set; }
    }
}
