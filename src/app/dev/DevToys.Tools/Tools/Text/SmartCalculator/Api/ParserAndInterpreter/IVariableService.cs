using DevToys.Tools.Tools.Text.SmartCalculator.Api.Data;

namespace DevToys.Tools.Tools.Text.SmartCalculator.Api.ParserAndInterpreter;

/// <summary>
/// A service for managing variables.
/// </summary>
public interface IVariableService
{
    /// <summary>
    /// Sets the <paramref name="value"/> of the given <paramref name="variableName"/>.
    /// If the variable doesn't exist yet, it will be created.
    /// <paramref name="variableName"/> is case sensitive.
    /// </summary>
    /// <param name="variableName">Case sensitive variable name.</param>
    /// <param name="value">The value of the variable.</param>
    void SetVariableValue(string variableName, IData? value);

    /// <summary>
    /// Gets the value of the given <paramref name="variableName"/>.
    /// </summary>
    /// <param name="variableName">Case sensitive variable name.</param>
    /// <returns>Returns null if the variable doesn't exist.</returns>
    IData? GetVariableValue(string variableName);
}
