namespace DevToys.Blazor.Components.UIElements;

public partial class UIWebViewPresenter : JSStyledComponentBase
{
    protected override string? JavaScriptFile => "./_content/DevToys.Blazor/Components/UIElements/UIWebViewPresenter.razor.js";

    [Parameter]
    public IUIWebView UIWebView { get; set; } = default!;

    private ElementReference _iframeElement;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        UIWebView.SourceChanged += UIWebView_SourceChanged;
    }

    public override async ValueTask DisposeAsync()
    {
        UIWebView.SourceChanged -= UIWebView_SourceChanged;
        await base.DisposeAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender)
        {
            await UpdateWebViewContentAsync();
        }
    }

    private void UIWebView_SourceChanged(object? sender, EventArgs e)
    {
        UpdateWebViewContentAsync().Forget();
    }

    private async Task UpdateWebViewContentAsync()
    {
        using (await Semaphore.WaitAsync(CancellationToken.None))
        {
            if (UIWebView.Source.HasValue)
            {
                if (UIWebView.Source.Value.TryPickT0(out string html, out Uri uri))
                {
                    await (await JSModule).InvokeVoidWithErrorHandlingAsync("webViewNavigateToHtml", _iframeElement, html);
                }
                else
                {
                    await (await JSModule).InvokeVoidWithErrorHandlingAsync("webViewNavigateToUri", _iframeElement, uri.ToString());
                }
            }
        }
    }
}
