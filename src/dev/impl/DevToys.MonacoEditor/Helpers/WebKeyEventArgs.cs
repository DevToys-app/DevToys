#nullable enable

using DevToys.MonacoEditor.CodeEditorControl;

namespace DevToys.MonacoEditor.Helpers
{
    public delegate void WebKeyEventHandler(CodeEditorCore sender, WebKeyEventArgs args);

    public sealed class WebKeyEventArgs
    {
        public int KeyCode { get; set; }

        // TODO: Make these some sort of flagged state enum?
        public bool CtrlKey { get; set; }

        public bool ShiftKey { get; set; }

        public bool AltKey { get; set; }

        public bool MetaKey { get; set; }

        public bool Handled { get; set; }
    }
}
