namespace DevToys.Tools.Helpers.Core;

internal delegate int ReadOnlyMemoryOfCharComparerDelegate(ReadOnlyMemory<char> x, ReadOnlyMemory<char> y);

internal sealed class CustomReadOnlyMemoryOfCharComparer : IComparer<ReadOnlyMemory<char>>
{
    private readonly ReadOnlyMemoryOfCharComparerDelegate _compareDelegate;

    internal CustomReadOnlyMemoryOfCharComparer(ReadOnlyMemoryOfCharComparerDelegate compareDelegate)
    {
        Guard.IsNotNull(compareDelegate);
        _compareDelegate = compareDelegate;
    }

    public int Compare(ReadOnlyMemory<char> x, ReadOnlyMemory<char> y)
    {
        return _compareDelegate(x, y);
    }
}
