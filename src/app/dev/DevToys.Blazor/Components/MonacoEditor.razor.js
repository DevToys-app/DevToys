// Initialize Monaco Editor
export function monacoInitialize(id, dotNetHelper, options) {
    // require is provided by loader.min.js.
    require.config({
        paths: { 'vs': options.path }
    });

    require(["vs/editor/editor.main"], () => {
        var container = document.getElementById(id);

        // Hide the Progress Ring
        monaco.editor.onDidCreateEditor((e) => {
            var progress = document.getElementById(id + "-loading");
            if (progress && progress.style) {
                progress.style.display = "none";
            }
        });

        // Create the Monaco Editor
        container.editor = monaco.editor.create(container, {
            ariaLabel: "online code editor",
            value: options.value,
            language: options.language,
            theme: options.theme,
            lineNumbers: options.lineNumbers ? "on" : "off",
            readOnly: options.readOnly,
        });

        // Keep a reference of dotNetHelper
        container.dotNetHelper = dotNetHelper;

        // Catch when the editor lost the focus (didType to immediate)
        container.editor.onDidBlurEditorText((e) => {
            var code = container.editor.getValue();
            container.dotNetHelper.invokeMethodAsync("UpdateValueAsync", code);
        });

        container.editor.updateOptions(options);

        window.editor = container.editor;   // To debug
    });
}

// Update the editor options
export function monacoSetOptions(id, options) {
    var container = document.getElementById(id);
    container.editor.setValue(options.value);
    container.editor.updateOptions({
        language: options.language,
        theme: options.theme
    });
}