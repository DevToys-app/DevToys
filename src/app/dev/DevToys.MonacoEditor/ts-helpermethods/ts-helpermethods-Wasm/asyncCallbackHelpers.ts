declare var Parent: ParentAccessor;
declare var Theme: ThemeAccessor;

type MethodWithReturnId = (parameter: string) => void;
type NumberCallback = (parameter: any) => void;
declare var asyncCallbackMap: { [promiseId: string]: NumberCallback };
declare var nextAsync: number;

nextAsync = 1;
asyncCallbackMap = {};

declare var returnValueCallbackMap: { [returnId: string]: string };
declare var nextReturn: number;

nextReturn = 1;
returnValueCallbackMap = {};

const asyncCallback = (promiseId: string, parameter: string) => {
    const promise = asyncCallbackMap[promiseId];
    if (promise) {
        //console.log('Async response: ' + parameter);
        promise(parameter);
    }
}

const returnValueCallback = (returnId: string, returnValue: string) => {
    //console.log('Return value for id ' + returnId + ' is ' + returnValue);
    returnValueCallbackMap[returnId] = returnValue;
}

const invokeAsyncMethod = <T>(syncMethod: NumberCallback): Promise<T> => {
    if (nextAsync == null) {
        nextAsync = 0;
    }
    if (asyncCallbackMap == null) {
        asyncCallbackMap = {};
    }
    const promise = new Promise<T>((resolve, reject) => {
        var nextId = nextAsync++;
        asyncCallbackMap[nextId] = resolve;
        syncMethod(`${nextId}`);
    });
    return promise;
}

const desantize = (parameter: string): string => {
    //System.Diagnostics.Debug.WriteLine($"Encoded String: {parameter}");
    if (parameter == null) return parameter;
    const replacements = "&\\\"'{}:,%";
    //System.Diagnostics.Debug.WriteLine($"Replacements: >{replacements}<");
    for (let i = 0; i < replacements.length; i++) {
        //console.log("Replacing: >%" + replacements.charCodeAt(i) + "< with >" + replacements.charAt(i) + "< ");
        parameter = replaceAll(parameter, "%" + replacements.charCodeAt(i), replacements.charAt(i));
    }

    //console.log("Decoded String: " + parameter );
    return parameter;
}

const invokeWithReturnValue = (methodToInvoke: MethodWithReturnId): string => {
    const nextId = nextReturn++;
    methodToInvoke(nextId + '');
    var json = returnValueCallbackMap[nextId];
    //console.log('Return json ' + json);
    json = desantize(json);
    return json;
}

const getParentValue = (editorContext: EditorContext, name: string): Promise<any> => {
    return new Promise((resolve, reject) => {
        const jsonString = invokeWithReturnValue((returnId) => editorContext.Accessor.getJsonValue(name, returnId));
        const obj = JSON.parse(jsonString);
        resolve(obj);
    });
}

const getParentJsonValue = function (editorContext: EditorContext, name: string): Promise<string> {
    return new Promise((resolve, reject) => {
        const json = invokeWithReturnValue((returnId) => editorContext.Accessor.getJsonValue(name, returnId));
        resolve(json);
    });
}

const callParentEventAsync = (editorContext: EditorContext, name: string, parameters: string[]): Promise<string> =>
    invokeAsyncMethod<string>(async (promiseId) => {
        let result = await editorContext.Accessor.callEvent(name,
            promiseId,
            parameters != null && parameters.length > 0 ? stringifyForMarshalling(parameters[0]) : null,
            parameters != null && parameters.length > 1 ? stringifyForMarshalling(parameters[1]) : null);
        if (result) {
            console.log('Parent event result: ' + name + ' -  ' + result);
            result = desantize(result);
            console.log('Desanitized: ' + name + ' -  ' + result);
        } else {
            console.log('No Parent event result for ' + name);
        }

        return result;
    });

const callParentActionWithParameters = (name: string, parameters: string[]): boolean =>
    Parent.callActionWithParameters(name,
        parameters != null && parameters.length > 0 ? stringifyForMarshalling(parameters[0]) : null,
        parameters != null && parameters.length > 1 ? stringifyForMarshalling(parameters[1]) : null);

const getAccentColorHtmlHex = function (editorContext: EditorContext): Promise<string> {
    return new Promise((resolve, reject) => {
        resolve(invokeWithReturnValue((returnId) => editorContext.Theme.getAccentColorHtmlHex(returnId)));
    });
}

const getThemeCurrentThemeName = function (editorContext: EditorContext): Promise<string> {
    return new Promise((resolve, reject) => {
        resolve(invokeWithReturnValue((returnId) => editorContext.Theme.getCurrentThemeName(returnId)));
    });
}

const getThemeIsHighContrast = function (editorContext: EditorContext): Promise<boolean> {
    return new Promise((resolve, reject) => {
        resolve(invokeWithReturnValue((returnId) => editorContext.Theme.getIsHighContrast(returnId)) == "true");
    });
}