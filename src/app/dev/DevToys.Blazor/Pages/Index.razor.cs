﻿using DevToys.Blazor.Components;
using DevToys.Blazor.Core.Services;
using DevToys.Blazor.Pages.Dialogs;
using DevToys.Blazor.Pages.SubPages;
using DevToys.Business.Services;
using DevToys.Business.ViewModels;
using DevToys.Core;
using DevToys.Core.Settings;
using DevToys.Core.Tools;
using DevToys.Core.Tools.ViewItems;
using DevToys.Localization.Strings.ToolGroupPage;
using Microsoft.AspNetCore.Components.Web;

namespace DevToys.Blazor.Pages;

public partial class Index : MefComponentBase
{
    /// <summary>
    /// Whether the main window should be maximized.
    /// </summary>
    private static readonly SettingDefinition<NavBarSidebarStates> UserPreferredNavBarState
        = new(
            name: nameof(UserPreferredNavBarState),
            defaultValue: NavBarSidebarStates.Expanded);

    private const int TitleBarMarginLeftWhenCompactOverlayMode = 0;
    private const int TitleBarMarginLeftWhenNavBarHidden = 90;
    private const int TitleBarMarginLeftWhenNavBarNotHidden = 47;

    private FirstStartDialog _firstStartDialog = default!;
    private NavBar<INotifyPropertyChanged, GuiToolViewItem> _navBar = default!;
    private IFocusable? _contentPage;

    [Import]
    internal MainWindowViewModel ViewModel { get; set; } = default!;

    [Import]
    internal GuiToolProvider GuiToolProvider { get; set; } = default!;

    [Import]
    internal TitleBarInfoProvider TitleBarInfoProvider { get; set; } = default!;

    [Import]
    internal IThemeListener ThemeListener { get; set; } = default!;

    [Import]
    internal ISettingsProvider SettingsProvider { get; set; } = default!;

    [Import]
    internal CommandLineLauncherService CommandLineLauncherService { get; set; } = default!;

    [Inject]
    internal ContextMenuService ContextMenuService { get; set; } = default!;

    [Inject]
    internal UIDialogService UIDialogService { get; set; } = default!;

    [Inject]
    internal GlobalDialogService GlobalDialogService { get; set; } = default!;

    [Inject]
    internal IWindowService WindowService { get; set; } = default!;

    /// <summary>
    /// Indicates whether we're transitioning to another selected menu item.
    /// </summary>
    public bool IsTransitioning { get; set; }

    [Parameter]
    public bool WindowHasFocus { get; set; }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        UIDialogService.IsDialogOpenedChanged += DialogService_IsDialogOpenedChanged;
        ViewModel.SelectedMenuItemChanged += ViewModel_SelectedMenuItemChanged;
        ViewModel.SelectedMenuItem ??= ViewModel.HeaderAndBodyToolViewItems[0];
        ContextMenuService.IsContextMenuOpenedChanged += ContextMenuService_IsContextMenuOpenedChanged;
        WindowService.WindowActivated += WindowService_WindowActivated;
        WindowService.WindowDeactivated += WindowService_WindowDeactivated;
        WindowService.WindowClosing += WindowService_WindowClosing;
        TitleBarInfoProvider.PropertyChanged += TitleBarMarginProvider_PropertyChanged;

