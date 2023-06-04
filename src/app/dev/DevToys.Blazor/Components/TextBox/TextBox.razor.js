var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
export function getSelectionLength(input) {
    return input.selectionEnd - input.selectionStart;
}
export function cut(input) {
    copy(input);
    input.value = input.value.slice(0, input.selectionStart) + input.value.slice(input.selectionEnd);
}
export function copy(input) {
    const text = input.value.substring(input.selectionStart, input.selectionEnd);
    navigator.clipboard.writeText(text);
}
export function paste(input) {
    return __awaiter(this, void 0, void 0, function* () {
        const text = yield navigator.clipboard.readText();
        input.value = input.value.slice(0, input.selectionStart) + text + input.value.slice(input.selectionEnd);
    });
}
export function selectAll(input) {
    input.setSelectionRange(0, input.value.length);
}
//# sourceMappingURL=TextBox.razor.js.map