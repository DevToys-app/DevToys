using System.Reflection;
using System.Runtime.CompilerServices;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Metadata;
using DevToys.Tools.Tools.Text.SmartCalculator.Features.Data;

namespace DevToys.Tools.Tools.Text.SmartCalculator.Features.Grammars;

[Export(typeof(UnitMapProvider))]
[Culture(SupportedCultures.English)]
[Culture(SupportedCultures.French)]
public sealed class UnitMapProvider
{
    private readonly Dictionary<string, UnitMap> _cultureToUnitMap = new();

    internal UnitMap LoadUnitMap(string culture)
    {
        culture = culture.Replace("-", "_");

        lock (_cultureToUnitMap)
        {
            if (!_cultureToUnitMap.TryGetValue(culture, out UnitMap? unitMap) || unitMap is null)
            {
                unitMap = LoadResource($"DevToys.Tools.Tools.Text.SmartCalculator.Features.Grammars.{culture}.UnitNames.json");
                _cultureToUnitMap[culture] = unitMap;
            }

            return unitMap;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static UnitMap LoadResource(string resourceName)
    {
        var assembly = Assembly.GetExecutingAssembly();

        using Stream? embeddedResourceStream = assembly.GetManifestResourceStream(resourceName);
        if (embeddedResourceStream is null)
            throw new Exception("Unable to find the UnitNames file.");

        using var textStreamReader = new StreamReader(embeddedResourceStream);

        return UnitMap.Load(textStreamReader.ReadToEnd());
    }
}
