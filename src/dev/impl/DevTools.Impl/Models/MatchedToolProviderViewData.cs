using DevTools.Core.Threading;
using DevTools.Providers;
using Windows.UI.Xaml.Controls;

namespace DevTools.Impl.Models
{
    public sealed class MatchedToolProviderViewData : MatchedToolProvider
    {
        internal TaskCompletionNotifier<IconElement> Icon => (TaskCompletionNotifier<IconElement>)ToolProvider.IconSource;

        public MatchedToolProviderViewData(ToolProviderMetadata metadata, IToolProvider toolProvider, MatchSpan[] matchedSpans)
            : base(metadata, toolProvider, matchedSpans)
        {
        }
    }
}
