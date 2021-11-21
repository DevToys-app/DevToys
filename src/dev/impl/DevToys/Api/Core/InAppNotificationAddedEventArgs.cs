#nullable enable

using System;
using DevToys.Shared.Core;

namespace DevToys.Api.Core
{
    public sealed class InAppNotificationAddedEventArgs : EventArgs
    {
        public string Title { get; }

        public string? Message { get; }

        public string? ActionableLinkText { get; }

        public Action? Action { get; }

        internal InAppNotificationAddedEventArgs(
            string title,
            string? message,
            string? actionableLinkText,
            Action? action)
        {
            Title = Arguments.NotNullOrWhiteSpace(title, nameof(title));
            Message = message;

            if ((string.IsNullOrWhiteSpace(actionableLinkText) && action != null)
                || (!string.IsNullOrWhiteSpace(actionableLinkText) && action == null))
            {
                throw new ArgumentException($"'{nameof(actionableLinkText)}' and '{nameof(action)}' should not be null.");
            }

            ActionableLinkText = actionableLinkText;
            Action = action;
        }
    }
}
