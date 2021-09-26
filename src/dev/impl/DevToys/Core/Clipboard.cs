#nullable enable

using DevToys.Api.Core;
using DevToys.Core.Threading;
using System;
using System.Composition;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace DevToys.Core
{
    [Export(typeof(IClipboard))]
    [Shared]
    internal sealed class Clipboard : IClipboard
    {
        private bool _isWindowInForeground;

        [ImportingConstructor]
        public Clipboard()
        {
            Window.Current.Activated += Window_Activated;
        }

        public Task<string> GetClipboardContentAsTextAsync()
        {
            return ThreadHelper.RunOnUIThreadAsync(async () =>
            {
                if (!_isWindowInForeground)
                {
                    throw new InvalidOperationException("Unable to retrieve the content of the clipboard because the application isn't in foreground.");
                }

                return (await Windows.ApplicationModel.DataTransfer.Clipboard.GetContent().GetTextAsync()) ?? string.Empty;
            });
        }

        private void Window_Activated(object sender, Windows.UI.Core.WindowActivatedEventArgs e)
        {
            _isWindowInForeground
                = e.WindowActivationState
                    is Windows.UI.Core.CoreWindowActivationState.CodeActivated
                    or Windows.UI.Core.CoreWindowActivationState.PointerActivated;
        }
    }
}
