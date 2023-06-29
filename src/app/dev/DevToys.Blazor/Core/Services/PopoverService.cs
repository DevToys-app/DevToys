namespace DevToys.Blazor.Core.Services;

public sealed class PopoverService : IAsyncDisposable
{
    private readonly DisposableSemaphore _semaphore = new();
    private readonly Dictionary<Guid, PopoverHandler> _handlers = new();
    private readonly IJSRuntime _jsRuntime;
    private bool _isInitialized = false;

    public PopoverService(IJSRuntime jsRuntime)
    {
        Guard.IsNotNull(jsRuntime);
        _jsRuntime = jsRuntime;
    }

    internal IEnumerable<PopoverHandler> Handlers => _handlers.Values.AsEnumerable();

    internal event EventHandler? FragmentsChanged;

    public async ValueTask DisposeAsync()
    {
        if (_isInitialized)
        {
            await _jsRuntime.InvokeVoidWithErrorHandlingAsync("devtoys.Popover.dispose");
        }
    }

    internal async Task InitializeIfNeeded()
    {
        if (!_isInitialized)
        {
            try
            {
                using (await _semaphore.WaitAsync(CancellationToken.None))
                {
                    if (!_isInitialized)
                    {
                        await _jsRuntime.InvokeVoidWithErrorHandlingAsync("devtoys.Popover.initialize", "main-layout", 0);
                        _isInitialized = true;
                    }
                }
            }
            finally
            {
            }
        }
    }

    internal PopoverHandler Register(RenderFragment fragment)
    {
        var handler = new PopoverHandler(fragment, _jsRuntime, () => { /*not doing anything on purpose for now*/ });
        _handlers.Add(handler.Id, handler);

        FragmentsChanged?.Invoke(this, EventArgs.Empty);

        return handler;
    }

    internal async Task<bool> Unregister(PopoverHandler handler)
    {
        if (handler == null || _handlers.Remove(handler.Id) == false)
        {
            return false;
        }

        await handler.Detach();

        FragmentsChanged?.Invoke(this, EventArgs.Empty);

        return true;
    }
}
