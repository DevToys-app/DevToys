using System.Threading;
using DevToys.Tools.Helpers;

namespace DevToys.UnitTests.Tools.Helpers;

public class ListCompareHelperTests
{
    [Theory]
    [InlineData("A\nBa\nC\nBa", "BA\nC\nD","Ba\nC")]
    [InlineData("A\nb\nC", "B\nC\nD", "b\nC")]
    [InlineData("A\nB\nC", "D\nE\nF", "")]
    [InlineData("A\nB\nC", "", "")]
    [InlineData("", "A\nB\nC", "")]
    internal void CompareAInterBCaseInsensitive(string listA, string listB, string expectedResult)
    {
        ResultInfo<string> compareResult = ListCompareHelper.CompareAsync(
            listA,
            listB,
            false,
            ListComparisonMode.AInterB,
            new MockILogger(),
            CancellationToken.None)
            .Result;

        compareResult.Data.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData("A\nBa\nC\nC", "Ba\nC\nD", "Ba\nC")]
    [InlineData("A\nb\nC", "B\nC\nD", "C")]
    [InlineData("A\nB\nC", "D\nE\nF", "")]
    [InlineData("A\nb\nC", "", "")]
    [InlineData("", "A\nb\nC", "")]
    internal void CompareAInterBCaseSensitive(string listA, string listB, string expectedResult)
    {
        ResultInfo<string> compareResult = ListCompareHelper.CompareAsync(
            listA,
            listB,
            true,
            ListComparisonMode.AInterB,
            new MockILogger(),
            CancellationToken.None)
            .Result;

        compareResult.Data.Should().Be(expectedResult);
    }


    [Theory]
    [InlineData("A\nBa\nC\nc", "Ba\nC\nD", "A")]
    [InlineData("A\nb\nC", "B\nC\nD", "A")]
    [InlineData("A\nB\nC", "D\nE\nF", "A\nB\nC")]
    [InlineData("A\nb\nC", "", "A\nb\nC")]
    [InlineData("", "A\nb\nC", "")]
    internal void CompareAOnlyCaseInSensitive(string listA, string listB, string expectedResult)
    {
        ResultInfo<string> compareResult = ListCompareHelper.CompareAsync(
            listA,
            listB,
            false,
            ListComparisonMode.AOnly,
            new MockILogger(),
            CancellationToken.None)
            .Result;

        compareResult.Data.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData("A\nBa\nC\nc", "Ba\nC\nD", "A\nc")]
    [InlineData("A\nb\nC", "B\nC\nD", "A\nb")]
    [InlineData("A\nB\nC", "D\nE\nF", "A\nB\nC")]
    [InlineData("A\nb\nC", "", "A\nb\nC")]
    [InlineData("", "A\nb\nC", "")]
    internal void CompareAOnlyCaseSensitive(string listA, string listB, string expectedResult)
    {
        ResultInfo<string> compareResult = ListCompareHelper.CompareAsync(
            listA,
            listB,
            true,
            ListComparisonMode.AOnly,
            new MockILogger(),
            CancellationToken.None)
            .Result;

        compareResult.Data.Should().Be(expectedResult);
    }


    [Theory]
    [InlineData("A\nBa\nC\nc", "Ba\nC\nD", "D")]
    [InlineData("A\nb\nC", "B\nC\nD", "D")]
    [InlineData("A\nB\nC", "D\nE\nF", "D\nE\nF")]
    [InlineData("A\nb\nC", "", "")]
    [InlineData("", "A\nb\nC", "A\nb\nC")]
    internal void CompareBOnlyCaseInSensitive(string listA, string listB, string expectedResult)
    {
        ResultInfo<string> compareResult = ListCompareHelper.CompareAsync(
            listA,
            listB,
            false,
            ListComparisonMode.BOnly,
            new MockILogger(),
            CancellationToken.None)
            .Result;

        compareResult.Data.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData("A\nBa\nC\nc", "Ba\nC\nD", "D")]
    [InlineData("A\nb\nC", "B\nC\nD", "B\nD")]
    [InlineData("A\nB\nC", "D\nE\nF", "D\nE\nF")]
    [InlineData("A\nb\nC", "", "")]
    [InlineData("", "A\nb\nC", "A\nb\nC")]
    internal void CompareBOnlyCaseSensitive(string listA, string listB, string expectedResult)
    {
        ResultInfo<string> compareResult = ListCompareHelper.CompareAsync(
            listA,
            listB,
            true,
            ListComparisonMode.BOnly,
            new MockILogger(),
            CancellationToken.None)
            .Result;

        compareResult.Data.Should().Be(expectedResult);
    }
}
