using System.CommandLine;
using System.CommandLine.Parsing;
using DevToys.Api;

namespace DevToys.CLI.Core;

internal sealed class AnyTypeOption : Option<object?>
{
    internal Type AnyTypeDefinition { get; }

    internal AnyTypeOption(
        string optionName,
        string? optionDescription,
        Type anyTypeDefinition,
        ParseArgument<object?> parseArgument)
        : base(optionName, parseArgument, isDefault: false, optionDescription)
    {
        Guard.IsTrue(
            anyTypeDefinition.GUID == AnyTypeIdentifiers.AnyTypeT2Guid
            || anyTypeDefinition.GUID == AnyTypeIdentifiers.AnyTypeT3Guid
            || anyTypeDefinition.GUID == AnyTypeIdentifiers.AnyTypeT4Guid);

        AnyTypeDefinition = anyTypeDefinition;
    }
}
