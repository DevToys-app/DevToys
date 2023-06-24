export function getSelectionLength(input) {
    return input.selectionEnd - input.selectionStart;
}
export function getSelectionSpan(input) {
    return [input.selectionStart, input.selectionEnd];
}
export function selectAll(input) {
    input.setSelectionRange(0, input.value.length);
}
export function initializeKeyboardTracking(input) {
    input.addEventListener("keydown", onKeyDown);
    input.addEventListener("keypress", onKeyPress);
}
export function increaseValue(input) {
    input.stepUp();
    return input.valueAsNumber;
}
export function decreaseValue(input) {
    input.stepDown();
    return input.valueAsNumber;
}
export function dispose(input) {
    input.removeEventListener("keydown", onKeyDown);
    input.removeEventListener("keydown", onKeyPress);
}
// Forbid Up/Down keys, as this text box should stays as a single-line text input anyway.
function onKeyDown(e) {
    if (e.key === "ArrowDown" || e.key === "ArrowUp") {
        e.preventDefault();
    }
}
function onKeyPress(e) {
    if (e.key === "ArrowDown" || e.key === "ArrowUp") {
        e.preventDefault();
    }
}
//# sourceMappingURL=TextBox.razor.js.map