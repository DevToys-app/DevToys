using DevTools.Core;
using DevTools.Core.Threading;
using DevTools.Providers;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace DevTools.Impl.Models
{
    public sealed class MatchedToolProviderViewData : MatchedToolProvider
    {
        private readonly IThread _thread;

        internal TaskCompletionNotifier<IconElement> Icon => (TaskCompletionNotifier<IconElement>)ToolProvider.IconSource;

        /// <summary>
        /// Gets whether the tool should be highlighted in the UI following a smart detection that the tool could be useful for the user.
        /// </summary>
        public bool IsRecommended { get; private set; }

        public MatchedToolProviderViewData(
            IThread thread,
            ToolProviderMetadata metadata,
            IToolProvider toolProvider,
            MatchSpan[] matchedSpans)
            : base(metadata, toolProvider, matchedSpans)
        {
            _thread = Arguments.NotNull(thread, nameof(thread));
        }

        internal async Task UpdateIsRecommendedAsync(string clipboardContent)
        {
            await TaskScheduler.Default;

            IsRecommended = ToolProvider.CanBeTreatedByTool(clipboardContent);

            _thread.RunOnUIThreadAsync(() =>
            {
                RaisePropertyChanged(nameof(IsRecommended));
            }).Forget();
        }
    }
}
