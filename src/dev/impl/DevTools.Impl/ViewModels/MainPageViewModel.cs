#nullable enable

using DevTools.Core;
using DevTools.Core.Injection;
using DevTools.Impl.Models;
using DevTools.Impl.Views;
using DevTools.Localization;
using DevTools.Providers;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Composition;
using System.Threading.Tasks;

namespace DevTools.Impl.ViewModels
{
    [Export(typeof(MainPageViewModel))]
    public sealed class MainPageViewModel : ObservableRecipient
    {
        private readonly IMefProvider _mefProvider;
        private readonly IToolProviderFactory _toolProviderFactory;

        internal MainPageStrings Strings = LanguageManager.Instance.MainPage;

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
        public MainPageViewModel(
            IMefProvider mefProvider,
            IToolProviderFactory toolProviderFactory)
        {
            _mefProvider = mefProvider;
            _toolProviderFactory = toolProviderFactory;

            SearchBoxTextChangedCommand = new RelayCommand(ExecuteSearchBoxTextChangedCommand);
            OpenToolInNewWindowCommand = new AsyncRelayCommand<IToolProvider>(ExecuteOpenToolInNewWindowCommandAsync);

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

        #region OpenToolInNewWindowCommand

        public IRelayCommand<IToolProvider> OpenToolInNewWindowCommand { get; }

        private async Task ExecuteOpenToolInNewWindowCommandAsync(IToolProvider? toolProvider)
        {
            Arguments.NotNull(toolProvider, nameof(toolProvider));

            // await _windowManager.OpenNewWindowAsync(typeof(MainPage), toolProvider);
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
