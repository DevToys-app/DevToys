#nullable enable

using System;
using System.Collections.Generic;
using DevToys.MonacoEditor.CodeEditorControl;
using DevToys.MonacoEditor.Monaco.Languages;
using Newtonsoft.Json;
using Windows.Foundation;

namespace DevToys.MonacoEditor.Monaco
{
    /// <summary>
    /// Helper to static Monaco.Languages Namespace methods.
    /// https://microsoft.github.io/monaco-editor/api/modules/monaco.languages.html
    /// </summary>
    public sealed class LanguagesHelper
    {
        private readonly WeakReference<CodeEditorCore> _editor;

        public LanguagesHelper(CodeEditorCore editor)
        {
            // We need the editor component in order to execute JavaScript within 
            // the WebView environment to retrieve data (even though this Monaco class is static).
            _editor = new WeakReference<CodeEditorCore>(editor);
        }

        public IAsyncOperation<IList<LanguageExtensionPoint>?>? GetLanguagesAsync()
        {
            if (_editor.TryGetTarget(out CodeEditorCore editor))
            {
                return editor.SendScriptAsync<IList<LanguageExtensionPoint>>("monaco.languages.getLanguages()").AsAsyncOperation();
            }

            return null;
        }

        public IAsyncAction? RegisterAsync(LanguageExtensionPoint language)
        {
            if (_editor.TryGetTarget(out CodeEditorCore editor))
            {
                return editor.InvokeScriptAsync("monaco.languages.register", language).AsAsyncAction();
            }

            return null;
        }

        public IAsyncAction? RegisterCompletionItemProviderAsync(string languageId, ICompletionItemProvider provider)
        {
            if (_editor.TryGetTarget(out CodeEditorCore editor))
            {
                // Wrapper around CompletionItem Provider to Monaco editor.
                // TODO: Add Incremented Id so that we can register multiple providers per language?
                editor.ParentAccessor?.RegisterEvent("CompletionItemProvider" + languageId, async (args) =>
                {
                    if (args != null && args.Length >= 2)
                    {
                        CompletionList? items = await provider.ProvideCompletionItemsAsync(editor.GetModel()!, JsonConvert.DeserializeObject<Position>(args[0]), JsonConvert.DeserializeObject<CompletionContext>(args[1]));

                        if (items != null)
                        {
                            return JsonConvert.SerializeObject(items);
                        }
                    }

                    return null;
                });

                editor.ParentAccessor?.RegisterEvent("CompletionItemRequested" + languageId, async (args) =>
                {
                    if (args != null && args.Length >= 2)
                    {
                        Position? position = JsonConvert.DeserializeObject<Position>(args[0]);
                        CompletionItem? requestedItem = JsonConvert.DeserializeObject<CompletionItem>(args[1]);
                        CompletionItem? completionItem = await provider.ResolveCompletionItemAsync(editor.GetModel()!, position, requestedItem);

                        if (completionItem != null)
                        {
                            return JsonConvert.SerializeObject(completionItem);
                        }
                    }

                    return null;
                });

                return editor.InvokeScriptAsync("registerCompletionItemProvider", new object[] { languageId, provider.TriggerCharacters }).AsAsyncAction();
            }

            return null;
        }

        public IAsyncAction? RegisterHoverProviderAsync(string languageId, IHoverProvider provider)
        {
            if (_editor.TryGetTarget(out CodeEditorCore editor))
            {
                // Wrapper around Hover Provider to Monaco editor.
                // TODO: Add Incremented Id so that we can register multiple providers per language?
                editor.ParentAccessor?.RegisterEvent("HoverProvider" + languageId, async (args) =>
                {
                    if (args != null && args.Length >= 1)
                    {
                        Hover? hover = await provider.ProvideHover(editor.GetModel()!, JsonConvert.DeserializeObject<Position>(args[0]));

                        if (hover != null)
                        {
                            return JsonConvert.SerializeObject(hover);
                        }
                    }

                    return string.Empty;
                });

                return editor.InvokeScriptAsync("registerHoverProvider", languageId).AsAsyncAction();
            }

            return null;
        }
    }
}
