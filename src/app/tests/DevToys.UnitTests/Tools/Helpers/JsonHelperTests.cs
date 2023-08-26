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
        JsonHelper.IsValid(input).Should().Be(expectedResult);
    }

    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData(" ", "")]
    [InlineData("   {  }  ", "{}")]
    [InlineData("   [  ]  ", "[]")]
    [InlineData("   { \"foo\": 123 }  ", "{\r\n  \"foo\": 123\r\n}")]
    public void FormatTwoSpaces(string input, string expectedResult)
    {
        JsonHelper.Format(input, Indentation.TwoSpaces, sortProperties: false).Should().Be(expectedResult);
    }

    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData(" ", "")]
    [InlineData("   {  }  ", "{}")]
    [InlineData("   [  ]  ", "[]")]
    [InlineData("   { \"foo\": 123 }  ", "{\r\n    \"foo\": 123\r\n}")]
    public void FormatFourSpaces(string input, string expectedResult)
    {
       JsonHelper.Format(input, Indentation.FourSpaces, sortProperties: false).Should().Be(expectedResult);
    }

    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData(" ", "")]
    [InlineData("   {  }  ", "{}")]
    [InlineData("   [  ]  ", "[]")]
    [InlineData("   { \"foo\": 123 }  ", "{\r\n\t\"foo\": 123\r\n}")]
    public void FormatOneTab(string input, string expectedResult)
    {
        JsonHelper.Format(input, Indentation.OneTab, sortProperties: false).Should().Be(expectedResult); ;
    }

    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData(" ", "")]
    [InlineData("   {  }  ", "{}")]
    [InlineData("   [  ]  ", "[]")]
    [InlineData("   { \"foo\": 123 }  ", "{\"foo\":123}")]
    public void FormatMinified(string input, string expectedResult)
    {
        JsonHelper.Format(input, Indentation.Minified, sortProperties: false).Should().Be(expectedResult);
    }

    [Theory]
    [InlineData("{ \"Date\": \"2012-04-21T18:25:43-05:00\" }", "{\"Date\":\"2012-04-21T18:25:43-05:00\"}")]
    public void FormatDoesNotAlterateDateTimes(string input, string expectedResult)
    {
        JsonHelper.Format(input, Indentation.Minified, sortProperties: false).Should().Be(expectedResult);
    }

    [Theory]
    [InlineData(
        "{\"a\": \"asdf\", \"c\" : 545, \"b\": 33, \"array\": [{\"a\": \"asdf\", \"c\" : 545, \"b\": 33, \"array\": []}]}",
        "{\"a\":\"asdf\",\"array\":[{\"a\":\"asdf\",\"array\":[],\"b\":33,\"c\":545}],\"b\":33,\"c\":545}")]
    public void FormatSortPropertiesAlphabetically(string input, string expectedResult)
    {
        JsonHelper.Format(input, Indentation.Minified, sortProperties: true).Should().Be(expectedResult);
    }

    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData(" ", "")]
    public void ConvertFromYamlShouldReturnEmptyString(string input, string expected)
    {
        // prepare & act
        string actual = JsonHelper.ConvertFromYaml(input, Indentation.FourSpaces);
        actual.Should().Be(expected);
    }

    [Fact(DisplayName = "Convert From Yaml with unsupported Indentation")]
    public void ConvertFromYamlWithUnsupportedIndentationShouldReturnEmptyString()
    {
        // prepare 
        string input = "- key: value";
        string expected = string.Empty;

        // act
        string actual = JsonHelper.ConvertFromYaml(input, Indentation.Minified);
        actual.Should().Be(expected);
    }

    [Fact(DisplayName = "Convert From Yaml with unsupported yaml")]
    public void ConvertFromYamlWithUnsupportedYamlShouldExceptionMessage()
    {
        // prepare 
        string input = "[";
        string expected = "While parsing a node, did not find expected node content.";

        // act
        string actual = JsonHelper.ConvertFromYaml(input, Indentation.Minified);
        actual.Should().Be(expected);
    }

    [Theory]
    [InlineData("   - key: value  ", "[\r\n  {\r\n    \"key\": \"value\"\r\n  }\r\n]")]
    [InlineData("   - key: value\r\n     key2: 1", "[\r\n  {\r\n    \"key\": \"value\",\r\n    \"key2\": 1\r\n  }\r\n]")]
    public void ConvertFromYamlWithTwoSpaces(string input, string expectedResult)
    {
        JsonHelper.ConvertFromYaml(input, Indentation.TwoSpaces).Should().Be(expectedResult);
    }

    [Theory]
    [InlineData("   - key: value  ", "[\r\n    {\r\n        \"key\": \"value\"\r\n    }\r\n]")]
    [InlineData("   - key: value\r\n     key2: 1", "[\r\n    {\r\n        \"key\": \"value\",\r\n        \"key2\": 1\r\n    }\r\n]")]
    public void ConvertFromYamlWithFourSpaces(string input, string expectedResult)
    {
        JsonHelper.ConvertFromYaml(input, Indentation.FourSpaces).Should().Be(expectedResult);
    }
}
