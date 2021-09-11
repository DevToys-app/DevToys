using DevTools.Core;
using DevTools.Providers;

namespace DevTools.Impl.Messages
{
    public sealed class NavigateToToolMessage
    {
        internal IToolViewModel ViewModel { get; }

        public NavigateToToolMessage(IToolViewModel viewModel)
        {
            ViewModel = Arguments.NotNull(viewModel, nameof(viewModel));
        }
    }
}
