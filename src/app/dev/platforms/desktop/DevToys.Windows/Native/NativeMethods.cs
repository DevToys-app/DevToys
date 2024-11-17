using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Dwm;
using Windows.Win32.UI.Controls;
using Windows.Win32.UI.WindowsAndMessaging;

namespace DevToys.Windows.Native;

internal static partial class NativeMethods
{
#pragma warning disable CA1416 // Validate platform compatibility
    internal static unsafe HRESULT SetWindowAttribute(HWND windowHandle, DWMWINDOWATTRIBUTE attribute, ref int parameter)
    {
        fixed (void* value = &parameter)
        {
            return PInvoke.DwmSetWindowAttribute(windowHandle, attribute, value, (uint)Marshal.SizeOf<int>());
        }
    }

    internal static int ExtendFrame(HWND windowHandle, MARGINS margins)
    {
        return PInvoke.DwmExtendFrameIntoClientArea(windowHandle, in margins);
    }

    internal static void HideAllWindowButton(HWND windowHandle)
    {
        _ = PInvoke.SetWindowLong(windowHandle, WINDOW_LONG_PTR_INDEX.GWL_STYLE, PInvoke.GetWindowLong(windowHandle, WINDOW_LONG_PTR_INDEX.GWL_STYLE) & ~HwndButtons.WS_SYSMENU);
    }

    internal static void SetWindowAsChildOf(HWND windowHandleChild, HWND windowHandleParent)
    {
        PInvoke.SetWindowLong(windowHandleChild, WINDOW_LONG_PTR_INDEX.GWL_STYLE, HwndSourceMessages.WS_CHILD);
        PInvoke.SetParent(windowHandleChild, windowHandleParent);
    }

    internal static void DisableMinimizeAndMaximizeCapabilities(HWND windowHandle)
    {
        HMENU systemMenuHandle = PInvoke.GetSystemMenu(windowHandle, false);
        PInvoke.EnableMenuItem(systemMenuHandle, (uint)PInvoke.SC_MAXIMIZE, MENU_ITEM_FLAGS.MF_BYCOMMAND | MENU_ITEM_FLAGS.MF_GRAYED);
        PInvoke.EnableMenuItem(systemMenuHandle, (uint)PInvoke.SC_MINIMIZE, MENU_ITEM_FLAGS.MF_BYCOMMAND | MENU_ITEM_FLAGS.MF_GRAYED);
    }

    internal static void EnableMinimizeAndMaximizeCapabilities(HWND windowHandle)
    {
        HMENU systemMenuHandle = PInvoke.GetSystemMenu(windowHandle, false);
        PInvoke.EnableMenuItem(systemMenuHandle, (uint)PInvoke.SC_MAXIMIZE, MENU_ITEM_FLAGS.MF_BYCOMMAND | MENU_ITEM_FLAGS.MF_ENABLED);
        PInvoke.EnableMenuItem(systemMenuHandle, (uint)PInvoke.SC_MINIMIZE, MENU_ITEM_FLAGS.MF_BYCOMMAND | MENU_ITEM_FLAGS.MF_ENABLED);
    }

    internal static void HideFromAltTab(HWND windowHandle)
    {
        int currentStyle = PInvoke.GetWindowLong(windowHandle, WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE);
        PInvoke.SetWindowLong(windowHandle, WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE, currentStyle | HwndSourceMessages.WS_EX_TOOLWINDOW);
    }

#pragma warning restore CA1416 // Validate platform compatibility
    /// <summary>
    /// Sets various information regarding DWM window attributes
    /// </summary>
    /// <param name="hwnd">The window handle whose information is to be changed</param>
    /// <param name="data">Pointer to a structure which both specifies and delivers the attribute data</param>
    /// <returns>Nonzero on success, zero otherwise.</returns>
    [DllImport("user32.dll")]
    internal static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

}
