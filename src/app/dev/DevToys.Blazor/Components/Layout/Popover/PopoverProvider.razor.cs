using DevToys.Blazor.Core.Services;

namespace DevToys.Blazor.Components;

public partial class PopoverProvider : IDisposable
{
    private bool _isConnectedToService = false;

    [Inject]
    internal PopoverService PopoverService { get; set; } = default!;

    public void Dispose()
    {
        PopoverService.FragmentsChanged -= PopoverService_FragmentsChanged;
        GC.SuppressFinalize(this);
    }

    protected override void OnInitialized()
    {
        PopoverService.FragmentsChanged += PopoverService_FragmentsChanged;
        _isConnectedToService = true;
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (_isConnectedToService == false)
        {
            PopoverService.FragmentsChanged -= PopoverService_FragmentsChanged; // make sure to avoid multiple registration
            PopoverService.FragmentsChanged += PopoverService_FragmentsChanged;
            _isConnectedToService = true;
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await PopoverService.InitializeIfNeeded();
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    private void PopoverService_FragmentsChanged(object? sender, EventArgs e)
    {
        InvokeAsync(StateHasChanged);
    }
}
