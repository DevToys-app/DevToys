using DevToys.Tools.Tools.Text.SmartCalculator.Api.Lexer;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Metadata;

namespace DevToys.Tools.Tools.Text.SmartCalculator.Api.ParserAndInterpreter;

/// <summary>
/// Provides a parser and interpreter for a statement.
/// </summary>
/// <example>
///     <code>
///         [Export(typeof(IStatementParserAndInterpreter))]
///         [Culture(SupportedCultures.Any)]
///         [Name("MyStatement")]
///         public class MyStatementParserAndInterpreter : IStatementParserAndInterpreter
///         {
///             [Import]
///             IParserAndInterpreterService ParserAndInterpreterService { get; }
///         }
///     </code>
/// </example>
public interface IStatementParserAndInterpreter
{
    /// <summary>
    /// Gets a service helping with parsing inner expressions, statement and perform arithmetic operation.
    /// </summary>
    IParserAndInterpreterService ParserAndInterpreterService { get; }

    /// <summary>
    /// Try to parse a statement at the <paramref name="currentToken"/> in the document.
    /// </summary>
    /// <param name="culture">See <see cref="SupportedCultures"/>.</param>
    /// <param name="currentToken">The current token being read in the tokenized line.</param>
    /// <param name="variableService">A service for managing variables.</param>
    /// <param name="result">A value that can be used to report the parsed statement, interpreted result or error.</param>
    /// <param name="cancellationToken">A token that cancels when needed.</param>
    /// <returns>Returns true if the parser and interpreter produced a <paramref name="result"/> without issue. Otherwise, false.</returns>
    /// <remarks>An exception of type <see cref="DataOperationException"/> can be thrown to indicate an error.</remarks>
    Task<bool> TryParseAndInterpretStatementAsync(
        string culture,
        LinkedToken currentToken,
        IVariableService variableService,
        StatementParserAndInterpreterResult result,
        CancellationToken cancellationToken);
}
