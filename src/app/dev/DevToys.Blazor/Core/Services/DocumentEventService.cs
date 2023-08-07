namespace DevToys.Blazor.Core.Services;

public sealed class DocumentEventService : IDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private readonly DotNetObjectReference<DocumentEventService> _reference;
    private readonly Dictionary<string, List<EventSubscription>> _callbackResolver = new();
    private readonly DisposableSemaphore _semaphore = new();

    private bool _isInitialized;

    public DocumentEventService(IJSRuntime jsRuntime)
    {
        Guard.IsNotNull(jsRuntime);
        _jsRuntime = jsRuntime;
        _reference = DotNetObjectReference.Create(this);
    }

    public async Task<IAsyncDisposable> SubscribeAsync(string eventName, Func<string, ValueTask> callback)
    {
        using (await _semaphore.WaitAsync(CancellationToken.None))
        {
            if (!_isInitialized)
            {
                await _jsRuntime.InvokeVoidWithErrorHandlingAsync("devtoys.DOM.registerDocumentEventService", _reference);
                _isInitialized = true;
            }
        }

        (EventSubscription eventSubscription, bool firstTimeRegistered) = await RegisterCallBackAsync(eventName, callback);

        if (firstTimeRegistered)
        {
            await _jsRuntime.InvokeVoidWithErrorHandlingAsync("devtoys.DOM.subscribeDocumentEvent", eventName);
        }

        return eventSubscription;
    }

    [JSInvokable]
    public void EventCallback(string eventName, string eventJson)
    {
        if (_callbackResolver.TryGetValue(eventName, out List<EventSubscription>? listeners))
        {
            for (int i = 0; i < listeners.Count; i++)
            {
                listeners[i].Invoke(eventJson);
            }
        }
    }

    private async ValueTask UnsubscribeAsync(EventSubscription listener)
    {
        using (await _semaphore.WaitAsync(CancellationToken.None))
        {
            if (_callbackResolver.TryGetValue(listener.EventName, out List<EventSubscription>? listeners))
            {
                listeners.Remove(listener);

                if (listeners.Count == 0)
                {
                    _callbackResolver.Remove(listener.EventName);
                    try
                    {
                        await _jsRuntime.InvokeVoidWithErrorHandlingAsync("devtoys.DOM.unsubscribeDocumentEvent", listener.EventName);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }
    }

    private async Task<(EventSubscription, bool firstTimeRegistered)> RegisterCallBackAsync(string eventName, Func<string, ValueTask> callback)
    {
        using (await _semaphore.WaitAsync(CancellationToken.None))
        {
            bool firstTimeRegistered = false;
            var eventSubscription = new EventSubscription(eventName, callback, this);

            if (!_callbackResolver.TryGetValue(eventName, out List<EventSubscription>? listeners))
            {
                listeners = new List<EventSubscription>();
                _callbackResolver[eventName] = listeners;
                firstTimeRegistered = true;
            }

            listeners.Add(eventSubscription);

            return new(eventSubscription, firstTimeRegistered);
        }
    }

    public void Dispose()
    {
        _semaphore.Dispose();
    }

    private class EventSubscription : IAsyncDisposable
    {
        private readonly Func<string, ValueTask> _callback;
        private readonly DocumentEventService _documentEventService;

        internal EventSubscription(string eventName, Func<string, ValueTask> callback, DocumentEventService documentEventService)
        {
            Guard.IsNotNull(eventName);
            Guard.IsNotNull(callback);
            Guard.IsNotNull(documentEventService);
            EventName = eventName;
            _callback = callback;
            _documentEventService = documentEventService;
        }

        internal string EventName { get; }

        internal void Invoke(string eventJson)
        {
            _callback.Invoke(eventJson);
        }

        public async ValueTask DisposeAsync()
        {
            await _documentEventService.UnsubscribeAsync(this);
        }
    }
}
