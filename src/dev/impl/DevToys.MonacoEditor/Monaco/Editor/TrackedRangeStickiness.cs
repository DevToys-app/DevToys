#nullable enable

namespace DevToys.MonacoEditor.Monaco.Editor
{
    /// <summary>
    /// Describes the behavior of decorations when typing/editing near their edges.
    /// Note: Please do not edit the values, as they very carefully match `DecorationRangeBehavior`
    /// </summary>
    public enum TrackedRangeStickiness
    {
        AlwaysGrowsWhenTypingAtEdges = 0,
        GrowsOnlyWhenTypingAfter = 3,
        GrowsOnlyWhenTypingBefore = 2,
        NeverGrowsWhenTypingAtEdges = 1
    }
}
