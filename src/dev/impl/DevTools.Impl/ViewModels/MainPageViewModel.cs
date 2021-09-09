#nullable enable

using DevTools.Core;
using DevTools.Core.Threading;
using DevTools.Impl.Models;
using DevTools.Localization;
using DevTools.Providers;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Composition;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace DevTools.Impl.ViewModels
{
    [Export(typeof(MainPageViewModel))]
    public sealed class MainPageViewModel : ObservableRecipient
    {
        private readonly IThread _thread;
        private readonly IToolProviderFactory _toolProviderFactory;
        private readonly IUriActivationProtocolService _launchProtocolService;
        private readonly List<MatchedToolProviderViewData> _allToolsMenuitems = new();
        private readonly DisposableSempahore _sempahore = new();

        private MatchedToolProviderViewData? _selectedItem;
        private string? _searchQuery;

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
        /// Gets or sets the selected menu item in the NavitationView.
        /// </summary>
        internal MatchedToolProviderViewData? SelectedMenuItem
        {
            get => _selectedItem;
            set
            {
                _thread.ThrowIfNotOnUIThread();
                if (value is not null)
                {
                    _selectedItem = value;
                    OnPropertyChanged(nameof(SelectedMenuItem));
                }
            }
        }

        /// <summary>
        /// The search query in the search bar.
        /// </summary>
        internal string? SearchQuery
        {
            get => _searchQuery;
            set
            {
                _thread.ThrowIfNotOnUIThread();
                if (_searchQuery != value)
                {
                    _searchQuery = value;
                    OnPropertyChanged();
                    UpdateMenuAsync(value).Forget();
                }
            }
        }

        [ImportingConstructor]
        public MainPageViewModel(
            IThread thread,
            IToolProviderFactory toolProviderFactory,
            IUriActivationProtocolService launchProtocolService)
        {
            _thread = thread;
            _toolProviderFactory = toolProviderFactory;
            _launchProtocolService = launchProtocolService;

            OpenToolInNewWindowCommand = new AsyncRelayCommand<ToolProviderMetadata>(ExecuteOpenToolInNewWindowCommandAsync);
        }

        #region OpenToolInNewWindowCommand

        public IRelayCommand<ToolProviderMetadata> OpenToolInNewWindowCommand { get; }

        private async Task ExecuteOpenToolInNewWindowCommandAsync(ToolProviderMetadata? metadata)
        {
            Arguments.NotNull(metadata, nameof(metadata));
            await _launchProtocolService.LaunchNewAppInstance(metadata!.ProtocolName);
        }

        #endregion

        /// <summary>
        /// Invoked when the Page is loaded and becomes the current source of a parent Frame.
        /// </summary>
        internal async Task OnNavigatedToAsync(object? arguments)
        {
            await UpdateMenuAsync(searchQuery: string.Empty).ConfigureAwait(false);

            if (arguments is string argumentsString && !string.IsNullOrWhiteSpace(argumentsString))
            {
                NameValueCollection parameters = HttpUtility.ParseQueryString(argumentsString.ToLower(CultureInfo.CurrentCulture));
                string? toolProviderProtocolName = parameters.Get(Constants.UriActivationProtocolToolArgument);

                if (!string.IsNullOrWhiteSpace(toolProviderProtocolName))
                {
                    MatchedToolProviderViewData? toolProviderViewData
                        = ToolsMenuItems.FirstOrDefault(
                            item => string.Equals(item.Metadata.ProtocolName, toolProviderProtocolName, StringComparison.OrdinalIgnoreCase));

                    if (toolProviderViewData is null)
                    {
                        toolProviderViewData
                            = FooterMenuItems.FirstOrDefault(
                                item => string.Equals(item.Metadata.ProtocolName, toolProviderProtocolName, StringComparison.OrdinalIgnoreCase));
                    }

                    await _thread.RunOnUIThreadAsync(() =>
                    {
                        SelectedMenuItem = toolProviderViewData;
                    });
                    return;
                }
            }

            await _thread.RunOnUIThreadAsync(() =>
            {
                SelectedMenuItem = ToolsMenuItems.FirstOrDefault() ?? FooterMenuItems.FirstOrDefault();
            });
        }

        private async Task UpdateMenuAsync(string? searchQuery)
        {
            await TaskScheduler.Default;

            using (await _sempahore.WaitAsync(CancellationToken.None))
            {
                var selectedMenuItem = SelectedMenuItem;

                bool firstTime = string.IsNullOrEmpty(searchQuery) && _allToolsMenuitems.Count == 0;

                var toolsMenuitems = new Dictionary<MatchedToolProviderViewData, MatchSpan[]?>();
                foreach (MatchedToolProvider matchedToolProvider in _toolProviderFactory.GetTools(searchQuery))
                {
                    MatchedToolProviderViewData item;
                    MatchSpan[]? matchSpans = null;
                    if (firstTime)
                    {
                        item
                           = new MatchedToolProviderViewData(
                               matchedToolProvider.Metadata,
                               matchedToolProvider.ToolProvider,
                               matchedToolProvider.MatchedSpans);
                        _allToolsMenuitems.Add(item);
                    }
                    else
                    {
                        item
                            = _allToolsMenuitems.FirstOrDefault(
                                m => string.Equals(
                                    m.Metadata.Name,
                                    matchedToolProvider.Metadata.Name,
                                    StringComparison.Ordinal));

                        matchSpans = matchedToolProvider.MatchedSpans;
                    }

                    toolsMenuitems[item] = matchSpans;
                }

                var footerMenuItems = new List<MatchedToolProviderViewData>();
                if (FooterMenuItems.Count == 0)
                {
                    foreach (MatchedToolProvider matchedToolProvider in _toolProviderFactory.GetFooterTools())
                    {
                        footerMenuItems.Add(
                            new MatchedToolProviderViewData(
                                matchedToolProvider.Metadata,
                                matchedToolProvider.ToolProvider,
                                matchedToolProvider.MatchedSpans));
                    }
                }

                await _thread.RunOnUIThreadAsync(() =>
                {
                    var snapshot = ToolsMenuItems.ToArray();
                    for (int i = 0; i < snapshot.Length; i++)
                    {
                        var item = snapshot[i];
                        if (!toolsMenuitems.ContainsKey(item))
                        {
                            ToolsMenuItems.Remove(item);
                        }
                    }

                    foreach (var item in toolsMenuitems)
                    {
                        if (!snapshot.Contains(item.Key))
                        {
                            ToolsMenuItems.Add(item.Key);
                        }

                        if (item.Value is not null)
                        {
                            item.Key.MatchedSpans = item.Value;
                        }
                    }

                    footerMenuItems.ForEach(item => FooterMenuItems.Add(item));
                });
            }
        }
    }
}
