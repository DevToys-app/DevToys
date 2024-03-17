using System.Threading;
using System.Threading.Tasks;
using DevToys.Tools.Helpers;
using DevToys.Tools.Models;

namespace DevToys.UnitTests.Tools.Helpers;

public class JsonHelperTests
{
    [Theory]
    [InlineData(null, true)]
    [InlineData("\"foo\"", true)]
    [InlineData("123", false)]
    [InlineData("", false)]
    [InlineData(" ", false)]
    [InlineData("   {  }  ", true)]
    [InlineData("   [  ]  ", true)]
    [InlineData("   { \"foo\": 123 }  ", true)]
    [InlineData("   bar { \"foo\": 123 }  ", false)]
    public async Task IsValid(string input, bool expectedResult)
    {
        (await JsonHelper.IsValidAsync(input, new MockILogger(), CancellationToken.None))
            .Should().Be(expectedResult);
    }

    [Theory]
    [InlineData(null, false, "")]
    [InlineData("", false, "")]
    [InlineData(" ", false, "")]
    [InlineData("   {  }  ", true, "{}")]
    [InlineData("   [  ]  ", true, "[]")]
    [InlineData("   { \"foo\": 123 }  ", true, "{\r\n  \"foo\": 123\r\n}")]
    public async Task FormatTwoSpaces(string input, bool expectedSucceeded, string expectedResult)
    {
        expectedResult = expectedResult.Replace("\r\n", Environment.NewLine);
        ResultInfo<string> result = await JsonHelper.FormatAsync(input, Indentation.TwoSpaces, false, new MockILogger(), CancellationToken.None);
        result.HasSucceeded.Should().Be(expectedSucceeded);
        result.Data.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData(null, false, "")]
    [InlineData("", false, "")]
    [InlineData(" ", false, "")]
    [InlineData("   {  }  ", true, "{}")]
    [InlineData("   [  ]  ", true, "[]")]
    [InlineData("   { \"foo\": 123 }  ", true, "{\r\n    \"foo\": 123\r\n}")]
    public async Task FormatFourSpaces(string input, bool expectedSucceeded, string expectedResult)
    {
        expectedResult = expectedResult.Replace("\r\n", Environment.NewLine);
        ResultInfo<string> result = await JsonHelper.FormatAsync(input, Indentation.FourSpaces, false, new MockILogger(), CancellationToken.None);
        result.HasSucceeded.Should().Be(expectedSucceeded);
        result.Data.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData(null, false, "")]
    [InlineData("", false, "")]
    [InlineData(" ", false, "")]
    [InlineData("   {  }  ", true, "{}")]
    [InlineData("   [  ]  ", true, "[]")]
    [InlineData("   { \"foo\": 123 }  ", true, "{\r\n\t\"foo\": 123\r\n}")]
    public async Task FormatOneTab(string input, bool expectedSucceeded, string expectedResult)
    {
        expectedResult = expectedResult.Replace("\r\n", Environment.NewLine);
        ResultInfo<string> result = await JsonHelper.FormatAsync(input, Indentation.OneTab, false, new MockILogger(), CancellationToken.None);
        result.HasSucceeded.Should().Be(expectedSucceeded);
        result.Data.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData(null, false, "")]
    [InlineData("", false, "")]
    [InlineData(" ", false, "")]
    [InlineData("   {  }  ", true, "{}")]
    [InlineData("   [  ]  ", true, "[]")]
    [InlineData("   { \"foo\": 123 }  ", true, "{\"foo\":123}")]
    public async Task FormatMinifiedAsync(string input, bool expectedSucceeded, string expectedResult)
    {
        ResultInfo<string> result = await JsonHelper.FormatAsync(input, Indentation.Minified, false, new MockILogger(), CancellationToken.None);
        result.HasSucceeded.Should().Be(expectedSucceeded);
        result.Data.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData("{ \"Date\": \"2012-04-21T18:25:43-05:00\" }", "{\"Date\":\"2012-04-21T18:25:43-05:00\"}")]
    public async Task FormatDoesNotAlterateDateTimesAsync(string input, string expectedResult)
    {
        ResultInfo<string> result = await JsonHelper.FormatAsync(input, Indentation.Minified, false, new MockILogger(), CancellationToken.None);
        result.Data.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData(
        "{\"a\": \"asdf\", \"c\" : 545, \"b\": 33, \"array\": [{\"a\": \"asdf\", \"c\" : 545, \"b\": 33, \"array\": []}]}",
        "{\"a\":\"asdf\",\"array\":[{\"a\":\"asdf\",\"array\":[],\"b\":33,\"c\":545}],\"b\":33,\"c\":545}")]
    public async Task FormatSortPropertiesAlphabeticallyAsync(string input, string expectedResult)
    {
        ResultInfo<string> result = await JsonHelper.FormatAsync(input, Indentation.Minified, true, new MockILogger(), CancellationToken.None);
        result.Data.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData(" ", "")]
    public void ConvertFromYamlShouldReturnEmptyString(string input, string expected)
    {
        // prepare & act
        ResultInfo<string> actual = JsonHelper.ConvertFromYaml(
            input,
            Indentation.FourSpaces,
            new MockILogger(),
            CancellationToken.None);
        actual.HasSucceeded.Should().BeFalse();
        actual.Data.Should().Be(expected);
    }

