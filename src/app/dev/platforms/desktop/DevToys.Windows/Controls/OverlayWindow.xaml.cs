using System.Windows;
using System.Windows.Interop;
using DevToys.Windows.Native;
using Windows.Win32.Foundation;

namespace DevToys.Windows.Controls;

public partial class OverlayWindow : Window
{
    public OverlayWindow()
    {
        InitializeComponent();
    }

    internal Func<nint, int, nint, nint, (nint, bool)>? WndProcHandler { get; set; }

    protected override void OnSourceInitialized(EventArgs e)
    {
        nint windowHandle = new WindowInteropHelper(this).Handle;
        var hwndSource = HwndSource.FromHwnd(windowHandle);
        hwndSource.AddHook(new HwndSourceHook(WndProc));

        // Hide window from Alt+Tab.
        NativeMethods.HideFromAltTab(new HWND(windowHandle));

        base.OnSourceInitialized(e);
    }

    private nint WndProc(nint hwnd, int msg, nint wParam, nint lParam, ref bool handled)
    {
        if (WndProcHandler is not null)
        {
            (nint result, bool h) = WndProcHandler(hwnd, msg, wParam, lParam);
            handled = h;
            return result;
        }

        return IntPtr.Zero;
    }
}
