#nullable enable

using DevToys.MonacoEditor.CodeEditorControl;
using System;
using Windows.Foundation.Metadata;

namespace DevToys.MonacoEditor.Helpers
{
    [AllowForWeb]
    public sealed class KeyboardListener
    {
        private readonly WeakReference<CodeEditor> parent;

        public KeyboardListener(CodeEditor parent) // TODO: Make Interface for event usage
        {
            this.parent = new WeakReference<CodeEditor>(parent);
        }

        /// <summary>
        /// Called from JavaScript, returns if event was handled or not.
        /// </summary>
        /// <param name="keycode"></param>
        /// <param name="ctrl"></param>
        /// <param name="shift"></param>
        /// <param name="alt"></param>
        /// <param name="meta"></param>
        /// <returns></returns>
        public bool KeyDown(int keycode, bool ctrl, bool shift, bool alt, bool meta)
        {
            if (parent.TryGetTarget(out CodeEditor editor))
            {
                return editor.TriggerKeyDown(new WebKeyEventArgs()
                {
                    KeyCode = keycode, // TODO: Convert to a virtual key or something?
                    CtrlKey = ctrl,
                    ShiftKey = shift,
                    AltKey = alt,
                    MetaKey = meta
                });
            }

            return false;
        }
    }
}
