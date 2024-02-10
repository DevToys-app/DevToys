using DevToys.Blazor;
using DevToys.Core;
using DevToys.MacOS.Controls.BlazorWebView;
using PredefinedSettings = DevToys.Core.Settings.PredefinedSettings;

namespace DevToys.MacOS.Views;

internal sealed class MainWindow : NSWindow
{
#if DEBUG
    private const bool EnableDeveloperTools = true;
#else
    private const bool EnableDeveloperTools = false;
#endif

    private readonly TitleBarInfoProvider _titleBarInfoProvider;
    private readonly ISettingsProvider _settingsProvider;
    private bool _isInitialized;

    internal static MainWindow Instance { get; } = new();

    private MainWindow()
        : base(
            new CGRect(0, 0, 1200, 600),
            NSWindowStyle.Closable | NSWindowStyle.Miniaturizable | NSWindowStyle.Resizable | NSWindowStyle.Titled | NSWindowStyle.FullSizeContentView,
            NSBackingStore.Buffered,
            false)
    {
        Guard.IsNotNull(AppDelegate.MefComposer);
        _settingsProvider = AppDelegate.MefComposer.Provider.Import<ISettingsProvider>();
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
        SetPositionAndSize();

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

    private void SetPositionAndSize()
    {
        // TODO : TEST this
        SixLabors.ImageSharp.Rectangle? bounds = _settingsProvider.GetSetting(PredefinedSettings.MainWindowBounds);

        if (bounds is null)
        {
            int width = (int)(Math.Max(screen.WorkingArea.Width - 400, 1200));
            int height = (int)(Math.Max(screen.WorkingArea.Height - 200, 600));

            // Center the window on the screen.
            bounds
                = new(
                    x: (int)(((screen.WorkingArea.Width / DPI_SCALE) - width) / 2),
                    y: (int)(((screen.WorkingArea.Height / DPI_SCALE) - height) / 2),
                    width,
                    height);
        }

        Left = bounds.Value.X;
        Top = bounds.Value.Y;
        Width = bounds.Value.Width;
        Height = bounds.Value.Height;

        if (_settingsProvider.GetSetting(PredefinedSettings.MainWindowMaximized))
        {
            WindowState = System.Windows.WindowState.Maximized;
        }
    }

    private void SavePositionAndSize()
    {
        var windowService = (WindowService)_serviceProvider.GetService<IWindowService>()!;
        if (!windowService.IsCompactOverlayMode)
        {
            _settingsProvider.SetSetting(
                PredefinedSettings.MainWindowBounds,
                new SixLabors.ImageSharp.Rectangle(
                    (int)Left,
                    (int)Top,
                    (int)Width,
                    (int)Height));

            _settingsProvider.SetSetting(PredefinedSettings.MainWindowMaximized, WindowState == System.Windows.WindowState.Maximized);
        }
    }

    private void TitleBarInfoProvider_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TitleBarInfoProvider.Title))
        {
            Title = _titleBarInfoProvider.Title ?? string.Empty;
        }
    }
}
