#nullable enable

using System;

namespace DevToys.Api.Core
{
    /// <summary>
    /// Provides a service allowing to show in-app notification and Windows' toast notification.
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// Raised when an in-app notification is added.
        /// </summary>
        event EventHandler<InAppNotificationAddedEventArgs>? InAppNotificationAdded;

        /// <summary>
        /// Displays an in-app notification.
        /// </summary>
        void ShowInAppNotification(string title, string? message = null);

        /// <summary>
        /// Displays an in-app notification with an actionable link.
        /// </summary>
        void ShowInAppNotification(string title, string actionableLinkText, Action actionableLinkBehavior, string? message = null);
    }
}
