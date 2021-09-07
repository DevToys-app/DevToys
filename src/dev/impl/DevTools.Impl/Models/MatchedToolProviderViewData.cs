using DevTools.Providers;
using Windows.UI.Xaml.Controls;

namespace DevTools.Impl.Models
{
    public sealed class MatchedToolProviderViewData : MatchedToolProvider
    {
        internal IconElement Icon => (IconElement)ToolProvider.IconSource;

        public MatchedToolProviderViewData(IToolProvider toolProvider, MatchSpan[] matchedSpans)
            : base(toolProvider, matchedSpans)
        {
        }
    }
}
