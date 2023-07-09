using System.Runtime.InteropServices;
using System.Windows.Interop;
using DevToys.Windows.Native;
using Windows.Win32.Foundation;

namespace DevToys.Windows.Controls;

internal sealed class HwndHostEx : HwndHost
{
    private readonly HWND _childHandle;

    public HwndHostEx(HWND handle)
    {
        _childHandle = handle;
    }

    protected override HandleRef BuildWindowCore(HandleRef windowHandleParent)
    {
        var handleRef = new HandleRef();

        if (_childHandle != IntPtr.Zero)
        {
            NativeMethods.SetWindowAsChildOf(_childHandle, new HWND(windowHandleParent.Handle));
            handleRef = new HandleRef(this, _childHandle);
        }

        return handleRef;
    }

    protected override void DestroyWindowCore(HandleRef hwnd)
    {
    }
}
