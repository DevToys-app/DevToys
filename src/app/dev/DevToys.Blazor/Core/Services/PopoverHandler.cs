using DevToys.Blazor.Components;

namespace DevToys.Blazor.Core.Services;

internal sealed class PopoverHandler
{
    private readonly DisposableSemaphore _semaphore = new();
    private readonly IJSRuntime _runtime;
    private readonly Action _updater;
    private bool _detached;

    public PopoverHandler(RenderFragment fragment, IJSRuntime jsInterop, Action updater)
    {
        Guard.IsNotNull(fragment);
        Guard.IsNotNull(jsInterop);
        Guard.IsNotNull(updater);
        Fragment = fragment;
        _runtime = jsInterop;
        _updater = updater;
    }

    public Guid Id { get; } = Guid.NewGuid();

    public RenderFragment Fragment { get; private set; }

    public bool IsConnected { get; private set; }

    public string? Class { get; private set; }

    public string? Style { get; private set; }

    public bool ShowContent { get; private set; }

    public DateTime? ActivationDate { get; private set; }

    public IDictionary<string, object>? AdditionalAttributes { get; set; } = new Dictionary<string, object>();

    public Render? ElementReference { get; set; }

    public void SetComponentBaseParameters(Popover componentBase, string @class, string style, bool showContent)
    {
        Class = @class;
        Style = style;
        AdditionalAttributes = componentBase.AdditionalAttributes;
        ShowContent = showContent;
        if (showContent)
        {
            ActivationDate = DateTime.Now;
        }
        else
        {
            ActivationDate = null;
        }
    }

    public async Task UpdateFragmentAsync(
        RenderFragment fragment,
        Popover componentBase,
        string @class,
        string style,
        bool showContent)
    {
        using (await _semaphore.WaitAsync(CancellationToken.None))
        {
            try
            {
                if (_detached)
                {
                    return;
                }

                Fragment = fragment;
                SetComponentBaseParameters(componentBase, @class, @style, showContent);
                ElementReference?.StateHasChanged();
                _updater.Invoke(); // <-- this doesn't do anything anymore except making unit tests happy
            }
            finally
            {
            }
        }
    }

    public async Task Initialize()
    {
        using (await _semaphore.WaitAsync(CancellationToken.None))
        {
            try
            {
                if (_detached)
                {
                    // If _detached is True, it means Detach() was invoked before Initialize() has had
                    // a chance to run. In this case, we just want to return and not do anything else
                    // otherwise we will end up with a memory leak.
                    return;
                }

                IsConnected = await _runtime.InvokeVoidWithErrorHandlingAsync("devtoys.Popover.connect", Id);
            }
            finally
            {
            }
        }
    }

    public async Task Detach()
    {
        using (await _semaphore.WaitAsync(CancellationToken.None))
        {
            try
            {
                _detached = true;

                if (IsConnected)
                {
                    await _runtime.InvokeVoidWithErrorHandlingAsync("devtoys.Popover.disconnect", Id);
                }
            }
            finally
            {
                IsConnected = false;
            }
        }
    }
}
