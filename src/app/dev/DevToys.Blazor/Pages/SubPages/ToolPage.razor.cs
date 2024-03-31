using DevToys.Blazor.Components;
using DevToys.Blazor.Core.Services;
using DevToys.Business.ViewModels;
using DevToys.Core.Debugger;
using DevToys.Core.Tools.ViewItems;

namespace DevToys.Blazor.Pages.SubPages;

public partial class ToolPage : MefComponentBase, IFocusable
{
    private ScrollViewer? _scrollViewer;
    private FullScreenContainer? _fullScreenContainer;

    [Inject]
    internal IWindowService WindowService { get; set; } = default!;

    [Inject]
    internal UIDialogService UIDialogService { get; set; } = default!;

    [Import]
    internal ToolPageViewModel ViewModel { get; set; } = default!;

    [Import]
    internal HotReloadService HotReloadService { get; set; } = default!;

    [Parameter]
    public GuiToolViewItem? GuiToolViewItem { get; set; }

    [Parameter]
    public bool IsInFullScreenMode { get; set; }

    [CascadingParameter]
    protected Index? IndexPage { get; set; }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        UIDialogService.CloseDialogRequested += DialogService_CloseDialogRequested;
        HotReloadService.HotReloadRequestUpdateApplication += OnHotReloadRequestUpdateApplication;
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (GuiToolViewItem is not null)
        {
            ViewModel.Load(GuiToolViewItem);
            if (ViewModel.ToolView is not null)
            {
                ViewModel.ToolView.PropertyChanged -= ToolView_PropertyChanged;
                ViewModel.ToolView.PropertyChanged += ToolView_PropertyChanged;
                ViewModel.ToolView.CurrentOpenedDialogChanged -= ToolView_CurrentOpenedDialogChanged;
                ViewModel.ToolView.CurrentOpenedDialogChanged += ToolView_CurrentOpenedDialogChanged;
            }
        }
    }

    public override async ValueTask DisposeAsync()
    {
        if (_fullScreenContainer is not null && _fullScreenContainer.IsInFullScreenMode)
        {
            // If we're switching to another tool or group and that the current tool is in full
            // screen mode, we need to force quit the full screen mode to avoid ending up in a bad state.
            await _fullScreenContainer.ForceQuitFullScreenModeAsync();
        }

        HotReloadService.HotReloadRequestUpdateApplication -= OnHotReloadRequestUpdateApplication;
        UIDialogService.CloseDialogRequested -= DialogService_CloseDialogRequested;

        if (ViewModel.ToolView is not null)
        {
            ViewModel.ToolView.PropertyChanged -= ToolView_PropertyChanged;
            ViewModel.ToolView.CurrentOpenedDialogChanged -= ToolView_CurrentOpenedDialogChanged;
        }

        await base.DisposeAsync();
    }

    public ValueTask<bool> FocusAsync()
    {
        Guard.IsNotNull(_scrollViewer);
        return JSRuntime.InvokeVoidWithErrorHandlingAsync("devtoys.DOM.setFocus", _scrollViewer.Element);
    }

    private void ToolView_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        StateHasChanged();
    }

    private void ToolView_CurrentOpenedDialogChanged(object? sender, EventArgs e)
    {
        Guard.IsNotNull(ViewModel.ToolView);
        if (ViewModel.ToolView.CurrentOpenedDialog is null)
        {
            UIDialogService.CloseDialog();
        }
        else
        {
            ViewModel.ToolView.CurrentOpenedDialog.IsOpenedChanged += DialogService_CloseDialogRequested;
            UIDialogService.TryOpenDialog(ViewModel.ToolView.CurrentOpenedDialog.IsDismissible);
        }
    }

    private void DialogService_CloseDialogRequested(object? sender, EventArgs e)
    {
        Guard.IsNotNull(ViewModel.ToolView);
        if (ViewModel.ToolView.CurrentOpenedDialog is not null)
        {
            ViewModel.ToolView.CurrentOpenedDialog.IsOpenedChanged -= DialogService_CloseDialogRequested;
        }

        UIDialogService.CloseDialog();
        ViewModel.ToolView.CurrentOpenedDialog?.Close();
    }

    private void OnIsInFullScreenModeChanged(bool isInFullScreenMode)
    {
        IsInFullScreenMode = isInFullScreenMode;
    }

    private void OnToggleFavorite()
    {
        Guard.IsNotNull(IndexPage);
        ViewModel.ToggleSelectedMenuItemFavoriteCommand.Execute(null);
        IndexPage.StateHasChanged();
    }

    private void OnHotReloadRequestUpdateApplication(object? sender, HotReloadEventArgs e)
    {
        ViewModel.RebuildViewCommand.Execute(null);
    }

    private void OnHotReloadButtonClick()
    {
        ViewModel.RebuildViewCommand.Execute(null);
    }
}
