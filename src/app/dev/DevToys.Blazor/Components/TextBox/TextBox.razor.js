export function getSelectionLength(input) {
    return input.selectionEnd - input.selectionStart;
}
export function getSelectionSpan(input) {
    return [input.selectionStart, input.selectionEnd];
}
export function selectAll(input) {
    input.setSelectionRange(0, input.value.length);
}
//# sourceMappingURL=TextBox.razor.js.map