using System.Globalization;
using System.Web;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;

namespace DevToys.MauiBlazor.Components;
public partial class NavbarAnchor : MefLayoutComponentBase, IDisposable
{
    private const string DefaultActiveClass = "active";

    private bool _isActive;
    private string _hrefAbsolute = string.Empty;
    private string _class = string.Empty;
    private string _href = string.Empty;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Parameter]
    public string Href
    {
        get => _href;
        set
        {
            _href = HttpUtility.UrlEncode(value);
        }
    }

    [Parameter]
    public string? Title { get; set; } = string.Empty;

    [CascadingParameter]
    public NavbarItem? NavbarItem { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public EventCallback<bool> Clicked { get; set; }

    protected override void AppendClasses(ClassHelper helper)
    {
        helper.Append("item-link");
        if (_isActive)
        {
            helper.Append("active");
        }
        helper.Append(Classess);
        base.AppendClasses(helper);
    }

    protected override void OnInitialized()
    {
        NavigationManager.LocationChanged += OnLocationChanged;
        base.OnInitialized();
    }

    protected override void OnParametersSet()
    {
        string? href = Href;
        if (AdditionalAttributes != null && AdditionalAttributes.TryGetValue("href", out object? obj))
        {
            href = Convert.ToString(obj, CultureInfo.InvariantCulture);
        }

        _hrefAbsolute = href == null ? null : NavigationManager.ToAbsoluteUri(href).AbsoluteUri;
        _isActive = ShouldMatch(NavigationManager.Uri);
        if (_isActive)
        {
            ClassesHasChanged();
            StateHasChanged();
        }
        base.OnParametersSet();
    }

    protected async Task OnClickHandler(MouseEventArgs eventArgs)
    {
        NavigationManager.NavigateTo(Href, false);
        await Clicked.InvokeAsync();
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs args)
    {
        bool shouldBeActiveNow = ShouldMatch(args.Location);
        Clicked.InvokeAsync(shouldBeActiveNow);
        if (shouldBeActiveNow != _isActive)
        {
            _isActive = shouldBeActiveNow;
            ClassesHasChanged();
            InvokeAsync(StateHasChanged);
        }
    }

    private bool ShouldMatch(string uri)
    {
        if (uri.Equals(_hrefAbsolute, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
        return false;
    }

    public void Dispose()
        => NavigationManager.LocationChanged -= OnLocationChanged;
}
