///<reference path="../monaco-editor/monaco.d.ts" />
declare var Parent: ParentAccessor;
declare var Keyboard: KeyboardListener;

declare var editor: monaco.editor.IStandaloneCodeEditor;
declare var model: monaco.editor.ITextModel;
declare var contexts: { [index: string]: monaco.editor.IContextKey<any> };//{};
declare var decorations: string[];
declare var modifingSelection:boolean; // Supress updates to selection when making edits.

var registerHoverProvider = function (languageId: string) {
    return monaco.languages.registerHoverProvider(languageId, {
        provideHover: function (model, position) {
            return Parent.callEvent("HoverProvider" + languageId, [JSON.stringify(position)]).then(result => {
                if (result) {
                    return JSON.parse(result);
                }
            });
        }
    });
}

var addAction = function (action: monaco.editor.IActionDescriptor) {
    action.run = function (ed) {
        Parent.callAction("Action" + action.id)
    };

    editor.addAction(action);
};

var addCommand = function (keybindingStr, handlerName, context) {
    return editor.addCommand(parseInt(keybindingStr), () => {
        Parent.callAction(handlerName);
    }, context);
};

var createContext = function (context) {
    if (context) {
        contexts[context.key] = editor.createContextKey(context.key, context.defaultValue);
    }
};

var updateContext = function (key, value) {
    contexts[key].set(value);
}

var updateContent = function (content) {
    // Need to ignore updates from us notifying of a change
    if (content != model.getValue()) {
        model.setValue(content);
    }
};

var updateDecorations = function (newHighlights) {
    if (newHighlights) {
        decorations = editor.deltaDecorations(decorations, newHighlights);
    } else {
        decorations = editor.deltaDecorations(decorations, []);
    }
};

var updateStyle = function (innerStyle) {
    var style = document.getElementById("dynamic");
    style.innerHTML = innerStyle;
};

var getOptions = function (): monaco.editor.IEditorOptions {
    let opt = null;
    try {
        opt = JSON.parse(Parent.getJsonValue("Options"));
    } finally {

    }

    if (opt != null && typeof opt === "object") {
        return opt;
    }

    return {};
};

var updateOptions = function (opt: monaco.editor.IEditorOptions) {
    if (opt != null && typeof opt === "object") {
        editor.updateOptions(opt);
    }
};

var updateLanguage = function (language) {
    monaco.editor.setModelLanguage(model, language);
};

var changeTheme = function (theme: string, highcontrast) {
    var newTheme = 'vs';
    if (highcontrast == "True" || highcontrast == "true") {
        newTheme = 'hc-black';
    } else if (theme == "Dark") {
        newTheme = 'vs-dark';
    }

    monaco.editor.setTheme(newTheme);
};

var setTheme = function (accentColor: string) {
    // Define themes
    // https://microsoft.github.io/monaco-editor/playground.html#customizing-the-appearence-exposed-colors
    monaco.editor.defineTheme('vs-dark', {
        base: 'vs-dark',
        inherit: true,
        rules: [],
        colors: {
            'foreground': '#FFFFFF',
            'editor.foreground': '#FFFFFF',
            'editor.background': '#00000000',
            'editor.lineHighlightBackground': '#FFFFFF19',
            'editorLineNumber.foreground': '#EEEEEE99',
            'editorLineNumber.activeForeground': '#EEEEEE99',
            'editor.inactiveSelectionBackground': '#00000000',
            'editor.selectionForeground': '#FFFFFF',
            'editor.selectionBackground': accentColor,
        }
    });
    monaco.editor.defineTheme('vs', {
        base: 'vs',
        inherit: true,
        rules: [],
        colors: {
            'foreground': '#000000',
            'editor.foreground': '#000000',
            'editor.background': '#FFFFFF00',
            'editor.lineHighlightBackground': '#00000019',
            'editorLineNumber.foreground': '#00000099',
            'editorLineNumber.activeForeground': '#00000099',
            'editor.inactiveSelectionBackground': '#00000000',
            'editor.selectionForeground': '#000000',
            'editor.selectionBackground': accentColor,
        }
    });
}

var keyDown = function (event) {
    //Debug.log("Key Down:" + event.keyCode + " " + event.ctrlKey);
    var result = Keyboard.keyDown(event.keyCode, event.ctrlKey, event.shiftKey, event.altKey, event.metaKey);
    if (result) {
        event.cancelBubble = true;
        event.preventDefault();
        event.stopPropagation();
        event.stopImmediatePropagation();
        return false;
    }
};