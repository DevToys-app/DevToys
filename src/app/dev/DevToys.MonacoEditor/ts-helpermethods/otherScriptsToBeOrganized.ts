///<reference path="../monaco-editor/monaco.d.ts" />

const registerHoverProvider = function (editorContext: EditorContext, languageId: string) {
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

const addAction = function (editorContext: EditorContext, action: monaco.editor.IActionDescriptor) {
    action.run = function (ed) {
        editorContext.Accessor.callAction("Action" + action.id)
    };

    if (editorContext.standaloneEditor != null) {
        editorContext.standaloneEditor.addAction(action);
    }
    else {
        editorContext.diffEditor.addAction(action);
    }
};

const addCommand = function (editorContext: EditorContext, keybindingStr, handlerName, context) {
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

const createContext = function (editorContext: EditorContext, context) {
    if (context) {
        if (editorContext.standaloneEditor != null) {
            editorContext.contexts[context.key] = editorContext.standaloneEditor.createContextKey(context.key, context.defaultValue);
        }
        else {
            editorContext.contexts[context.key] = editorContext.diffEditor.createContextKey(context.key, context.defaultValue);
        }
    }
};

const updateContext = function (editorContext: EditorContext, key, value) {
    editorContext.contexts[key].set(value);
}

// link:CodeEditor.cs:updateContent
const updateContent = function (editorContext: EditorContext, content) {
    // Need to ignore updates from us notifying of a change
    if (content !== editorContext.model.getValue()) {
        editorContext.model.setValue(content);
    }
};

// link:CodeEditor.cs:updateDiffContent
var updateDiffContent = function (editorContext: EditorContext, left, right) {
    var diffModel = (editorContext.model as unknown) as monaco.editor.IDiffEditorModel;

    // Need to ignore updates from us notifying of a change
    if (left != diffModel.original.getValue()) {
        diffModel.original.setValue(left);
    }

    if (right != diffModel.modified.getValue()) {
        diffModel.modified.setValue(right);
    }
};

const updateDecorations = function (editorContext: EditorContext, newHighlights) {
    if (editorContext.standaloneEditor != null) {
        if (newHighlights) {
            editorContext.decorations = editorContext.standaloneEditor.deltaDecorations(editorContext.decorations, newHighlights);
        } else {
            editorContext.decorations = editorContext.standaloneEditor.deltaDecorations(editorContext.decorations, []);
        }
    }
};

const updateStyle = function (editorContext: EditorContext, innerStyle) {
    var style = document.getElementById("dynamic");
    style.innerHTML = innerStyle;
};

const getOptions = function (editorContext: EditorContext): Promise<monaco.editor.IEditorOptions> {
    let opt = null;
    try {
        opt = getParentValue(editorContext, "Options");
    } finally {

    }

    return opt;
};

var getDiffOptions = function (editorContext: EditorContext): monaco.editor.IDiffEditorOptions {
    let opt = null;
    try {
        opt = getParentValue(editorContext, "DiffOptions");
    } finally {

    }

    return opt;
};

const updateOptions = function (editorContext: EditorContext, opt: monaco.editor.IEditorOptions) {
    if (opt !== null && typeof opt === "object") {
        editorContext.editor.updateOptions(opt);
    }
};

var updateDiffOptions = function (editorContext: EditorContext, opt: monaco.editor.IDiffEditorOptions) {
    if (opt !== null && typeof opt === "object") {
        editorContext.diffEditor.updateOptions(opt);
    }
};

const updateLanguage = function (editorContext: EditorContext, language: string) {
    monaco.editor.setModelLanguage(editorContext.model, language);
};

var _theme = "Dark";
var _highContrast = false;

const changeTheme = function (editorContext: EditorContext, theme: string, highContrast: boolean, hasFocus: boolean) {
    if (theme == undefined) {
        theme = _theme;
    }

    if (highContrast == undefined) {
        highContrast = _highContrast;
    }

    let state = hasFocus ? "active" : "inactive";
    let newTheme = 'devtoys-' + state;

    if (highContrast) {
        newTheme = 'hc-black';
    } else if (theme == "Dark") {
        newTheme = 'devtoys-dark-' + state;
    }

    _theme = theme;
    _highContrast = highContrast;

    monaco.editor.setTheme(newTheme);
};

var setTheme = function (editorContext: EditorContext, accentColor: string) {
    // Define themes
    // https://microsoft.github.io/monaco-editor/playground.html#customizing-the-appearence-exposed-colors

    // remove quotes.
    accentColor = accentColor.replace(/"/g, '');

    monaco.editor.defineTheme('devtoys-dark-active', {
        base: 'vs-dark',
        inherit: true,
        rules: [],
        colors: {
            'foreground': '#FFFFFF',
            'editor.foreground': '#FFFFFF',
            'editor.background': '#202123',
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

    monaco.editor.defineTheme('devtoys-dark-inactive', {
        base: 'vs-dark',
        inherit: true,
        rules: [],
        colors: {
            'foreground': '#FFFFFF',
            'editor.foreground': '#FFFFFF',
            'editor.background': '#343434',
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

    monaco.editor.defineTheme('devtoys-active', {
        base: 'vs',
        inherit: true,
        rules: [],
        colors: {
            'foreground': '#000000',
            'editor.foreground': '#000000',
            'editor.background': '#FFFFFF',
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

    monaco.editor.defineTheme('devtoys-inactive', {
        base: 'vs',
        inherit: true,
        rules: [],
        colors: {
            'foreground': '#000000',
            'editor.foreground': '#000000',
            'editor.background': '#FDFDFD',
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
}

const keyDown = async function (element: any, event) {
    //Debug.log("Key Down:" + event.keyCode + " " + event.ctrlKey);
    //const result = await editorContext.Keyboard.keyDown(event.keyCode, event.ctrlKey, event.shiftKey, event.altKey, event.metaKey);
    //if (result) {
    //    event.cancelBubble = true;
    //    event.preventDefault();
    //    event.stopPropagation();
    //    event.stopImmediatePropagation();
    //    return false;
    //}
};


const stringifyForMarshalling = (value: any): string => sanitize(value)

const sanitize = (jsonString: string): string => {
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
}

const replaceAll = (str: string, find: string, rep: string): string => {
    if (find == "\\") {
        find = "\\\\";
    }
    return (`${str}`).replace(new RegExp(find, "g"), rep);
}