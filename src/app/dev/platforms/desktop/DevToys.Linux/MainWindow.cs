using DevToys.Api;
using DevToys.Blazor;
using DevToys.Blazor.Core.Services;
using DevToys.Business.Services;
using DevToys.Core;
using DevToys.Linux.Components;
using DevToys.Linux.Core;
using Gtk;
using Microsoft.Extensions.DependencyInjection;
using WebKit;
using Settings = WebKit.Settings;

namespace DevToys.Linux;

internal class MainWindow
{
#if DEBUG
    private const bool EnableDeveloperTools = true;
#else
    private const bool EnableDeveloperTools = false;
#endif

#pragma warning disable IDE0044 // Add readonly modifier
    [Import]
    private IThemeListener _themeListener = default!;

    [Import]
    private IFileStorage _fileStorage = default!;

    [Import]
    private IFontProvider _fontProvider = default!;

    [Import]
    private TitleBarInfoProvider _titleBarInfoProvider = default!;

    [Import]
    private CommandLineLauncherService _commandLineLauncherService = default!;
#pragma warning restore IDE0044 // Add readonly modifier

    private readonly BlazorWebView _blazorGtkWebView;
    private readonly Gtk.Window _window;

    internal MainWindow(IServiceProvider serviceProvider, Adw.Application application)
    {
        serviceProvider.GetService<IMefProvider>()!.SatisfyImports(this);
        Guard.IsNotNull(_themeListener);
        Guard.IsNotNull(_titleBarInfoProvider);
        _titleBarInfoProvider.PropertyChanged += TitleBarInfoProvider_PropertyChanged;

        _blazorGtkWebView = new BlazorWebView(serviceProvider, EnableDeveloperTools);
        _blazorGtkWebView.BlazorWebViewInitialized += OnBlazorWebViewInitialized;

        // Create and open main window.
        _window = Gtk.ApplicationWindow.New(application);
        _window.Title = _titleBarInfoProvider.TitleWithToolName ?? string.Empty;
        _window.SetDefaultSize(1280, 800);
        _window.SetChild(_blazorGtkWebView.View);

        var windowService = (WindowService)serviceProvider.GetService<IWindowService>()!;
        ((ThemeListener)_themeListener).SetMainWindow(_window, windowService);
        windowService.SetMainWindow(_window);
        ((FileStorage)_fileStorage).MainWindow = _window;
        ((FontProvider)_fontProvider).MainWindow = _window;

        // Navigate to our Blazor webpage.
        _blazorGtkWebView.RootComponents.Add(new RootComponent { Selector = "#app", ComponentType = typeof(Main) });
        _blazorGtkWebView.HostPage = "wwwroot/index.html";

        _window.Show();
    }

    private void OnBlazorWebViewInitialized(object? sender, EventArgs args)
    {
        InitializeLowPriorityServices();
    }

    private void TitleBarInfoProvider_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TitleBarInfoProvider.TitleWithToolName))
        {
            _window.Title = _titleBarInfoProvider.TitleWithToolName ?? string.Empty;
        }
    }

    private void InitializeLowPriorityServices()
    {
        // Treat command line arguments.
        _commandLineLauncherService.HandleCommandLineArguments();
    }
}
