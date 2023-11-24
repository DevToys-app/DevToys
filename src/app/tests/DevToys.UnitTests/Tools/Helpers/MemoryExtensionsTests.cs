#nullable enable

using DevToys.Tools.Helpers;
using DevToys.Tools.Models;

namespace DevToys.UnitTests.Tools.Helpers;
public class MemoryExtensionsTests
{
    [Theory]
    [InlineData(null, false, EndOfLineSequence.Unknown)]
    [InlineData("", false, EndOfLineSequence.Unknown)]
    [InlineData(" ", false, EndOfLineSequence.Unknown)]
    [InlineData("\r", true, EndOfLineSequence.CarriageReturn)]
    [InlineData("\n", true, EndOfLineSequence.LineFeed)]
    [InlineData("\r\n", true, EndOfLineSequence.CarriageReturnLineFeed)]
    [InlineData("\r ", true, EndOfLineSequence.CarriageReturn)]
    [InlineData("\n\r", true, EndOfLineSequence.LineFeed)]
    [InlineData("\n\r ", true, EndOfLineSequence.LineFeed)]
    internal void IsLineBreak(string? input, bool expectedResult, EndOfLineSequence expectedLikeBreakType)
    {
        bool result = input.AsSpan().IsLineBreak(0, out EndOfLineSequence lineBreakType);

        result.Should().Be(expectedResult);
        lineBreakType.Should().Be(expectedLikeBreakType);
    }

    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData("Hello", "Hello")]
    [InlineData("Hello world", "world")]
    [InlineData("Hello world.", "world")]
    [InlineData("Hello world...", "world")]
    [InlineData("Hello world ...", "world")]
    [InlineData("Hello world ... ", "world")]
    [InlineData("Hello-world", "world")]
    internal void GetLastWord(string? input, string? expectedResult)
    {
        ReadOnlySpan<char> result = input.AsSpan().GetLastWord();

        new string(result).Should().Be(expectedResult);
    }

    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData("hello", "hello")]
    [InlineData("hello\r", "hello", "")]
    [InlineData("hello\n", "hello", "")]
    [InlineData("hello\r\n", "hello", "")]
    [InlineData("hello\n\r", "hello", "", "")]
    [InlineData("hello\nworld\rhi", "hello", "world", "hi")]
    [InlineData("hello \n world \r hi \r\n ", "hello ", " world ", " hi ", " ")]
    internal void ToLines(string? input, params string[] expectLines)
    {
        List<ReadOnlyMemory<char>> result = input.AsMemory().ToLines();

        result.Select(x => new string(x.Span)).Should().BeEquivalentTo(expectLines);
    }
}
