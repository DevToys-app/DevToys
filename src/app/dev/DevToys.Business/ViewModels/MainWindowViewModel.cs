using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DevToys.Api;
using DevToys.Core;
using DevToys.Core.Models;
using DevToys.Core.Settings;
using DevToys.Core.Tools;
using DevToys.Core.Tools.ViewItems;
using DevToys.Core.Version;
using DevToys.Core.Web;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace DevToys.Business.ViewModels;

[Export]
internal sealed partial class MainWindowViewModel : ObservableRecipient
{
    private readonly ILogger _logger;
    private readonly GuiToolProvider _guiToolProvider;
    private readonly SmartDetectionService _smartDetectionService;
    private readonly IClipboard _clipboard;
    private readonly ISettingsProvider _settingsProvider;
    private readonly IWebClientService _webClientService;
    private readonly IVersionService _versionService;
    private readonly Stack<INotifyPropertyChanged> _navigationHistory = new();

    private IReadOnlyList<SmartDetectedTool>? _oldSmartDetectedTools;
    private object? _oldRawClipboardData;
    private bool _passSmartDetectedDataToNextSelectedToolIsAllowed;
    private INotifyPropertyChanged? _selectedMenuItem;
    private bool _isGoingBack;

    [ImportingConstructor]
    public MainWindowViewModel(
        GuiToolProvider guiToolProvider,
        SmartDetectionService smartDetectionService,
        IClipboard clipboard,
        ISettingsProvider settingsProvider,
        IWebClientService webClientService,
        IVersionService versionService)
    {
        _logger = this.Log();
        _guiToolProvider = guiToolProvider;
        _smartDetectionService = smartDetectionService;
        _clipboard = clipboard;
        _settingsProvider = settingsProvider;
        _webClientService = webClientService;
        _versionService = versionService;
        Messenger.Register<MainWindowViewModel, ChangeSelectedMenuItemMessage>(this, OnChangeSelectedMenuItemMessageReceived);

        if (_settingsProvider.GetSetting(PredefinedSettings.CheckForUpdate))
        {
            CheckForUpdateAsync().ForgetSafely();
        }
    }

    /// <summary>
    /// Raised when the <see cref="SelectedMenuItem"/> property changed.
    /// </summary>
    internal event EventHandler<EventArgs>? SelectedMenuItemChanged;

    /// <summary>
    /// Gets a hierarchical list containing all the tools available, ordered, to display in the top and body menu.
    /// This includes "All tools" menu item, recents and favorites.
    /// </summary>
    internal ReadOnlyObservableCollection<INotifyPropertyChanged?> HeaderAndBodyToolViewItems => _guiToolProvider.HeaderAndBodyToolViewItems;

    /// <summary>
    /// Gets a flat list containing all the footer tools available, ordered.
    /// </summary>
    internal ReadOnlyObservableCollection<INotifyPropertyChanged?> FooterToolViewItems => _guiToolProvider.FooterToolViewItems;

