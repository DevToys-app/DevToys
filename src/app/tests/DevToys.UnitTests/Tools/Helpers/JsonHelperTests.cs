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
    public void IsValid(string input, bool expectedResult)
    {
        JsonHelper.IsValid(input, new MockILogger()).Should().Be(expectedResult);
    }

    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData(" ", "")]
    [InlineData("   {  }  ", "{}")]
    [InlineData("   [  ]  ", "[]")]
    [InlineData("   { \"foo\": 123 }  ", "{\r\n  \"foo\": 123\r\n}")]
    public async Task FormatTwoSpaces(string input, string expectedResult)
    {
        string result = await JsonHelper.Format(input, Indentation.TwoSpaces, false, new MockILogger(), CancellationToken.None);
        result.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData(" ", "")]
    [InlineData("   {  }  ", "{}")]
    [InlineData("   [  ]  ", "[]")]
    [InlineData("   { \"foo\": 123 }  ", "{\r\n    \"foo\": 123\r\n}")]
    public async Task FormatFourSpaces(string input, string expectedResult)
    {
        string result = await JsonHelper.Format(input, Indentation.FourSpaces, false, new MockILogger(), CancellationToken.None);
        result.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData(" ", "")]
    [InlineData("   {  }  ", "{}")]
    [InlineData("   [  ]  ", "[]")]
    [InlineData("   { \"foo\": 123 }  ", "{\r\n\t\"foo\": 123\r\n}")]
    public async Task FormatOneTab(string input, string expectedResult)
    {
        string result = await JsonHelper.Format(input, Indentation.OneTab, false, new MockILogger(), CancellationToken.None);
        result.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData(" ", "")]
    [InlineData("   {  }  ", "{}")]
    [InlineData("   [  ]  ", "[]")]
    [InlineData("   { \"foo\": 123 }  ", "{\"foo\":123}")]
    public async Task FormatMinifiedAsync(string input, string expectedResult)
    {
        string result = await JsonHelper.Format(input, Indentation.Minified, false, new MockILogger(), CancellationToken.None);
        result.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData("{ \"Date\": \"2012-04-21T18:25:43-05:00\" }", "{\"Date\":\"2012-04-21T18:25:43-05:00\"}")]
    public async Task FormatDoesNotAlterateDateTimesAsync(string input, string expectedResult)
    {
        string result = await JsonHelper.Format(input, Indentation.Minified, false, new MockILogger(), CancellationToken.None);
        result.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData(
        "{\"a\": \"asdf\", \"c\" : 545, \"b\": 33, \"array\": [{\"a\": \"asdf\", \"c\" : 545, \"b\": 33, \"array\": []}]}",
        "{\"a\":\"asdf\",\"array\":[{\"a\":\"asdf\",\"array\":[],\"b\":33,\"c\":545}],\"b\":33,\"c\":545}")]
    public async Task FormatSortPropertiesAlphabeticallyAsync(string input, string expectedResult)
    {
        string result = await JsonHelper.Format(input, Indentation.Minified, true, new MockILogger(), CancellationToken.None);
        result.Should().Be(expectedResult);
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
        ResultInfo<string> result = JsonHelper.ConvertFromYaml(
             input,
             Indentation.FourSpaces,
             new MockILogger(),
             CancellationToken.None);
        result.Data.Should().Be(expectedResult);
    }
}
