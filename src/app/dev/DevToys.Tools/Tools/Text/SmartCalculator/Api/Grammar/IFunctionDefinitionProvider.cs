namespace DevToys.Tools.Tools.Text.SmartCalculator.Api.Grammar;

/// <summary>
/// Represents a function definition provider.
/// </summary>
public interface IFunctionDefinitionProvider
{
    /// <summary>
    /// Loads the function definitions for the given culture.
    /// </summary>
    IReadOnlyList<Dictionary<string, Dictionary<string, string[]>>> LoadFunctionDefinitions(string culture);
}
