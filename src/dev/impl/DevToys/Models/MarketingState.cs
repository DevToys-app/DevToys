#nullable enable

using System;
using Newtonsoft.Json;

namespace DevToys.Models
{
    internal sealed class MarketingState
    {
        [JsonProperty]
        internal DateTime LastProblemEncounteredDate { get; set; }

        [JsonProperty]
        internal int StartSinceLastProblemEncounteredCount { get; set; }

        [JsonProperty]
        internal int SmartDetectionCount { get; set; }

        [JsonProperty]
        internal DateTime LastUpdateDate { get; set; }

        [JsonProperty]
        internal int ToolSuccessfulyWorkedCount { get; set; }

        [JsonProperty]
        internal DateTime LastAppRatingOfferDate { get; set; }

        [JsonProperty]
        internal int AppRatingOfferCount { get; set; }

        [JsonProperty]
        internal bool AppGotRated { get; set; }
    }
}
