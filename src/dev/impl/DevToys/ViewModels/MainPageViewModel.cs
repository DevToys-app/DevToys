﻿#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Composition;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using DevToys.Api.Core;
using DevToys.Api.Core.Navigation;
using DevToys.Api.Core.Settings;
using DevToys.Api.Tools;
using DevToys.Core;
using DevToys.Core.Collections;
using DevToys.Core.Settings;
using DevToys.Core.Threading;
using DevToys.Messages;
using DevToys.Models;
using DevToys.Shared.Core;
using DevToys.Shared.Core.Threading;
using DevToys.ViewModels.Tools;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.System;
using Windows.UI.StartScreen;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using ThreadPriority = DevToys.Core.Threading.ThreadPriority;

namespace DevToys.ViewModels
{
    [Export(typeof(MainPageViewModel))]
    public sealed class MainPageViewModel
        : ObservableRecipient,
        IRecipient<ChangeSelectedMenuItemMessage>,
        IRecipient<OpenToolInNewWindowMessage>,
        IRecipient<PinToolToStartMessage>,
        IRecipient<AddToFavoritesMessage>,
        IRecipient<RemoveFromFavoritesMessage>
    {
        private readonly IClipboard _clipboard;
        private readonly IToolProviderFactory _toolProviderFactory;
        private readonly IUriActivationProtocolService _launchProtocolService;
        private readonly ISettingsProvider _settingsProvider;
        private readonly INotificationService _notificationService;
        private readonly IMarketingService _marketingService;
        private readonly IWindowManager _windowManager;
        private readonly DisposableSempahore _sempahore = new();
        private readonly Task _menuInitializationTask;
        private readonly NavigationViewItemSeparator _headerSeparatorControl = new();

        private ToolProviderViewItem? _selectedItem;
        private NavigationViewDisplayMode _navigationViewDisplayMode;
        private bool _isNavigationViewPaneOpened;
        private string? _searchQuery;
        private string? _clipboardContent;
        private bool _pasteInFirstSelectedToolIsAllowed;
        private bool _isInCompactOverlayMode;
        private bool _isUpdatingSelectedItem;
        private bool _allowSelectAutomaticallyRecommendedTool = true;

        internal MainPageStrings Strings = LanguageManager.Instance.MainPage;

        internal ITitleBar TitleBar { get; }

        /// <summary>
        /// Items at the top of the NavigationView.
        /// </summary>
        internal ExtendedObservableCollection<object> ToolsMenuItems { get; } = new();

        /// <summary>
        /// Items at the bottom of the NavigationView.
        /// </summary>
        internal ExtendedObservableCollection<ToolProviderViewItem> FooterMenuItems { get; } = new();

        /// <summary>
        /// Gets or sets the selected menu item in the NavitationView.
        /// </summary>
        internal ToolProviderViewItem? SelectedMenuItem
        {
            get => _selectedItem;
            set => SetSelectedMenuItem(value, _clipboardContent, programmaticalSelection: false);
        }

        /// <summary>
        /// Gets the text to show in the header of the app. The property returned null when is in compact overlay mode.
        /// </summary>
        internal string? HeaderText => SelectedMenuItem?.ToolProvider.SearchDisplayName;

        /// <summary>
        /// Gets the text to show in the header of the app. The property returned null when is in compact overlay mode.
        /// </summary>
        internal string? WindowTitle
        {
            get
            {
                if (IsInCompactOverlayMode)
                {
                    return Strings.GetFormattedWindowTitleWithToolName(SelectedMenuItem?.ToolProvider.SearchDisplayName);
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
                ThreadHelper.ThrowIfNotOnUIThread();
                if (_searchQuery != value)
                {
                    SetProperty(ref _searchQuery, value);
                }
            }
        }

        /// <summary>
        /// Gets or sets the list of items to displayed in the Search Box after a search.
        /// </summary>
        internal ExtendedObservableCollection<ToolProviderViewItem> SearchResults { get; } = new();

        /// <summary>
        /// Gets whether the window is in Compact Overlay mode or not.
        /// </summary>
        internal bool IsInCompactOverlayMode
        {
            get => _isInCompactOverlayMode;
            private set
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                if (_isInCompactOverlayMode != value)
                {
                    SetProperty(ref _isInCompactOverlayMode, value);
                    OnPropertyChanged(nameof(HeaderText));
                    OnPropertyChanged(nameof(WindowTitle));
                }
            }
        }

        /// <summary>
        /// Gets or sets in what mode the navigation view is displayed.
        /// </summary>
        internal NavigationViewDisplayMode NavigationViewDisplayMode
        {
            get => _navigationViewDisplayMode;
            set
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                SetProperty(ref _navigationViewDisplayMode, value);
            }
        }

