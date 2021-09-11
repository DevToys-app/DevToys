#nullable enable

using DevTools.Core.Threading;
using System;
using System.Composition;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace DevTools.Core.Impl
{
    [Export(typeof(IClipboard))]
    [Shared]
    internal sealed class Clipboard : IClipboard
    {
        private readonly IThread _thread;

        private bool _isWindowInForeground;

        [ImportingConstructor]
        public Clipboard(IThread thread)
        {
            _thread = thread;

            Window.Current.Activated += Window_Activated;
        }

        public Task<string> GetClipboardContentAsTextAsync()
        {
            return _thread.RunOnUIThreadAsync(async () =>
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
