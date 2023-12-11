using DevToys.Blazor;
using DevToys.Core;
using DevToys.MacOS.Controls.BlazorWebView;

namespace DevToys.MacOS.Views;

internal sealed class MainWindow : NSWindow
{
#if DEBUG
    private const bool EnableDeveloperTools = true;
#else
    private const bool EnableDeveloperTools = false;
#endif

    private readonly TitleBarInfoProvider _titleBarInfoProvider;
    private bool _isInitialized;

    internal static MainWindow Instance { get; } = new();

    private MainWindow()
        : base(
            new CGRect(0, 0, 1280, 800),
            NSWindowStyle.Closable | NSWindowStyle.Miniaturizable | NSWindowStyle.Resizable | NSWindowStyle.Titled | NSWindowStyle.FullSizeContentView,
            NSBackingStore.Buffered,
            false)
    {
        Guard.IsNotNull(AppDelegate.MefComposer);
        _titleBarInfoProvider = AppDelegate.MefComposer.Provider.Import<TitleBarInfoProvider>();
        _titleBarInfoProvider.PropertyChanged += TitleBarInfoProvider_PropertyChanged;

        Title = _titleBarInfoProvider.Title ?? string.Empty;
    }

    internal void Show()
    {
        if (!_isInitialized)
        {
            _isInitialized = true;
            InitializeView();
        }

        // Center and show the window on screen.
        Center();
        MakeKeyAndOrderFront(this);

        // Make this window the main window of the app.
        MakeMainWindow();
    }

    private void InitializeView()
    {
        // Try to get the title bar view.
        NSView titleBar = StandardWindowButton(NSWindowButton.CloseButton).Superview;

        // Make the title bar transparent
        TitlebarAppearsTransparent = true;

        // Hide the title text and icon
        TitleVisibility = NSWindowTitleVisibility.Visible;

        // Create a NSVisualEffectView
        Guard.IsNotNull(ContentView);
        var visualEffectView = new NSVisualEffectView(ContentView.Bounds)
        {
            Material = NSVisualEffectMaterial.UnderWindowBackground,
            BlendingMode = NSVisualEffectBlendingMode.BehindWindow,
            State = NSVisualEffectState.FollowsWindowActiveState,
            AutoresizingMask = NSViewResizingMask.HeightSizable | NSViewResizingMask.WidthSizable,
            WantsLayer = true
        };

        // Set the NSVisualEffectView as the content of the NSWindow
        ContentView = visualEffectView;

        // Create WebView and add it to NSVisualEffectView
        Guard.IsNotNull(AppDelegate.ServiceProvider);
        var webView = new BlazorWkWebView(AppDelegate.ServiceProvider, EnableDeveloperTools);
        visualEffectView.AddSubview(webView.View);

        // Make the WKWebView resizing with the window and anchor it below the title bar, so we can still move the window with the title bar.
        webView.View.AutoresizingMask = NSViewResizingMask.HeightSizable | NSViewResizingMask.WidthSizable;
        webView.View.Frame
            = new CGRect(
                visualEffectView.Bounds.X,
                visualEffectView.Bounds.Y,
                visualEffectView.Bounds.Width,
                visualEffectView.Bounds.Height - titleBar.Bounds.Height);

        // Navigate to our Blazor webpage.
        webView.RootComponents.Add(new RootComponent { Selector = "#app", ComponentType = typeof(Main) });
        webView.HostPage = "wwwroot/index.html";
    }

    private void TitleBarInfoProvider_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TitleBarInfoProvider.Title))
        {
            Title = _titleBarInfoProvider.Title ?? string.Empty;
        }
    }
}
