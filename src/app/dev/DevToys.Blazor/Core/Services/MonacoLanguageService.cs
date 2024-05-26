using DevToys.Blazor.Components.Monaco.Languages;
using DevToys.Blazor.Components.UIElements;
using DevToys.Core.Tools.Metadata;
using Microsoft.Extensions.Logging;

namespace DevToys.Blazor.Core.Services;

public sealed partial class MonacoLanguageService
{
    private readonly Dictionary<string, Dictionary<string, CancellationTokenSource>> _languageToTextModelNameToCancellationTokenMapForSemanticTokenization = new();
    private readonly HashSet<string> _registeredLanguages = new();
    private readonly DotNetObjectReference<MonacoLanguageService> _reference;
    private readonly ILogger _logger;
    private readonly IJSRuntime _jsRuntime;
    private readonly DisposableSemaphore _semaphore = new();

#pragma warning disable IDE0044 // Add readonly modifier
    [ImportMany]
    private IEnumerable<Lazy<ILanguageService, LanguageServiceMetadata>> _languageServices = default!;
#pragma warning restore IDE0044 // Add readonly modifier

    public MonacoLanguageService(IMefProvider mefProvider, IJSRuntime jsRuntime)
    {
        _logger = this.Log();
        _jsRuntime = jsRuntime;
        _reference = DotNetObjectReference.Create(this);
        mefProvider.SatisfyImports(this);
    }

    internal async Task RegisterLanguageAsync(string languageName)
    {
        if (!string.IsNullOrWhiteSpace(languageName))
        {
            using (await _semaphore.WaitAsync(CancellationToken.None))
            {
                if (!_registeredLanguages.Add(languageName)
                    || !_languageServices.Any(s => string.Equals(s.Metadata.InternalComponentName, languageName, StringComparison.CurrentCulture)))
                {
                    return;
                }

                await _jsRuntime.InvokeVoidWithErrorHandlingAsync("devtoys.MonacoEditorLanguage.registerLanguage", languageName, _reference);
            }
        }
    }

