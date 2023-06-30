/* eslint-disable no-var */
export function registerResizeHandlerAndSetupDropZone(id: string, dotNetObjRef: DotNet.DotNetObject): void {
    const element = document.getElementById(id);

    // Setup resize observer.
    const resizeObserver = new ResizeObserver((elements) => {
        dotNetObjRef.invokeMethodAsync("OnComponentResize", Math.trunc(elements[0].contentRect.width));
    });
    resizeObserver.observe(element);
    (<any>element).resizeObserver = resizeObserver;

    // Setup drop zone.

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

export function dispose(id: string): void {
    const element = document.getElementById(id);

    // Stop resize observer.
    (<ResizeObserver>(<any>element).resizeObserver).disconnect();

    // Stop drop zone
    ["dragenter", "dragover", "dragleave", "drop"].forEach(eventName => {
        element.removeEventListener(eventName, preventDefaults, false);
        document.body.removeEventListener(eventName, preventDefaults, false);
    });
    ["dragenter", "dragover"].forEach(eventName => {
        element.removeEventListener(eventName, highlight, false);
    });
    ["dragleave", "drop"].forEach(eventName => {
        element.removeEventListener(eventName, unhighlight, false);
    });
}

function preventDefaults(e: DragEvent): void {
    e.preventDefault();
    e.stopPropagation();
}

function highlight(e: DragEvent) {
    if (e.dataTransfer.items.length === 1 && e.dataTransfer.items[0].kind === "file") {
        e.dataTransfer.dropEffect = "copy";
        (<HTMLElement>e.target).classList.add("dragging");
    } else {
        e.dataTransfer.dropEffect = "none";
    }
}


function unhighlight(e: DragEvent) {
    (<HTMLElement>e.target).classList.remove("dragging");
}

function handleDrop(e: DragEvent) {
    const dt = e.dataTransfer;
    const files: FileList = dt.files;
    const element = (<HTMLElement>e.target);

    if (files.length === 1) {
        const inputFileElement = element.querySelector("input[type=file]") as HTMLInputElement | null;
        if (inputFileElement === null) {
            throw new Error("");
        }

        inputFileElement.files = files;
        const event = new Event("change", { bubbles: true });
        inputFileElement.dispatchEvent(event);
    }
}