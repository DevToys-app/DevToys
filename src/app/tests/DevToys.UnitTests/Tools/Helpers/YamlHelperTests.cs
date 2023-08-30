﻿using System.Threading;
using DevToys.Tools.Helpers;
using DevToys.Tools.Models;
using DevToys.UnitTests.Mocks;

namespace DevToys.UnitTests.Tools.Helpers;

public class YamlHelperTests
{
    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData(" ", false)]
    [InlineData("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.", false)]
    [InlineData("foo :\n  bar :\n    - boo: 1\n    - rab: 2\n    - plop: 3", true)]
    public void IsValid(string input, bool expectedResult)
    {
        YamlHelper.IsValid(input).Should().Be(expectedResult);
    }

    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData(" ", "")]
    public void ConvertFromJsonShouldReturnEmptyString(string input, string expected)
    {
        // prepare & act
        string actual = YamlHelper.ConvertFromJson(
            input,
            Indentation.FourSpaces,
            new MockILogger(),
            CancellationToken.None);

        actual.Should().Be(expected);
    }

    [Fact(DisplayName = "Convert From Json with unsupported Indentation")]
    public void ConvertFromJsonWithUnsupportedIndentationShouldReturnEmptyString()
    {
        // prepare 
        string input = "{}";
        string expected = string.Empty;

        // act
        string actual = YamlHelper.ConvertFromJson(
            input,
            Indentation.Minified,
            new MockILogger(),
            CancellationToken.None);

        actual.Should().Be(expected);
    }

    [Fact(DisplayName = "Convert From Json with unsupported Json")]
    public void ConvertFromJsonWithUnsupportedJsonShouldExceptionMessage()
    {
        // prepare 
        string input = "-";
        string expected = "Expected a digit ('0'-'9'), but instead reached end of data. LineNumber: 0 | BytePositionInLine: 1.";

        // act
        string actual = YamlHelper.ConvertFromJson(
            input,
            Indentation.Minified,
            new MockILogger(),
            CancellationToken.None);

        actual.Should().Be(expected);
    }

    [Theory]
    [InlineData("{\r\n    \"key\": \"value\"\r\n  }", "key: value\r\n")]
    [InlineData("{\r\n    \"key\": \"value\",\r\n    \"key2\": 1\r\n  }", "key: value\r\nkey2: 1\r\n")]
    public void ConvertFromJsonWithTwoSpaces(string input, string expectedResult)
    {
        YamlHelper.ConvertFromJson(
            input,
            Indentation.TwoSpaces,
            new MockILogger(),
            CancellationToken.None).Should().Be(expectedResult);
    }

    [Theory]
    [InlineData("{\r\n    \"key\": \"value\"\r\n  }", "key: value\r\n")]
    [InlineData("{\r\n    \"key\": \"value\",\r\n    \"key2\": 1\r\n  }", "key: value\r\nkey2: 1\r\n")]
    public void ConvertFromJsonWithFourSpaces(string input, string expectedResult)
    {
        YamlHelper.ConvertFromJson(
            input,
            Indentation.FourSpaces,
            new MockILogger(),
            CancellationToken.None).Should().Be(expectedResult);
    }

    [Theory]
    [InlineData("[\r\n  {\r\n    \"key\": \"value\"\r\n  }\r\n]", "- key: value\r\n")]
    [InlineData("[\r\n  {\r\n    \"key\": \"value\",\r\n    \"key2\": 1\r\n  }\r\n]", "- key: value\r\n  key2: 1\r\n")]
    public void ConvertFromJsonWithJsonRootArrayWithTwoSpaces(string input, string expectedResult)
    {
        YamlHelper.ConvertFromJson(
            input,
            Indentation.TwoSpaces,
            new MockILogger(),
            CancellationToken.None).Should().Be(expectedResult);
    }

    [Theory]
    [InlineData("[\r\n  {\r\n    \"key\": \"value\"\r\n  }\r\n]", "-   key: value\r\n")]
    [InlineData("[\r\n  {\r\n    \"key\": \"value\",\r\n    \"key2\": 1\r\n  }\r\n]", "-   key: value\r\n    key2: 1\r\n")]
    public void ConvertFromJsonWithJsonRootArrayWithFourSpaces(string input, string expectedResult)
    {
        YamlHelper.ConvertFromJson(
            input,
            Indentation.FourSpaces,
            new MockILogger(),
            CancellationToken.None).Should().Be(expectedResult);
    }
}