    [JSInvokable]
    public async Task<CompletionItem[]> GetAutoCompletionItemsAsync(string languageName, string textModelName, int startPosition, int endPosition)
    {
        if (TryGetLanguageServiceAndPresenter(
            languageName,
            textModelName,
            out Lazy<ILanguageService, LanguageServiceMetadata>? languageService,
            out UIMultiLineTextInputPresenter? presenter))
        {
            var cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;
            if (!Debugger.IsAttached)
            {
                // When not debugging, the language service has 2 seconds to provide completion items.
                cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(2));
            }

            try
            {
                await TaskSchedulerAwaiter.SwitchOffMainThreadAsync(cancellationToken);

                var span = TextSpan.FromBounds(startPosition, endPosition);
                Task<IReadOnlyList<AutoCompletionItem>> completionItemsTask
                    = languageService.Value.GetAutoCompletionItemsAsync(
                        presenter.UIMultiLineTextInput.Text,
                        span,
                        cancellationToken);

                // Wait for the completion items or the cancellation token to be triggered.
                await Task.WhenAny(completionItemsTask, cancellationToken.AsTask());

                cancellationToken.ThrowIfCancellationRequested();

                // The task has completed successfully.
                IReadOnlyList<AutoCompletionItem>? completionItems = await completionItemsTask;
                if (completionItems is null)
                {
                    return Array.Empty<CompletionItem>();
                }

                var results = new CompletionItem[completionItems.Count];
                for (int i = 0; i < completionItems.Count; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    AutoCompletionItem item = completionItems[i];
                    results[i]
                        = new CompletionItem
                        {
                            Label = new CompletionItemLabel
                            {
                                Label = item.Title,
                                Description = item.Description
                            },
                            Kind = (CompletionItemKind)item.Kind,
                            Documentation = item.Documentation,
                            InsertText = item.TextToInsert,
                            Preselect = item.ShouldBePreselected,
                            InsertTextRules = CompletionItemInsertTextRule.InsertAsSnippet
                        };
                }

                return results;
            }
            catch (NotImplementedException) { }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                LogGetAutoCompletionItemsAsyncFailed(ex, languageService.Metadata.InternalComponentName);
            }
            finally
            {
                cancellationTokenSource.Dispose();
            }
        }

        return Array.Empty<CompletionItem>();
    }

    [JSInvokable]
    public async Task<int[]> GetSemanticTokensAsync(string languageName, string textModelName)
    {
        if (TryGetLanguageServiceAndPresenter(
            languageName,
            textModelName,
            out Lazy<ILanguageService, LanguageServiceMetadata>? languageService,
            out UIMultiLineTextInputPresenter? presenter))
        {
            CancellationToken cancellationToken;
            using (await _semaphore.WaitAsync(CancellationToken.None))
            {
                // Cancel the previous semantic token request, if any in progress.
                if (!_languageToTextModelNameToCancellationTokenMapForSemanticTokenization.TryGetValue(languageName, out Dictionary<string, CancellationTokenSource>? textModelNameToCancellationTokenMap))
                {
                    textModelNameToCancellationTokenMap = new();
                    _languageToTextModelNameToCancellationTokenMapForSemanticTokenization.Add(languageName, textModelNameToCancellationTokenMap);
                }

                if (textModelNameToCancellationTokenMap.TryGetValue(textModelName, out CancellationTokenSource? cancellationTokenSource))
                {
                    cancellationTokenSource.Cancel();
                    cancellationTokenSource.Dispose();
                }

                textModelNameToCancellationTokenMap[textModelName] = cancellationTokenSource = new CancellationTokenSource();
                cancellationToken = cancellationTokenSource.Token;
            }

            try
            {
                await TaskSchedulerAwaiter.SwitchOffMainThreadAsync(cancellationToken);

                // Get the semantic tokens from the language service.
                IReadOnlyList<SemanticToken> semanticTokens
                    = await languageService.Value.GetSemanticTokensAsync(
                        presenter.UIMultiLineTextInput.Text,
                        cancellationToken);

                if (semanticTokens is null)
                {
                    return Array.Empty<int>();
                }

                cancellationToken.ThrowIfCancellationRequested();

                // Each token is represented by 5 integers: start line, start column, length, tokenType and modifier.
                int[] results = new int[semanticTokens.Count * 5];
                for (int i = 0; i < semanticTokens.Count; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    SemanticToken token = semanticTokens[i];
                    results[i * 5] = token.DeltaLine;
                    results[i * 5 + 1] = token.DeltaColumn;
                    results[i * 5 + 2] = token.Length;
                    results[i * 5 + 3] = (int)token.TokenType;
                    results[i * 5 + 4] = (int)token.TokenModifier;
                }
                return results;
            }
            catch (NotImplementedException) { }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                LogGetSemanticTokensAsyncFailed(ex, languageService.Metadata.InternalComponentName);
            }
        }

        return Array.Empty<int>();
    }

    private bool TryGetLanguageServiceAndPresenter(
        string languageName,
        string textModelName,
        out Lazy<ILanguageService, LanguageServiceMetadata> languageService,
        out UIMultiLineTextInputPresenter presenter)
    {
        languageService
            = _languageServices.FirstOrDefault(
                s => string.Equals(s.Metadata.InternalComponentName, languageName, StringComparison.CurrentCulture))!;

        presenter = UIMultilineTextInputHelper.GetPresenter(textModelName)!;
        return languageService is not null
            && presenter is not null
            && string.Equals(presenter.UIMultiLineTextInput.SyntaxColorizationLanguageName, languageName, StringComparison.CurrentCulture);
    }

    [LoggerMessage(0, LogLevel.Error, "Unexpectedly failed to get auto-completion items from the language service for '{languageName}'.")]
    partial void LogGetAutoCompletionItemsAsyncFailed(Exception ex, string languageName);

    [LoggerMessage(1, LogLevel.Error, "Unexpectedly failed to get semantic tokens from the language service for '{languageName}'.")]
    partial void LogGetSemanticTokensAsyncFailed(Exception ex, string languageName);
}
