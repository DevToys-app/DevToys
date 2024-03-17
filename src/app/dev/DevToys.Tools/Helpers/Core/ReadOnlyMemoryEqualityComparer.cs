namespace DevToys.Tools.Helpers.Core;
internal class ReadOnlyMemoryEqualityComparer : IEqualityComparer<ReadOnlyMemory<char>>
{
    private readonly StringComparison _comparisonType;

    public ReadOnlyMemoryEqualityComparer(bool caseSensitive)
    {
        _comparisonType = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
    }

    public bool Equals(ReadOnlyMemory<char> x, ReadOnlyMemory<char> y)
    {
        return x.Span.Equals(y.Span, _comparisonType);
    }

    public int GetHashCode(ReadOnlyMemory<char> obj)
    {
        return obj.ToString().GetHashCode(_comparisonType);
    }
}
