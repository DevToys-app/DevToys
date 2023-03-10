﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DevToys.Api;
using DevToys.Core.Tools;
using DevToys.Core.Tools.ViewItems;
using DevToys.Localization.Strings.ToolPage;

namespace DevToys.Business.ViewModels;

[Export]
[PartCreationPolicy(CreationPolicy.NonShared)]
internal sealed partial class ToolPageViewModel : ObservableRecipient
{
#pragma warning disable IDE0044 // Add readonly modifier
    [Import]
    private GuiToolProvider _guiToolProvider = null!;
#pragma warning restore IDE0044 // Add readonly modifier

    private GuiToolViewItem? _guiToolViewItem;

    [ObservableProperty]
    private string _headerText = string.Empty;

    /// <summary>
    /// Indicates whether the <see cref="SelectedMenuItem"/> can be added or removed from favorites.
    /// </summary>
    internal bool IsSelectedMenuItemSupportFavorite => _guiToolViewItem is not null && !_guiToolViewItem.ToolInstance.NotFavorable;

    /// <summary>
    /// Indicates whether the <see cref="SelectedMenuItem"/> is a favorite tool or not.
    /// </summary>
    internal bool IsSelectedMenuItemAFavoriteTool => _guiToolViewItem is not null && _guiToolProvider.GetToolIsFavorite(_guiToolViewItem.ToolInstance);

    /// <summary>
    /// Gets the UI of the tool.
    /// </summary>
    internal IUIElement? ToolView => _guiToolViewItem?.ToolInstance.View;

    internal void Load(GuiToolViewItem guiToolViewItem)
    {
        Guard.IsNotNull(guiToolViewItem);

        // Add the tool to most recent ones.
        _guiToolProvider.SetMostRecentUsedTool(guiToolViewItem.ToolInstance);

        _guiToolViewItem = guiToolViewItem;

        HeaderText = guiToolViewItem.ToolInstance.LongOrShortDisplayTitle;
    }

    /// <summary>
    /// Toggles the favorite status of the <see cref="SelectedMenuItem"/>.
    /// </summary>
    [RelayCommand]
    private void ToggleSelectedMenuItemFavorite()
    {
        Guard.IsNotNull(_guiToolViewItem);
        _guiToolProvider.SetToolIsFavorite(_guiToolViewItem.ToolInstance, !_guiToolProvider.GetToolIsFavorite(_guiToolViewItem.ToolInstance));
        OnPropertyChanged(nameof(IsSelectedMenuItemAFavoriteTool));
    }

    internal string GetFavoriteButtonText(bool isSelectedMenuItemAFavoriteTool)
    {
        if (isSelectedMenuItemAFavoriteTool)
        {
            return ToolPage.RemoveFromFavorites;
        }

        return ToolPage.AddToFavorites;
    }
}
