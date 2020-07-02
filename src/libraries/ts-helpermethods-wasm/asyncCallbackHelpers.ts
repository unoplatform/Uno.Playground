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

var asyncCallback = function (promiseId: string, parameter: string) {
    var promise = asyncCallbackMap[promiseId];
    if (promise) {
        //console.log('Async response: ' + parameter);
        promise(parameter);
    }
}

var returnValueCallback = function (returnId: string, returnValue: string) {
    //console.log('Return value for id ' + returnId + ' is ' + returnValue);
    returnValueCallbackMap[returnId] = returnValue;
}

var invokeAsyncMethod = function <T> (syncMethod: NumberCallback) : Promise<T>
{
    if (nextAsync==null) {
        nextAsync = 0;
    }
    if (asyncCallbackMap==null) {
        asyncCallbackMap = {};
    }
    var promise = new Promise<T>((resolve, reject) => {
        var nextId = nextAsync++;
        asyncCallbackMap[nextId] = resolve;
        syncMethod(nextId?.toString());
    });
    return promise;
}

var replaceAll = function (str: string, find: string, rep: string): string {
    if (find == "\\")
    {
        find = "\\\\";
    }
    return (str+'').replace(new RegExp(find, 'g'), rep);
}

var sanitize = function (jsonString: string): string {
    if (jsonString == null) {
        //console.log('Sanitized is null');
        return null;
    }

    var replacements = "%&\\\"'{}:,";
    for (var i = 0; i < replacements.length; i++) {
        jsonString = replaceAll(jsonString, replacements.charAt(i), "%" + replacements.charCodeAt(i));
    }
    //console.log('Sanitized: ' + jsonString);
    return jsonString;
}

var desantize = function (parameter: string): string {
    //System.Diagnostics.Debug.WriteLine($"Encoded String: {parameter}");
    if (parameter == null) return parameter;
    var replacements = "&\\\"'{}:,%";
    //System.Diagnostics.Debug.WriteLine($"Replacements: >{replacements}<");
    for (var i = 0; i < replacements.length; i++)
    {
        //console.log("Replacing: >%" + replacements.charCodeAt(i) + "< with >" + replacements.charAt(i) + "< ");
        parameter = replaceAll(parameter, "%" + replacements.charCodeAt(i), replacements.charAt(i));
    }

    //console.log("Decoded String: " + parameter );
    return parameter;
}

var stringifyForMarshalling=function (value: any): string {
    return sanitize(value);
}

var invokeWithReturnValue = function (methodToInvoke: MethodWithReturnId): string {
    var nextId = nextReturn++;
    methodToInvoke(nextId + '');
    var json = returnValueCallbackMap[nextId];
    //console.log('Return json ' + json);
    json = desantize(json);
    return json;
}

var getParentValue = function (name: string): any {
    var jsonString = invokeWithReturnValue((returnId) => Parent.getJsonValue(name, returnId));
    var obj = JSON.parse(jsonString);
    return obj;
}

var getParentJsonValue = function (name: string): string {
    return invokeWithReturnValue((returnId) => Parent.getJsonValue(name, returnId));

    //var nextId = nextReturn++;


    //console.log('Getting parent json for ' + name + ' with id ' + nextId);
    //var json = Parent.getJsonValue(name, nextId+'');
    //console.log('Parent json ' + json);
    //json = returnValueCallbackMap[nextId];
    //console.log('Parent Return json ' + json);

    ////json = desantize(json);
    ////console.log('Parent json (desanitized) ' + json);
    //return json;
}

var getThemeIsHighContrast = function (): boolean {
    return invokeWithReturnValue((returnId) => Theme.getIsHighContrast(returnId))=="true";
}

var getThemeCurrentThemeName = function (): string {
    return invokeWithReturnValue((returnId) => Theme.getCurrentThemeName(returnId));
}


var callParentEventAsync = function (name: string, parameters: string[]): Promise<string>  {
    return invokeAsyncMethod<string>((promiseId) => Parent.callEvent(name, promiseId,
        parameters!=null && parameters.length > 0 ? stringifyForMarshalling(parameters[0]) : null,
        parameters != null && parameters.length > 1 ? stringifyForMarshalling(parameters[1]) : null))
        .then(result => {
        if (result) {
            //console.log('Parent event result: ' + name + ' -  ' +  result);
            result = desantize(result);
            //console.log('Desanitized: ' + name + ' -  ' + result);
        }
        else {
            //console.log('No Parent event result for ' + name);
            }

        return result;
    });
}

var callParentActionWithParameters = function (name: string, parameters: string[]): boolean {
    return Parent.callActionWithParameters(name,
        parameters != null && parameters.length > 0 ? stringifyForMarshalling(parameters[0]) : null,
        parameters != null && parameters.length > 1 ? stringifyForMarshalling(parameters[1]) : null);

}
