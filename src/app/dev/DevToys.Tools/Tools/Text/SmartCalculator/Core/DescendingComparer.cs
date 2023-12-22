namespace DevToys.Tools.Tools.Text.SmartCalculator.Core;

internal sealed class DescendingComparer<T> : IComparer<T?> where T : IComparable<T>
{
    public int Compare(T? x, T? y)
    {
        return y!.CompareTo(x!);
    }
}
