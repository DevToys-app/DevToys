namespace DevToys.Blazor.Components;

public abstract class JSStyledComponentBase : StyledComponentBase, IAsyncDisposable
{
    private Task<IJSObjectReference>? _jsModule;

    /// <summary>
    /// Gets the JavaScript file to use in this component.
    /// </summary>
    protected virtual string? JavaScriptFile { get; }

    [Inject]
    protected IJSRuntime JSRuntime { get; set; } = default!;

    protected Task<IJSObjectReference> JSModule
    {
        get
        {
            lock (JSRuntime)
            {
                if (_jsModule is null)
                {
                    Guard.IsNotNullOrWhiteSpace(JavaScriptFile);
                    _jsModule = JSRuntime.InvokeAsync<IJSObjectReference>("import", JavaScriptFile).AsTask();
                }

                return _jsModule;
            }
        }
    }

    /// <summary />
    public virtual async ValueTask DisposeAsync()
    {
        if (_jsModule is not null)
        {
            await (await _jsModule).DisposeAsync();
        }

        GC.SuppressFinalize(this);
    }
}
