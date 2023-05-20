namespace DevToys.MauiBlazor.Components;

public partial class MonacoEditor : JSStyledComponentBase, IAsyncDisposable
{
    private const string JAVASCRIPT_FILE = "./Components/MonacoEditor.razor.js";
    private const string MONACO_VS_PATH = "./lib/monaco-editor/min/vs";

    private DotNetObjectReference<MonacoEditor>? _objRef = null;
    private string _value = """
                            using System;
                            void Main()
                            {
                                Console.WriteLine("Hello World");
                            }
                            """;

    protected string? ClassValue => new CssBuilder(FinalCssClasses).Build();

    protected string? StyleValue => new StyleBuilder()
        .AddStyle("height", Height, () => !string.IsNullOrEmpty(Height))
        .AddStyle("width", Width, () => !string.IsNullOrEmpty(Width))
        .AddStyle("border: calc(var(--stroke-width) * 1px) solid var(--neutral-stroke-rest)")
        .AddStyle(Style)
        .Build();

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    private IJSObjectReference JSModule { get; set; } = default!;

    /// <summary>
    /// Language used by the editor: csharp, javascript, ...
    /// </summary>
    [Parameter]
    public string Language { get; set; } = "csharp";

    /// <summary>
    /// Height of this component.
    /// </summary>
    [Parameter]
    public string Height { get; set; } = "300px";

    /// <summary>
    /// Width of this component.
    /// </summary>
    [Parameter]
    public string Width { get; set; } = "100%";

    /// <summary>
    /// Theme of the editor (Light or Dark).
    /// </summary>
    [Parameter]
    public bool IsDarkMode { get; set; } = false;

    /// <summary>
    /// Gets or sets the value of the input. This should be used with two-way binding.
    /// </summary>
    [Parameter]
    public string Value
    {
        get
        {
            return _value;
        }

        set
        {
            _value = value;
        }
    }

    /// <summary>
    /// Gets or sets a callback that updates the bound value.
    /// </summary>
    [Parameter]
    public EventCallback<string> OnValueChanged { get; set; }

    /// <summary />
    protected override async Task OnParametersSetAsync()
    {
        if (JSModule != null)
        {
            await JSModule.InvokeVoidAsync(
                "monacoSetOptions",
                Id,
                new { Value = this.Value, Theme = GetTheme(IsDarkMode), Language = this.Language, });
        }
    }

    /// <summary />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            JSModule = await JSRuntime.InvokeAsync<IJSObjectReference>("import", JAVASCRIPT_FILE);
            _objRef = DotNetObjectReference.Create(this);

            var options = new
            {
                Value = Value,
                Language = Language,
                Theme = GetTheme(IsDarkMode),
                Path = MONACO_VS_PATH,
                LineNumbers = true,
                ReadOnly = false,
            };
            await JSModule.InvokeVoidAsync("monacoInitialize", Id, _objRef, options);
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    /// <summary />
    private string GetTheme(bool isDarkMode)
    {
        return isDarkMode ? "vs-dark" : "vs";
    }

    /// <summary />
    [JSInvokable]
    public async Task UpdateValueAsync(string value)
    {
        _value = value;
        if (OnValueChanged.HasDelegate)
        {
            await OnValueChanged.InvokeAsync(_value);
        }
    }

    /// <summary />
    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        if (JSModule is not null)
        {
            await JSModule.DisposeAsync();
        }

        if (_objRef is not null)
        {
            _objRef.Dispose();
        }
    }
}
