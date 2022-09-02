#nullable enable

namespace DevToys.Models
{
    public record ValidationBase
    {
        internal bool IsValid { get; set; }

        internal string? ErrorMessage { get; set; }
    }
}
