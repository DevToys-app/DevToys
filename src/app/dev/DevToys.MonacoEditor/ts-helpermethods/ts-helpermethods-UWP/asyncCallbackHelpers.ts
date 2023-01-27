var callParentEventAsync = function (editorContext: EditorContext, name: string, parameters: string[]): Promise<string> {
    return editorContext.Accessor.callEvent(name, parameters);
}

var callParentActionWithParameters = function (editorContext: EditorContext, name: string, parameters: string[]): boolean {
    return editorContext.Accessor.callActionWithParameters(name, parameters);
}

var getParentJsonValue = function (editorContext: EditorContext, name: string): Promise<string> {
    return editorContext.Accessor.getJsonValue(name);
}

var getParentValue = async function (editorContext: EditorContext, name: string): Promise<any> {
    var jsonString = await editorContext.Accessor.getJsonValue(name);
    var obj = JSON.parse(jsonString);
    return obj;
}

const getAccentColorHtmlHex = function (editorContext: EditorContext): Promise<string> {
    return editorContext.Theme.accentColorHtmlHex;
}

const getThemeCurrentThemeName = function (editorContext: EditorContext): Promise<string> {
    return editorContext.Theme.currentThemeName;
}

const getThemeIsHighContrast = function (editorContext: EditorContext): Promise<boolean> {
    return editorContext.Theme.isHighContrast;
}