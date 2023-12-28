export function getSelectionLength(input: HTMLInputElement): number {
    return input.selectionEnd - input.selectionStart;
}

export function getSelectionSpan(input: HTMLInputElement): number[] {
    return [ input.selectionStart, input.selectionEnd ];
}

export function selectAll(input: HTMLInputElement): void {
    input.setSelectionRange(0, input.value.length);
}

export function initializeKeyboardTracking(input: HTMLInputElement): void {
    input.addEventListener("keydown", onKeyDown);
    input.addEventListener("keypress", onKeyPress);
}

export function increaseValue(input: HTMLInputElement): number {
    input.stepUp();
    const value = input.valueAsNumber;
    return isNaN(value) ? parseFloat(input.max) : value;
}

export function decreaseValue(input: HTMLInputElement): number {
    input.stepDown();
    const value = input.valueAsNumber;
    return isNaN(value) ? parseFloat(input.min) : value;
}

export function dispose(input: HTMLInputElement): void {
    input.removeEventListener("keydown", onKeyDown);
    input.removeEventListener("keydown", onKeyPress);
}

// Forbid Up/Down keys, as this text box should stays as a single-line text input anyway.
function onKeyDown(e: KeyboardEvent): void {
    // We want to allow the use of ArrowDown & ArrowUp to change values in case of input type number
    const input = e.target as HTMLInputElement;
    if (input.type === "number") {
        return;
    }
    if (e.key === "ArrowDown" || e.key === "ArrowUp") {
        e.preventDefault();
    }
}

function onKeyPress(e: KeyboardEvent): void {
    if (e.key === "ArrowDown" || e.key === "ArrowUp") {
        e.preventDefault();
    }
}