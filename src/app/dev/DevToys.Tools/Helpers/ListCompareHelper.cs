using Markdig.Helpers;
using Microsoft.Extensions.Logging;

namespace DevToys.Tools.Helpers;

internal static partial class ListCompareHelper
{
    public static async ValueTask<ResultInfo<string>> CompareAsync(
        string firstList,
        string secondList,
        bool caseSensitive,
        ListComparisonMode comparisonMode,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        await TaskSchedulerAwaiter.SwitchOffMainThreadAsync(cancellationToken);
        ResultInfo<string> compareResult;
        List<string> listCompared;
        List<string> listA = new List<string>(firstList.Split(Environment.NewLine));
        List<string> listB = new List<string>(secondList.Split(Environment.NewLine));
        StringComparer stringComparer = StringComparer.CurrentCultureIgnoreCase;

        if (caseSensitive)
        {
            stringComparer = StringComparer.CurrentCulture;
        }

        switch (comparisonMode)
        {
            case ListComparisonMode.AInterB:
                listCompared = GetAInterB(listA, listB, stringComparer);
                break;
            case ListComparisonMode.AOnly:
                listCompared = GetAOnly(listA, listB, stringComparer);
                break;
            case ListComparisonMode.BOnly:
                listCompared = GetBOnly(listA, listB, stringComparer);
                break;
            default:
                throw new NotSupportedException();
        }

        compareResult = new(string.Join('\n', listCompared), true);
        try
        {
            return compareResult;
        }
        catch (Exception ex)
        {
            cancellationToken.ThrowIfCancellationRequested();
            logger.LogError(ex, "Unexpected error");
            return new(ex.Message, false);
        }
    }

    private static List<string> GetAInterB(List<string> listA, List<string> listB, StringComparer stringComparer)
    {
        return listA.Intersect(listB, stringComparer).ToList();
    }

    private static List<string> GetAOnly(List<string> listA, List<string> listB, StringComparer stringComparer)
    {
        return listA.Except(listB, stringComparer).ToList();
    }

    private static List<string> GetBOnly(List<string> listA, List<string> listB, StringComparer stringComparer)
    {
        return listB.Except(listA, stringComparer).ToList();
    }

}

