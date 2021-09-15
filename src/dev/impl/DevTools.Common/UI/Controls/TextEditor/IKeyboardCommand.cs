#nullable enable

using Windows.System;

namespace DevTools.Common.UI.Controls.TextEditor
{
    public interface IKeyboardCommand<T>
    {
        bool Hit(bool ctrlDown, bool altDown, bool shiftDown, VirtualKey key);

        bool ShouldExecute(IKeyboardCommand<T>? lastCommand);

        bool ShouldHandleAfterExecution();

        bool ShouldSwallowAfterExecution();

        void Execute(T args);
    }
}