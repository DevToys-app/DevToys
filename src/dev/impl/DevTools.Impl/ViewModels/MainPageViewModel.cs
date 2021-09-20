#nullable enable

using DevTools.Common;
using DevTools.Core;
using DevTools.Core.Collections;
using DevTools.Core.Threading;
using DevTools.Impl.Messages;
using DevTools.Impl.Models;
using DevTools.Providers;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Composition;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Windows.Foundation;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;
using Microsoft.Toolkit.Mvvm.Messaging;
using Windows.UI.Xaml;
using DevTools.Core.Settings;
using ThreadPriority = DevTools.Core.Threading.ThreadPriority;

namespace DevTools.Impl.ViewModels
{
    [Export(typeof(MainPageViewModel))]
    public sealed class MainPageViewModel : ObservableRecipient
    {
        private readonly IThread _thread;
        private readonly IClipboard _clipboard;
        private readonly IToolProviderFactory _toolProviderFactory;
        private readonly IUriActivationProtocolService _launchProtocolService;
        private readonly ISettingsProvider _settingsProvider;
        private readonly List<MatchedToolProviderViewData> _allToolsMenuitems = new();
        private readonly DisposableSempahore _sempahore = new();
        private readonly Task _firstUpdateMenuTask;

        private MatchedToolProviderViewData? _selectedItem;
        private NavigationViewDisplayMode _navigationViewDisplayMode;
        private bool _isNavigationViewPaneOpened;
        private string? _searchQuery;
        private bool _isInCompactOverlayMode;
        private bool _isUpdatingSelectedItem;
        private bool _allowSelectAutomaticallyRecommendedTool = true;

        internal MainPageStrings Strings = LanguageManager.Instance.MainPage;

        internal ITitleBar TitleBar { get; }

        /// <summary>
        /// Items at the top of the NavigationView.
        /// </summary>
        internal OrderedObservableCollection<MatchedToolProviderViewData> ToolsMenuItems { get; } = new();

        /// <summary>
        /// Items at the bottom of the NavigationView. That includes Settings.
        /// </summary>
        internal OrderedObservableCollection<MatchedToolProviderViewData> FooterMenuItems { get; } = new();

        /// <summary>
        /// Gets or sets the selected menu item in the NavitationView.
        /// </summary>
        internal MatchedToolProviderViewData? SelectedMenuItem
        {
            get => _selectedItem;
            set
            {
                _thread.ThrowIfNotOnUIThread();
                if (_isUpdatingSelectedItem)
                {
                    return;
                }

                _isUpdatingSelectedItem = true;
                if (value is not null)
                {
                    _selectedItem = value;

                    IToolViewModel toolViewModel = _toolProviderFactory.GetToolViewModel(_selectedItem.ToolProvider);
                    Messenger.Send(new NavigateToToolMessage(toolViewModel));

                    OnPropertyChanged();
                    OnPropertyChanged(nameof(HeaderText));
                }
                _isUpdatingSelectedItem = false;
            }
        }

        /// <summary>
        /// Gets the text to show in the header of the app. The property returned null when is in compact overlay mode.
        /// </summary>
        internal string? HeaderText
        {
            get
            {
                if (IsInCompactOverlayMode)
                {
                    return null;
                }

                return SelectedMenuItem?.ToolProvider.DisplayName;
            }
        }

        /// <summary>
        /// Gets the text to show in the header of the app. The property returned null when is in compact overlay mode.
        /// </summary>
        internal string? WindowTitle
        {
            get
            {
                if (IsInCompactOverlayMode)
                {
                    return Strings.GetFormattedWindowTitleWithToolName(SelectedMenuItem?.ToolProvider.DisplayName);
                }

                return Strings.WindowTitle;
            }
        }

        /// <summary>
        /// Gets or sets search query in the search bar.
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

        /// <summary>
        /// Gets whether the window is in Compact Overlay mode or not.
        /// </summary>
        internal bool IsInCompactOverlayMode
        {
            get => _isInCompactOverlayMode;
            private set
            {
                _thread.ThrowIfNotOnUIThread();
                if (_isInCompactOverlayMode != value)
                {
                    _isInCompactOverlayMode = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(HeaderText));
                    OnPropertyChanged(nameof(WindowTitle));
                }
            }
        }

