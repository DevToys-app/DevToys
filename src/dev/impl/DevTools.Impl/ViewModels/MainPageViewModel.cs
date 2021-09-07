#nullable enable

using DevTools.Impl.Models;
using DevTools.Providers;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Composition;

namespace DevTools.Impl.ViewModels
{
    [Export(typeof(MainPageViewModel))]
    [Shared]
    public sealed class MainPageViewModel : ObservableRecipient
    {
        private readonly IToolProviderFactory _toolProviderFactory;

        /// <summary>
        /// Items at the top of the NavigationView.
        /// </summary>
        internal ObservableCollection<MatchedToolProviderViewData> ToolsMenuItems { get; } = new();

        /// <summary>
        /// Items at the bottom of the NavigationView. That includes Settings.
        /// </summary>
        internal ObservableCollection<MatchedToolProviderViewData> FooterMenuItems { get; } = new();

        /// <summary>
        /// The search query in the search bar.
        /// </summary>
        internal string? SearchQuery { get; set; }

        [ImportingConstructor]
        public MainPageViewModel(IToolProviderFactory toolProviderFactory)
        {
            _toolProviderFactory = toolProviderFactory;

            SearchBoxTextChangedCommand = new RelayCommand(ExecuteSearchBoxTextChangedCommand);

            SetFooterMenuItems();
            SearchTools(searchQuery: string.Empty);
        }

        #region SearchBoxTextChangedCommand

        internal IRelayCommand SearchBoxTextChangedCommand { get; }

        private void ExecuteSearchBoxTextChangedCommand()
        {
            SearchTools(SearchQuery);
        }

        #endregion

        private void SearchTools(string? searchQuery)
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
