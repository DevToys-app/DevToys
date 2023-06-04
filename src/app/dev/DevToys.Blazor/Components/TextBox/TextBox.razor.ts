export function getSelectionLength(input: HTMLInputElement): number {
    return input.selectionEnd - input.selectionStart;
}

export function getSelectionSpan(input: HTMLInputElement): number[] {
    return [ input.selectionStart, input.selectionEnd ];
}

export function selectAll(input: HTMLInputElement): void {
    input.setSelectionRange(0, input.value.length);
}