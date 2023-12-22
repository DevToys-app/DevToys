using DevToys.Tools.Tools.Text.SmartCalculator.Api;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.AbstractSyntaxTree;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Core;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Data;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Data.Definition;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Grammar;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Lexer;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.Metadata;
using DevToys.Tools.Tools.Text.SmartCalculator.Api.ParserAndInterpreter;
using Microsoft.Extensions.Logging;
using Microsoft.Recognizers.Text;

namespace DevToys.Tools.Tools.Text.SmartCalculator.Features.ExpressionParsersAndInterpreters.Function;

[Export(typeof(IExpressionParserAndInterpreter))]
[Name(PredefinedExpressionParserAndInterpreterNames.FunctionExpression)]
[Culture(SupportedCultures.Any)]
[Order(After = PredefinedExpressionParserAndInterpreterNames.ConditionalExpression, Before = PredefinedExpressionParserAndInterpreterNames.NumericalExpression)]
internal sealed class FunctionExpressionParserAndInterpreter : IExpressionParserAndInterpreter
{
    private readonly Dictionary<string, IEnumerable<IFunctionDefinitionProvider>> _applicableFunctionDefinitionProviders = new();
    private readonly Dictionary<string, IReadOnlyList<FunctionDefinition>> _applicableFunctionDefinitions = new();

    [Import]
    public ILexer Lexer { get; set; } = null!;

    [Import]
    public IParserAndInterpreterService ParserAndInterpreterService { get; set; } = null!;

    [ImportMany]
    public IEnumerable<Lazy<IFunctionDefinitionProvider, CultureCodeMetadata>> FunctionDefinitionProviders { get; set; } = null!;

    [ImportMany]
    public IEnumerable<Lazy<IFunctionInterpreter, FunctionInterpreterMetadata>> FunctionInterpreters { get; set; } = null!;

