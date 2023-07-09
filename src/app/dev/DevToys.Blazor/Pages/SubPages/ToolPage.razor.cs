using DevToys.Blazor.Components;
using DevToys.Blazor.Core.Services;
using DevToys.Business.ViewModels;
using DevToys.Core.Tools.ViewItems;

namespace DevToys.Blazor.Pages.SubPages;

public partial class ToolPage : MefComponentBase, IDisposable
{
    [Inject]
    internal IWindowService WindowService { get; set; } = default!;

    [Import]
    internal ToolPageViewModel ViewModel { get; set; } = default!;

    [Parameter]
    public GuiToolViewItem? GuiToolViewItem { get; set; }

    [Parameter]
    public bool IsInFullScreenMode { get; set; }

    [CascadingParameter]
    protected Index? IndexPage { get; set; }

    protected override void OnInitialized()
    {
        base.OnInitialized();
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (GuiToolViewItem is not null)
        {
            ViewModel.Load(GuiToolViewItem);
            ViewModel.ToolView.PropertyChanged -= ToolView_PropertyChanged;
            ViewModel.ToolView.PropertyChanged += ToolView_PropertyChanged;
        }
    }

    public void Dispose()
    {
        ViewModel.ToolView.PropertyChanged -= ToolView_PropertyChanged;
        GC.SuppressFinalize(this);
    }

    private void ToolView_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        StateHasChanged();
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
}