        /// <summary>
        /// Gets or sets whether the pane is opened.
        /// </summary>
        internal bool IsNavigationViewPaneOpened
        {
            get => _isNavigationViewPaneOpened;
            set
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                SetProperty(ref _isNavigationViewPaneOpened, value);
            }
        }

        [ImportingConstructor]
        public MainPageViewModel(
            IClipboard clipboard,
            ITitleBar titleBar,
            IToolProviderFactory toolProviderFactory,
            IUriActivationProtocolService launchProtocolService,
            ISettingsProvider settingsProvider,
            INotificationService notificationService,
            IMarketingService marketingService,
            IWindowManager windowManager)
        {
            _clipboard = clipboard;
            _toolProviderFactory = toolProviderFactory;
            _launchProtocolService = launchProtocolService;
            _settingsProvider = settingsProvider;
            _notificationService = notificationService;
            _marketingService = marketingService;
            _windowManager = windowManager;
            TitleBar = titleBar;

            OpenToolInNewWindowCommand = new AsyncRelayCommand<ToolProviderMetadata>(ExecuteOpenToolInNewWindowCommandAsync);
            PinToolToStartCommand = new AsyncRelayCommand<ToolProviderMetadata>(ExecutePinToolToStartCommandAsync);
            AddToFavoritesCommand = new RelayCommand<ToolProviderViewItem>(ExecuteAddToFavoritesCommand);
            RemoveFromFavoritesCommand = new RelayCommand<ToolProviderViewItem>(ExecuteRemoveFromFavoritesCommand);
            ChangeViewModeCommand = new AsyncRelayCommand<ApplicationViewMode>(ExecuteChangeViewModeCommandAsync);
            SearchBoxTextChangedCommand = new AsyncRelayCommand<Windows.UI.Xaml.Controls.AutoSuggestBoxTextChangedEventArgs>(ExecuteSearchBoxTextChangedCommandAsync);
            SearchBoxQuerySubmittedCommand = new AsyncRelayCommand<Windows.UI.Xaml.Controls.AutoSuggestBoxQuerySubmittedEventArgs>(ExecuteSearchBoxQuerySubmittedCommandAsync);

            _menuInitializationTask = BuildMenuAsync();

            Window.Current.Activated += Window_Activated;

            // Activate the view model's messenger.
            IsActive = true;
        }

        #region OpenToolInNewWindowCommand

        public IAsyncRelayCommand<ToolProviderMetadata> OpenToolInNewWindowCommand { get; }

        private async Task ExecuteOpenToolInNewWindowCommandAsync(ToolProviderMetadata? metadata)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            Arguments.NotNull(metadata, nameof(metadata));
            await _launchProtocolService.LaunchNewAppInstance(metadata!.ProtocolName);
        }

        #endregion

        #region PinToolToStartCommand

        public IAsyncRelayCommand<ToolProviderMetadata> PinToolToStartCommand { get; }

        private async Task ExecutePinToolToStartCommandAsync(ToolProviderMetadata? metadata)
        {
            try
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                Arguments.NotNull(metadata, nameof(metadata));

                if (SecondaryTile.Exists(metadata!.Name))
                {
                    return;
                }

                IEnumerable<ToolProviderViewItem> toolProviders = _toolProviderFactory.GetAllTools();
                ToolProviderViewItem toolProvider = toolProviders.First(tool => tool.Metadata == metadata);

                if (!await _launchProtocolService.PinToolToStart(toolProvider))
                {
                    if (await _windowManager.ShowContentDialogAsync(Strings.PinToolToStartProblem, LanguageManager.Instance.Common.Ok, LanguageManager.Instance.Settings.OpenLogs))
                    {
                        await Logger.OpenLogsAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogFault("Pin to start command", ex);
            }
        }

        #endregion

        #region AddToFavoritesCommand

        public IRelayCommand<ToolProviderViewItem> AddToFavoritesCommand { get; }

        private void ExecuteAddToFavoritesCommand(ToolProviderViewItem? toolProviderViewItem)
        {
            try
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                Arguments.NotNull(toolProviderViewItem, nameof(toolProviderViewItem));

                _toolProviderFactory.SetToolIsFavorite(toolProviderViewItem!, true);

                int index = ToolsMenuItems.IndexOf(_headerSeparatorControl);
                Assumes.IsTrue(index >= -1, nameof(index));

                ToolsMenuItems.Insert(
                    index,
                    ToolProviderViewItem.CreateToolProviderViewItemWithLongMenuDisplayName(toolProviderViewItem!));
            }
            catch (Exception ex)
            {
                Logger.LogFault("add to favorites command", ex);
            }
        }

        #endregion

        #region RemoveFromFavoritesCommand

        public IRelayCommand<ToolProviderViewItem> RemoveFromFavoritesCommand { get; }

        private void ExecuteRemoveFromFavoritesCommand(ToolProviderViewItem? toolProviderViewItem)
        {
            try
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                Arguments.NotNull(toolProviderViewItem, nameof(toolProviderViewItem));

                int index = ToolsMenuItems.IndexOf(_headerSeparatorControl);
                Assumes.IsTrue(index >= -1, nameof(index));

                for (int i = 0; i < index; i++)
                {
                    var tool = ToolsMenuItems[i] as ToolProviderViewItem;
                    if (tool is not null
                        && tool.IsFavorite
                        && string.Equals(tool.Metadata.Name, toolProviderViewItem!.Metadata.Name, StringComparison.Ordinal))
                    {
                        ToolsMenuItems.RemoveAt(i);
                        break;
                    }
                }

                _toolProviderFactory.SetToolIsFavorite(toolProviderViewItem!, false);
            }
            catch (Exception ex)
            {
                Logger.LogFault("remove to favorites command", ex);
            }
        }

        #endregion

        #region ChangeViewModeCommand

        public IAsyncRelayCommand<ApplicationViewMode> ChangeViewModeCommand { get; }

        private async Task ExecuteChangeViewModeCommandAsync(ApplicationViewMode applicationViewMode)
        {
            Assumes.NotNull(SelectedMenuItem, nameof(SelectedMenuItem));

            var compactOptions = ViewModePreferences.CreateDefault(ApplicationViewMode.CompactOverlay);
            compactOptions.CustomSize = new Size(SelectedMenuItem!.Metadata.CompactOverlayWidth, SelectedMenuItem.Metadata.CompactOverlayHeight);

            if (await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(applicationViewMode, compactOptions))
            {
                await ThreadHelper.RunOnUIThreadAsync(() =>
                {
                    IsInCompactOverlayMode = applicationViewMode == ApplicationViewMode.CompactOverlay;
                });
            }
        }

        #endregion

        #region SearchBoxTextChangedCommand

        public IAsyncRelayCommand<Windows.UI.Xaml.Controls.AutoSuggestBoxTextChangedEventArgs> SearchBoxTextChangedCommand { get; }

        private async Task ExecuteSearchBoxTextChangedCommandAsync(Windows.UI.Xaml.Controls.AutoSuggestBoxTextChangedEventArgs? parameters)
        {
            Arguments.NotNull(parameters, nameof(parameters));

            await TaskScheduler.Default;

            ToolProviderViewItem[]? searchResult = null;

            if (parameters!.Reason == Windows.UI.Xaml.Controls.AutoSuggestionBoxTextChangeReason.UserInput)
            {
                string? searchQuery = SearchQuery;
                if (!string.IsNullOrEmpty(searchQuery))
                {
                    IEnumerable<ToolProviderViewItem> matchedTools
                        = await _toolProviderFactory.SearchToolsAsync(searchQuery!).ConfigureAwait(false);

                    if (matchedTools.Any())
                    {
                        searchResult = matchedTools.ToArray();
                    }
                    else
                    {
                        searchResult = new[]
                        {
                            new ToolProviderViewItem(new ToolProviderMetadata(), new NoResultFoundMockToolProvider(), isFavorite: false)
                        };
                    }
                }
            }

            await ThreadHelper.RunOnUIThreadAsync(() =>
            {
                if (searchResult is null)
                {
                    SearchResults.Clear();
                }
                else
                {
                    SearchResults.Update(searchResult);
                }
            });
        }

        #endregion

        #region SearchBoxQuerySubmittedCommand

        public IAsyncRelayCommand<Windows.UI.Xaml.Controls.AutoSuggestBoxQuerySubmittedEventArgs> SearchBoxQuerySubmittedCommand { get; }

        private async Task ExecuteSearchBoxQuerySubmittedCommandAsync(Windows.UI.Xaml.Controls.AutoSuggestBoxQuerySubmittedEventArgs? parameters)
        {
            Arguments.NotNull(parameters, nameof(parameters));

            ThreadHelper.ThrowIfNotOnUIThread();

            if (string.IsNullOrEmpty(parameters!.QueryText))
            {
                // Nothing has been search. Do nothing.
                return;
            }

            if (parameters.ChosenSuggestion is null)
            {
                IEnumerable<ToolProviderViewItem> matchedTools
                    = await _toolProviderFactory.SearchToolsAsync(parameters.QueryText)
                    .ConfigureAwait(true); // make sure to stay on the UI thread.

                SetSelectedMenuItem(
                    SearchResultToolProvider.CreateResult(
                        parameters.QueryText,
                        matchedTools),
                    clipboardContentData: null);
                return;
            }
            else if (((ToolProviderViewItem)parameters.ChosenSuggestion).ToolProvider is NoResultFoundMockToolProvider)
            {
                SetSelectedMenuItem(
                    SearchResultToolProvider.CreateResult(
                        parameters.QueryText,
                        Array.Empty<ToolProviderViewItem>()),
                    clipboardContentData: null);
                return;
            }

            SetSelectedMenuItem((ToolProviderViewItem)parameters.ChosenSuggestion!, clipboardContentData: null);
        }

        #endregion

        public void Receive(ChangeSelectedMenuItemMessage message)
        {
            IEnumerable<ToolProviderViewItem> toolProviders = _toolProviderFactory.GetAllTools();
            ToolProviderViewItem toolProvider = toolProviders.First(tool => tool.ToolProvider == message.ToolProvider);
            SetSelectedMenuItem(toolProvider, clipboardContentData: null);
        }

        public void Receive(OpenToolInNewWindowMessage message)
        {
            OpenToolInNewWindowCommand.Execute(message.ToolProviderMetadata);
        }

        public void Receive(PinToolToStartMessage message)
        {
            PinToolToStartCommand.Execute(message.ToolProviderMetadata);
        }

        public void Receive(AddToFavoritesMessage message)
        {
            AddToFavoritesCommand.Execute(message.Tool);
        }

        public void Receive(RemoveFromFavoritesMessage message)
        {
            RemoveFromFavoritesCommand.Execute(message.Tool);
        }

        /// <summary>
        /// Invoked when the Page is loaded and becomes the current source of a parent Frame.
        /// </summary>
        internal async Task OnNavigatedToAsync(NavigationParameter parameters)
        {
            // Make sure the menu is populated.
            await _menuInitializationTask.ConfigureAwait(false);

            ToolProviderViewItem? toolProviderViewDataToSelect = null;
            if (!string.IsNullOrWhiteSpace(parameters.Query))
            {
                NameValueCollection queryParameters = HttpUtility.ParseQueryString(parameters.Query!.ToLower(CultureInfo.CurrentCulture));
                string? toolProviderProtocolName = queryParameters.Get(Constants.UriActivationProtocolToolArgument);

                if (!string.IsNullOrWhiteSpace(toolProviderProtocolName))
                {
                    // The user opened a new instance of the app that should go a certain desired tool.
                    // Let's make sure we won't switch to a recommended tool detected automatically.
                    _allowSelectAutomaticallyRecommendedTool = false;

                    IEnumerable<ToolProviderViewItem> toolProviders = _toolProviderFactory.GetAllTools();

                    toolProviderViewDataToSelect
                        = toolProviders.FirstOrDefault(
                            item => string.Equals(item.Metadata.ProtocolName, toolProviderProtocolName, StringComparison.OrdinalIgnoreCase));

                    // Wait a little bit here. We do that so the NavigationView gets a chance to render. Without this wait, selecting a tool
                    // that is a child to a parent menu item won't expand the parents.
                    await Task.Delay(100).ConfigureAwait(false);
                }
            }

            _marketingService.NotifyAppStarted();
            ShowReleaseNoteAsync().Forget();
            ShowAvailableUpdateAsync().Forget();

            await ThreadHelper.RunOnUIThreadAsync(
                ThreadPriority.Low,
                () =>
                {
                    SetSelectedMenuItem(
                        toolProviderViewDataToSelect
                        ?? ToolsMenuItems.FirstOrDefault(item => item is ToolProviderViewItem) as ToolProviderViewItem
                        ?? FooterMenuItems.FirstOrDefault(),
                        null);
                });
        }

        private void SetSelectedMenuItem(ToolProviderViewItem? value, string? clipboardContentData, bool programmaticalSelection = true)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (_isUpdatingSelectedItem)
            {
                return;
            }

            _isUpdatingSelectedItem = true;
            try
            {
                if (value is not null)
                {
                    _selectedItem = value;
                    IToolViewModel toolViewModel = _toolProviderFactory.GetToolViewModel(_selectedItem.ToolProvider);

                    if (!_pasteInFirstSelectedToolIsAllowed // If this is not the first tool we select since the last time tools have been recommended
                        || !_selectedItem.IsRecommended // or that the selected tool isn't recommended
                        || !_settingsProvider.GetSetting(PredefinedSettings.SmartDetectionPaste)) // or that the user doesn't want to paste automatically in recommended tools
                    {
                        clipboardContentData = null;
                    }

                    _pasteInFirstSelectedToolIsAllowed = false;

                    IDisposable? menuItemShouldBeExpandedLock = null;
                    if (programmaticalSelection && NavigationViewDisplayMode is NavigationViewDisplayMode.Expanded)
                    {
                        menuItemShouldBeExpandedLock = value.ForceMenuItemShouldBeExpanded();
                    }
                    Messenger.Send(new NavigateToToolMessage(toolViewModel, clipboardContentData));

                    OnPropertyChanged(nameof(SelectedMenuItem));
                    OnPropertyChanged(nameof(HeaderText));
                    OnPropertyChanged(nameof(WindowTitle));
                    menuItemShouldBeExpandedLock?.Dispose();
                }
            }
            catch (Exception ex)
            {
                Logger.LogFault("NavigationView", ex, "Unable to select a menu item");
            }
            _isUpdatingSelectedItem = false;
        }

        private async Task BuildMenuAsync()
        {
            await TaskScheduler.Default;

            try
            {
                var tasks = new List<Task<IEnumerable<ToolProviderViewItem>>>
                {
                    _toolProviderFactory.GetToolsTreeAsync(),
                    _toolProviderFactory.GetHeaderToolsAsync(),
                    _toolProviderFactory.GetFooterToolsAsync()
                };

                await Task.WhenAll(tasks).ConfigureAwait(false);

                IEnumerable<ToolProviderViewItem> tools = await tasks[0];
                IEnumerable<ToolProviderViewItem> headerTools = await tasks[1];
                IEnumerable<ToolProviderViewItem> footerTools = await tasks[2];

                await ThreadHelper.RunOnUIThreadAsync(
                    ThreadPriority.Low,
                    () =>
                    {
                        ToolsMenuItems.AddRange(headerTools);
                        ToolsMenuItems.Add(_headerSeparatorControl);
                        ToolsMenuItems.AddRange(tools);
                        FooterMenuItems.AddRange(footerTools);
                    });
            }
            catch (Exception ex)
            {
                Logger.LogFault("Update main menu after a search", ex, string.Empty);
            }
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
            string? clipboardContent = await _clipboard.GetClipboardContentAsTextAsync().ConfigureAwait(false);

            if (string.Equals(clipboardContent, _clipboardContent))
            {
                // The clipboard didn't change. Do no compute recommended tools again.
                return;
            }

            // Make sure the menu is populated.
            await _menuInitializationTask.ConfigureAwait(false);

            IEnumerable<ToolProviderViewItem> allTools = _toolProviderFactory.GetAllTools();

            ToolProviderViewItem[] oldRecommendedTools
                = allTools
                    .Where(item => item.IsRecommended)
                    .ToArray(); // Make a copy so we can compare with a newer list once we computed recommended items.

            // Start check what tools can treat the clipboard content.
            var tasks = new List<Task>();
            foreach (ToolProviderViewItem tool in allTools)
            {
                ToolProviderViewItem currentTool = tool;
                tasks.Add(
                    Task.Run(async () =>
                    {
                        try
                        {
                            await currentTool.UpdateIsRecommendedAsync(clipboardContent).ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            Logger.LogFault("SmartDetection - Check if tool is recommended", ex, $"Tool : {currentTool.Metadata.Name}");
                        }
                    }));
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);

            ToolProviderViewItem[] newRecommendedTools
                = allTools
                    .Where(item => item.IsRecommended)
                    .ToArray();

            _clipboardContent = clipboardContent;
            if (oldRecommendedTools.SequenceEqual(newRecommendedTools))
            {
                // The detected recommended tools is the same than before. Let's make sure we won't
                // paste automatically.
                _pasteInFirstSelectedToolIsAllowed = false;
                return;
            }
            else
            {
                _pasteInFirstSelectedToolIsAllowed = true;
                if (newRecommendedTools.Length > 0)
                {
                    _marketingService.NotifySmartDetectionWorked();
                }
            }

            using (await _sempahore.WaitAsync(CancellationToken.None).ConfigureAwait(false))
            {
                ToolProviderViewItem? toolProvider = GetRecommendedToolProvider(newRecommendedTools);
                if (toolProvider != null
                    && IsToolDisplayedInMenu(ToolsMenuItems.OfType<ToolProviderViewItem>(), toolProvider))
                {
                    // One unique tool is recommended.
                    // The recommended tool is displayed in the top menu.
                    // The recommended tool is different that the ones that were recommended before (if any...).
                    // Let's select automatically this tool.
                    await ThreadHelper.RunOnUIThreadAsync(
                        ThreadPriority.High,
                        () =>
                        {
                            if (!IsInCompactOverlayMode && _allowSelectAutomaticallyRecommendedTool)
                            {
                                SetSelectedMenuItem(toolProvider, _clipboardContent);
                            }
                        });
                }
            }
        }

        private ToolProviderViewItem? GetRecommendedToolProvider(ToolProviderViewItem[] items)
        {
            ToolProviderViewItem favoriteItem = items.FirstOrDefault(i => i.IsFavorite);
            if (favoriteItem != null)
            {
                return favoriteItem;
            }
            else if (items.Length == 1)
            {
                return items[0];
            }

            return null;
        }

        private bool IsToolDisplayedInMenu(IEnumerable<ToolProviderViewItem> tools, ToolProviderViewItem ToolProviderViewItem)
        {
            Arguments.NotNull(tools, nameof(tools));
            Arguments.NotNull(ToolProviderViewItem, nameof(ToolProviderViewItem));

            if (tools.Contains(ToolProviderViewItem))
            {
                return true;
            }

            foreach (ToolProviderViewItem tool in tools)
            {
                if (IsToolDisplayedInMenu(tool.ChildrenTools, ToolProviderViewItem))
                {
                    return true;
                }
            }

            return false;
        }

        private async Task ShowReleaseNoteAsync()
        {
            // Make sure we work in background.
            await TaskScheduler.Default;

            PackageVersion v = Package.Current.Id.Version;
            string? currentVersion = $"{v.Major}.{v.Minor}.{v.Build}.{v.Revision}";
            string? lastVersion = _settingsProvider.GetSetting(PredefinedSettings.LastVersionRan);

            if (!_settingsProvider.GetSetting(PredefinedSettings.FirstTimeStart) && currentVersion != lastVersion)
            {
                _notificationService.ShowInAppNotification(
                    Strings.GetFormattedNotificationReleaseNoteTitle(currentVersion),
                    Strings.NotificationReleaseNoteActionableActionText,
                    () =>
                    {
                        ThreadHelper.ThrowIfNotOnUIThread();
                        Launcher.LaunchUriAsync(new Uri("https://github.com/veler/DevToys/releases")).AsTask().Forget();
                    },
                    await AssetsHelper.GetReleaseNoteAsync());

                _notificationService.ShowInAppNotification(
                    Strings.SurveyTitle,
                    Strings.SurveyYesButton,
                    () =>
                    {
                        ThreadHelper.ThrowIfNotOnUIThread();
                        Launcher.LaunchUriAsync(new Uri("https://forms.office.com/r/eh27t4Z4nB")).AsTask().Forget();
                    },
                    Strings.SurveyDescription);

                _marketingService.NotifyAppJustUpdated();
            }

            _settingsProvider.SetSetting(PredefinedSettings.FirstTimeStart, false);
            _settingsProvider.SetSetting(PredefinedSettings.LastVersionRan, currentVersion);
        }

        private async Task ShowAvailableUpdateAsync()
        {
            // Make sure we work in background.
            await TaskScheduler.Default;

            PackageUpdateAvailabilityResult result = await Package.Current.CheckUpdateAvailabilityAsync();

            if (result.Availability is PackageUpdateAvailability.Required or PackageUpdateAvailability.Available)
            {
                _notificationService.ShowInAppNotification(
                    Strings.NotificationUpdateAvailableTitle,
                    Strings.NotificationUpdateAvailableActionableActionText,
                    () =>
                    {
                        ThreadHelper.ThrowIfNotOnUIThread();
                        Windows.System.Launcher.LaunchUriAsync(new Uri("ms-windows-store://downloadsandupdates")).AsTask().Forget();
                    });
            }
        }

        private void Window_Activated(object sender, Windows.UI.Core.WindowActivatedEventArgs e)
        {
            if (e.WindowActivationState
                    is Windows.UI.Core.CoreWindowActivationState.PointerActivated
                    or Windows.UI.Core.CoreWindowActivationState.CodeActivated)
            {
                UpdateRecommendedToolsAsync().ForgetSafely((ex) => Logger.LogFault("SmartDetection", ex));
            }
        }
    }
}