        /// <summary>
        /// Gets or sets in what mode the navigation view is displayed.
        /// </summary>
        public NavigationViewDisplayMode NavigationViewDisplayMode // Important to keep this property Public to bind it to ChangePropertyAction in the XAML.
        {
            get => _navigationViewDisplayMode;
            set
            {
                _thread.ThrowIfNotOnUIThread();
                _navigationViewDisplayMode = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ActualNavigationViewDisplayMode));
            }
        }

        /// <summary>
        /// Gets or sets whether the pane is opened.
        /// </summary>
        public bool IsNavigationViewPaneOpened // Important to keep this property Public to bind it to ChangePropertyAction in the XAML.
        {
            get => _isNavigationViewPaneOpened;
            set
            {
                _thread.ThrowIfNotOnUIThread();
                _isNavigationViewPaneOpened = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ActualNavigationViewDisplayMode));
            }
        }

        /// <summary>
        /// Gets the actual <see cref="NavigationViewDisplayMode"/> to apply to the navigation view menu items.
        /// </summary>
        public NavigationViewDisplayMode ActualNavigationViewDisplayMode
        {
            get
            {
                if (NavigationViewDisplayMode == NavigationViewDisplayMode.Expanded)
                {
                    if (IsNavigationViewPaneOpened)
                    {
                        return NavigationViewDisplayMode.Expanded;
                    }
                    else
                    {
                        return NavigationViewDisplayMode.Compact;
                    }
                }
                else
                {
                    return NavigationViewDisplayMode;
                }
            }
        }

        [ImportingConstructor]
        public MainPageViewModel(
            IThread thread,
            IClipboard clipboard,
            ITitleBar titleBar,
            IToolProviderFactory toolProviderFactory,
            IUriActivationProtocolService launchProtocolService,
            ISettingsProvider settingsProvider)
        {
            _thread = thread;
            _clipboard = clipboard;
            _toolProviderFactory = toolProviderFactory;
            _launchProtocolService = launchProtocolService;
            _settingsProvider = settingsProvider;
            TitleBar = titleBar;

            NavigationViewItemClickCommand = new RelayCommand<MatchedToolProviderViewData>(ExecuteNavigationViewItemClickCommand);
            OpenToolInNewWindowCommand = new AsyncRelayCommand<ToolProviderMetadata>(ExecuteOpenToolInNewWindowCommandAsync);
            ChangeViewModeCommand = new AsyncRelayCommand<ApplicationViewMode>(ExecuteChangeViewModeCommandAsync);

            _firstUpdateMenuTask = UpdateMenuAsync(searchQuery: string.Empty);

            Window.Current.Activated += Window_Activated;

            // Activate the view model's messenger.
            IsActive = true;
        }

        #region NavigationViewItemClickCommand

        public IRelayCommand<MatchedToolProviderViewData> NavigationViewItemClickCommand { get; }

        private void ExecuteNavigationViewItemClickCommand(MatchedToolProviderViewData? item)
        {
            Arguments.NotNull(item, nameof(item));
            SelectedMenuItem = item;
        }

        #endregion

        #region OpenToolInNewWindowCommand

        public IAsyncRelayCommand<ToolProviderMetadata> OpenToolInNewWindowCommand { get; }

        private async Task ExecuteOpenToolInNewWindowCommandAsync(ToolProviderMetadata? metadata)
        {
            Arguments.NotNull(metadata, nameof(metadata));
            await _launchProtocolService.LaunchNewAppInstance(metadata!.ProtocolName);
        }

        #endregion

        #region ChangeViewModeCommand

        public IAsyncRelayCommand<ApplicationViewMode> ChangeViewModeCommand { get; }

        private async Task ExecuteChangeViewModeCommandAsync(ApplicationViewMode applicationViewMode)
        {
            Assumes.NotNull(SelectedMenuItem, nameof(SelectedMenuItem));

            ViewModePreferences compactOptions = ViewModePreferences.CreateDefault(ApplicationViewMode.CompactOverlay);
            compactOptions.CustomSize = new Size(SelectedMenuItem!.Metadata.CompactOverlayWidth, SelectedMenuItem.Metadata.CompactOverlayHeight);

            if (await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(applicationViewMode, compactOptions))
            {
                await _thread.RunOnUIThreadAsync(() =>
                {
                    IsInCompactOverlayMode = applicationViewMode == ApplicationViewMode.CompactOverlay;
                });
            }
        }

        #endregion

        /// <summary>
        /// Invoked when the Page is loaded and becomes the current source of a parent Frame.
        /// </summary>
        internal async Task OnNavigatedToAsync(object? arguments)
        {
            // Make sure the menu items exist.
            await _firstUpdateMenuTask.ConfigureAwait(false);

            MatchedToolProviderViewData? toolProviderViewDataToSelect = null;
            if (arguments is string argumentsString && !string.IsNullOrWhiteSpace(argumentsString))
            {
                NameValueCollection parameters = HttpUtility.ParseQueryString(argumentsString.ToLower(CultureInfo.CurrentCulture));
                string? toolProviderProtocolName = parameters.Get(Constants.UriActivationProtocolToolArgument);

                if (!string.IsNullOrWhiteSpace(toolProviderProtocolName))
                {
                    // The user opened a new instance of the app that should go a certain desired tool.
                    // Let's make sure we won't switch to a recommended tool detected automatically.
                    _allowSelectAutomaticallyRecommendedTool = false;

                    toolProviderViewDataToSelect
                        = ToolsMenuItems.FirstOrDefault(
                            item => string.Equals(item.Metadata.ProtocolName, toolProviderProtocolName, StringComparison.OrdinalIgnoreCase));

                    if (toolProviderViewDataToSelect is null)
                    {
                        toolProviderViewDataToSelect
                            = FooterMenuItems.FirstOrDefault(
                                item => string.Equals(item.Metadata.ProtocolName, toolProviderProtocolName, StringComparison.OrdinalIgnoreCase));
                    }
                }
            }

            await _thread.RunOnUIThreadAsync(
                ThreadPriority.Low,
                () =>
                {
                    SelectedMenuItem = toolProviderViewDataToSelect ?? ToolsMenuItems.FirstOrDefault() ?? FooterMenuItems.FirstOrDefault();
                });
        }

        private async Task UpdateMenuAsync(string? searchQuery)
        {
            await TaskScheduler.Default;

            var newToolsMenuitems = new Dictionary<MatchedToolProviderViewData, MatchSpan[]?>();
            var footerMenuItems = new List<MatchedToolProviderViewData>();

            using (await _sempahore.WaitAsync(CancellationToken.None).ConfigureAwait(false))
            {
                bool firstTime = string.IsNullOrEmpty(searchQuery) && _allToolsMenuitems.Count == 0;

                foreach (MatchedToolProvider matchedToolProvider in _toolProviderFactory.GetTools(searchQuery))
                {
                    MatchedToolProviderViewData item;
                    MatchSpan[]? matchSpans = null;
                    if (firstTime)
                    {
                        item
                           = new MatchedToolProviderViewData(
                               _thread,
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

                    newToolsMenuitems[item] = matchSpans;
                }

                if (FooterMenuItems.Count == 0)
                {
                    foreach (MatchedToolProvider matchedToolProvider in _toolProviderFactory.GetFooterTools())
                    {
                        footerMenuItems.Add(
                            new MatchedToolProviderViewData(
                                _thread,
                                matchedToolProvider.Metadata,
                                matchedToolProvider.ToolProvider,
                                matchedToolProvider.MatchedSpans));
                    }
                }
            }

            await _thread.RunOnUIThreadAsync(
                ThreadPriority.Low,
                () =>
                {
                    var oldSelectedItem = SelectedMenuItem;
                    ToolsMenuItems.Clear();
                    SelectedMenuItem = null;

                    foreach (var item in newToolsMenuitems)
                    {
                        if (item.Value is not null)
                        {
                            item.Key.MatchedSpans = item.Value;
                        }
                    }

                    ToolsMenuItems.Update(newToolsMenuitems.Keys);
                    footerMenuItems.ForEach(item => FooterMenuItems.Add(item));

                    SelectedMenuItem = oldSelectedItem;
                });
        }

        private async Task UpdateRecommendedToolsAsync()
        {
            if (IsInCompactOverlayMode || !_settingsProvider.GetSetting(PredefinedSettings.SmartDetection))
            {
                return;
            }

            // Make sure we work in background.
            await TaskScheduler.Default;

            // Retrieve the clipboard content.
            string clipboardContent = await _clipboard.GetClipboardContentAsTextAsync().ConfigureAwait(false);

            // Make sure the menu items exist.
            await _firstUpdateMenuTask.ConfigureAwait(false);

            MatchedToolProviderViewData[] oldRecommendedTools = _allToolsMenuitems.Where(item => item.IsRecommended).ToArray();

            // Start check what tools can treat the clipboard content.
            var tasks = new List<Task>();
            for (int i = 0; i < _allToolsMenuitems.Count; i++)
            {
                MatchedToolProviderViewData tool = _allToolsMenuitems[i];
                tasks.Add(
                    Task.Run(async () =>
                    {
                        try
                        {
                            await tool.UpdateIsRecommendedAsync(clipboardContent).ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            // TODO: Log this.
                        }
                    }));
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);

            MatchedToolProviderViewData[] recommendedTools
                = _allToolsMenuitems
                .Where(item => item.IsRecommended && !item.Metadata.IsFooterItem)
                .ToArray();

            using (await _sempahore.WaitAsync(CancellationToken.None).ConfigureAwait(false))
            {
                if (recommendedTools.Length == 1
                    && ToolsMenuItems.Contains(recommendedTools[0])
                    && (oldRecommendedTools.Length != 1 || oldRecommendedTools[0] != recommendedTools[0]))
                {
                    // One unique tool is recommended.
                    // The recommended tool is displayed in the top menu.
                    // The recommended tool is different that the ones that were recommended before (if any...).
                    // Let's select automatically this tool.
                    await _thread.RunOnUIThreadAsync(
                        ThreadPriority.High,
                        () =>
                        {
                            if (!IsInCompactOverlayMode && _allowSelectAutomaticallyRecommendedTool)
                            {
                                SelectedMenuItem = recommendedTools[0];
                            }
                        });
                }
            }
        }

        private void Window_Activated(object sender, Windows.UI.Core.WindowActivatedEventArgs e)
        {
            if (e.WindowActivationState
                    is Windows.UI.Core.CoreWindowActivationState.PointerActivated
                    or Windows.UI.Core.CoreWindowActivationState.CodeActivated)
            {
                UpdateRecommendedToolsAsync().Forget();
            }
        }
    }
}
