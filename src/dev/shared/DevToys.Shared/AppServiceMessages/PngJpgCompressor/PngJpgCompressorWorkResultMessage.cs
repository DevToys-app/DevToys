using DevToys.Shared.Core.OOP;
using Newtonsoft.Json;

namespace DevToys.Shared.AppServiceMessages.PngJpgCompressor
{
    internal sealed class PngJpgCompressorWorkResultMessage : AppServiceMessageBase
    {
        [JsonProperty]
        internal string? ErrorMessage { get; set; }
    }
}
