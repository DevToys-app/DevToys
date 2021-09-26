#nullable enable

using DevToys.Api.Core.Navigation;
using DevToys.Core.Threading;
using System;
using System.Composition;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.UI.Xaml.Controls;

namespace DevToys.Core.Navigation
{
    [Export(typeof(IWindowManager))]
    internal sealed class WindowManager : IWindowManager
    {
        public Task<bool> ShowContentDialogAsync(object content, string primaryButtonText, string? secondaryButtonText = null, string? title = null)
        {
            return ThreadHelper.RunOnUIThreadAsync(async () =>
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
