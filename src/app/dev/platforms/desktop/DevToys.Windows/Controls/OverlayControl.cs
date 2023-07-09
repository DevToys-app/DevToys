using System.Windows.Controls;
using System.Windows.Interop;
using Windows.Win32.Foundation;

namespace DevToys.Windows.Controls;

internal sealed class OverlayControl : ContentControl, IDisposable
{
    private OverlayWindow? _overlayWindow;
    private HwndHostEx? _host;

    public OverlayControl()
    {
        Loaded += OverlayControl_Loaded;
    }

    internal Func<nint, int, nint, nint, (nint, bool)>? WndProc { get; set; }

    private void OverlayControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        object content = Content;

        _overlayWindow = new OverlayWindow
        {
            Content = content,
            WndProcHandler = WndProcHandler
        };
        _overlayWindow.Show();

        var windowHandle = new HWND(new WindowInteropHelper(_overlayWindow).Handle);

        _host = new HwndHostEx(windowHandle);
        Content = _host;
    }

    public void Dispose()
    {
        _host?.Dispose();
        _overlayWindow?.Close();
    }

    private (nint, bool) WndProcHandler(nint hwnd, int msg, nint wParam, nint lParam)
    {
        if (WndProc is not null)
        {
            return WndProc(hwnd, msg, wParam, lParam);
        }

        return (IntPtr.Zero, false);
    }
}
