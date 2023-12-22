using CommunityToolkit.Common.Deferred;

namespace DevToys.Tools.Tools.Text.SmartCalculator.Core;

internal sealed class ParserAndInterpreterResultUpdatedEventArgs : DeferredCancelEventArgs
{
    internal IReadOnlyList<ParserAndInterpreterResultLine>? ResultPerLines { get; }

    public ParserAndInterpreterResultUpdatedEventArgs(IReadOnlyList<ParserAndInterpreterResultLine>? resultPerLines)
    {
        ResultPerLines = resultPerLines;
    }
}
