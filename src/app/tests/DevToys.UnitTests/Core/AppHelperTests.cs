using DevToys.Core;

namespace DevToys.UnitTests.Core;

public class AppHelperTests
{
    [Theory]
    [InlineData(new[] { "" }, "tool", "")]
    [InlineData(new[] { "tool" }, "tool", "")]
    [InlineData(new[] { "--tool:" }, "tool", "")]
    [InlineData(new[] { "--tool:value" }, "tool", "value")]
    [InlineData(new[] { "--tool:", "value" }, "tool", "value")]
    [InlineData(new[] { "--tool:", "\"value with space\"" }, "tool", "value with space")]
    public void GetCommandLineArgument(string[] arguments, string searchedArgumentName, string expectedResult)
    {
        AppHelper.GetCommandLineArgument(arguments, searchedArgumentName).Should().Be(expectedResult);
    }
}
