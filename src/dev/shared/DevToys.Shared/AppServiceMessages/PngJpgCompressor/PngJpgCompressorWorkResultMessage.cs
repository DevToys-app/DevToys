using DevToys.Shared.Core.OOP;
using Newtonsoft.Json;

namespace DevToys.Shared.AppServiceMessages.PngJpgCompressor
{
    internal sealed class PngJpgCompressorWorkResultMessage : AppServiceMessageBase
    {
        [JsonProperty]
        internal string? ErrorMessage { get; set; }

        [JsonProperty]
        internal long NewFileSize { get; set; }

        [JsonProperty]
        internal double PercentageSaved { get; set; }

        [JsonProperty]
        internal string TempCompressedFilePath { get; set; } = string.Empty;
    }
}
