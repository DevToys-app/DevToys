using DevToys.Tools.Tools.Text.SmartCalculator.Api.Lexer;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Metadata;

namespace DevToys.Tools.Tools.Text.SmartCalculator.Api.ParserAndInterpreter;

/// <summary>
/// Provides a service for parsing and interpreting inner expression and statement
/// </summary>
public interface IParserAndInterpreterService
{
    /// <summary>
    /// Gets a service for performing arithmetic operations.
    /// </summary>
    IArithmeticAndRelationOperationService ArithmeticAndRelationOperationService { get; }

    /// <summary>
    /// Try to parse an expression at the given token.
    /// </summary>
    /// <param name="culture">See <see cref="SupportedCultures"/>.</param>
    /// <param name="currentToken">The current token being read in the tokenized line.</param>
    /// <param name="variableService">A service for managing variables.</param>
    /// <param name="result">A value that can be used to report the parsed statement, interpreted result or error.</param>
    /// <param name="cancellationToken">A token that cancels when needed.</param>
    /// <returns>Returns true if the parser and interpreter produced a <paramref name="result"/> without issue. Otherwise, false.</returns>
    Task<bool> TryParseAndInterpretExpressionAsync(
        string culture,
        LinkedToken? currentToken,
        IVariableService variableService,
        ExpressionParserAndInterpreterResult result,
        CancellationToken cancellationToken);

    /// <summary>
    /// Try to parse an expression using the given list of expression parser and interpreter.
    /// </summary>
    /// <param name="expressionParserAndInterpreterNames">Unordered list of names of expression parser and interpreter to retrieve</param>
    /// <param name="culture">See <see cref="SupportedCultures"/>.</param>
    /// <param name="currentToken">The current token being read in the tokenized line.</param>
    /// <param name="variableService">A service for managing variables.</param>
    /// <param name="result">A value that can be used to report the parsed statement, interpreted result or error.</param>
    /// <param name="cancellationToken">A token that cancels when needed.</param>
    /// <returns>Returns true if the parser and interpreter produced a <paramref name="result"/> without issue. Otherwise, false.</returns>
    Task<bool> TryParseAndInterpretExpressionAsync(
        string[] expressionParserAndInterpreterNames,
        string culture,
        LinkedToken? currentToken,
        IVariableService variableService,
        ExpressionParserAndInterpreterResult result,
        CancellationToken cancellationToken);

    /// <summary>
    /// Try to parse an expression until a specific token type and text is found.
    /// </summary>
    /// <param name="culture">See <see cref="SupportedCultures"/>.</param>
    /// <param name="currentToken">The current token being read in the tokenized line.</param>
    /// <param name="parseUntilTokenIsOfType">A <see cref="PredefinedTokenAndDataTypeNames"/> at which it should stop trying to parse an expression.</param>
    /// <param name="parseUntilTokenHasText">(optional) A token's text at which it should stop trying to parse an expression</param>
    /// <param name="variableService">A service for managing variables.</param>
    /// <param name="result">A value that can be used to report the parsed statement, interpreted result or error.</param>
    /// <param name="cancellationToken">A token that cancels when needed.</param>
    /// <returns>Returns true if the parser and interpreter produced a <paramref name="result"/> without issue. Otherwise, false.</returns>
    Task<bool> TryParseAndInterpretExpressionAsync(
        string culture,
        LinkedToken? currentToken,
        string parseUntilTokenIsOfType,
        string? parseUntilTokenHasText,
        IVariableService variableService,
        ExpressionParserAndInterpreterResult result,
        CancellationToken cancellationToken);

    /// <summary>
    /// Try to parse an expression until a specific token type and text is found.
    /// </summary>
    /// <param name="culture">See <see cref="SupportedCultures"/>.</param>
    /// <param name="currentToken">The current token being read in the tokenized line.</param>
    /// <param name="parseUntilTokenIsOfType">A <see cref="PredefinedTokenAndDataTypeNames"/> at which it should stop trying to parse an expression.</param>
    /// <param name="parseUntilTokenHasText">(optional) A token's text at which it should stop trying to parse an expression</param>
    /// <param name="nestedTokenType">A <see cref="PredefinedTokenAndDataTypeNames"/> that marks a nested expression (see example)</param>
    /// <param name="variableService">A service for managing variables.</param>
    /// <param name="result">A value that can be used to report the parsed statement, interpreted result or error.</param>
    /// <param name="cancellationToken">A token that cancels when needed.</param>
    /// <returns>Returns true if the parser and interpreter produced a <paramref name="result"/> without issue. Otherwise, false.</returns>
    /// <example>
    /// Having <paramref name="parseUntilTokenIsOfType"/> = <see cref="PredefinedTokenAndDataTypeNames.RightParenth"/> and
    /// <paramref name="nestedTokenType"/> = <see cref="PredefinedTokenAndDataTypeNames.LeftParenth"/> makes that a document
    /// like `(123 + |456((123) + 2)) where `|` marks the current token, the whole expression `456((123) + 2)` will be parsed.
    /// </example>
    Task<bool> TryParseAndInterpretExpressionAsync(
        string culture,
        LinkedToken? currentToken,
        string parseUntilTokenIsOfType,
        string? parseUntilTokenHasText,
        string nestedTokenType,
        IVariableService variableService,
        ExpressionParserAndInterpreterResult result,
        CancellationToken cancellationToken);

