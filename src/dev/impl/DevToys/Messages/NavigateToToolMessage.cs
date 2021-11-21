#nullable enable

using DevToys.Api.Tools;
using DevToys.Shared.Core;

namespace DevToys.Messages
{
    public sealed class NavigateToToolMessage
    {
        internal IToolViewModel ViewModel { get; }

        internal string? ClipboardContentData { get; }

        public NavigateToToolMessage(IToolViewModel viewModel, string? clipboardContentData)
        {
            ViewModel = Arguments.NotNull(viewModel, nameof(viewModel));
            ClipboardContentData = clipboardContentData;
        }
    }
}
