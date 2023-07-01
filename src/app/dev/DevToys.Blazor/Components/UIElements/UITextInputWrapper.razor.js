export function registerResizeHandler(id, dotNetObjRef) {
    const element = document.getElementById(id);
    // Setup resize observer.
    const resizeObserver = new ResizeObserver((elements) => {
        dotNetObjRef.invokeMethodAsync("OnComponentResize", Math.trunc(elements[0].contentRect.width));
    });
    resizeObserver.observe(element);
    element.resizeObserver = resizeObserver;
}
export function registerDropZone(id) {
    const element = document.getElementById(id);
    // Prevent default drag behaviors
    ["dragenter", "dragover", "dragleave", "drop"].forEach(eventName => {
        element.addEventListener(eventName, preventDefaults, false);
    });
    // Highlight drop area when item is dragged over it
    ["dragenter", "dragover"].forEach(eventName => {
        element.addEventListener(eventName, highlight, false);
    });
    ["dragleave", "drop"].forEach(eventName => {
        element.addEventListener(eventName, unhighlight, false);
    });
    // Handle dropped files
    element.addEventListener("drop", handleDrop, false);
}
export function dispose(id) {
    const element = document.getElementById(id);
    // Stop resize observer.
    element.resizeObserver.disconnect();
    // Stop drop zone
    ["dragenter", "dragover", "dragleave", "drop"].forEach(eventName => {
        element.removeEventListener(eventName, preventDefaults, false);
    });
    ["dragenter", "dragover"].forEach(eventName => {
        element.removeEventListener(eventName, highlight, false);
    });
    ["dragleave", "drop"].forEach(eventName => {
        element.removeEventListener(eventName, unhighlight, false);
    });
}
function preventDefaults(e) {
    e.preventDefault();
    e.stopPropagation();
}
function highlight(e) {
    if (e.dataTransfer.items.length === 1 && e.dataTransfer.items[0].kind === "file") {
        e.dataTransfer.dropEffect = "copy";
        e.target.classList.add("dragging");
    }
    else {
        e.dataTransfer.dropEffect = "none";
    }
}
function unhighlight(e) {
    e.target.classList.remove("dragging");
}
function handleDrop(e) {
    const dt = e.dataTransfer;
    const files = dt.files;
    const element = e.target;
    if (files.length === 1) {
        const inputFileElement = element.querySelector("input[type=file]");
        if (inputFileElement === null) {
            throw new Error("");
        }
        inputFileElement.files = files;
        const event = new Event("change", { bubbles: true });
        inputFileElement.dispatchEvent(event);
    }
}
//# sourceMappingURL=UITextInputWrapper.razor.js.map