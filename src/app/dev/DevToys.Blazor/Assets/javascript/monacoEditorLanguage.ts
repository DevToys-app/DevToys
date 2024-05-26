// eslint-disable-next-line @typescript-eslint/triple-slash-reference
///<reference path="../../node_modules/monaco-editor/monaco.d.ts" />

class MonacoEditorLanguage {
    // register a new language for Monaco Editor.
    public static registerLanguage(languageName: string, dotNetObjRef: DotNet.DotNetObject): void {

        // Register a new language
        monaco.languages.register({ id: languageName });

        // Register a completion item provider for the new language
        MonacoEditorLanguage.registerAutoCompletionSupport(languageName, dotNetObjRef);

        // Register a semantic tokens provider for the new language
        MonacoEditorLanguage.registerSemanticTokenizationSupport(languageName, dotNetObjRef);
    }

    private static registerAutoCompletionSupport(languageName: string, dotNetObjRef: DotNet.DotNetObject): void {
        monaco.languages.registerCompletionItemProvider(
            languageName,
            {
                provideCompletionItems: async function (
                    model: monaco.editor.ITextModel,
                    position: monaco.Position,
                    context: monaco.languages.CompletionContext,
                    token: monaco.CancellationToken) {

                    const word: monaco.editor.IWordAtPosition
                        = model.getWordUntilPosition(position);

                    const range: monaco.IRange = {
                        startLineNumber: position.lineNumber,
                        endLineNumber: position.lineNumber,
                        startColumn: word.startColumn,
                        endColumn: word.endColumn,
                    };

                    const start = model.getOffsetAt(new monaco.Position(range.startLineNumber, range.startColumn));
                    const end = model.getOffsetAt(new monaco.Position(range.endLineNumber, range.endColumn));

                    let modelName: string = model.uri.path;
                    if (modelName.startsWith("/")) {
                        modelName = modelName.substring(1);
                    }

                    // Retrieve auto-completion items from the extension
                    const suggestions: monaco.languages.CompletionItem[]
                        = await dotNetObjRef.invokeMethodAsync("GetAutoCompletionItemsAsync", languageName, modelName, start, end);

                    return {
                        suggestions: suggestions,
                    };
                },
            }
        );
    }

    private static registerSemanticTokenizationSupport(languageName: string, dotNetObjRef: DotNet.DotNetObject): void {
        monaco.languages.registerDocumentSemanticTokensProvider(
            languageName,
            {
                provideDocumentSemanticTokens: async function(
                    model: monaco.editor.ITextModel,
                    lastResultId: string,
                    token: monaco.CancellationToken) {

                    let modelName: string = model.uri.path;
                    if (modelName.startsWith("/")) {
                        modelName = modelName.substring(1);
                    }

                    // Retrieve semantic tokens from the extension
                    const semanticTokens: number[] = await dotNetObjRef.invokeMethodAsync("GetSemanticTokensAsync", languageName, modelName);

                    return {
                        data: new Uint32Array(semanticTokens),
                        resultId: null,
                    };
                },

                getLegend: function(): monaco.languages.SemanticTokensLegend {
                    return MonacoEditorLanguage.getSemanticTokenLegends();
                },

                releaseDocumentSemanticTokens: function (resultId: string): void {
                    // Nothing to do here
                }
            });
    }

    // Returns default semantic token legends as defined in the Monaco Editor documentation.
    // https://code.visualstudio.com/api/language-extensions/semantic-highlight-guide#standard-token-types-and-modifiers
    private static getSemanticTokenLegends(): monaco.languages.SemanticTokensLegend {
        return {
            tokenTypes: [
                "namespace",
                "class",
                "enum",
                "interface",
                "struct",
                "typeParameter",
                "type",
                "parameter",
                "variable",
                "property",
                "enumMember",
                "decorator",
                "event",
                "function",
                "method",
                "macro",
                "label",
                "comment",
                "string",
                "keyword",
                "number",
                "regexp",
                "operator",
            ],
            tokenModifiers: [
                "declaration",
                "definition",
                "readonly",
                "static",
                "deprecated",
                "abstract",
                "async",
                "modification",
                "documentation",
                "defaultLibrary"
            ],
        };
    }
}

export default MonacoEditorLanguage;