#nullable enable

using System;
using System.Composition;
using DevToys.Api.Core;

namespace DevToys.Core
{
    [Export(typeof(INotificationService))]
    [Shared]
    internal sealed class NotificationService : INotificationService
    {
        public event EventHandler<InAppNotificationAddedEventArgs>? InAppNotificationAdded;

        public void ShowInAppNotification(string title, string? message = null)
        {
            InAppNotificationAdded?.Invoke(this, new InAppNotificationAddedEventArgs(title, message, null, null));
        }

        public void ShowInAppNotification(string title, string actionableLinkText, Action actionableLinkBehavior, string? message = null)
        {
            InAppNotificationAdded?.Invoke(this, new InAppNotificationAddedEventArgs(title, message, actionableLinkText, actionableLinkBehavior));
        }
    }
}
