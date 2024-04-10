using DevToys.Api;
using DevToys.Blazor;
using DevToys.Blazor.Core.Services;
using DevToys.Business.Services;
using DevToys.Core;
using DevToys.Core.Tools;
using DevToys.MacOS.Controls.BlazorWebView;
using DevToys.MacOS.Core;
using DevToys.MacOS.Core.Helpers;
using Microsoft.Extensions.DependencyInjection;
using SixLabors.ImageSharp;
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
    private readonly CommandLineLauncherService _commandLineLauncherService;
    private bool _isInitialized;

    internal static MainWindow Instance { get; } = new();

    private MainWindow()
        : base(
            new CGRect(0, 0, 1200, 600),
            NSWindowStyle.Closable | NSWindowStyle.Miniaturizable | NSWindowStyle.Resizable | NSWindowStyle.Titled |
            NSWindowStyle.FullSizeContentView,
            NSBackingStore.Buffered,
            false)
    {
        Guard.IsNotNull(AppDelegate.MefComposer);
        _settingsProvider = AppDelegate.MefComposer.Provider.Import<ISettingsProvider>();
        _commandLineLauncherService = AppDelegate.MefComposer.Provider.Import<CommandLineLauncherService>();
        _titleBarInfoProvider = AppDelegate.MefComposer.Provider.Import<TitleBarInfoProvider>();
        _titleBarInfoProvider.PropertyChanged += TitleBarInfoProvider_PropertyChanged;

        Title = _titleBarInfoProvider.TitleWithToolName ?? string.Empty;

        WillClose += OnWillClose;
    }

    private void OnWillClose(object? sender, EventArgs e)
    {
        SavePositionAndSize();

        Guard.IsNotNull(AppDelegate.MefComposer);

        // Dispose every disposable tool instance.
        AppDelegate.MefComposer.Provider.Import<GuiToolProvider>().DisposeTools();

        // Clear older temp files.
        FileHelper.ClearTempFiles(Constants.AppTempFolder);
    }

    internal async Task ShowAsync()
    {
        if (!_isInitialized)
        {
            _isInitialized = true;
            InitializeView();
        }

        // Center and show the window on screen.
        await SetPositionAndSizeAsync();
        MakeKeyAndOrderFront(this);

        // Make this window the main window of the app.
        MakeMainWindow();
        OrderFront(null);
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
        webView.BlazorWebViewInitialized += OnBlazorWebViewInitialized;
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

    private async ValueTask SetPositionAndSizeAsync()
    {
        Rectangle? bounds = _settingsProvider.GetSetting(PredefinedSettings.MainWindowBounds);
        bool shouldCenterWindow = bounds is null;

        if (shouldCenterWindow)
        {
            await ThreadHelper.RunOnUIThreadAsync(() =>
            {
                int width = (int)Math.Max(Screen.Frame.Width - 400, 1200);
                int height = (int)Math.Max(Screen.Frame.Height - 200, 600);
                bounds
                    = new Rectangle(
                        x: 0,
                        y: 0,
                        width,
                        height);
            });
        }

        Guard.IsNotNull(bounds);
        CGRect newFrame = Frame;
        newFrame.Width = bounds.Value.Width;
        newFrame.Height = bounds.Value.Height;
        newFrame.X = bounds.Value.X;
        newFrame.Y = bounds.Value.Y;
        SetFrame(newFrame, display: true);

        if (shouldCenterWindow)
        {
            // Center the window on the screen.
            Center();
        }

        IsZoomed = _settingsProvider.GetSetting(PredefinedSettings.MainWindowMaximized);
    }

    private void SavePositionAndSize()
    {
        Guard.IsNotNull(AppDelegate.ServiceProvider);
        var windowService = (WindowService)AppDelegate.ServiceProvider.GetService<IWindowService>()!;
        if (!windowService.IsCompactOverlayMode)
        {
            _settingsProvider.SetSetting(
                PredefinedSettings.MainWindowBounds,
                new Rectangle(
                    (int)Frame.Left,
                    (int)Frame.Top,
                    (int)Frame.Width,
                    (int)Frame.Height));

            _settingsProvider.SetSetting(PredefinedSettings.MainWindowMaximized, IsZoomed);
        }
    }

    private void OnBlazorWebViewInitialized(object? sender, EventArgs e)
    {
        InitializeLowPriorityServices();
    }

    private void TitleBarInfoProvider_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TitleBarInfoProvider.TitleWithToolName))
        {
            Title = _titleBarInfoProvider.TitleWithToolName ?? string.Empty;
        }
    }

    private void InitializeLowPriorityServices()
    {
        // Treat command line arguments.
        _commandLineLauncherService.HandleCommandLineArguments();
    }
}
