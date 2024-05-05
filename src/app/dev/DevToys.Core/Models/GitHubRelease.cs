using System.Text.Json.Serialization;

namespace DevToys.Core.Models;

[DebuggerDisplay($"{{{nameof(Name)}}}")]
internal sealed class GitHubRelease
{
    [JsonPropertyName("prerelease")]
    public bool PreRelease { get; set; }

    [JsonPropertyName("draft")]
    public bool Draft { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}
