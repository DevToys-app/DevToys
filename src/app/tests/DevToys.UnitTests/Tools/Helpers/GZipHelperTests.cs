using System.Threading;
using System.Threading.Tasks;
using DevToys.Tools.Helpers;

namespace DevToys.UnitTests.Tools.Helpers;

public class GZipHelperTests
{
    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData(" ", false)]
    [InlineData("          ", false)]
    [InlineData("H4sIAAAAAAAAC/NIzcnJBwCCidH3BQAAAA==", true)]
    [InlineData("  H4sIAAAAAAAAC/NIzcnJBwCCidH3BQAAAA==", true)]
    [InlineData("              H4sIAAAAAAAAC/NIzcnJBwCCidH3BQAAAA==", true)]
    [InlineData("H4sI", true)]
    [InlineData("H4sIA", true)]
    [InlineData("H4s", false)]
    public void IsValid(string input, bool expectedResult)
    {
        GZipHelper.IsGZip(input).Should().Be(expectedResult);
    }

    [Theory]
    [InlineData(null, "")]
    [InlineData("", "H4sIAAAAAAAACgMAAAAAAAAAAAA=")]
    [InlineData("  ", "H4sIAAAAAAAAClNQAACVFjPvAgAAAA==")]
    [InlineData("Hello World!", "H4sIAAAAAAAACvNIzcnJVwjPL8pJUQQAoxwpHAwAAAA=")]
    [InlineData("Hello World! é)à", "H4sIAAAAAAAACvNIzcnJVwjPL8pJUVQ4vFLz8AIAeJm72xIAAAA=")]
    [InlineData("H4sIAAAAAAAACvNIzcnJVwjPL8pJUVQ4vFLz8AIAeJm72xIAAAA=", "H4sIAAAAAAAACvMwKfZ0hALnMj/PquQ8r7DyrAAfiwKv0LBAkzI3nyoLR0/HVK9cc6MKsFJbAI7LDrY0AAAA")]
    internal async Task CompressToGZip(string input, string expectedResult)
    {
        (await GZipHelper.CompressGZipDataAsync(
            input,
            new MockILogger(),
            CancellationToken.None))
            .compressedData
            .Should()
            .Be(expectedResult);
    }

    [Theory]
    [InlineData(null, "")]
    [InlineData("H4sIAAAAAAAACgMAAAAAAAAAAAA=", "")]
    [InlineData("H4sIAAAAAAAAClNQAACVFjPvAgAAAA==", "  ")]
    [InlineData("H4sIAAAAAAAACvNIzcnJVwjPL8pJUQQAoxwpHAwAAAA=", "Hello World!")]
    [InlineData("H4sIAAAAAAAACvNIzcnJVwjPL8pJUVQ4vFLz8AIAeJm72xIAAAA=", "Hello World! é)à")]
    [InlineData("H4sIAAAAAAAACvMwKfZ0hALnMj/PquQ8r7DyrAAfiwKv0LBAkzI3nyoLR0/HVK9cc6MKsFJbAI7LDrY0AAAA", "H4sIAAAAAAAACvNIzcnJVwjPL8pJUVQ4vFLz8AIAeJm72xIAAAA=")]
    [InlineData("Hello", "<Invalid GZip data>")]
    internal async Task DecompressGZip(string input, string expectedResult)
    {
        (await GZipHelper.DecompressGZipDataAsync(
            input,
            new MockILogger(),
            CancellationToken.None))
            .decompressedData
            .Should()
            .Be(expectedResult);
    }
}
