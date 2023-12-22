using System.Reflection;
using System.Runtime.CompilerServices;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Grammar;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Metadata;
using Newtonsoft.Json;

namespace DevToys.Tools.Tools.Text.SmartCalculator.Features.Grammars;

[Export(typeof(IFunctionDefinitionProvider))]
[Culture(SupportedCultures.English)]
public sealed class FunctionDefinitionProvider : IFunctionDefinitionProvider
{
    private readonly Dictionary<string, List<Dictionary<string, Dictionary<string, string[]>>>> _cultureToFunctionDefinition = new();

    public IReadOnlyList<Dictionary<string, Dictionary<string, string[]>>> LoadFunctionDefinitions(string culture)
    {
        culture = culture.Replace("-", "_");

        lock (_cultureToFunctionDefinition)
        {
            if (!_cultureToFunctionDefinition.TryGetValue(culture, out List<Dictionary<string, Dictionary<string, string[]>>>? functionDefinitions) || functionDefinitions is null)
            {
                functionDefinitions = new();

                Dictionary<string, Dictionary<string, string[]>>? parsedJson
                    = LoadResource(
                        $"DevToys.Tools.Tools.Text.SmartCalculator.Features.Grammars.{culture}.FunctionDefinition.json");

                if (parsedJson is not null)
                    functionDefinitions.Add(parsedJson);

                _cultureToFunctionDefinition[culture] = functionDefinitions;
            }

            return functionDefinitions;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Dictionary<string, Dictionary<string, string[]>>? LoadResource(string resourceName)
    {
        var assembly = Assembly.GetExecutingAssembly();

        using Stream? embeddedResourceStream = assembly.GetManifestResourceStream(resourceName);
        if (embeddedResourceStream is null)
            throw new Exception("Unable to find the UnitNames file.");

        using var textStreamReader = new StreamReader(embeddedResourceStream);

        Dictionary<string, Dictionary<string, string[]>>? parsedJson
            = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string[]>>>(
                textStreamReader.ReadToEnd());

        return parsedJson;
    }
}
