export function registerDropZone(id: string): void {
    const element = document.getElementById(id);

    // Prevent default drag behaviors
    ["dragenter", "dragover", "dragleave", "drop"].forEach(eventName => {
        element.addEventListener(eventName, preventDefaults, false);
    });

    // Highlight drop area when item is dragged over it
    ["dragenter", "dragover"].forEach(eventName => {
        element.addEventListener(eventName, highlight, false);
    });
    element.addEventListener("dragleave", unhighlightIfNeeded, false);
    element.addEventListener("drop", unhighlight, false);

    // Handle dropped files
    element.addEventListener("drop", handleDrop, false);
}

export function dispose(id: string): void {
    const element = document.getElementById(id);

    // Stop drop zone
    ["dragenter", "dragover", "dragleave", "drop"].forEach(eventName => {
        element.removeEventListener(eventName, preventDefaults, false);
    });
    ["dragenter", "dragover"].forEach(eventName => {
        element.removeEventListener(eventName, highlight, false);
    });
    element.removeEventListener("dragleave", unhighlightIfNeeded, false);
    element.removeEventListener("drop", unhighlight, false);
}

function preventDefaults(e: DragEvent): void {
    e.preventDefault();
    e.stopPropagation();
}

function highlight(e: DragEvent) {
    if (e.dataTransfer.items.length > 0 && e.dataTransfer.items[0].kind === "file") {
        e.dataTransfer.dropEffect = "copy";
        (<HTMLElement>e.currentTarget).classList.add("dragging");
    } else {
        e.dataTransfer.dropEffect = "none";
    }
}

function unhighlightIfNeeded(e: DragEvent) {
    // Get the location on screen of the element.
    const rect = (<HTMLElement>e.currentTarget).getBoundingClientRect();

    // Check the mouseEvent coordinates are outside of the rectangle
    if (e.x > rect.left + rect.width || e.x < rect.left
        || e.y > rect.top + rect.height || e.y < rect.top) {
        unhighlight(e);
    }
}

function unhighlight(e: DragEvent) {
    (<HTMLElement>e.currentTarget).classList.remove("dragging");
}

function handleDrop(e: DragEvent) {
    const dt = e.dataTransfer;
    const files: FileList = dt.files;
    const element = (<HTMLElement>e.currentTarget);

    if (files.length > 0) {
        const inputFileElement = element.querySelector("input[type=file]") as HTMLInputElement | null;
        if (inputFileElement === null) {
            throw new Error("");
        }

        inputFileElement.files = files;
        const event = new Event("change", { bubbles: true });
        inputFileElement.dispatchEvent(event);
    }
}