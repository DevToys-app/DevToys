using System.Runtime.InteropServices;
using System.Security;

namespace DevToys.Windows.Native;

internal static partial class NativeMethods
{
    [SecurityCritical]
    [DllImport("ntdll.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [SuppressMessage("Interoperability", "SYSLIB1054:Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time", Justification = "Cannot marshal OSVERSIONINFOEX")]
    internal static extern int RtlGetVersion(out OSVERSIONINFOEX versionInfo);

    [LibraryImport("dwmapi.dll")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static partial int DwmSetWindowAttribute(nint hwnd, DWMWINDOWATTRIBUTE dwAttribute, ref int pvAttribute, int cbAttribute);

    [LibraryImport("dwmapi.dll")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static partial int DwmExtendFrameIntoClientArea(nint hwnd, ref MARGINS pMarInset);

    [LibraryImport("user32.dll", EntryPoint = "SetWindowLongA")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static partial int SetWindowLong(nint hWnd, int nIndex, int dwNewLong);

    [LibraryImport("user32.dll", EntryPoint = "GetWindowLongPtrA")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static partial int GetWindowLong(nint hWnd, int nIndex);

    [LibraryImport("user32.dll")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static partial nint SetParent(nint hWndChild, nint hWndNewParent);

    [LibraryImport("user32.dll")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    internal static partial nint GetDC(nint ptr);

    [LibraryImport(libraryName: "gdi32.dll", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    internal static partial int GetDeviceCaps(nint hdc, int nIndex);

    [LibraryImport("user32.dll", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    internal static partial int ReleaseDC(nint window, nint dc);

    internal static int SetWindowAttribute(nint windowHandle, DWMWINDOWATTRIBUTE attribute, int parameter)
    {
        return DwmSetWindowAttribute(windowHandle, attribute, ref parameter, Marshal.SizeOf<int>());
    }

    internal static int ExtendFrame(nint windowHandle, MARGINS margins)
    {
        return DwmExtendFrameIntoClientArea(windowHandle, ref margins);
    }

    internal static int RoundWindowCorner(nint windowHandle, DWM_WINDOW_CORNER_PREFERENCE cornerPreference = DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_ROUND)
    {
        DWMWINDOWATTRIBUTE attribute = DWMWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE;
        int preference = (int)cornerPreference;
        return DwmSetWindowAttribute(windowHandle, attribute, ref preference, sizeof(uint));
    }

    internal static void HideAllWindowButton(nint windowHandle)
    {
        _ = SetWindowLong(windowHandle, HwndButtons.GWL_STYLE, GetWindowLong(windowHandle, HwndButtons.GWL_STYLE) & ~HwndButtons.WS_SYSMENU);
    }

    internal static void SetWindowAsChildOf(nint windowHandleChild, nint windowHandleParent)
    {
        SetWindowLong(windowHandleChild, HwndButtons.GWL_STYLE, HwndSourceMessages.WS_CHILD);
        SetParent(windowHandleChild, windowHandleParent);
    }
}
