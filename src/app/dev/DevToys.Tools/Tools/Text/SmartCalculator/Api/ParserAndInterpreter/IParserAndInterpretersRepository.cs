using DevToys.Tools.Tools.Text.SmartCalculator.Api.Data;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Metadata;

namespace DevToys.Tools.Tools.Text.SmartCalculator.Api.ParserAndInterpreter;

/// <summary>
/// A service that helps accessing parsers and interpreters.
/// </summary>
internal interface IParserAndInterpretersRepository
{
    /// <summary>
    /// Gets all the data parsers that apply to the given <paramref name="culture"/>.
    /// </summary>
    /// <param name="culture">See <see cref="SupportedCultures"/></param>
    /// <returns>An ordered list of item by priority.</returns>
    IEnumerable<IDataParser> GetApplicableDataParsers(string culture);

    /// <summary>
    /// Gets all the statement parsers and interpreters that apply to the given <paramref name="culture"/>.
    /// </summary>
    /// <param name="culture">See <see cref="SupportedCultures"/></param>
    /// <returns>An ordered list of item by priority.</returns>
    IEnumerable<IStatementParserAndInterpreter> GetApplicableStatementParsersAndInterpreters(string culture);

    /// <summary>
    /// Gets all the expression parsers and interpreters that apply to the given <paramref name="culture"/>.
    /// </summary>
    /// <param name="culture">See <see cref="SupportedCultures"/></param>
    /// <returns>An ordered list of item by priority.</returns>
    IEnumerable<IExpressionParserAndInterpreter> GetApplicableExpressionParsersAndInterpreters(string culture);

    /// <summary>
    /// Gets a specific list of expression parsers and interpreters.
    /// </summary>
    /// <param name="culture">See <see cref="SupportedCultures"/></param>
    /// <param name="expressionParserAndInterpreterNames">Unordered list of names of expression parser and interpreter to retrieve</param>
    /// <returns>An ordered list of item by priority.</returns>
    IExpressionParserAndInterpreter[] GetExpressionParserAndInterpreters(string culture, params string[] expressionParserAndInterpreterNames);
}
