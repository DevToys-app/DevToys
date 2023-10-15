using System.CommandLine.Parsing;
using DevToys.CLI.Core;

namespace DevToys.UnitTests.Core;
public class AnyTypeParserTests
{
    [Fact]
    public void AnyTypeParser_Float_String()
    {
        var option
            = new AnyTypeOption(
                "test",
                "",
                typeof(AnyType<string, float>),
                DummyParse);

        object result = AnyTypeParser.ParseAnyType(option, "1.5");
        result.Should().BeOfType<AnyType<string, float>>();
        result.Should().Be(new AnyType<string, float>(1.5f));

        result = AnyTypeParser.ParseAnyType(option, "5");
        result.Should().BeOfType<AnyType<string, float>>();
        result.Should().Be(new AnyType<string, float>(5f));

        result = AnyTypeParser.ParseAnyType(option, "a");
        result.Should().BeOfType<AnyType<string, float>>();
        result.Should().Be(new AnyType<string, float>("a"));

        result = AnyTypeParser.ParseAnyType(option, "");
        result.Should().BeOfType<AnyType<string, float>>();
        result.Should().Be(new AnyType<string, float>(""));
    }

    private static object DummyParse(ArgumentResult argumentResult)
    {
        throw new NotImplementedException();
    }
}
