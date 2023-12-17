using DevToys.Tools.Helpers.LoremIpsum;
using Microsoft.Extensions.Logging;

namespace DevToys.Tools.Tools.Generators.LoremIpsum;

[Export(typeof(ICommandLineTool))]
[Name(internalComponentName: "LoremIpsumGenerator")]
[CommandName(
    Name = "loremipsum",
    Alias = "li",
    ResourceManagerBaseName = "DevToys.Tools.Tools.Generators.LoremIpsum.LoremIpsumGenerator",
    DescriptionResourceName = nameof(LoremIpsumGenerator.Description))]
internal sealed class LoremIpsumGeneratorCommandLineTool : ICommandLineTool
{
    [CommandLineOption(
        Name = "corpus",
        Alias = "c",
        DescriptionResourceName = nameof(LoremIpsumGenerator.CorpusOptionDescription))]
    internal LipsumsCorpus Corpus { get; set; } = LipsumsCorpus.LoremIpsum;

    [CommandLineOption(
        Name = "type",
        Alias = "t",
        DescriptionResourceName = nameof(LoremIpsumGenerator.TypeOptionDescription))]
    internal Features Type { get; set; } = Features.Paragraphs;

    [CommandLineOption(
        Name = "length",
        Alias = "l",
        DescriptionResourceName = nameof(LoremIpsumGenerator.LengthOptionDescription))]
    internal int Length { get; set; } = 1;

    public ValueTask<int> InvokeAsync(ILogger logger, CancellationToken cancellationToken)
    {
        if (Length <= 0)
        {
            return ValueTask.FromResult(-1);
        }

        var generator = new LipsumGenerator(Corpus);
        string generatedText = generator.GenerateLipsum(Length, Type);

        Console.WriteLine(generatedText);
        return ValueTask.FromResult(0);
    }
}
