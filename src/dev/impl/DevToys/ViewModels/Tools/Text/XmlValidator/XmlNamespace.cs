using System;

namespace DevToys.ViewModels.Tools.XmlValidator
{
    public record XmlNamespace(string Prefix, string Uri)
    {
        public string Prefix { get; } = Prefix ?? throw new ArgumentNullException(nameof(Prefix));
        public string Uri { get; } = Uri ?? throw new ArgumentNullException(nameof(Uri));
    }
}
