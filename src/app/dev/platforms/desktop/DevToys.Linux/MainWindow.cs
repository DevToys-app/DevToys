using DevToys.Api;
using DevToys.Blazor.Core.Services;
using DevToys.Core;
using DevToys.Linux.Components;
using DevToys.Linux.Core;
using Microsoft.Extensions.DependencyInjection;

namespace DevToys.Linux;

internal class MainWindow
{
#pragma warning disable IDE0044 // Add readonly modifier
    [Import]
    private IThemeListener _themeListener = default!;

    [Import]
    private IFileStorage _fileStorage = default!;

    [Import]
    private TitleBarInfoProvider _titleBarInfoProvider = default!;
#pragma warning restore IDE0044 // Add readonly modifier

    private readonly BlazorWebView _blazorWebView;
    private readonly Gtk.Window _window;

    internal MainWindow(IServiceProvider serviceProvider, Adw.Application application)
    {
        serviceProvider.GetService<IMefProvider>()!.SatisfyImports(this);
        Guard.IsNotNull(_themeListener);
        Guard.IsNotNull(_titleBarInfoProvider);
        _titleBarInfoProvider.PropertyChanged += TitleBarInfoProvider_PropertyChanged;

        _blazorWebView = new BlazorWebView(serviceProvider);

        // Allow opening developer tools
        WebKit.Settings webViewSettings = _blazorWebView.GetSettings();
#if DEBUG
        webViewSettings.EnableDeveloperExtras = true;
#endif
        webViewSettings.JavascriptCanAccessClipboard = true;
        webViewSettings.EnableBackForwardNavigationGestures = false;
        _blazorWebView.SetSettings(webViewSettings);

        // Create and open main window.
        _window = Gtk.ApplicationWindow.New(application);
        _window.Title = _titleBarInfoProvider.Title ?? string.Empty;
        _window.SetDefaultSize(1280, 800);
        _window.SetChild(_blazorWebView);

        var windowService = (WindowService)serviceProvider.GetService<IWindowService>()!;
        ((ThemeListener)_themeListener).SetMainWindow(_window, windowService);
        windowService.SetMainWindow(_window);
        ((FileStorage)_fileStorage).MainWindow = _window;

        _window.Show();
    }

    private void TitleBarInfoProvider_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TitleBarInfoProvider.Title))
        {
            _window.Title = _titleBarInfoProvider.Title ?? string.Empty;
        }
    }
}
