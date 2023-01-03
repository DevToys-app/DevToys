using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Languages;

public interface WorkspaceFileEditOptions
{
    [JsonProperty("copy", NullValueHandling = NullValueHandling.Ignore)]
    bool Copy { get; set; }

    [JsonProperty("folder", NullValueHandling = NullValueHandling.Ignore)]
    bool Folder { get; set; }

    [JsonProperty("ignoreIfExists", NullValueHandling = NullValueHandling.Ignore)]
    bool IgnoreIfExists { get; set; }

    [JsonProperty("ignoreIfNotExists", NullValueHandling = NullValueHandling.Ignore)]
    bool IgnoreIfNotExists { get; set; }

    [JsonProperty("maxSize", NullValueHandling = NullValueHandling.Ignore)]
    double MaxSize { get; set; }

    [JsonProperty("overwrite", NullValueHandling = NullValueHandling.Ignore)]
    bool Overwrite { get; set; }

    [JsonProperty("recursive", NullValueHandling = NullValueHandling.Ignore)]
    bool Recursive { get; set; }

    [JsonProperty("skipTrashBin", NullValueHandling = NullValueHandling.Ignore)]
    bool SkipTrashBin { get; set; }
}