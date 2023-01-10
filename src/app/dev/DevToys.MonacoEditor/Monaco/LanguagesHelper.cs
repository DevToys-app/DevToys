using DevToys.MonacoEditor.Monaco.Languages;
using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco;

/// <summary>
/// Helper to static Monaco.Languages Namespace methods.
/// https://microsoft.github.io/monaco-editor/api/modules/monaco.languages.html
/// </summary>
public sealed class LanguagesHelper
{
    private readonly WeakReference<CodeEditor> _editor;

    [Obsolete("Use <Editor Instance>.Languages.* instead of constructing your own LanguagesHelper.")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public LanguagesHelper(CodeEditor editor) // TODO: Make Internal later.
    {
        // We need the editor component in order to execute JavaScript within 
        // the WebView environment to retrieve data (even though this Monaco class is static).
        _editor = new WeakReference<CodeEditor>(editor);
    }

    public Task<IList<ILanguageExtensionPoint>?> GetLanguagesAsync()
    {
        if (_editor.TryGetTarget(out CodeEditor? editor))
        {
            return editor.SendScriptAsync<IList<ILanguageExtensionPoint>>("monaco.languages.getLanguages()");
        }

        return Task.FromResult<IList<ILanguageExtensionPoint>?>(null);
    }

    public Task RegisterAsync(ILanguageExtensionPoint language)
    {
        if (_editor.TryGetTarget(out CodeEditor? editor))
        {
            return editor.InvokeScriptAsync("monaco.languages.register", language);
        }

        return Task.CompletedTask;
    }

    public Task RegisterCodeActionProviderAsync(string languageId, CodeActionProvider provider)
    {
        if (_editor.TryGetTarget(out CodeEditor? editor))
        {
            // link:registerCodeActionProvider.ts:ProvideCodeActions
            editor.ParentAccessor.RegisterEvent("ProvideCodeActions" + languageId, async (args) =>
            {
                if (args != null && args.Length >= 2)
                {
                    Range? range = JsonConvert.DeserializeObject<Range>(args[0]);
                    CodeActionContext? context = JsonConvert.DeserializeObject<CodeActionContext>(args[1]);
                    Guard.IsNotNull(context);
                    Guard.IsNotNull(range);

                    CodeActionList list = await provider.ProvideCodeActionsAsync(editor.GetModel(), range, context);

                    if (list != null)
                    {
                        return JsonConvert.SerializeObject(list);
                    }
                }

                return string.Empty;
            });

            // link:registerCodeActionProvider.ts:registerCodeActionProvider
            return editor.InvokeScriptAsync("registerCodeActionProvider", new object[] { languageId });
        }

        return Task.CompletedTask;
    }

    public Task RegisterCodeLensProviderAsync(string languageId, CodeLensProvider provider)
    {
        if (_editor.TryGetTarget(out CodeEditor? editor))
        {
            // link:registerCodeLensProvider.ts:ProvideCodeLenses
            editor.ParentAccessor.RegisterEvent("ProvideCodeLenses" + languageId, async (args) =>
            {
                CodeLensList list = await provider.ProvideCodeLensesAsync(editor.GetModel());

                if (list != null)
                {
                    return JsonConvert.SerializeObject(list);
                }

                return string.Empty;
            });

            // link:registerCodeLensProvider.ts:ResolveCodeLens
            editor.ParentAccessor.RegisterEvent("ResolveCodeLens" + languageId, async (args) =>
            {
                if (args != null && args.Length >= 1)
                {
                    CodeLens lens = await provider.ResolveCodeLensAsync(editor.GetModel(), JsonConvert.DeserializeObject<CodeLens>(args[0])!);

                    if (lens != null)
                    {
                        return JsonConvert.SerializeObject(lens);
                    }
                }

                return string.Empty;
            });

            // link:registerCodeLensProvider.ts:registerCodeLensProvider
            return editor.InvokeScriptAsync("registerCodeLensProvider", new object[] { languageId });
        }

        return Task.CompletedTask;
    }

    public Task RegisterColorProviderAsync(string languageId, DocumentColorProvider provider)
    {
        if (_editor.TryGetTarget(out CodeEditor? editor))
        {
            // link:registerColorProvider.ts:ProvideColorPresentations
            editor.ParentAccessor.RegisterEvent("ProvideColorPresentations" + languageId, async (args) =>
            {
                if (args != null && args.Length >= 1)
                {
                    IEnumerable<ColorPresentation> items = await provider.ProvideColorPresentationsAsync(editor.GetModel(), JsonConvert.DeserializeObject<ColorInformation>(args[0])!);

                    if (items != null)
                    {
                        return JsonConvert.SerializeObject(items);
                    }
                }

                return string.Empty;
            });

            // link:registerColorProvider.ts:ProvideDocumentColors
            editor.ParentAccessor.RegisterEvent("ProvideDocumentColors" + languageId, async (args) =>
            {
                IEnumerable<ColorInformation> items = await provider.ProvideDocumentColorsAsync(editor.GetModel());

                if (items != null)
                {
                    return JsonConvert.SerializeObject(items);
                }

                return string.Empty;
            });

            // link:registerColorProvider.ts:registerColorProvider
            return editor.InvokeScriptAsync("registerColorProvider", new object[] { languageId });
        }

        return Task.CompletedTask;
    }

    public Task RegisterCompletionItemProviderAsync(string languageId, CompletionItemProvider provider)
    {
        if (_editor.TryGetTarget(out CodeEditor? editor))
        {
            // TODO: Add Incremented Id so that we can register multiple providers per language?
            // link:registerCompletionItemProvider.ts:CompletionItemProvider
            editor.ParentAccessor.RegisterEvent("CompletionItemProvider" + languageId, async (args) =>
            {
                if (args != null && args.Length >= 2)
                {
                    CompletionList items = await provider.ProvideCompletionItemsAsync(editor.GetModel(), JsonConvert.DeserializeObject<Position>(args[0])!, JsonConvert.DeserializeObject<CompletionContext>(args[1])!);

                    if (items != null)
                    {
                        Debug.WriteLine("Items: " + items);
                        string serialized = JsonConvert.SerializeObject(items);
                        Debug.WriteLine("Items in JSON: " + serialized);
                        return serialized;
                    }
                }

                return string.Empty;
            });

            // link:registerCompletionItemProvider.ts:CompletionItemRequested
            editor.ParentAccessor.RegisterEvent("CompletionItemRequested" + languageId, async (args) =>
            {
                if (args != null && args.Length >= 1)
                {
                    CompletionItem? requestedItem = JsonConvert.DeserializeObject<CompletionItem>(args[0]);
                    Guard.IsNotNull(requestedItem);
                    CompletionItem completionItem = await provider.ResolveCompletionItemAsync(editor.GetModel(), requestedItem);

                    if (completionItem != null)
                    {
                        return JsonConvert.SerializeObject(completionItem);
                    }
                }

                return string.Empty;
            });

            // link:registerCompletionItemProvider.ts:registerCompletionItemProvider
            return editor.InvokeScriptAsync("registerCompletionItemProvider", new object[] { languageId, provider.TriggerCharacters });
        }

        return Task.CompletedTask;
    }

    public Task RegisterHoverProviderAsync(string languageId, HoverProvider provider)
    {
        if (_editor.TryGetTarget(out CodeEditor? editor))
        {
            // Wrapper around Hover Provider to Monaco editor.
            // TODO: Add Incremented Id so that we can register multiple providers per language?
            editor.ParentAccessor.RegisterEvent("HoverProvider" + languageId, async (args) =>
            {
                Debug.WriteLine($"Hover provider.......... {args != null}");
                if (args != null && args.Length >= 1)
                {
                    Hover hover = await provider.ProvideHover(editor.GetModel(), JsonConvert.DeserializeObject<Position>(args[0])!);

                    if (hover != null)
                    {
                        return JsonConvert.SerializeObject(hover);
                    }
                }

                return string.Empty;
            });

            // link:otherScriptsToBeOrganized.ts:registerHoverProvider
            return editor.InvokeScriptAsync("registerHoverProvider", languageId);
        }

        return Task.CompletedTask;
    }
}
