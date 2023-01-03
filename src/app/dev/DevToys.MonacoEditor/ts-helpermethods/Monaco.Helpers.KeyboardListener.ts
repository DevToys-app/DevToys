//namespace DevToys.MonacoEditor.Monaco.Helpers {
interface KeyboardListener {
    keyDown(keycode: number, ctrl: boolean, shift: boolean, alt: boolean, meta: boolean): boolean;
}
//}