using DevToys.Blazor.Core.Services;

namespace DevToys.Blazor.Components;

public partial class Dialog : JSStyledComponentBase, IFocusable
{
    private bool _isOpen;
    private IDisposable? _session;

    [Inject]
    internal GlobalDialogService DialogService { get; set; } = default!;

    /// <summary>
    /// Gets or sets the content to be rendered inside the component.
    /// </summary>
    [Parameter]
    public RenderFragment? Content { get; set; }

    /// <summary>
    /// Gets or sets the content to be rendered inside the component.
    /// </summary>
    [Parameter]
    public RenderFragment? Footer { get; set; }

    [Parameter]
    public bool IsOpen { get; set; }

    [Parameter]
    public bool Dismissible { get; set; }

    /// <summary>
    /// Raised when the dialog got dismissed.
    /// </summary>
    [Parameter]
    public EventCallback OnDismissed { get; set; }

    /// <summary>
    /// Raised when the dialog got closed.
    /// </summary>
    [Parameter]
    public EventCallback OnClosed { get; set; }

    internal event EventHandler? Closed;

    public ValueTask<bool> FocusAsync()
    {
        return JSRuntime.InvokeVoidWithErrorHandlingAsync("devtoys.DOM.setFocus", Element);
    }

    public bool TryOpen()
    {
        IsOpen = DialogService.TryOpenDialog(out _session);
        _isOpen = IsOpen;
        if (IsOpen)
        {
            StateHasChanged();
        }

        return IsOpen;
    }

    public Task WaitUntilClosedAsync()
    {
        if (!IsOpen)
        {
            return Task.CompletedTask;
        }

        var tcs = new TaskCompletionSource();
        EventHandler handler = null!;
        handler = (sender, e) =>
        {
            Closed -= handler;
            tcs.SetResult();
        };

        Closed += handler;

        return tcs.Task;
    }

    internal void Close()
    {
        IsOpen = false;
        _isOpen = false;
        _session?.Dispose();

        StateHasChanged();

        Closed?.Invoke(this, EventArgs.Empty);
        if (OnClosed.HasDelegate)
        {
            OnClosed.InvokeAsync();
        }
    }

    public override ValueTask DisposeAsync()
    {
        Close();
        return base.DisposeAsync();
    }

    protected override void OnParametersSet()
    {
        if (IsOpen != _isOpen)
        {
            if (IsOpen)
            {
                _isOpen = TryOpen();
            }
            else
            {
                Close();
            }
        }

        base.OnParametersSet();
    }

    protected override Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            if (IsOpen)
            {
                FocusAsync();
            }
        }

        return base.OnAfterRenderAsync(firstRender);
    }

    private void OnDismiss()
    {
        if (Dismissible)
        {
            Close();
            if (OnDismissed.HasDelegate)
            {
                OnDismissed.InvokeAsync();
            }
        }
    }
}
