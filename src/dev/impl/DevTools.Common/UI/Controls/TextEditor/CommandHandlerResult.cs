#nullable enable

namespace DevTools.Common.UI.Controls.TextEditor
{
    public class CommandHandlerResult
    {
        public CommandHandlerResult(bool shouldHandle, bool shouldSwallow)
        {
            ShouldHandle = shouldHandle;
            ShouldSwallow = shouldSwallow;
        }

        /// <summary>
        /// "ShouldHandle == true" means the keyboard command event should be handled after execution
        /// Meaning you should set KeyRoutedEventArgs.Handled to true after execution
        /// All child OnKeyDown event will not be received and should not be triggered if it is true
        /// </summary>
        public bool ShouldHandle { get; }

        /// <summary>
        /// "ShouldSwallow == true" means the keyboard command event should not go to it's children
        /// Meaning you should not call base.OnKeyDown after execution
        /// All parent OnKeyDown event will not be received and should not be triggered if it is true
        /// </summary>
        public bool ShouldSwallow { get; }
    }
}