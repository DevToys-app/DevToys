///<reference path="../monaco-editor/monaco.d.ts" />

const registerCompletionItemProvider = function (editorContext: EditorContext, languageId, characters) {
    return monaco.languages.registerCompletionItemProvider(languageId, {
        triggerCharacters: characters,
        provideCompletionItems: function (model, position, context, token) {
            return callParentEventAsync(editorContext, "CompletionItemProvider" + languageId, [JSON.stringify(position), JSON.stringify(context)]).then(result => {
                if (result) {
                    const list: monaco.languages.CompletionList = JSON.parse(result);

                    // Add dispose method for IDisposable that Monaco is looking for.
                    list.dispose = () => { };

                    return list;
                }
            });
        },
        resolveCompletionItem: function (item, token) {
            return callParentEventAsync(editorContext, "CompletionItemRequested" + languageId, [JSON.stringify(item)]).then(result => {
                if (result) {
                    return JSON.parse(result);
                }
            });
        }
    });
}