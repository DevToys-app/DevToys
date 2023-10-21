using System.CommandLine;
using System.CommandLine.Parsing;

namespace DevToys.CLI.Core;

internal sealed class OneOfOption : Option<object?>
{
    internal Type OneOfTypeDefinition { get; }

    internal OneOfOption(
        string optionName,
        string? optionDescription,
        Type oneOfTypeDefinition,
        ParseArgument<object?> parseArgument)
        : base(optionName, parseArgument, isDefault: false, optionDescription)
    {
        Guard.IsTrue(oneOfTypeDefinition.IsAssignableTo(typeof(OneOf.IOneOf)));

        OneOfTypeDefinition = oneOfTypeDefinition;
    }
}
