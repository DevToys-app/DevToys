﻿namespace DevToys.Blazor.Components;

public abstract class JSStyledComponentBase : StyledComponentBase, IAsyncDisposable
{
    private Task<IJSObjectReference>? _jsModule;
    private DotNetObjectReference<JSStyledComponentBase>? _reference;

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

    /// <summary>
    /// Gets the reference for the current component.
    /// </summary>
    /// <value>The reference.</value>
    protected DotNetObjectReference<JSStyledComponentBase> Reference
    {
        get
        {
            _reference ??= DotNetObjectReference.Create(this);
            return _reference;
        }
    }

    /// <summary />
    public virtual async ValueTask DisposeAsync()
    {
        _reference?.Dispose();
        _reference = null;

        if (_jsModule is not null)
        {
            await (await _jsModule).DisposeAsync();
        }

        GC.SuppressFinalize(this);
    }
}
