#nullable enable

using DevTools.Impl.Models;
using DevTools.Providers;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Composition;

namespace DevTools.Impl.ViewModels
{
    [Export(typeof(MainPageViewModel))]
    [Shared]
    public sealed class MainPageViewModel : ObservableRecipient
    {
        private readonly IToolProviderFactory _toolProviderFactory;

        internal ObservableCollection<MatchedToolProviderViewData> ToolsMenuItems { get; } = new();

        internal ObservableCollection<MatchedToolProviderViewData> FooterMenuItems { get; } = new();

        [ImportingConstructor]
        public MainPageViewModel(IToolProviderFactory toolProviderFactory)
        {
            _toolProviderFactory = toolProviderFactory;

            SetFooterMenuItems();
            SearchTools(searchQuery: string.Empty);
        }

        private void SearchTools(string searchQuery)
        {
            ToolsMenuItems.Clear();
            foreach (MatchedToolProvider matchedToolProvider in _toolProviderFactory.GetTools(searchQuery))
            {
                ToolsMenuItems.Add(
                    new MatchedToolProviderViewData(
                        matchedToolProvider.ToolProvider,
                        matchedToolProvider.MatchedSpans));
            }
        }

        private void SetFooterMenuItems()
        {
            FooterMenuItems.Clear();
            foreach (MatchedToolProvider matchedToolProvider in _toolProviderFactory.GetFooterTools())
            {
                FooterMenuItems.Add(
                    new MatchedToolProviderViewData(
                        matchedToolProvider.ToolProvider,
                        matchedToolProvider.MatchedSpans));
            }
        }
    }
}
