var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
//}
var callParentEventAsync = function (editorContext, name, parameters) {
    return editorContext.Accessor.callEvent(name, parameters);
};
var callParentActionWithParameters = function (editorContext, name, parameters) {
    return editorContext.Accessor.callActionWithParameters(name, parameters);
};
var getParentJsonValue = function (editorContext, name) {
    return editorContext.Accessor.getJsonValue(name);
};
var getParentValue = function (editorContext, name) {
    return __awaiter(this, void 0, void 0, function* () {
        var jsonString = yield editorContext.Accessor.getJsonValue(name);
        var obj = JSON.parse(jsonString);
        return obj;
    });
};
const getAccentColorHtmlHex = function (editorContext) {
    return editorContext.Theme.accentColorHtmlHex;
};
const getThemeCurrentThemeName = function (editorContext) {
    return editorContext.Theme.currentThemeName;
};
const getThemeIsHighContrast = function (editorContext) {
    return editorContext.Theme.isHighContrast;
};
//}
//}
//}
///<reference path="../monaco-editor/monaco.d.ts" />
class EditorContext {
    static registerEditorForElement(element, editor) {
        var value = EditorContext.getEditorForElement(element);
        value.editor = editor;
        return value;
    }
    static getEditorForElement(element) {
        var context = EditorContext._editors.get(element);
        if (!context) {
            context = new EditorContext();
            context.htmlElement = element;
            EditorContext._editors.set(element, context);
        }
        return context;
    }
    constructor() {
        this.modifingSelection = false;
        this.contexts = {};
        this.decorations = [];
    }
}
EditorContext._editors = new Map();
///<reference path="../monaco-editor/monaco.d.ts" />
const createMonacoEditor = (basePath, element) => {
    let editorContext = EditorContext.getEditorForElement(element);
    let debug = editorContext.Debug;
    debug.log("Create dynamic style element");
    var head = document.head || document.getElementsByTagName('head')[0];
    var style = document.createElement('style');
    style.id = 'dynamic';
    head.appendChild(style);
    debug.log("Starting Monaco Load");
    window.require.config({ paths: { 'vs': `${basePath}/monaco-editor/min/vs` } });
    window.require(['vs/editor/editor.main'], function () {
        return __awaiter(this, void 0, void 0, function* () {
            yield initializeMonacoEditorAsync(editorContext);
        });
    });
};
const initializeMonacoEditorAsync = (editorContext) => __awaiter(this, void 0, void 0, function* () {
    let debug = editorContext.Debug;
    let accessor = editorContext.Accessor;
    debug.log("Determining if a standard editor or diff view editor should be created.");
    let isDiffView = yield getParentValue(editorContext, "IsDiffViewMode");
    isDiffView = (isDiffView === "true" || isDiffView == true);
    if (isDiffView) {
        yield initializeDiffModeMonacoEditorAsync(editorContext);
    }
    else {
        yield initializeStandardMonacoEditorAsync(editorContext);
    }
    //// Set theme
    debug.log("Getting accessor theme value");
    let theme = yield getParentJsonValue(editorContext, "ActualTheme");
    switch (theme) {
        case "0":
            theme = "Default";
            break;
        case "1":
            theme = "Light";
            break;
        case "2":
            theme = "Dark";
            break;
        default:
            debug.log("Unknown theme");
    }
    debug.log("Current theme value - " + theme);
    if (theme == "Default") {
        debug.log("Loading default theme");
        theme = yield getThemeCurrentThemeName(editorContext);
    }
    debug.log("Changing theme");
    setTheme(editorContext, yield getAccentColorHtmlHex(editorContext));
    changeTheme(editorContext, theme, yield getThemeIsHighContrast(editorContext));
    // Update Monaco Size when we receive a window resize event
    debug.log("Listen for resize events on the window and resize the editor");
    window.addEventListener("resize", () => {
        editorContext.editor.layout();
    });
    // Disable WebView Scrollbar so Monaco Scrollbar can do heavy lifting
    document.body.style.overflow = 'hidden';
    // Callback to Parent that we're loaded
    debug.log("Loaded Monaco");
    accessor.callAction("Loaded");
    debug.log("Ending Monaco Load");
});
const initializeStandardMonacoEditorAsync = (editorContext) => __awaiter(this, void 0, void 0, function* () {
    let debug = editorContext.Debug;
    let accessor = editorContext.Accessor;
    debug.log("Grabbing Monaco Options");
    // link:CodeEditor.Options
    let opt = yield getOptions(editorContext);
    debug.log("Getting Parent Text value");
    // link:CodeEditor.Text
    opt["value"] = yield getParentValue(editorContext, "Text");
    debug.log("Creating Editor");
    const editor = monaco.editor.create(editorContext.htmlElement, opt);
    EditorContext.registerEditorForElement(editorContext.htmlElement, editor);
    editorContext.standaloneEditor = editor;
    debug.log("Getting & Configuring Editor model");
    let model = editor.getModel();
    model.updateOptions({
        bracketColorizationOptions: { enabled: true }
    });
    editorContext.model = model;
    // Listen for Content Changes
    debug.log("Listening for changes in the editor model - " + (!model));
    model.onDidChangeContent((event) => __awaiter(this, void 0, void 0, function* () {
        // link:CodeEditor.Text
        yield accessor.setValue("Text", stringifyForMarshalling(model.getValue()));
    }));
    // Listen for Selection Changes
    debug.log("Listening for changes in the editor selection");
    editor.onDidChangeCursorSelection((event) => __awaiter(this, void 0, void 0, function* () {
        if (!editorContext.modifingSelection) {
            // link:CodeEditor.SelectedText
            yield accessor.setValue("SelectedText", stringifyForMarshalling(model.getValueInRange(event.selection)));
            // link:CodeEditor.SelectedRange of type Selection
            yield accessor.setValueWithType("SelectedRange", stringifyForMarshalling(JSON.stringify(event.selection)), "Selection");
        }
    }));
    // Listen for focus changes.
    debug.log("Listen for focus events");
    editorContext.standaloneEditor.onDidFocusEditorWidget((event) => __awaiter(this, void 0, void 0, function* () {
        accessor.callAction("GotFocus");
    }));
    editorContext.standaloneEditor.onDidBlurEditorWidget((event) => __awaiter(this, void 0, void 0, function* () {
        accessor.callAction("LostFocus");
    }));
});
const initializeDiffModeMonacoEditorAsync = (editorContext) => __awaiter(this, void 0, void 0, function* () {
    let debug = editorContext.Debug;
    let accessor = editorContext.Accessor;
    debug.log("Grabbing Monaco Options");
    // link:CodeEditor.DiffOptions
    let opt = yield getDiffOptions(editorContext);
    debug.log("Creating Diff Editor");
    const editor = monaco.editor.createDiffEditor(editorContext.htmlElement, opt);
    EditorContext.registerEditorForElement(editorContext.htmlElement, editor);
    editorContext.diffEditor = editor;
    debug.log("Getting & Configuring Diff Editor model");
    let originalModel = monaco.editor.createModel("", "text/plain");
    let modifiedModel = monaco.editor.createModel("", "text/plain");
    editor.setModel({
        original: originalModel,
        modified: modifiedModel
    });
    // Listen for focus changes.
    debug.log("Listen for focus events");
    editorContext.diffEditor.getOriginalEditor().onDidFocusEditorWidget((event) => __awaiter(this, void 0, void 0, function* () {
        accessor.callAction("GotFocus");
    }));
    editorContext.diffEditor.getModifiedEditor().onDidFocusEditorWidget((event) => __awaiter(this, void 0, void 0, function* () {
        accessor.callAction("GotFocus");
    }));
    editorContext.diffEditor.getOriginalEditor().onDidBlurEditorWidget((event) => __awaiter(this, void 0, void 0, function* () {
        accessor.callAction("LostFocus");
    }));
    editorContext.diffEditor.getModifiedEditor().onDidBlurEditorWidget((event) => __awaiter(this, void 0, void 0, function* () {
        accessor.callAction("LostFocus");
    }));
});
///<reference path="../monaco-editor/monaco.d.ts" />
const registerHoverProvider = function (editorContext, languageId) {
    return monaco.languages.registerHoverProvider(languageId, {
        provideHover: function (model, position) {
            return callParentEventAsync(editorContext, "HoverProvider" + languageId, [JSON.stringify(position)]).then(result => {
                if (result) {
                    return JSON.parse(result);
                }
            });
        }
    });
};
const addAction = function (editorContext, action) {
    action.run = function (ed) {
        editorContext.Accessor.callAction("Action" + action.id);
    };
    if (editorContext.standaloneEditor != null) {
        editorContext.standaloneEditor.addAction(action);
    }
    else {
        editorContext.diffEditor.addAction(action);
    }
};
const addCommand = function (editorContext, keybindingStr, handlerName, context) {
    if (editorContext.standaloneEditor != null) {
        return editorContext.standaloneEditor.addCommand(parseInt(keybindingStr), function () {
            const objs = [];
            if (arguments) { // Use arguments as Monaco will pass each as it's own parameter, so we don't know how many that may be.
                for (let i = 1; i < arguments.length; i++) { // Skip first one as that's the sender?
                    objs.push(JSON.stringify(arguments[i]));
                }
            }
            editorContext.Accessor.callActionWithParameters(handlerName, objs);
        }, context);
    }
    else {
        return editorContext.diffEditor.addCommand(parseInt(keybindingStr), function () {
            const objs = [];
            if (arguments) { // Use arguments as Monaco will pass each as it's own parameter, so we don't know how many that may be.
                for (let i = 1; i < arguments.length; i++) { // Skip first one as that's the sender?
                    objs.push(JSON.stringify(arguments[i]));
                }
            }
            editorContext.Accessor.callActionWithParameters(handlerName, objs);
        }, context);
    }
};
const createContext = function (editorContext, context) {
    if (context) {
        if (editorContext.standaloneEditor != null) {
            editorContext.contexts[context.key] = editorContext.standaloneEditor.createContextKey(context.key, context.defaultValue);
        }
        else {
            editorContext.contexts[context.key] = editorContext.diffEditor.createContextKey(context.key, context.defaultValue);
        }
    }
};
const updateContext = function (editorContext, key, value) {
    editorContext.contexts[key].set(value);
};
// link:CodeEditor.cs:updateContent
const updateContent = function (editorContext, content) {
    // Need to ignore updates from us notifying of a change
    if (content !== editorContext.model.getValue()) {
        editorContext.model.setValue(content);
    }
};
// link:CodeEditor.cs:updateDiffContent
var updateDiffContent = function (editorContext, left, right) {
    var diffModel = editorContext.model;
    // Need to ignore updates from us notifying of a change
    if (left != diffModel.original.getValue()) {
        diffModel.original.setValue(left);
    }
    if (right != diffModel.modified.getValue()) {
        diffModel.modified.setValue(right);
    }
};
const updateDecorations = function (editorContext, newHighlights) {
    if (editorContext.standaloneEditor != null) {
        if (newHighlights) {
            editorContext.decorations = editorContext.standaloneEditor.deltaDecorations(editorContext.decorations, newHighlights);
        }
        else {
            editorContext.decorations = editorContext.standaloneEditor.deltaDecorations(editorContext.decorations, []);
        }
    }
};
const updateStyle = function (innerStyle) {
    var style = document.getElementById("dynamic");
    style.innerHTML = innerStyle;
};
const getOptions = function (editorContext) {
    let opt = null;
    try {
        opt = getParentValue(editorContext, "Options");
    }
    finally {
    }
    return opt;
};
var getDiffOptions = function (editorContext) {
    let opt = null;
    try {
        opt = getParentValue(editorContext, "DiffOptions");
    }
    finally {
    }
    return opt;
};
const updateOptions = function (editorContext, opt) {
    if (opt !== null && typeof opt === "object") {
        editorContext.editor.updateOptions(opt);
    }
};
var updateDiffOptions = function (editorContext, opt) {
    if (opt !== null && typeof opt === "object") {
        editorContext.diffEditor.updateOptions(opt);
    }
};
const updateLanguage = function (editorContext, language) {
    monaco.editor.setModelLanguage(editorContext.model, language);
};
const changeTheme = function (editorContext, theme, highcontrast) {
    let newTheme = 'devtoys';
    if (highcontrast == "True" || highcontrast == "true") {
        newTheme = 'hc-black';
    }
    else if (theme == "Dark") {
        newTheme = 'devtoys-dark';
    }
    monaco.editor.setTheme(newTheme);
};
var setTheme = function (editorContext, accentColor) {
    // Define themes
    // https://microsoft.github.io/monaco-editor/playground.html#customizing-the-appearence-exposed-colors
    // remove quotes.
    accentColor = accentColor.replace(/"/g, '');
    monaco.editor.defineTheme('devtoys-dark', {
        base: 'vs-dark',
        inherit: true,
        rules: [],
        colors: {
            'foreground': '#FFFFFF',
            'editor.foreground': '#FFFFFF',
            'editor.background': '#00000000',
            'editor.lineHighlightBackground': '#FFFFFF00',
            'editor.lineHighlightBorder': '#FFFFFF19',
            'editorLineNumber.foreground': '#EEEEEE99',
            'editorLineNumber.activeForeground': '#EEEEEEFF',
            'editor.inactiveSelectionBackground': '#00000000',
            'editor.selectionForeground': '#FFFFFF',
            'editor.selectionBackground': accentColor,
            'editorWidget.background': '#252526'
        }
    });
    monaco.editor.defineTheme('devtoys', {
        base: 'vs',
        inherit: true,
        rules: [],
        colors: {
            'foreground': '#000000',
            'editor.foreground': '#000000',
            'editor.background': '#FFFFFF00',
            'editor.lineHighlightBackground': '#00000000',
            'editor.lineHighlightBorder': '#00000019',
            'editorLineNumber.foreground': '#00000099',
            'editorLineNumber.activeForeground': '#000000FF',
            'editor.inactiveSelectionBackground': '#00000000',
            'editor.selectionForeground': '#000000',
            'editor.selectionBackground': accentColor,
            'editorWidget.background': '#F3F3F3'
        }
    });
};
const keyDown = function (element, event) {
    return __awaiter(this, void 0, void 0, function* () {
        //Debug.log("Key Down:" + event.keyCode + " " + event.ctrlKey);
        //const result = await editorContext.Keyboard.keyDown(event.keyCode, event.ctrlKey, event.shiftKey, event.altKey, event.metaKey);
        //if (result) {
        //    event.cancelBubble = true;
        //    event.preventDefault();
        //    event.stopPropagation();
        //    event.stopImmediatePropagation();
        //    return false;
        //}
    });
};
const stringifyForMarshalling = (value) => sanitize(value);
const sanitize = (jsonString) => {
    if (jsonString == null) {
        //console.log('Sanitized is null');
        return null;
    }
    const replacements = "%&\\\"'{}:,";
    for (let i = 0; i < replacements.length; i++) {
        jsonString = replaceAll(jsonString, replacements.charAt(i), `%${replacements.charCodeAt(i)}`);
    }
    //console.log('Sanitized: ' + jsonString);
    return jsonString;
};
const replaceAll = (str, find, rep) => {
    if (find == "\\") {
        find = "\\\\";
    }
    return (`${str}`).replace(new RegExp(find, "g"), rep);
};
///<reference path="../monaco-editor/monaco.d.ts" />
function isTextEdit(edit) {
    return edit.edit !== undefined;
}
const registerCodeActionProvider = function (editorContext, languageId) {
    return monaco.languages.registerCodeActionProvider(languageId, {
        provideCodeActions: function (model, range, context, token) {
            return callParentEventAsync(editorContext, "ProvideCodeActions" + languageId, [JSON.stringify(range), JSON.stringify(context)]).then(result => {
                if (result) {
                    const list = JSON.parse(result);
                    // Need to add in the model.uri to any edits to connect the dots
                    if (list.actions &&
                        list.actions.length > 0) {
                        list.actions.forEach((action) => {
                            if (action.edit &&
                                action.edit.edits &&
                                action.edit.edits.length > 0) {
                                action.edit.edits.forEach((inneredit) => {
                                    if (isTextEdit(inneredit)) {
                                        inneredit.resource = model.uri;
                                    }
                                });
                            }
                        });
                    }
                    // Add dispose method for IDisposable that Monaco is looking for.
                    list.dispose = () => { };
                    return list;
                }
            });
        },
    });
};
///<reference path="../monaco-editor/monaco.d.ts" />
const registerCodeLensProvider = function (editorContext, any, languageId) {
    return monaco.languages.registerCodeLensProvider(languageId, {
        provideCodeLenses: function (model, token) {
            return callParentEventAsync(editorContext, "ProvideCodeLenses" + languageId, []).then(result => {
                if (result) {
                    const list = JSON.parse(result);
                    // Add dispose method for IDisposable that Monaco is looking for.
                    list.dispose = () => { };
                    return list;
                }
                return null;
            });
        },
        resolveCodeLens: function (model, codeLens, token) {
            return callParentEventAsync(editorContext, "ResolveCodeLens" + languageId, [JSON.stringify(codeLens)]).then(result => {
                if (result) {
                    return JSON.parse(result);
                }
                return null;
            });
        }
        // TODO: onDidChange, don't know what this does.
    });
};
///<reference path="../monaco-editor/monaco.d.ts" />
const registerColorProvider = function (editorContext, languageId) {
    return monaco.languages.registerColorProvider(languageId, {
        provideColorPresentations: function (model, colorInfo, token) {
            return callParentEventAsync(editorContext, "ProvideColorPresentations" + languageId, [JSON.stringify(colorInfo)]).then(result => {
                if (result) {
                    return JSON.parse(result);
                }
            });
        },
        provideDocumentColors: function (model, token) {
            return callParentEventAsync(editorContext, "ProvideDocumentColors" + languageId, []).then(result => {
                if (result) {
                    return JSON.parse(result);
                }
            });
        }
    });
};
///<reference path="../monaco-editor/monaco.d.ts" />
const registerCompletionItemProvider = function (editorContext, languageId, characters) {
    return monaco.languages.registerCompletionItemProvider(languageId, {
        triggerCharacters: characters,
        provideCompletionItems: function (model, position, context, token) {
            return callParentEventAsync(editorContext, "CompletionItemProvider" + languageId, [JSON.stringify(position), JSON.stringify(context)]).then(result => {
                if (result) {
                    const list = JSON.parse(result);
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
};
///<reference path="../monaco-editor/monaco.d.ts" />
// link:CodeEditor.Properties.cs:updateSelectedContent
const updateSelectedContent = function (element, content) {
    var editorContext = EditorContext.getEditorForElement(element);
    let selection = editorContext.editor.getSelection();
    // Need to ignore updates from us notifying of a change
    if (content != editorContext.model.getValueInRange(selection)) {
        editorContext.modifingSelection = true;
        let range = new monaco.Range(selection.startLineNumber, selection.startColumn, selection.endLineNumber, selection.endColumn);
        let op = { identifier: { major: 1, minor: 1 }, range, text: content, forceMoveMarkers: true };
        // Make change to selection
        //TODO how to properly fix this code?
        //model.pushEditOperations([], [op]);
        editorContext.model.pushEditOperations([], [op], null);
        // Update selection to new text.
        const newEndLineNumber = selection.startLineNumber + content.split('\r').length - 1; // TODO: Not sure if line end is situational/platform specific... investigate more.
        const newEndColumn = (selection.startLineNumber === selection.endLineNumber)
            ? selection.startColumn + content.length
            : content.length - content.lastIndexOf('\r');
        selection = selection.setEndPosition(newEndLineNumber, newEndColumn);
        // Update other selection bound for direction.
        //TODO how to properly fix this code?
        selection = selection.setEndPosition(selection.endLineNumber, selection.endColumn);
        //if (selection.getDirection() == monaco.SelectionDirection.LTR) {
        //    selection.positionColumn = selection.endColumn;
        //    selection.positionLineNumber = selection.endLineNumber;
        //} else {
        //    selection.selectionStartColumn = selection.endColumn;
        //    selection.selectionStartLineNumber = selection.endLineNumber;
        //}
        editorContext.modifingSelection = false;
        editorContext.editor.setSelection(selection);
    }
};
//# sourceMappingURL=uno-monaco-helpers.g.js.map