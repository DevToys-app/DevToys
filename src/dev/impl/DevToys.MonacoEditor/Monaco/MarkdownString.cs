#nullable enable

using System.Collections.Generic;
using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco
{
    public sealed class MarkdownString
    {
        [JsonProperty("isTrusted")]
        public bool IsTrusted { get; set; }

        [JsonProperty("supportThemeIcons", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SupportThemeIcons { get; set; }

        [JsonProperty("uris", NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, Uri>? Uris { get; set; }

        [JsonProperty("value")]
        public string? Value { get; set; }

        public MarkdownString(string svalue)
            : this(svalue, false)
        {
        }

        public MarkdownString(string svalue, bool isTrusted)
        {
            Value = svalue;
            IsTrusted = isTrusted;
        }
    }
}
