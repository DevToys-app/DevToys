using DevToys.Tools.Helpers;
using DevToys.Tools.Models;

namespace DevToys.UnitTests.Tools.Helpers;

public class ListCompareHelperTests
{
    [Theory]
    [InlineData("A\nBa\nC\nC", "Ba\nC\nD", "Ba\nC")]
    [InlineData("A\rBa\rC\rC", "Ba\rC\rD", "Ba\nC")]
    [InlineData("A\r\nBa\r\nC\r\nC", "Ba\r\nC\r\nD", "Ba\nC")]
    [InlineData("A\r\nBa\nC\nC", "Ba\nC\r\nD", "Ba\nC")]
    internal void CompareWithMultipleSeparators(string listA, string listB, string expectedResult)
    {
        expectedResult = expectedResult.ReplaceLineEndings();
        ResultInfo<string> compareResult = ListCompareHelper.Compare(
            listA,
            listB,
            true,
            ListComparisonMode.AInterB,
            new MockILogger());

        compareResult.Data.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData("A\nBa\nC\nBa", "BA\nC\nD", "Ba\nC")]
    [InlineData("A\nb\nC", "B\nC\nD", "b\nC")]
    [InlineData("A\nB\nC", "D\nE\nF", "")]
    [InlineData("A\nB\nC", "", "")]
    [InlineData("", "A\nB\nC", "")]
    internal void CompareAInterBCaseInsensitive(string listA, string listB, string expectedResult)
    {
        expectedResult = expectedResult.ReplaceLineEndings();
        ResultInfo<string> compareResult = ListCompareHelper.Compare(
            listA,
            listB,
            false,
            ListComparisonMode.AInterB,
            new MockILogger());

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
        expectedResult = expectedResult.ReplaceLineEndings();
        ResultInfo<string> compareResult = ListCompareHelper.Compare(
            listA,
            listB,
            true,
            ListComparisonMode.AInterB,
            new MockILogger());

        compareResult.Data.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData("A\nBa\nC\nBa", "BA\nC\nD", "A\nBa\nC\nD")]
    [InlineData("A\nb\nC", "B\nC\nD", "A\nb\nC\nD")]
    [InlineData("A\nB\nC", "D\nE\nF", "A\nB\nC\nD\nE\nF")]
    [InlineData("A\nB\nC", "", "A\nB\nC")]
    [InlineData("", "A\nB\nC", "A\nB\nC")]
    internal void CompareAUnionBCaseInsensitive(string listA, string listB, string expectedResult)
    {
        expectedResult = expectedResult.ReplaceLineEndings();
        ResultInfo<string> compareResult = ListCompareHelper.Compare(
            listA,
            listB,
            false,
            ListComparisonMode.AUnionB,
            new MockILogger());

        compareResult.Data.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData("A\nBa\nC\nBa", "BA\nC\nD", "A\nBa\nC\nBA\nD")]
    [InlineData("A\nb\nC", "B\nC\nD", "A\nb\nC\nB\nD")]
    [InlineData("A\nB\nC", "D\nE\nF", "A\nB\nC\nD\nE\nF")]
    [InlineData("A\nB\nC", "", "A\nB\nC")]
    [InlineData("", "A\nB\nC", "A\nB\nC")]
    internal void CompareAUnionBCaseSensitive(string listA, string listB, string expectedResult)
    {
        expectedResult = expectedResult.ReplaceLineEndings();
        ResultInfo<string> compareResult = ListCompareHelper.Compare(
            listA,
            listB,
            true,
            ListComparisonMode.AUnionB,
            new MockILogger());

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
        expectedResult = expectedResult.ReplaceLineEndings();
        ResultInfo<string> compareResult = ListCompareHelper.Compare(
            listA,
            listB,
            false,
            ListComparisonMode.AOnly,
            new MockILogger());

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
        expectedResult = expectedResult.ReplaceLineEndings();
        ResultInfo<string> compareResult = ListCompareHelper.Compare(
            listA,
            listB,
            true,
            ListComparisonMode.AOnly,
            new MockILogger());

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
        expectedResult = expectedResult.ReplaceLineEndings();
        ResultInfo<string> compareResult = ListCompareHelper.Compare(
            listA,
            listB,
            false,
            ListComparisonMode.BOnly,
            new MockILogger());

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
        expectedResult = expectedResult.ReplaceLineEndings();
        ResultInfo<string> compareResult = ListCompareHelper.Compare(
            listA,
            listB,
            true,
            ListComparisonMode.BOnly,
            new MockILogger());

        compareResult.Data.Should().Be(expectedResult);
    }
}
