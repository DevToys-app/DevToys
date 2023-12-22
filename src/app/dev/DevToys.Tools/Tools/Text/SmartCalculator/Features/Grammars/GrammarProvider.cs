using System.Reflection;
using System.Runtime.CompilerServices;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Grammar;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Metadata;
using Microsoft.Recognizers.Text;

namespace DevToys.Tools.Tools.Text.SmartCalculator.Features.Grammars;

[Export(typeof(IGrammarProvider))]
[Culture(SupportedCultures.English)]
internal class GrammarProvider : IGrammarProvider
{
    public IReadOnlyList<TokenDefinitionGrammar>? LoadTokenDefinitionGrammars(string culture)
    {
        culture = culture.Replace("-", "_");

        var grammars = new List<TokenDefinitionGrammar>();
        TokenDefinitionGrammar? grammar = LoadGrammar($"DevToys.Tools.Tools.Text.SmartCalculator.Features.Grammars.{culture}.TokenDefinition.json");
        if (grammar is not null)
            grammars.Add(grammar);

        grammar = LoadGrammar($"DevToys.Tools.Tools.Text.SmartCalculator.Features.Grammars.SpecialTokenDefinition.json");
        if (grammar is not null)
            grammars.Add(grammar);

        return grammars;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static TokenDefinitionGrammar? LoadGrammar(string resourceName)
    {
        var assembly = Assembly.GetExecutingAssembly();

        using Stream? embeddedResourceStream = assembly.GetManifestResourceStream(resourceName);
        if (embeddedResourceStream is null)
            throw new Exception("Unable to find the grammar file.");

        using var textStreamReader = new StreamReader(embeddedResourceStream);

        return TokenDefinitionGrammar.Load(textStreamReader.ReadToEnd());
    }
}
