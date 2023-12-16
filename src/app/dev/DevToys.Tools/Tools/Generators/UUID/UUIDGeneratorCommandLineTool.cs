using DevToys.Tools.Helpers;
using DevToys.Tools.Models;
using Microsoft.Extensions.Logging;

namespace DevToys.Tools.Tools.Generators.UUID;

[Export(typeof(ICommandLineTool))]
[Name("UUIDGenerator")]
[CommandName(
    Name = "uuid",
    Alias = "guid",
    ResourceManagerBaseName = "DevToys.Tools.Tools.Generators.UUID.UUIDGenerator",
    DescriptionResourceName = nameof(UUIDGenerator.Description))]
internal sealed class UUIDGeneratorCommandLineTool : ICommandLineTool
{
    [CommandLineOption(
        Name = "version",
        Alias = "v",
        DescriptionResourceName = nameof(UUIDGenerator.VersionOptionDescription))]
    internal UuidVersion Version { get; set; } = UuidVersion.Four;

    [CommandLineOption(
        Name = "hyphens",
        Alias = "h",
        DescriptionResourceName = nameof(UUIDGenerator.HyphensOptionDescription))]
    internal bool Hyphens { get; set; } = true;

    [CommandLineOption(
        Name = "uppercase",
        Alias = "u",
        DescriptionResourceName = nameof(UUIDGenerator.UppercaseOptionDescription))]
    internal bool Uppercase { get; set; } = false;

    public ValueTask<int> InvokeAsync(ILogger logger, CancellationToken cancellationToken)
    {
        string newUuid
            = UuidHelper.GenerateUuid(
                Version,
                Hyphens,
                Uppercase);

        Console.WriteLine(newUuid);
        return ValueTask.FromResult(0);
    }
}
