#nullable enable

using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco
{
    /// <summary>
    /// A range in the editor. This interface is suitable for serialization.
    /// </summary>
    public interface IRange
    {
        /// <summary>
        /// Line number on which the range starts (starts at 1).
        /// </summary>
        [JsonProperty("startLineNumber")]
        uint StartLineNumber { get; }

        /// <summary>
        /// Column on which the range starts in line `startLineNumber` (starts at 1).
        /// </summary>
        [JsonProperty("startColumn")]
        uint StartColumn { get; }

        /// <summary>
        /// Line number on which the range ends.
        /// </summary>
        [JsonProperty("endLineNumber")]
        uint EndLineNumber { get; }

        /// <summary>
        /// Column on which the range ends in line `endLineNumber`.
        /// </summary>
        [JsonProperty("endColumn")]
        uint EndColumn { get; }
    }
}
