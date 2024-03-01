using DevToys.Tools.Models;
using Markdig.Helpers;
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
            string[] separators = { "\r\n", "\n", "\r" };
            ResultInfo<string> compareResult;
            var listA = new List<string>(firstList.Split(separators, StringSplitOptions.RemoveEmptyEntries));
            var listB = new List<string>(secondList.Split(separators, StringSplitOptions.RemoveEmptyEntries));
            StringComparer stringComparer = StringComparer.CurrentCultureIgnoreCase;

            if (caseSensitive)
            {
                stringComparer = StringComparer.CurrentCulture;
            }

            IEnumerable<string> listCompared = comparisonMode switch
            {
                ListComparisonMode.AInterB => GetAInterB(listA, listB, stringComparer),
                ListComparisonMode.AOnly => GetAOnly(listA, listB, stringComparer),
                ListComparisonMode.BOnly => GetBOnly(listA, listB, stringComparer),
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

    private static IEnumerable<string> GetAInterB(List<string> listA, List<string> listB, StringComparer stringComparer)
    {
        return listA.Intersect(listB, stringComparer);
    }

    private static IEnumerable<string> GetAOnly(List<string> listA, List<string> listB, StringComparer stringComparer)
    {
        return listA.Except(listB, stringComparer);
    }

    private static IEnumerable<string> GetBOnly(List<string> listA, List<string> listB, StringComparer stringComparer)
    {
        return listB.Except(listA, stringComparer);
    }
}

