//namespace DevToys.MonacoEditor.Monaco.Helpers {
interface ParentAccessor {
    callAction(name: string): boolean;
    callActionWithParameters(name: string, parameters: string[]): boolean;
    callEvent(name: string, parameters: string[]): Promise<string>
    close();
    getChildValue(name: string, child: string): Promise<any>;
    getJsonValue(name: string): Promise<string>;
    getValue(name: string): Promise<any>;
    setValue(name: string, value: any): Promise<undefined>;
    setValueWithType(name: string, value: string, type: string): Promise<undefined>;
    callActionWithParameters(name: string, parameter1: string, parameter2: string): boolean;
    callEvent(name: string, callbackMethod: string, parameter1: string, parameter2: string);
    getJsonValue(name: string, returnId: string);
}

//}