    // Can't use CommunityToolkit.MVVM due to https://github.com/dotnet/roslyn/issues/57239#issuecomment-1437895948
    /// <summary>
    /// Gets or sets the selected menu item in the NavigationView.
    /// </summary>
    public INotifyPropertyChanged? SelectedMenuItem
    {
        get => _selectedMenuItem;
        set
        {
            // Save the previous selected item.
            if (!_isGoingBack && _selectedMenuItem is not null && value is not null && _selectedMenuItem != value)
            {
                _navigationHistory.Push(_selectedMenuItem);
                OnPropertyChanged(nameof(CanGoBack));
            }

            if (value is null && _selectedMenuItem is not null)
            {
                // Somehow, the NavigationView end up with no selected item. An example of scenario where it can happen is when
                // the selected item is a favorite one and the user remove it from the favorites.
                // What we do in this scenario is that we try to find the equivalent item corresponding to the selected item we had.
                // If we don't find anything, let's selected the first item in the menu.
                SetProperty(ref _selectedMenuItem, GetBestMenuItemToSelect(_selectedMenuItem));
            }
            else
            {
                SetProperty(ref _selectedMenuItem, value);
            }

            var guiToolViewItem = value as GuiToolViewItem;
            if (guiToolViewItem is not null)
            {
                _smartDetectionService.ActiveToolInstance = guiToolViewItem.ToolInstance;
            }
            else
            {
                _smartDetectionService.ActiveToolInstance = null;
            }

            OnPropertyChanged(nameof(IsSelectedMenuItemATool));
            SelectedMenuItemChanged?.Invoke(this, EventArgs.Empty);

            guiToolViewItem?.RaiseGotSelected();

            // If we selected a tool (not a group)
            if (guiToolViewItem is not null)
            {
                // Try to pass Smart Detection data, if the selected tool fits the detected data.
                if (_passSmartDetectedDataToNextSelectedToolIsAllowed)
                {
                    _passSmartDetectedDataToNextSelectedToolIsAllowed = false;
                    if (_oldSmartDetectedTools is not null)
                    {
                        SmartDetectedTool? smartDetectedTool
                            = _oldSmartDetectedTools.FirstOrDefault(tool => tool.ToolInstance == guiToolViewItem.ToolInstance);
                        if (smartDetectedTool is not null)
                        {
                            // Send the data to the tool.
                            guiToolViewItem.ToolInstance.PassSmartDetectedData(smartDetectedTool.DataTypeName, smartDetectedTool.ParsedData);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Gets whether the <see cref="SelectedMenuItem"/> is a tool or not.
    /// </summary>
    internal bool IsSelectedMenuItemATool => SelectedMenuItem is GuiToolViewItem;

    /// <summary>
    /// Gets whether the user can navigate back to the previous <see cref="SelectedMenuItem"/>.
    /// </summary>
    internal bool CanGoBack => _navigationHistory.Count > 0;

    // Can't use CommunityToolkit.MVVM due to https://github.com/dotnet/roslyn/issues/57239#issuecomment-1437895948
    /// <summary>
    /// Gets or sets the search query typed by the user.
    /// </summary>
    internal string SearchQuery { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of search results from the <see cref="SearchQuery"/>.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<GuiToolViewItem> _searchResults = new();

    /// <summary>
    /// Gets or sets whether an update for the app is available online.
    /// </summary>
    [ObservableProperty]
    private bool _updateAvailable = false;

    /// <summary>
    /// Navigates back to the previous <see cref="SelectedMenuItem"/>.
    /// </summary>
    internal void GoBack()
    {
        Guard.IsTrue(CanGoBack);

        _isGoingBack = true;
        INotifyPropertyChanged previousItem = _navigationHistory.Pop();
        OnPropertyChanged(nameof(CanGoBack));

        SelectedMenuItem = GetBestMenuItemToSelect(previousItem);

        _isGoingBack = false;
    }

    /// <summary>
    /// Get the data from the clipboard and try to detect tools that can accept the clipboard data as an input.
    /// </summary>
    internal async Task RunSmartDetectionAsync(bool isInCompactOverlayMode, bool isDialogOpened)
    {
        if (isInCompactOverlayMode
            || isDialogOpened
            || !_settingsProvider.GetSetting(PredefinedSettings.SmartDetection))
        {
            return;
        }

        Guard.IsNotEmpty((IReadOnlyList<INotifyPropertyChanged>)HeaderAndBodyToolViewItems);

        try
        {
            using var cancellationTokenSource
                = new CancellationTokenSource(
                    Debugger.IsAttached ? TimeSpan.FromHours(1) : TimeSpan.FromSeconds(2));

            CancellationToken cancellationToken = cancellationTokenSource.Token;
            INotifyPropertyChanged? selectedMenuBeforeSmartDetection = SelectedMenuItem;
            Guard.IsNotNull(selectedMenuBeforeSmartDetection);

            // Retrieve clipboard content.
            object? rawClipboardData = await _clipboard.GetClipboardDataAsync();

            // If the clipboard content has changed since the last time
            if (!cancellationToken.IsCancellationRequested && !AreOldAndNewClipboardDataEqual(_oldRawClipboardData, rawClipboardData))
            {
                // Reset recommended tools.
                _guiToolProvider.ForEachToolViewItem(toolViewItem => toolViewItem.IsRecommended = false);

                // Detect tools to recommend.
                IReadOnlyList<SmartDetectedTool> detectedTools
                    = await _smartDetectionService.DetectAsync(rawClipboardData, strict: true, cancellationToken)
                        .ConfigureAwait(true);

                GuiToolViewItem? firstToolViewItem = null;

                // For each recommended tool, set IsRecommended.
                for (int i = 0; i < detectedTools.Count; i++)
                {
                    SmartDetectedTool detectedTool = detectedTools[i];
                    IEnumerable<GuiToolViewItem> toolViewItems = _guiToolProvider.GetViewItemFromTool(detectedTool.ToolInstance);
                    GuiToolViewItem? toolViewItem = toolViewItems.FirstOrDefault();
                    if (toolViewItem != null)
                    {
                        if (i == 0)
                        {
                            firstToolViewItem = toolViewItem;
                        }
                        toolViewItem.IsRecommended = true;
                    }
                }

                // If user's setting allow us to jump to a tool automatically and paste the data automatically
                if (_settingsProvider.GetSetting(PredefinedSettings.SmartDetectionPaste))
                {
                    // If one unique tool got found
                    // And that the current selected menu item is a group, or that the user didn't selected another menu item since the smart detection started
                    if (detectedTools.Count == 1 && (SelectedMenuItem == selectedMenuBeforeSmartDetection || SelectedMenuItem is GroupViewItem && firstToolViewItem is not null))
                    {
                        // Then let's navigate immediately to it and set the detected data as an input.
                        SelectedMenuItem = firstToolViewItem;

                        // Send the data to the tool.
                        detectedTools[0].ToolInstance.PassSmartDetectedData(detectedTools[0].DataTypeName, detectedTools[0].ParsedData);
                    }
                    else if (detectedTools.Count > 1)
                    {
                        // Next time user navigates to a tool, if this one has been detected by Smart Detection,
                        // we will pass data to it.
                        _passSmartDetectedDataToNextSelectedToolIsAllowed = true;
                    }
                }

                _oldSmartDetectedTools = detectedTools;
                _oldRawClipboardData = rawClipboardData;
            }
        }
        catch (OperationCanceledException)
        {
            // Ignore
        }
        catch (Exception ex)
        {
            LogSmartDetectionFailed(ex);
        }
    }

    /// <summary>
    /// Command invoked when the search box's text changed.
    /// </summary>
    [RelayCommand]
    private void SearchBoxTextChanged()
    {
        _guiToolProvider.SearchTools(SearchQuery, SearchResults);
    }

    /// <summary>
    /// Command invoked when the user press Enter in the search box or explicitly select an item in the search result list.
    /// </summary>
    /// <param name="chosenSuggestion">Equals to the selected item in the search result list, or null if nothing is selected by the user and or if there's no result at all.</param>
    [RelayCommand]
    private void SearchBoxQuerySubmitted(object? chosenSuggestion)
    {
        var selectedSearchResultItem = chosenSuggestion as GuiToolViewItem;
        if (selectedSearchResultItem is null && SearchResults.Count > 0)
        {
            selectedSearchResultItem = SearchResults[0];
        }

        if (selectedSearchResultItem is null || selectedSearchResultItem == GuiToolProvider.NoResultFoundItem)
        {
            return;
        }

        // Select the actual menu item in the navigation view. This will trigger the navigation.
        SelectedMenuItem = GetBestMenuItemToSelect(selectedSearchResultItem);
    }

    private INotifyPropertyChanged GetBestMenuItemToSelect(object currentSelectedMenuItem)
    {
        Guard.IsNotEmpty((IReadOnlyList<INotifyPropertyChanged>)HeaderAndBodyToolViewItems);
        Guard.IsNotNull(currentSelectedMenuItem);

        if (currentSelectedMenuItem is GroupViewItem groupViewItem)
        {
            return groupViewItem;
        }

        GuiToolInstance? guiToolInstance = null;
        if (currentSelectedMenuItem is GuiToolViewItem guiToolViewItem)
        {
            guiToolInstance = guiToolViewItem.ToolInstance;
        }
        else if (currentSelectedMenuItem is GuiToolInstance instance)
        {
            guiToolInstance = instance;
        }

        if (guiToolInstance is not null)
        {
            GuiToolViewItem? itemToSelect = _guiToolProvider.GetViewItemFromTool(guiToolInstance).FirstOrDefault();
            if (itemToSelect is not null)
            {
                return itemToSelect;
            }
        }

        INotifyPropertyChanged? firstItem = HeaderAndBodyToolViewItems[0];
        Guard.IsNotNull(firstItem);
        return firstItem;
    }

    private void OnChangeSelectedMenuItemMessageReceived(MainWindowViewModel vm, ChangeSelectedMenuItemMessage message)
    {
        // Select the actual menu item in the navigation view. This will trigger the navigation.
        SelectedMenuItem = GetBestMenuItemToSelect(message.Value);

        // If this is not null, it means that the user has selected a tool that has been detected by Smart Detection.
        if (message.SmartDetectionInfo is not null)
        {
            // Send the data to the tool.
            message.Value.PassSmartDetectedData(message.SmartDetectionInfo.DataTypeName, message.SmartDetectionInfo.ParsedData);
        }
    }

    private async Task CheckForUpdateAsync()
    {
        using CancellationTokenSource cancellationTokenSource = new();
        cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(5));
        CancellationToken token = cancellationTokenSource.Token;

        UpdateAvailable = await AppHelper.CheckForUpdateAsync(_webClientService, _versionService, token);
    }

    private static bool AreOldAndNewClipboardDataEqual(object? oldData, object? newData)
    {
        if (oldData is null && newData is null)
        {
            return true;
        }

        if (oldData is null || newData is null)
        {
            return false;
        }

        if (oldData.GetType() != newData.GetType())
        {
            return false;
        }

        if (oldData is string oldString && newData is string newString)
        {
            return string.Equals(oldString, newString, StringComparison.Ordinal);
        }

        if (oldData is FileInfo[] oldFileList && newData is FileInfo[] newFileList)
        {
            return AreFileListEqual(oldFileList, newFileList);
        }

        if (oldData is Image<Rgba32> oldImage && newData is Image<Rgba32> newImage)
        {
            return AreImageEqual(oldImage, newImage);
        }

        return object.Equals(oldData, newData);
    }

    private static bool AreFileListEqual(FileInfo[] source, FileInfo[] target)
    {
        if (source.Length != target.Length)
        {
            return false;
        }

        for (int i = 0; i < source.Length; i++)
        {
            if (!string.Equals(source[i].FullName, target[i].FullName, StringComparison.Ordinal))
            {
                return false;
            }
        }

        return true;
    }

    private static bool AreImageEqual(Image<Rgba32> source, Image<Rgba32> target)
    {
        if (source.Width != target.Width || source.Height != target.Height)
        {
            return false;
        }

        for (int x = 0; x < source.Width; x++)
        {
            for (int y = 0; y < source.Height; y++)
            {
                if (source[x, y] != target[x, y])
                {
                    return false;
                }
            }
        }

        return true;
    }

    [LoggerMessage(1, LogLevel.Warning, "Failed to perform smart detection.")]
    partial void LogSmartDetectionFailed(Exception ex);
}