    public async Task<bool> TryParseAndInterpretExpressionAsync(
        string culture,
        LinkedToken currentToken,
        IVariableService variableService,
        ExpressionParserAndInterpreterResult result,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<FunctionDefinition> functionDefinitions = GetOrderedFunctionDefinitions(culture);
        Dictionary<Tuple<LinkedToken, string, string>, (bool, ExpressionParserAndInterpreterResult)> parsedExpressionCache = new();

        // For each applicable function definition
        for (int i = 0; i < functionDefinitions.Count; i++)
        {
            // TODO: Optimization:
            //       Many functions start with the same set of words. We could group them in addition of their lenght
            //       so the search of matching function would run faster. Although, we'd need to do a .GetText() on 
            //       each token, which isn't efficient either.
            FunctionDefinition functionDefinition = functionDefinitions[i];

            var detectedData = new List<IData>();
            bool functionDetected = true;
            LinkedToken lastToken = currentToken;
            LinkedToken? documentToken = currentToken;
            LinkedToken? functionDefinitionToken = functionDefinition.TokenizedFunctionDefinition;

            // For each token in the function definition grammar.
            while (documentToken is not null
                && functionDefinitionToken is not null)
            {
                // 1. Check whether functionDefinitionToken.Token corresponds to a data type / expression / statement.
                // 2. If so, try to parse that expression / statement. We will need to interpret the expression to know if it's the expected data type.
                // 3. if the token doesn't correspond to a data type / expression...etc, then let's just compare it with the documentToken.

                if (IsSpecialToken(functionDefinitionToken.Token, out bool isStatementExpected))
                {
                    string nextExpectedFunctionTokenType = functionDefinitionToken.Next?.Token.Type ?? string.Empty;
                    string nextExpectedFunctionTokenText = functionDefinitionToken.Next?.Token.GetText() ?? string.Empty;

                    bool foundStatementOrExpression;
                    IData? resultedData;
                    AbstractSyntaxTreeBase? parsedExpressionOrStatement;

                    if (isStatementExpected)
                    {
                        StatementParserAndInterpreterResult statementResult = new();
                        foundStatementOrExpression
                            = await ParserAndInterpreterService.TryParseAndInterpretStatementAsync(
                                culture,
                                documentToken,
                                nextExpectedFunctionTokenType,
                                nextExpectedFunctionTokenText,
                                variableService,
                                statementResult,
                                cancellationToken);
                        resultedData = statementResult.ResultedData;
                        parsedExpressionOrStatement = statementResult.ParsedStatement;
                        if (statementResult.Error is not null)
                            result.Error = statementResult.Error;
                    }
                    else
                    {
                        // Super important!
                        // We're caching the expression we find, so we can get the expression faster if
                        // we need it again when trying to parse another function. This optimization
                        // made some benchmark going from a mean of 17sec to 400ms.
                        ExpressionParserAndInterpreterResult expressionResult;
                        var cacheKey = new Tuple<LinkedToken, string, string>(documentToken, nextExpectedFunctionTokenType, nextExpectedFunctionTokenText);
                        if (parsedExpressionCache.TryGetValue(cacheKey, out (bool found, ExpressionParserAndInterpreterResult expResult) r))
                        {
                            foundStatementOrExpression = r.found;
                            expressionResult = r.expResult;
                        }
                        else
                        {
                            expressionResult = new();
                            foundStatementOrExpression
                                = await ParserAndInterpreterService.TryParseAndInterpretExpressionAsync(
                                    new[] { PredefinedExpressionParserAndInterpreterNames.PrimitiveExpression },
                                    culture,
                                    documentToken,
                                    nextExpectedFunctionTokenType,
                                    nextExpectedFunctionTokenText,
                                    variableService,
                                    expressionResult,
                                    cancellationToken);
                            parsedExpressionCache[cacheKey] = new(foundStatementOrExpression, expressionResult);
                        }

                        resultedData = expressionResult.ResultedData;
                        parsedExpressionOrStatement = expressionResult.ParsedExpression;
                        if (expressionResult.Error is not null)
                            result.Error = expressionResult.Error;
                    }

                    if (!foundStatementOrExpression
                        || resultedData is null
                        || !MatchType(functionDefinitionToken.Token, resultedData, parsedExpressionOrStatement))
                    {
                        functionDetected = false;
                        break;
                    }

                    detectedData.Add(resultedData);
                    documentToken = documentToken.GetTokenAfter(parsedExpressionOrStatement!.LastToken);
                    lastToken = parsedExpressionOrStatement!.LastToken;
                }
                else if (!documentToken.Token.Is(functionDefinitionToken.Token.Type, functionDefinitionToken.Token.GetText()))
                {
                    functionDetected = false;
                    break;
                }
                else
                {
                    lastToken = documentToken;
                }

                documentToken = documentToken?.Next;
                functionDefinitionToken = functionDefinitionToken.Next;

                cancellationToken.ThrowIfCancellationRequested();
            }

            cancellationToken.ThrowIfCancellationRequested();

            if (functionDetected)
            {
                (bool functionSucceeded, IData? functionResult, DataOperationException? error)
                    = await InterpretFunctionAsync(
                        culture,
                        functionDefinition,
                        detectedData,
                        cancellationToken);

                if (error is not null)
                    result.Error = error;

                if (functionSucceeded)
                {
                    result.ParsedExpression = new FunctionExpression(functionDefinition, currentToken, lastToken);
                    result.ResultedData = functionResult;
                    return true;
                }
            }
        }

        return false;
    }

    private IReadOnlyList<FunctionDefinition> GetOrderedFunctionDefinitions(string culture)
    {
        lock (_applicableFunctionDefinitions)
        {
            if (_applicableFunctionDefinitions.TryGetValue(culture, out IReadOnlyList<FunctionDefinition>? definitions) && definitions is not null)
                return definitions;

            var result = new List<FunctionDefinition>();
            // For each IFunctionDefinitionProvider
            foreach (IFunctionDefinitionProvider functionProvider in GetApplicableFunctionDefinitionProviders(culture))
            {
                // Load the function definitions.
                IReadOnlyList<Dictionary<string, Dictionary<string, string[]>>> parsedJsons = functionProvider.LoadFunctionDefinitions(culture);
                if (parsedJsons is not null)
                {
                    for (int i = 0; i < parsedJsons.Count; i++)
                    {
                        foreach (string functionCategory in parsedJsons[i].Keys)
                        {
                            Dictionary<string, string[]> functionDefinitions = parsedJsons[i][functionCategory];
                            foreach (string functionName in functionDefinitions.Keys)
                            {
                                string[] functionGrammars = functionDefinitions[functionName];
                                for (int j = 0; j < functionGrammars.Length; j++)
                                {
                                    // Tokenize the function grammar.
                                    IReadOnlyList<TokenizedTextLine> tokenizedGrammarLines = Lexer.Tokenize(culture, functionGrammars[j]);
                                    Guard.HasSizeEqualTo(tokenizedGrammarLines, 1);
                                    TokenizedTextLine tokenizedGrammar = tokenizedGrammarLines[0];
                                    Guard.IsNotNull(tokenizedGrammar.Tokens);
                                    result.Add(new FunctionDefinition($"{functionCategory}.{functionName}", tokenizedGrammar.Tokens));
                                }
                            }
                        }
                    }
                }
            }

            // Order the function definitions by amount of tokens, then by grammar length.
            definitions
                = result
                .OrderByDescending(f => f.TokenCount)
                .ThenByDescending(f => f.TokenizedFunctionDefinition.Token.LineTextIncludingLineBreak.Length)
                .ToList();
            Guard.HasSizeEqualTo(definitions, result.Count);

            _applicableFunctionDefinitions[culture] = definitions;
            return definitions;
        }
    }

