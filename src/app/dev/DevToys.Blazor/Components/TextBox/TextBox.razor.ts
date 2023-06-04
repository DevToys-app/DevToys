export function getSelectionLength(input: HTMLInputElement): number {
    return input.selectionEnd - input.selectionStart;
}

export function cut(input: HTMLInputElement): void {
    copy(input);
    input.value = input.value.slice(0, input.selectionStart) + input.value.slice(input.selectionEnd);
}

export function copy(input: HTMLInputElement): void {
    const text = input.value.substring(input.selectionStart, input.selectionEnd);
    navigator.clipboard.writeText(text);
}

export async function paste(input: HTMLInputElement): Promise<void> {
    const text = await navigator.clipboard.readText();
    input.value = input.value.slice(0, input.selectionStart) + text + input.value.slice(input.selectionEnd);
}

export function selectAll(input: HTMLInputElement): void {
    input.setSelectionRange(0, input.value.length);
}