﻿using System.Runtime.InteropServices;
using DevToys.Tools.Helpers;
using DevToys.Tools.Models;
using DevToys.UnitTests.Mocks;

namespace DevToys.UnitTests.Tools.Helpers;

public class XmlHelperTests
{
    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData(" ", false)]
    [InlineData("<>", false)]
    [InlineData("</>", false)]
    [InlineData("<xml />", true)]
    [InlineData("<root><xml /></root>    ", true)]
    [InlineData("    <root><xml test=\"true\" /></root>", true)]
    [InlineData("    <root><xml test=\"true\" /></root    ", false)]
    public void IsValid(string input, bool expectedResult)
    {
        XmlHelper.IsValid(input, new MockILogger()).Should().Be(expectedResult);
    }

    [Theory]
    [MemberData(nameof(GetDataFormatTwoSpaces), parameters: 7)]
    public void FormatTwoSpaces(string input, string expectedResult, bool newLineOnAttributes = false)
    {
        ResultInfo<string> actual = XmlHelper.Format(input, Indentation.TwoSpaces, newLineOnAttributes, new MockILogger());
        actual.Data.Should().Be(expectedResult);
    }

    public static IEnumerable<object[]> GetDataFormatTwoSpaces(int numTests)
    {
        var allData = new List<object[]>()
        {
            new object[] { null, "" },
            new object[] { "","" },
            new object[] { " ",""},
            new object[] { "<xml />", "<xml />" },
        };

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            allData.AddRange(
                new List<object[]>
                {
                    new object[] {"<root><xml /></root>", "<root>\r\n  <xml />\r\n</root>"},
                    new object[] {"<root><xml test=\"true\" /></root>", "<root>\r\n  <xml test=\"true\" />\r\n</root>", false},
                    new object[] {"<root><xml test=\"true\" /></root>", "<root>\r\n  <xml\r\n    test=\"true\" />\r\n</root>", true},
                }
            );
        }
        else
        {
            allData.AddRange(
                new List<object[]>
                {
                    new object[] {"<root><xml /></root>", "<root>\n  <xml />\n</root>"},
                    new object[] {"<root><xml test=\"true\" /></root>", "<root>\n  <xml test=\"true\" />\n</root>", false},
                    new object[] {"<root><xml test=\"true\" /></root>", "<root>\n  <xml\n    test=\"true\" />\n</root>", true},
                }
            );
        }

        return allData.Take(numTests);
    }


    [Theory]
    [MemberData(nameof(GetDataFormatFourSpaces), parameters: 7)]
    public void FormatFourSpaces(string input, string expectedResult, bool newLineOnAttributes = false)
    {
        ResultInfo<string> actual = XmlHelper.Format(input, Indentation.FourSpaces, newLineOnAttributes, new MockILogger());
        actual.Data.Should().Be(expectedResult);
    }

    public static IEnumerable<object[]> GetDataFormatFourSpaces(int numTests)
    {
        var allData = new List<object[]>()
        {
            new object[] { null, "" },
            new object[] { "","" },
            new object[] { " ",""},
            new object[] { "<xml />", "<xml />" },
        };

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            allData.AddRange(
                    new List<object[]>
                    {
                        new object[] { "<root><xml /></root>", "<root>\r\n    <xml />\r\n</root>" },
                        new object[] { "<root><xml test=\"true\" /></root>", "<root>\r\n    <xml test=\"true\" />\r\n</root>", false },
                        new object[] { "<root><xml test=\"true\" /></root>", "<root>\r\n    <xml\r\n        test=\"true\" />\r\n</root>", true },
                    }
                );
        }
        else
        {
            allData.AddRange(
                new List<object[]>
                {
                    new object[] { "<root><xml /></root>", "<root>\n    <xml />\n</root>" },
                    new object[] { "<root><xml test=\"true\" /></root>", "<root>\n    <xml test=\"true\" />\n</root>", false },
                    new object[] { "<root><xml test=\"true\" /></root>", "<root>\n    <xml\n        test=\"true\" />\n</root>", true },
                }
            );
        }

        return allData.Take(numTests);
    }

    [Theory]
    [MemberData(nameof(GetDataFormatOneTab), parameters: 7)]
    public void FormatOneTab(string input, string expectedResult, bool newLineOnAttributes = false)
    {
        ResultInfo<string> actual = XmlHelper.Format(input, Indentation.OneTab, newLineOnAttributes, new MockILogger());
        actual.Data.Should().Be(expectedResult);
    }

    public static IEnumerable<object[]> GetDataFormatOneTab(int numTests)
    {
        var allData = new List<object[]>()
        {
            new object[] { null, "" },
            new object[] { "","" },
            new object[] { " ",""},
            new object[] { "<xml />", "<xml />" },
        };

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            allData.AddRange(
                new List<object[]>
                {
                    new object[] {"<root><xml /></root>", "<root>\r\n\t<xml />\r\n</root>"},
                    new object[] {"<root><xml test=\"true\" /></root>", "<root>\r\n\t<xml test=\"true\" />\r\n</root>", false},
                    new object[] {"<root><xml test=\"true\" /></root>", "<root>\r\n\t<xml\r\n\t\ttest=\"true\" />\r\n</root>", true},
                }
            );
        }
        else
        {
            allData.AddRange(
                new List<object[]>
                {
                    new object[] {"<root><xml /></root>", "<root>\n\t<xml />\n</root>"},
                    new object[] {"<root><xml test=\"true\" /></root>", "<root>\n\t<xml test=\"true\" />\n</root>", false},
                    new object[] {"<root><xml test=\"true\" /></root>", "<root>\n\t<xml\n\t\ttest=\"true\" />\n</root>", true},
                }
            );
        }

        return allData.Take(numTests);
    }


    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData(" ", "")]
    [InlineData("<xml />", "<xml />")]
    [InlineData("<root>\r\n    <xml />\r\n</root>", "<root><xml /></root>")]
    [InlineData("<root>\r\n      <xml />\r\n</root>", "<root><xml /></root>")]
    [InlineData("<root>\r\n    <xml test=\"true\" />\r\n</root>", "<root><xml test=\"true\" /></root>", false)]
    [InlineData("<root>\r\n    <xml\r\n        test=\"true\" />\r\n</root>", "<root><xml test=\"true\" /></root>", true)]
    [InlineData("<root>\r\n\t<xml />\r\n</root>", "<root><xml /></root>")]
    [InlineData("<root>\r\n\t<xml test=\"true\" />\r\n</root>", "<root><xml test=\"true\" /></root>", false)]
    [InlineData("<root>\r\n\t<xml\r\n\t\ttest=\"true\" />\r\n</root>", "<root><xml test=\"true\" /></root>", true)]
    public void FormatMinified(string input, string expectedResult, bool newLineOnAttributes = false)
    {
        ResultInfo<string> actual = XmlHelper.Format(input, Indentation.Minified, newLineOnAttributes, new MockILogger());
        actual.Data.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData("<?xml version=\"1.0\" ?><Document version=\"0.1\"><Element /></Document>", "<?xml version=\"1.0\" ?><Document version=\"0.1\"><Element /></Document>")]
    [InlineData("<?xml version=\"1.0\" encoding=\"utf-8\"?><Document version=\"0.1\"><Element /></Document>", "<?xml version=\"1.0\" encoding=\"utf-8\"?><Document version=\"0.1\"><Element /></Document>")]
    [InlineData("<?xml version=\"1.0\" encoding=\"utf-16\"?><Document version=\"0.1\"><Element /></Document>", "<?xml version=\"1.0\" encoding=\"utf-16\"?><Document version=\"0.1\"><Element /></Document>")]
    [InlineData("<?xml version=\"1.0\" encoding=\"test\"?><Document version=\"0.1\"><Element /></Document>", "<?xml version=\"1.0\" encoding=\"test\"?><Document version=\"0.1\"><Element /></Document>")]
    [InlineData("<Document version=\"0.1\"><Element /></Document>", "<Document version=\"0.1\"><Element /></Document>")]
    public void CheckEncoding(string input, string expectedResult, bool newLineOnAttributes = false)
    {
        ResultInfo<string> actual = XmlHelper.Format(input, Indentation.Minified, newLineOnAttributes, new MockILogger());
        actual.Data.Should().Be(expectedResult);
    }
}
