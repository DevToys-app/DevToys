using DevToys.Tools.Tools.Text.SmartCalculator.Api.Data;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Grammar;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Metadata;

namespace DevToys.Tools.Tools.Text.SmartCalculator.Api.ParserAndInterpreter;

/// <summary>
/// Provides an interpreter for a function.
/// </summary>
/// <example>
///     <code>
///         [Export(typeof(IFunctionInterpreter))]
///         [Name("general.myFunction")]
///         [Culture(SupportedCultures.English)]
///         public class MyFunctionInterpreter : IFunctionInterpreter
///         {
///         }
///     </code>
/// </example>
/// <remarks>
/// The function should be defined in a grammar file and at least one <see cref="IFunctionDefinitionProvider"/>
/// should be exported.
/// </remarks>
public interface IFunctionInterpreter
{
    /// <summary>
    /// Interprets the function.
    /// </summary>
    /// <param name="culture">See <see cref="SupportedCultures"/></param>
    /// <param name="functionDefinition">The definition of the function being interpreted.</param>
    /// <param name="detectedData">The detected data that go as parameters of the function</param>
    /// <param name="cancellationToken">A token that cancels when needed.</param>
    /// <returns>The result of the interpretation. Returns null if no result has been found.</returns>
    /// <remarks>An exception of type <see cref="DataOperationException"/> can be thrown to indicate an error.</remarks>
    Task<IData?> InterpretFunctionAsync(
        string culture,
        FunctionDefinition functionDefinition,
        IReadOnlyList<IData> detectedData,
        CancellationToken cancellationToken);
}
