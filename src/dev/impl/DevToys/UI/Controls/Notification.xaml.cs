#nullable enable

using DevToys.Api.Core;
using DevToys.Core.Threading;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace DevToys.UI.Controls
{
    public sealed partial class Notification : UserControl
    {
        private readonly ObservableCollection<InAppNotificationAddedEventArgs> _notifications = new();

        public static readonly DependencyProperty NotificationServiceProperty
            = DependencyProperty.Register(
                nameof(NotificationService),
                typeof(INotificationService),
                typeof(Notification),
                new PropertyMetadata(null, OnNotificationServicePropertyChangedCallback));

        public INotificationService? NotificationService
        {
            get => (INotificationService?)GetValue(NotificationServiceProperty);
            set => SetValue(NotificationServiceProperty, value);
        }

        public Notification()
        {
            InitializeComponent();
            ItemsControl.ItemsSource = _notifications;
        }

        private void ListenToNotifications(INotificationService notificationService)
        {
            notificationService.InAppNotificationAdded += NotificationService_InAppNotificationAdded;
        }

        private void NotificationService_InAppNotificationAdded(object sender, InAppNotificationAddedEventArgs e)
        {
            ThreadHelper.RunOnUIThreadAsync(() =>
            {
                _notifications.Add(e);
            });
        }

        private void InfoBar_Closing(Microsoft.UI.Xaml.Controls.InfoBar sender, Microsoft.UI.Xaml.Controls.InfoBarClosingEventArgs args)
        {
            _notifications.Remove((InAppNotificationAddedEventArgs)sender.DataContext);
        }

        private void ActionableButton_Click(object sender, RoutedEventArgs e)
        {
            var model = (InAppNotificationAddedEventArgs)((FrameworkElement)sender).DataContext;
            model.Action!.Invoke();
            _notifications.Remove(model);
        }

        private static void OnNotificationServicePropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            INotificationService? notificationService = e.NewValue as INotificationService;
            if (notificationService is not null)
            {
                ((Notification)d).ListenToNotifications(notificationService);
            }
        }
    }
}
