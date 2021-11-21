#nullable enable

using Newtonsoft.Json;

namespace DevToys.Shared.Core.OOP
{
    public sealed class AppServiceProgressMessage : AppServiceMessageBase
    {
        [JsonProperty]
        internal int ProgressPercentage { get; set; }

        [JsonProperty]
        internal string? Message { get; set; }
    }
}