        TitleBarInfoProvider.TitleBarMarginRight = 40;
        WindowHasFocus = true;
    }

    private void DialogService_IsDialogOpenedChanged(object? sender, EventArgs e)
    {
        StateHasChanged();
    }

    private void ViewModel_SelectedMenuItemChanged(object? sender, EventArgs e)
    {
        // This will force the page content to clear our, disposing the current tool group or tool component before creating a new one, instead
        // of re-using the one currently displayed.
        IsTransitioning = true;
        StateHasChanged();
    }

    private void ContextMenuService_IsContextMenuOpenedChanged(object? sender, EventArgs e)
    {
        StateHasChanged();
    }

    private void TitleBarMarginProvider_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        StateHasChanged();
    }

    private void WindowService_WindowActivated(object? sender, EventArgs e)
    {
        WindowHasFocus = true;
        StateHasChanged();

        // Start Smart Detection
        ViewModel.RunSmartDetectionAsync(WindowService.IsCompactOverlayMode, GlobalDialogService.IsDialogOpened)
            .ContinueWith(async _ =>
            {
                await InvokeAsync(StateHasChanged);
            });
    }

    private void WindowService_WindowDeactivated(object? sender, EventArgs e)
    {
        WindowHasFocus = false;
        StateHasChanged();
    }

    private void WindowService_WindowClosing(object? sender, EventArgs e)
    {
        SettingsProvider.SetSetting(UserPreferredNavBarState, _navBar.UserPreferredState);
    }

    private void OnBackButtonClicked()
    {
        ViewModel.GoBack();
    }

    private void OnToggleCompactOverlayModeButtonClick()
    {
        WindowService.IsCompactOverlayMode = !WindowService.IsCompactOverlayMode;

        OnHiddenStateChanged(_navBar.IsHiddenMode);
    }

    private void OnHiddenStateChanged(bool isHidden)
    {
        if (WindowService.IsCompactOverlayMode)
        {
            TitleBarInfoProvider.TitleBarMarginLeft = TitleBarMarginLeftWhenCompactOverlayMode;
        }
        else if (isHidden)
        {
            TitleBarInfoProvider.TitleBarMarginLeft = TitleBarMarginLeftWhenNavBarHidden;
        }
        else
        {
            TitleBarInfoProvider.TitleBarMarginLeft = TitleBarMarginLeftWhenNavBarNotHidden;
        }
    }

    private void OnSearchQueryChanged(string searchQuery)
    {
        ViewModel.SearchQuery = searchQuery;
        ViewModel.SearchBoxTextChangedCommand.Execute(null);
    }

    private void OnSearchQuerySubmitted(GuiToolViewItem? selectedItem)
    {
        ViewModel.SearchBoxQuerySubmittedCommand.Execute(selectedItem);
    }

    private Task OnBuildingContextMenuAsync(ListBoxItemBuildingContextMenuEventArgs args)
    {
        Guard.IsNotNull(args.ItemValue);

        // Create the context menu for the items in the NavBar.
        if (args.ContextMenuItems.Count == 0)
        {
            // Open in new window
            if (args.ItemValue is GuiToolViewItem item)
            {
                args.ContextMenuItems.Add(
                new ContextMenuItem
                {
                    IconGlyph = '\uEE7A',
                    Text = ToolGroupPage.OpenInNewWindow,
                    OnClick = EventCallback.Factory.Create<DropDownListItem>(this, OnOpenInNewWindowContextMenuItemClick)
                });

                void OnOpenInNewWindowContextMenuItemClick()
                {
                    CommandLineLauncherService.LaunchTool(item.ToolInstance.InternalComponentName);
                }
            }
        }

        return Task.CompletedTask;
    }

    private void OnMouseUp(MouseEventArgs ev)
    {
        if (ev.Button == 3)
        {
            if (ViewModel.CanGoBack)
            {
                ViewModel.GoBack();
            }
        }
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            _navBar.UserPreferredState = SettingsProvider.GetSetting(UserPreferredNavBarState);

            // Focus on the Search Box.
            _navBar.TryFocusSearchBoxAsync();

            if (SettingsProvider.GetSetting(PredefinedSettings.IsFirstStart))
            {
                _firstStartDialog.Open();
            }

            // Start Smart Detection
            ViewModel.RunSmartDetectionAsync(WindowService.IsCompactOverlayMode, GlobalDialogService.IsDialogOpened).Forget();
        }

        if (IsTransitioning)
        {
            // This will force the page content to re-populate.
            IsTransitioning = false;
            StateHasChanged();

            if (_contentPage is not null && !firstRender)
            {
                _contentPage.FocusAsync().Forget();
            }
        }

        base.OnAfterRender(firstRender);
    }
}
