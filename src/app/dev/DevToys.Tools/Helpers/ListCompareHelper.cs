using DevToys.Tools.Helpers.Core;
using DevToys.Tools.Models;
using Microsoft.Extensions.Logging;

namespace DevToys.Tools.Helpers;

internal static partial class ListCompareHelper
{
    public static ResultInfo<string> Compare(
        string firstList,
        string secondList,
        bool caseSensitive,
        ListComparisonMode comparisonMode,
        ILogger logger)
    {
        try
        {
            ResultInfo<string> compareResult;
            var comparer = new ReadOnlyMemoryEqualityComparer(caseSensitive);

            IEnumerable<ReadOnlyMemory<char>> listA = firstList.AsMemory().ToLines();
            IEnumerable<ReadOnlyMemory<char>> listB = secondList.AsMemory().ToLines();

            IEnumerable<ReadOnlyMemory<char>> listCompared = comparisonMode switch
            {
                ListComparisonMode.AInterB => GetAInterB(listA, listB, comparer),
                ListComparisonMode.AUnionB => GetAUnionB(listA, listB, comparer),
                ListComparisonMode.AOnly => GetAOnly(listA, listB, comparer),
                ListComparisonMode.BOnly => GetBOnly(listA, listB, comparer),
                _ => throw new NotSupportedException(),
            };

            compareResult = new(string.Join(Environment.NewLine, listCompared), true);
            return compareResult;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error");
            return new(ex.Message, false);
        }
    }

    private static IEnumerable<ReadOnlyMemory<char>> GetAInterB(IEnumerable<ReadOnlyMemory<char>> listA, IEnumerable<ReadOnlyMemory<char>> listB, ReadOnlyMemoryEqualityComparer comparer)
    {
        var setB = new HashSet<ReadOnlyMemory<char>>(listB, comparer);
        var uniqueElements = new HashSet<ReadOnlyMemory<char>>(comparer);
        foreach (ReadOnlyMemory<char> item in listA)
        {
            if (setB.Contains(item, comparer))
            {
                if (uniqueElements.Add(item))
                {
                    yield return item;
                }
            }
        }
    }

    private static IEnumerable<ReadOnlyMemory<char>> GetAUnionB(IEnumerable<ReadOnlyMemory<char>> listA, IEnumerable<ReadOnlyMemory<char>> listB, ReadOnlyMemoryEqualityComparer comparer)
    {
        var uniqueElements = new HashSet<ReadOnlyMemory<char>>(comparer);
        foreach (ReadOnlyMemory<char> item in listA.Union(listB))
        {
            if (uniqueElements.Add(item))
            {
                yield return item;
            }
        }
    }

    private static IEnumerable<ReadOnlyMemory<char>> GetAOnly(IEnumerable<ReadOnlyMemory<char>> listA, IEnumerable<ReadOnlyMemory<char>> listB, ReadOnlyMemoryEqualityComparer comparer)
    {
        var setB = new HashSet<ReadOnlyMemory<char>>(listB, comparer);
        var uniqueElements = new HashSet<ReadOnlyMemory<char>>(comparer);
        foreach (ReadOnlyMemory<char> item in listA)
        {
            if (!setB.Contains(item, comparer))
            {
                if (uniqueElements.Add(item))
                {
                    yield return item;
                }
            }
        }
    }

    private static IEnumerable<ReadOnlyMemory<char>> GetBOnly(IEnumerable<ReadOnlyMemory<char>> listA, IEnumerable<ReadOnlyMemory<char>> listB, ReadOnlyMemoryEqualityComparer comparer)
    {
        var setA = new HashSet<ReadOnlyMemory<char>>(listA, comparer);
        var uniqueElements = new HashSet<ReadOnlyMemory<char>>(comparer);
        foreach (ReadOnlyMemory<char> item in listB)
        {
            if (!setA.Contains(item, comparer))
            {
                if (uniqueElements.Add(item))
                {
                    yield return item;
                }
            }
        }
    }
}
