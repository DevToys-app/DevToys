using System.Threading.Tasks;

namespace DevTools.Core
{
    /// <summary>
    /// Provides a set of methods to manager windows and dialogs.
    /// </summary>
    public interface IWindowManager
    {
        /// <summary>
        /// Prompt a message dialog to the user.
        /// </summary>
        /// <param name="content">The content to show in the dialog. It can be a UI element or a text.</param>
        /// <param name="primaryButtonText">The text of the primary button.</param>
        /// <param name="secondaryButtonText">The text of the secondary button. If null or empty, the button will be hidden.</param>
        /// <param name="title">The title of the message. If null, use the application name.</param>
        /// <returns>Returns <code>true</code> if the user clicks on the primary button.</returns>
        Task<bool> ShowContentDialogAsync(
            object content,
            string primaryButtonText,
            string? secondaryButtonText = null,
            string? title = null);
    }
}
