//namespace DevToys.MonacoEditor.Helpers {
    interface ParentAccessor {
        callAction(name: string): boolean;
        callEvent(name: string, parameters: string[]): Promise<string>;
        close();
        getChildValue(name: string, child: string): any;
        getJsonValue(name: string): string;
        getValue(name: string): any;
        setValue(name: string, value: any);
        setValue(name: string, value: string, type: string);
    }
//}