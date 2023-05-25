namespace DevToys.Windows.Native;

internal static class HwndSourceMessages
{
    internal const int
        WM_NCHITTEST = 0x0084,
        WM_NCLBUTTONDOWN = 0x00A1,
        WM_MAXIMIZE = 0x0024,
        WM_NCPAINT = 0x0085,
        WM_ERASEBKGND = 0x0014,
        WM_PAINT = 0x000F,
        WM_GETMINMAXINFO = 0x0024,
        WM_MOVING = 0x0216,
        WM_SETCURSOR = 0x20,
        WM_GETTEXT = 0xD,
        WM_WINDOWPOSCHANGING = 0x46,
        WM_SIZING = 0x0214,
        WS_CHILD = 0x40000000;
}
