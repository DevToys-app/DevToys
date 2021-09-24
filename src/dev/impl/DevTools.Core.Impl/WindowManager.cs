#nullable enable

using DevTools.Core.Threading;
using System;
using System.Composition;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.UI.Xaml.Controls;

namespace DevTools.Core.Impl
{
    [Export(typeof(IWindowManager))]
    internal sealed class WindowManager : IWindowManager
    {
        private readonly IThread _thread;

        [ImportingConstructor]
        public WindowManager(IThread thread)
        {
            _thread = thread;
        }

        public Task<bool> ShowContentDialogAsync(object content, string primaryButtonText, string? secondaryButtonText = null, string? title = null)
        {
            return _thread.RunOnUIThreadAsync(async () =>
            {
                var confirmationDialog = new ContentDialog
                {
                    Title = title ?? Package.Current.DisplayName,
                    Content = content,
                    CloseButtonText = Arguments.NotNullOrWhiteSpace(primaryButtonText, nameof(primaryButtonText))
                };

                if (!string.IsNullOrEmpty(secondaryButtonText))
                {
                    confirmationDialog.PrimaryButtonText = secondaryButtonText;
                }

                return await confirmationDialog.ShowAsync() == ContentDialogResult.Primary;
            });
        }
    }
}
