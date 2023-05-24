using System.Reflection;

namespace DevToys.MacOS;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, EventArgs e)
    {
        Loaded -= OnLoaded;

        // Required on MacOS 13.3 and greater to get Web developer tools to work.
#if DEBUG && MACCATALYST13_3_OR_GREATER
        if (blazorWebView.Handler?.PlatformView is WebKit.WKWebView view)
        {
            view.SetValueForKey(Foundation.NSObject.FromObject(true), new Foundation.NSString("inspectable"));
        }
#endif
    }
}
