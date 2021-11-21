#nullable enable

using System.Threading.Tasks;
using DevToys.Api.Tools;
using DevToys.Core.Threading;
using DevToys.Shared.Core.Threading;
using Windows.UI.Xaml.Controls;

namespace DevToys.Models
{
    public sealed class MatchedToolProviderViewData : MatchedToolProvider
    {
        internal TaskCompletionNotifier<IconElement> Icon => (TaskCompletionNotifier<IconElement>)ToolProvider.IconSource;

        /// <summary>
        /// Gets whether the tool should be highlighted in the UI following a smart detection that the tool could be useful for the user.
        /// </summary>
        public bool IsRecommended { get; private set; }

        public MatchedToolProviderViewData(
            ToolProviderMetadata metadata,
            IToolProvider toolProvider,
            MatchSpan[] matchedSpans)
            : base(metadata, toolProvider, matchedSpans)
        {
        }

        internal async Task UpdateIsRecommendedAsync(string clipboardContent)
        {
            await TaskScheduler.Default;

            IsRecommended = ToolProvider.CanBeTreatedByTool(clipboardContent);

            ThreadHelper.RunOnUIThreadAsync(() =>
            {
                RaisePropertyChanged(nameof(IsRecommended));
            }).Forget();
        }
    }
}
