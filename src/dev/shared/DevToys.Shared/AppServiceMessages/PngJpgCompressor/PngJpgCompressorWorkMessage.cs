using DevToys.Shared.Core.OOP;
using Newtonsoft.Json;

namespace DevToys.Shared.AppServiceMessages.PngJpgCompressor
{
    internal sealed class PngJpgCompressorWorkMessage : AppServiceMessageBase
    {
        [JsonProperty]
        internal string FilePath { get; set; } = null!;
    }
}