    private IEnumerable<IFunctionDefinitionProvider> GetApplicableFunctionDefinitionProviders(string culture)
    {
        lock (_applicableFunctionDefinitionProviders)
        {
            if (_applicableFunctionDefinitionProviders.TryGetValue(culture, out IEnumerable<IFunctionDefinitionProvider>? providers) && providers is not null)
                return providers;

            providers
                = FunctionDefinitionProviders.Where(
                    p => p.Metadata.CultureCodes.Any(
                        c => CultureHelper.IsCultureApplicable(c, culture)))
                .Select(p => p.Value);

            _applicableFunctionDefinitionProviders[culture] = providers;
            return providers;
        }
    }

    private async Task<(bool, IData?, DataOperationException?)> InterpretFunctionAsync(
        string culture,
        FunctionDefinition functionDefinition,
        IReadOnlyList<IData> detectedData,
        CancellationToken cancellationToken)
    {
        IFunctionInterpreter functionInterpreter = GetFunctionInterpreter(culture, functionDefinition);

        DataOperationException? error = null;

        try
        {
            IData? data
                = await functionInterpreter.InterpretFunctionAsync(
                    culture,
                    functionDefinition,
                    detectedData,
                    cancellationToken);

            return (true, data, error);
        }
        catch (OperationCanceledException)
        {
            // Ignore.
        }
        catch (DataOperationException doe)
        {
            error = doe;
        }
        catch (Exception ex)
        {
            this.Log().LogError(
                "FunctionInterpreter.Fault",
                ex,
                ("FunctionName", functionDefinition.FunctionFullName));
        }

        return (false, null, error);
    }

    /// <summary>
    /// Gets the function interpreter for the given <paramref name="functionDefinition"/> and <paramref name="culture"/>.
    /// </summary>
    private IFunctionInterpreter GetFunctionInterpreter(string culture, FunctionDefinition functionDefinition)
    {
        IFunctionInterpreter functionInterpreter
            = FunctionInterpreters.First(
                p => p.Metadata.CultureCodes.Any(c => CultureHelper.IsCultureApplicable(c, culture))
                     && string.Equals(p.Metadata.Name, functionDefinition.FunctionFullName, StringComparison.Ordinal))
            .Value;

        return functionInterpreter;
    }

    /// <summary>
    /// Checks whether the given <paramref name="token"/> is a special token (STATEMENT, PERCENTAGE...etc).
    /// </summary>
    private static bool IsSpecialToken(IToken token, out bool isStatement)
    {
        isStatement = false;

        if (token.IsTokenTextEqualTo("STATEMENT", StringComparison.Ordinal))
        {
            isStatement = true;
            return true;
        }

        if (token.IsTokenTextEqualTo("EXPRESSION", StringComparison.Ordinal))
            return true;

        for (int i = token.StartInLine; i < token.EndInLine; i++)
        {
            if (!char.IsLetter(token.LineTextIncludingLineBreak[i]) || !char.IsUpper(token.LineTextIncludingLineBreak[i]))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Verified whether the current <paramref name="functionDefinitionToken"/> expects the given <paramref name="resultedData"/>
    /// or more generally the given <paramref name="parsedStatementOrExpression"/>.
    /// </summary>
    private static bool MatchType(IToken functionDefinitionToken, IData? resultedData, AbstractSyntaxTreeBase? parsedStatementOrExpression)
    {
        if (resultedData is null)
            return false;

        string tokenText = functionDefinitionToken.GetText();

        switch (tokenText)
        {
            case "STATEMENT":
                if (parsedStatementOrExpression is Statement)
                    return true;
                break;

            case "EXPRESSION":
                if (parsedStatementOrExpression is Expression)
                    return true;
                break;

            case "BOOLEAN":
                return resultedData is BooleanData;

            default:
                if (resultedData.IsOfSubtype(tokenText) || resultedData.IsOfType(tokenText))
                    return true;
                break;
        }

        return false;
    }
}
