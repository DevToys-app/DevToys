namespace DevToys.ViewModels.Tools.XmlValidator.Parsing
{
    public record ParsingResultBase
    {
        public bool IsValid { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
