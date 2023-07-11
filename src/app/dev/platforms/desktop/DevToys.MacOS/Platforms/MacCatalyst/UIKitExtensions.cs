using CoreGraphics;
using System.Runtime.InteropServices;
using Foundation;
using ObjCRuntime;
using UIKit;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices.Sensors;
using System.Drawing;

namespace DevToys.MacOS;

/// <summary>
/// UIKit Extensions
/// </summary>
internal static class UIKitExtensions
{
    /// <summary>
    /// NSWindowLevel.Status
    /// </summary>
    private const int NSWindowLevel_Status = 9;

    /// <summary>
    /// NSWindowLevel.Base
    /// </summary>
    private const int NSWindowLevel_Base = 0;

    private static readonly ulong CloseButton = 0;
    private static readonly ulong MiniaturizeButton = 1;
    private static readonly ulong ZoomButton = 2;

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern IntPtr IntPtr_objc_msgSend_nfloat(IntPtr receiver, IntPtr selector, NFloat arg1);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern IntPtr IntPtr_objc_msgSend_IntPtr(IntPtr receiver, IntPtr selector, IntPtr arg1);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern IntPtr IntPtr_objc_msgSend_UInt64(IntPtr receiver, IntPtr selector, ulong arg1);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern IntPtr IntPtr_objc_msgSend(IntPtr receiver, IntPtr selector);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern void void_objc_msgSend_IntPtr(IntPtr receiver, IntPtr selector, IntPtr arg1);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern void void_objc_msgSend_bool_IntPtr(IntPtr receiver, IntPtr selector, bool arg1, IntPtr arg2);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern void void_objc_msgSend_IntPtr_IntPtr(IntPtr receiver, IntPtr selector, IntPtr arg1, IntPtr arg2);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern void void_objc_msgSend_IntPtr_bool(IntPtr receiver, IntPtr selector, IntPtr arg1, bool arg2);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern void void_objc_msgSend_IntPtr_bool_bool(IntPtr receiver, IntPtr selector, IntPtr arg1, bool arg2, bool arg3);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern void void_objc_msgSend_bool(IntPtr receiver, IntPtr selector, bool arg1);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern void void_objc_msgSend_ulong(IntPtr receiver, IntPtr selector, ulong arg1);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern void void_objc_msgSend_Int64(IntPtr receiver, IntPtr selector, long arg1);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern void void_objc_msgSend_CGPoint(IntPtr receiver, IntPtr selector, CGPoint arg1);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern CGRect RectangleF_objc_msgSend_stret(IntPtr receiver, IntPtr selector);

    internal static bool IsZoomed(this UIWindow window)
    {
        var nsWindow = window.GetNSWindowFromUIWindow();
        if (nsWindow is null)
        {
            return false;
        }

        return IntPtr_objc_msgSend(nsWindow.Handle, Selector.GetHandle("isZoomed")) != IntPtr.Zero;
    }

    internal static void ForceUnzoom(this UIWindow window)
    {
        if (IsZoomed(window))
        {
            var nsWindow = window.GetNSWindowFromUIWindow();
            if (nsWindow is null)
            {
                return;
            }

            void_objc_msgSend_IntPtr(nsWindow.Handle, Selector.GetHandle("zoom:"), nsWindow.Handle);
        }
    }

    internal static void ForceZoom(this UIWindow window)
    {
        if (!IsZoomed(window))
        {
            var nsWindow = window.GetNSWindowFromUIWindow();
            if (nsWindow is null)
            {
                return;
            }

            void_objc_msgSend_IntPtr(nsWindow.Handle, Selector.GetHandle("zoom:"), nsWindow.Handle);
        }
    }

    internal static void MoveWindow(this UIWindow window, double x, double y)
    {
        var nsWindow = window.GetNSWindowFromUIWindow();
        if (nsWindow is null)
        {
            return;
        }

        void_objc_msgSend_CGPoint(nsWindow.Handle, Selector.GetHandle("setFrameTopLeftPoint:"), new CGPoint(x, y));
    }

    internal static CGPoint GetWindowPosition(this UIWindow window)
    {
        var nsWindow = window.GetNSWindowFromUIWindow();
        if (nsWindow is null)
        {
            return CGPoint.Empty;
        }

        return RectangleF_objc_msgSend_stret(nsWindow.Handle, Selector.GetHandle("frame")).Location;
    }

    internal static void ToggleWindowAlwaysOnTop(this UIWindow window, bool alwaysOnTop)
    {
        var nsWindow = window.GetNSWindowFromUIWindow();
        if (nsWindow is null)
        {
            return;
        }

        if (alwaysOnTop)
        {
            void_objc_msgSend_Int64(nsWindow.Handle, Selector.GetHandle("setLevel:"), NSWindowLevel_Status);
        }
        else
        {
            void_objc_msgSend_Int64(nsWindow.Handle, Selector.GetHandle("setLevel:"), NSWindowLevel_Base);
        }
    }

    internal static void ToggleTitleBarButtons(this UIWindow window, bool hideButtons)
    {
        var nsWindow = window.GetNSWindowFromUIWindow();
        if (nsWindow is null)
        {
            return;
        }

        var closeButton = Runtime.GetNSObject(IntPtr_objc_msgSend_UInt64(nsWindow.Handle, Selector.GetHandle("standardWindowButton:"), CloseButton));

        if (closeButton is null)
        {
            return;
        }

        var miniaturizeButton = Runtime.GetNSObject(IntPtr_objc_msgSend_UInt64(nsWindow.Handle, Selector.GetHandle("standardWindowButton:"), MiniaturizeButton));
        if (miniaturizeButton is null)
        {
            return;
        }

        var zoomButton = Runtime.GetNSObject(IntPtr_objc_msgSend_UInt64(nsWindow.Handle, Selector.GetHandle("standardWindowButton:"), ZoomButton));

        if (zoomButton is null)
        {
            return;
        }

        //void_objc_msgSend_bool(closeButton.Handle, Selector.GetHandle("setHidden:"), hideButtons);
        void_objc_msgSend_bool(miniaturizeButton.Handle, Selector.GetHandle("setHidden:"), hideButtons);
        void_objc_msgSend_bool(zoomButton.Handle, Selector.GetHandle("setHidden:"), hideButtons);
    }

    private static NSObject? GetNSWindowFromUIWindow(this UIWindow window)
    {
        if (window is null)
        {
            return null;
        }

        var sharedApplication = GetNSApplicationSharedApplication();

        var applicationDelegate = sharedApplication?.PerformSelector(new Selector("delegate"));
        if (applicationDelegate is null)
        {
            return null;
        }

        return GetNSWindow(window, applicationDelegate);
    }

    private static NSObject? GetNSWindow(UIWindow window, NSObject applicationDelegate)
    {
        var nsWindowHandle = IntPtr_objc_msgSend_IntPtr(applicationDelegate.Handle, Selector.GetHandle("hostWindowForUIWindow:"), window.Handle);
        var nsWindow = Runtime.GetNSObject<NSObject>(nsWindowHandle);
        if (nsWindow is null)
        {
            return GetNSWindow(window, applicationDelegate);
        }

        return nsWindow;
    }

    private static NSObject? GetNSApplicationSharedApplication()
    {
        var nsapp = Runtime.GetNSObject(Class.GetHandle("NSApplication"));
        if (nsapp is null)
        {
            return null;
        }

        return nsapp.PerformSelector(new Selector("sharedApplication"));
    }
}