    /// <summary>
    /// Try to parse an expression until a specific token type and text is found using the given list of expression parser and interpreter..
    /// </summary>
    /// <param name="expressionParserAndInterpreterNames">Unordered list of names of expression parser and interpreter to retrieve</param>
    /// <param name="culture">See <see cref="SupportedCultures"/>.</param>
    /// <param name="currentToken">The current token being read in the tokenized line.</param>
    /// <param name="parseUntilTokenIsOfType">A <see cref="PredefinedTokenAndDataTypeNames"/> at which it should stop trying to parse an expression.</param>
    /// <param name="parseUntilTokenHasText">(optional) A token's text at which it should stop trying to parse an expression</param>
    /// <param name="variableService">A service for managing variables.</param>
    /// <param name="result">A value that can be used to report the parsed statement, interpreted result or error.</param>
    /// <param name="cancellationToken">A token that cancels when needed.</param>
    /// <returns>Returns true if the parser and interpreter produced a <paramref name="result"/> without issue. Otherwise, false.</returns>
    Task<bool> TryParseAndInterpretExpressionAsync(
        string[] expressionParserAndInterpreterNames,
        string culture,
        LinkedToken? currentToken,
        string parseUntilTokenIsOfType,
        string? parseUntilTokenHasText,
        IVariableService variableService,
        ExpressionParserAndInterpreterResult result,
        CancellationToken cancellationToken);

    /// <summary>
    /// Try to parse an expression until a specific token type and text is found using the given list of expression parser and interpreter..
    /// </summary>
    /// <param name="expressionParserAndInterpreterNames">Unordered list of names of expression parser and interpreter to retrieve</param>
    /// <param name="culture">See <see cref="SupportedCultures"/>.</param>
    /// <param name="currentToken">The current token being read in the tokenized line.</param>
    /// <param name="parseUntilTokenIsOfType">A <see cref="PredefinedTokenAndDataTypeNames"/> at which it should stop trying to parse an expression.</param>
    /// <param name="parseUntilTokenHasText">(optional) A token's text at which it should stop trying to parse an expression</param>
    /// <param name="nestedTokenType">A <see cref="PredefinedTokenAndDataTypeNames"/> that marks a nested expression (see example)</param>
    /// <param name="variableService">A service for managing variables.</param>
    /// <param name="result">A value that can be used to report the parsed statement, interpreted result or error.</param>
    /// <param name="cancellationToken">A token that cancels when needed.</param>
    /// <returns>Returns true if the parser and interpreter produced a <paramref name="result"/> without issue. Otherwise, false.</returns>
    /// <example>
    /// Having <paramref name="parseUntilTokenIsOfType"/> = <see cref="PredefinedTokenAndDataTypeNames.RightParenth"/> and
    /// <paramref name="nestedTokenType"/> = <see cref="PredefinedTokenAndDataTypeNames.LeftParenth"/> makes that a document
    /// like `(123 + |456((123) + 2)) where `|` marks the current token, the whole expression `456((123) + 2)` will be parsed.
    /// </example>
    Task<bool> TryParseAndInterpretExpressionAsync(
        string[] expressionParserAndInterpreterNames,
        string culture,
        LinkedToken? currentToken,
        string parseUntilTokenIsOfType,
        string? parseUntilTokenHasText,
        string nestedTokenType,
        IVariableService variableService,
        ExpressionParserAndInterpreterResult result,
        CancellationToken cancellationToken);

    /// <summary>
    /// Try to parse a statement until a specific token type and text is found.
    /// </summary>
    /// <param name="culture">See <see cref="SupportedCultures"/>.</param>
    /// <param name="currentToken">The current token being read in the tokenized line.</param>
    /// <param name="parseUntilTokenIsOfType">A <see cref="PredefinedTokenAndDataTypeNames"/> at which it should stop trying to parse a statement.</param>
    /// <param name="parseUntilTokenHasText">(optional) A token's text at which it should stop trying to parse a statement</param>
    /// <param name="variableService">A service for managing variables.</param>
    /// <param name="result">A value that can be used to report the parsed statement, interpreted result or error.</param>
    /// <param name="cancellationToken">A token that cancels when needed.</param>
    /// <returns>Returns true if the parser and interpreter produced a <paramref name="result"/> without issue. Otherwise, false.</returns>
    Task<bool> TryParseAndInterpretStatementAsync(
        string culture,
        LinkedToken? currentToken,
        string parseUntilTokenIsOfType,
        string? parseUntilTokenHasText,
        IVariableService variableService,
        StatementParserAndInterpreterResult result,
        CancellationToken cancellationToken);
}
