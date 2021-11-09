#nullable enable

using System;
using System.Composition;
using System.Threading.Tasks;
using DevToys.Api.Core;
using DevToys.Core.Threading;
using Windows.ApplicationModel.DataTransfer;
using Windows.Security.EnterpriseData;
using Windows.UI.Xaml;

namespace DevToys.Core
{
    [Export(typeof(IClipboard))]
    [Shared]
    internal sealed class Clipboard : IClipboard
    {
        private const string TextFormat = "Text";

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

                DataPackageView clipboardContent = Windows.ApplicationModel.DataTransfer.Clipboard.GetContent();
                ProtectionPolicyEvaluationResult accessResult = await clipboardContent.RequestAccessAsync();
                if (accessResult == ProtectionPolicyEvaluationResult.Allowed
                    && clipboardContent.Contains(TextFormat))
                {
                    return await clipboardContent.GetTextAsync(TextFormat) ?? string.Empty;
                }

                return string.Empty;
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