    [Fact(DisplayName = "Convert From Yaml with unsupported Indentation")]
    public void ConvertFromYamlWithUnsupportedIndentationShouldReturnEmptyString()
    {
        // prepare 
        string input = "- key: value";
        string expected = string.Empty;

        // act
        ResultInfo<string> actual = JsonHelper.ConvertFromYaml(
            input,
            Indentation.Minified,
            new MockILogger(),
            CancellationToken.None);
        actual.HasSucceeded.Should().BeFalse();
        actual.Data.Should().Be(expected);
    }

    [Fact(DisplayName = "Convert From Yaml with unsupported yaml")]
    public void ConvertFromYamlWithUnsupportedYamlShouldExceptionMessage()
    {
        // prepare 
        string input = "[";
        string expected = "While parsing a node, did not find expected node content.";

        // act
        ResultInfo<string> actual = JsonHelper.ConvertFromYaml(
            input,
            Indentation.Minified,
            new MockILogger(),
            CancellationToken.None);
        actual.HasSucceeded.Should().BeFalse();
        actual.Data.Should().Be(expected);
    }

    [Theory]
    [InlineData("   - key: value  ", "[\r\n  {\r\n    \"key\": \"value\"\r\n  }\r\n]")]
    [InlineData("   - key: value\r\n     key2: 1", "[\r\n  {\r\n    \"key\": \"value\",\r\n    \"key2\": 1\r\n  }\r\n]")]
    public void ConvertFromYamlWithTwoSpaces(string input, string expectedResult)
    {
        expectedResult = expectedResult.Replace("\r\n", Environment.NewLine);
        ResultInfo<string> result = JsonHelper.ConvertFromYaml(
             input,
             Indentation.TwoSpaces,
             new MockILogger(),
             CancellationToken.None);
        result.Data.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData("   - key: value  ", "[\r\n    {\r\n        \"key\": \"value\"\r\n    }\r\n]")]
    [InlineData("   - key: value\r\n     key2: 1", "[\r\n    {\r\n        \"key\": \"value\",\r\n        \"key2\": 1\r\n    }\r\n]")]
    public void ConvertFromYamlWithFourSpaces(string input, string expectedResult)
    {
        expectedResult = expectedResult.Replace("\r\n", Environment.NewLine);
        ResultInfo<string> result = JsonHelper.ConvertFromYaml(
             input,
             Indentation.FourSpaces,
             new MockILogger(),
             CancellationToken.None);
        result.Data.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData(null, null, "")]
    [InlineData("{\"a\": \"asdf\", \"c\" : 545, \"b\": 33, \"array\": [{\"a\": \"asdf\", \"c\" : 545, \"b\": 42, \"array\": []}]}", "array.[0].b", "[\r\n  42\r\n]")]
    public async Task JsonPathTests(string json, string jsonPath, string expectedResult)
    {
        expectedResult = expectedResult.Replace("\r\n", Environment.NewLine);
        (await JsonHelper.TestJsonPathAsync(json, jsonPath, new MockILogger(), CancellationToken.None))
            .Data.Should().Be(expectedResult);
    }
}
