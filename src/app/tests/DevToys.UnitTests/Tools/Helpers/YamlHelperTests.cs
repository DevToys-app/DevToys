using System.Runtime.InteropServices;
using System.Threading;
using DevToys.Tools.Helpers;
using DevToys.Tools.Models;

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
        YamlHelper.IsValid(input, new MockILogger()).Should().Be(expectedResult);
    }

    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData(" ", "")]
    public void ConvertFromJsonShouldReturnEmptyString(string input, string expected)
    {
        // prepare & act
        ResultInfo<string> actual = YamlHelper.ConvertFromJson(
            input,
            Indentation.FourSpaces,
            new MockILogger(),
            CancellationToken.None);

        actual.HasSucceeded.Should().BeFalse();
        actual.Data.Should().Be(expected);
    }

    [Fact(DisplayName = "Convert From Json with unsupported Indentation")]
    public void ConvertFromJsonWithUnsupportedIndentationShouldReturnEmptyString()
    {
        // prepare 
        string input = "{}";
        string expected = string.Empty;

        // act
        ResultInfo<string> actual = YamlHelper.ConvertFromJson(
            input,
            Indentation.Minified,
            new MockILogger(),
            CancellationToken.None);

        actual.HasSucceeded.Should().BeFalse();
        actual.Data.Should().Be(expected);
    }

    [Fact(DisplayName = "Convert From Json with unsupported Json")]
    public void ConvertFromJsonWithUnsupportedJsonShouldExceptionMessage()
    {
        // prepare 
        string input = "-";
        string expected = "Expected a digit ('0'-'9'), but instead reached end of data. LineNumber: 0 | BytePositionInLine: 1.";

        // act
        ResultInfo<string> actual = YamlHelper.ConvertFromJson(
            input,
            Indentation.Minified,
            new MockILogger(),
            CancellationToken.None);

        actual.HasSucceeded.Should().BeFalse();
        actual.Data.Should().Be(expected);
    }

    [Theory]
    [MemberData(nameof(GetDataConvertFromJsonWithTwoSpaces), parameters: 2)]
    public void ConvertFromJsonWithTwoSpaces(string input, string expectedResult)
    {
        ResultInfo<string> result = YamlHelper.ConvertFromJson(
             input,
             Indentation.TwoSpaces,
             new MockILogger(),
             CancellationToken.None);
        result.Data.Should().Be(expectedResult);
    }

    public static IEnumerable<object[]> GetDataConvertFromJsonWithTwoSpaces(int numTests)
    {
        var allData = new List<object[]>();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            allData.AddRange(
                new List<object[]>
                {
                    new object[] {"{\r\n    \"key\": \"value\"\r\n  }", "key: value\r\n"},
                    new object[] {"{\r\n    \"key\": \"value\",\r\n    \"key2\": 1\r\n  }", "key: value\r\nkey2: 1\r\n"},
                }
            );
        }
        else
        {
            allData.AddRange(
                new List<object[]>
                {
                    new object[] {"{\n    \"key\": \"value\"\n  }", "key: value\n"},
                    new object[] {"{\n    \"key\": \"value\",\n    \"key2\": 1\n  }", "key: value\nkey2: 1\n"},
                }
            );
        }

        return allData.Take(numTests);
    }


    [Theory]
    [MemberData(nameof(GetDataConvertFromJsonWithFourSpaces), parameters: 2)]
    public void ConvertFromJsonWithFourSpaces(string input, string expectedResult)
    {
        ResultInfo<string> result = YamlHelper.ConvertFromJson(
             input,
             Indentation.FourSpaces,
             new MockILogger(),
             CancellationToken.None);
        result.Data.Should().Be(expectedResult);
    }

    public static IEnumerable<object[]> GetDataConvertFromJsonWithFourSpaces(int numTests)
    {
        var allData = new List<object[]>();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            allData.AddRange(
                new List<object[]>
                {
                    new object[] {"{\r\n    \"key\": \"value\"\r\n  }", "key: value\r\n"},
                    new object[] {"{\r\n    \"key\": \"value\",\r\n    \"key2\": 1\r\n  }", "key: value\r\nkey2: 1\r\n"},
                }
            );
        }
        else
        {
            allData.AddRange(
                new List<object[]>
                {
                    new object[] {"{\n    \"key\": \"value\"\n  }", "key: value\n"},
                    new object[] {"{\n    \"key\": \"value\",\n    \"key2\": 1\n  }", "key: value\nkey2: 1\n"},
                }
            );
        }

        return allData.Take(numTests);
    }

    [Theory]
    [MemberData(nameof(GetDataConvertFromJsonWithJsonRootArrayWithTwoSpaces), parameters: 2)]
    public void ConvertFromJsonWithJsonRootArrayWithTwoSpaces(string input, string expectedResult)
    {
        ResultInfo<string> result = YamlHelper.ConvertFromJson(
             input,
             Indentation.TwoSpaces,
             new MockILogger(),
             CancellationToken.None);
        result.Data.Should().Be(expectedResult);
    }

    public static IEnumerable<object[]> GetDataConvertFromJsonWithJsonRootArrayWithTwoSpaces(int numTests)
    {
        var allData = new List<object[]>();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            allData.AddRange(
                new List<object[]>
                {
                    new object[] {"[\r\n  {\r\n    \"key\": \"value\"\r\n  }\r\n]", "- key: value\r\n"},
                    new object[] {"[\r\n  {\r\n    \"key\": \"value\",\r\n    \"key2\": 1\r\n  }\r\n]", "- key: value\r\n  key2: 1\r\n"},   }
            );
        }
        else
        {
            allData.AddRange(
                new List<object[]>
                {
                    new object[] {"[\n  {\n    \"key\": \"value\"\n  }\n]", "- key: value\n"},
                    new object[] {"[\n  {\n    \"key\": \"value\",\n    \"key2\": 1\n  }\n]", "- key: value\n  key2: 1\n"},
                }
            );
        }

        return allData.Take(numTests);
    }

    [Theory]
    [MemberData(nameof(GetDataConvertFromJsonWithJsonRootArrayWithFourSpaces), parameters: 2)]
    public void ConvertFromJsonWithJsonRootArrayWithFourSpaces(string input, string expectedResult)
    {
        ResultInfo<string> result = YamlHelper.ConvertFromJson(
             input,
             Indentation.FourSpaces,
             new MockILogger(),
             CancellationToken.None);
        result.Data.Should().Be(expectedResult);
    }

    public static IEnumerable<object[]> GetDataConvertFromJsonWithJsonRootArrayWithFourSpaces(int numTests)
    {
        var allData = new List<object[]>();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            allData.AddRange(
                new List<object[]>
                {
                    new object[] {"[\r\n  {\r\n    \"key\": \"value\"\r\n  }\r\n]", "-   key: value\r\n"},
                    new object[] {"[\r\n  {\r\n    \"key\": \"value\",\r\n    \"key2\": 1\r\n  }\r\n]", "-   key: value\r\n    key2: 1\r\n"},
                }
            );
        }
        else
        {
            allData.AddRange(
                new List<object[]>
                {
                    new object[] {"[\n  {\n    \"key\": \"value\"\n  }\n]", "-   key: value\n"},
                    new object[] {"[\n  {\n    \"key\": \"value\",\n    \"key2\": 1\n  }\n]", "-   key: value\n    key2: 1\n"},

                }
            );
        }

        return allData.Take(numTests);
    }
}
