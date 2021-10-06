#nullable enable

using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Editor
{
    /// <summary>
    /// Selects the folding strategy. 'auto' uses the strategies contributed for the current
    /// document, 'indentation' uses the indentation based folding strategy.
    /// Defaults to 'auto'.
    /// </summary>
    [JsonConverter(typeof(FoldingStrategyConverter))]
    public enum FoldingStrategy
    {
        Auto,
        Indentation
    }
}
