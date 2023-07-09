using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Dwm;
using Windows.Win32.UI.Controls;
using Windows.Win32.UI.WindowsAndMessaging;

namespace DevToys.Windows.Native;

internal static partial class NativeMethods
{
    internal static unsafe HRESULT SetWindowAttribute(HWND windowHandle, DWMWINDOWATTRIBUTE attribute, ref int parameter)
    {
#pragma warning disable CA1416 // Validate platform compatibility
        fixed (void* value = &parameter)
        {
            return PInvoke.DwmSetWindowAttribute(windowHandle, attribute, value, (uint)Marshal.SizeOf<int>());
        }
#pragma warning restore CA1416 // Validate platform compatibility
    }

    internal static int ExtendFrame(HWND windowHandle, MARGINS margins)
    {
#pragma warning disable CA1416 // Validate platform compatibility
        return PInvoke.DwmExtendFrameIntoClientArea(windowHandle, in margins);
#pragma warning restore CA1416 // Validate platform compatibility
    }

    internal static void HideAllWindowButton(HWND windowHandle)
    {
#pragma warning disable CA1416 // Validate platform compatibility
        _ = PInvoke.SetWindowLong(windowHandle, WINDOW_LONG_PTR_INDEX.GWL_STYLE, PInvoke.GetWindowLong(windowHandle, WINDOW_LONG_PTR_INDEX.GWL_STYLE) & ~HwndButtons.WS_SYSMENU);
#pragma warning restore CA1416 // Validate platform compatibility
    }

    internal static void SetWindowAsChildOf(HWND windowHandleChild, HWND windowHandleParent)
    {
#pragma warning disable CA1416 // Validate platform compatibility
        PInvoke.SetWindowLong(windowHandleChild, WINDOW_LONG_PTR_INDEX.GWL_STYLE, HwndSourceMessages.WS_CHILD);
        PInvoke.SetParent(windowHandleChild, windowHandleParent);
#pragma warning restore CA1416 // Validate platform compatibility
    }
}
