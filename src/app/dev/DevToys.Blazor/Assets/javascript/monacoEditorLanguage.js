// eslint-disable-next-line @typescript-eslint/triple-slash-reference
///<reference path="../../node_modules/monaco-editor/monaco.d.ts" />
var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
class MonacoEditorLanguage {
    // register a new language for Monaco Editor.
    static registerLanguage(languageName, dotNetObjRef) {
        // Register a new language
        monaco.languages.register({ id: languageName });
        // Register a completion item provider for the new language
        MonacoEditorLanguage.registerAutoCompletionSupport(languageName, dotNetObjRef);
        // Register a semantic tokens provider for the new language
        MonacoEditorLanguage.registerSemanticTokenizationSupport(languageName, dotNetObjRef);
    }
    static registerAutoCompletionSupport(languageName, dotNetObjRef) {
        monaco.languages.registerCompletionItemProvider(languageName, {
            provideCompletionItems: function (model, position, context, token) {
                return __awaiter(this, void 0, void 0, function* () {
                    const word = model.getWordUntilPosition(position);
                    const range = {
                        startLineNumber: position.lineNumber,
                        endLineNumber: position.lineNumber,
                        startColumn: word.startColumn,
                        endColumn: word.endColumn,
                    };
                    const start = model.getOffsetAt(new monaco.Position(range.startLineNumber, range.startColumn));
                    const end = model.getOffsetAt(new monaco.Position(range.endLineNumber, range.endColumn));
                    let modelName = model.uri.path;
                    if (modelName.startsWith("/")) {
                        modelName = modelName.substring(1);
                    }
                    // Retrieve auto-completion items from the extension
                    const suggestions = yield dotNetObjRef.invokeMethodAsync("GetAutoCompletionItemsAsync", languageName, modelName, start, end);
                    return {
                        suggestions: suggestions,
                    };
                });
            },
        });
    }
    static registerSemanticTokenizationSupport(languageName, dotNetObjRef) {
        monaco.languages.registerDocumentSemanticTokensProvider(languageName, {
            provideDocumentSemanticTokens: function (model, lastResultId, token) {
                return __awaiter(this, void 0, void 0, function* () {
                    let modelName = model.uri.path;
                    if (modelName.startsWith("/")) {
                        modelName = modelName.substring(1);
                    }
                    // Retrieve semantic tokens from the extension
                    const semanticTokens = yield dotNetObjRef.invokeMethodAsync("GetSemanticTokensAsync", languageName, modelName);
                    return {
                        data: new Uint32Array(semanticTokens),
                        resultId: null,
                    };
                });
            },
            getLegend: function () {
                return MonacoEditorLanguage.getSemanticTokenLegends();
            },
            releaseDocumentSemanticTokens: function (resultId) {
                // Nothing to do here
            }
        });
    }
    // Returns default semantic token legends as defined in the Monaco Editor documentation.
    // https://code.visualstudio.com/api/language-extensions/semantic-highlight-guide#standard-token-types-and-modifiers
    static getSemanticTokenLegends() {
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
//# sourceMappingURL=monacoEditorLanguage.js.map