///<reference path="../monaco-editor/monaco.d.ts" />
declare var Parent: ParentAccessor;

var registerCodeLensProvider = function (languageId) {
    return monaco.languages.registerCodeLensProvider(languageId, {
        provideCodeLenses: function (model, token) {
            return callParentEventAsync("ProvideCodeLenses" + languageId, null).then(result => {
                if (result) {
                    return JSON.parse(result);
                }
                return null;

            });
        },
        resolveCodeLens: function (model, codeLens, token) {
            return callParentEventAsync("ResolveCodeLens" + languageId, [JSON.stringify(codeLens)]).then(result => {
                if (result) {
                    return JSON.parse(result);
                }
                return null;
            });
        }
        // TODO: onDidChange, don't know what this does.
    });
}