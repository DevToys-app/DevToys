using System.Threading;
using DevToys.Tools.Helpers;
using DevToys.UnitTests.Mocks;

namespace DevToys.UnitTests.Tools.Helpers;

public class Base64HelperTests
{
    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData(" ", false)]
    [InlineData("aGVsbG8gd29ybGQ=", true)]
    [InlineData("aGVsbG8gd2f9ybGQ=", false)]
    [InlineData("SGVsbG8gV29y", true)]
    [InlineData("SGVsbG8gVa29y", false)]
    public void IsValid(string input, bool expectedResult)
    {
        Base64Helper.IsBase64DataStrict(input).Should().Be(expectedResult);
    }

    [Theory]
    [InlineData(null, "", Base64Encoding.Utf8)]
    [InlineData("Hello World!", "SGVsbG8gV29ybGQh", Base64Encoding.Utf8)]
    [InlineData("Hello World! é)à", "SGVsbG8gV29ybGQhIMOpKcOg", Base64Encoding.Utf8)]
    [InlineData("Hello World! é)à", "SGVsbG8gV29ybGQhID8pPw==", Base64Encoding.Ascii)]
    internal void FromTextToBase64(string input, string expectedResult, Base64Encoding encoding)
    {
        Base64Helper.FromTextToBase64(
            input,
            encoding,
            new MockILogger(),
            CancellationToken.None)
            .Should()
            .Be(expectedResult);
    }

    [Theory]
    [InlineData(null, "", Base64Encoding.Utf8)]
    [InlineData("SGVsbG8gV29ybGQh", "Hello World!", Base64Encoding.Utf8)]
    [InlineData("SGVsbG8gV29ybGQhIMOpKcOg", "Hello World! é)à", Base64Encoding.Utf8)]
    [InlineData("SGVsbG8gV29ybGQhID8pPw==", "Hello World! ?)?", Base64Encoding.Ascii)]
    internal void FromBase64ToText(string input, string expectedResult, Base64Encoding encoding)
    {
        Base64Helper.FromBase64ToText(
            input,
            encoding,
            new MockILogger(),
            CancellationToken.None)
            .Should()
            .Be(expectedResult);
    }
}
