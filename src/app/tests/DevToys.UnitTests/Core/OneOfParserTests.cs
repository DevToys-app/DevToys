using System.CommandLine.Parsing;
using System.Globalization;
using DevToys.CLI.Core;
using OneOf;

namespace DevToys.UnitTests.Core;
public class OneOfParserTests
{
    [Fact]
    public void OneOfParser_Float_String()
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

        var option
            = new OneOfOption(
                "test",
                "",
                typeof(OneOf<string, float>),
                DummyParse);

        object result = OneOfParser.ParseOneOf(option, "1.5");
        result.Should().BeOfType<OneOf<string, float>>();
        result.Should().Be((OneOf<string, float>)1.5f);

        result = OneOfParser.ParseOneOf(option, "5");
        result.Should().BeOfType<OneOf<string, float>>();
        result.Should().Be((OneOf<string, float>)5f);

        result = OneOfParser.ParseOneOf(option, "a");
        result.Should().BeOfType<OneOf<string, float>>();
        result.Should().Be((OneOf<string, float>)"a");

        result = OneOfParser.ParseOneOf(option, "");
        result.Should().BeOfType<OneOf<string, float>>();
        result.Should().Be((OneOf<string, float>)"");
    }

    private static object DummyParse(ArgumentResult argumentResult)
    {
        throw new NotImplementedException();
    }
}
