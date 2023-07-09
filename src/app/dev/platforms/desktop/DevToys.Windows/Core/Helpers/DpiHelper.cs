using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;

namespace DevToys.Windows.Core.Helpers;

/// <summary>
/// An helper class to help with DPI.
/// </summary>
internal sealed class DpiHelper
{
    private const double LogicalDpi = 96.0;

    internal DpiHelper(Window window)
    {
#pragma warning disable CA1416 // Validate platform compatibility
        var windowInteropHelper = new WindowInteropHelper(window);
        HDC dC = PInvoke.GetDC(new HWND(windowInteropHelper.Handle));
        if (dC != nint.Zero)
        {
            _ = DeviceDpiX = PInvoke.GetDeviceCaps(dC, GET_DEVICE_CAPS_INDEX.LOGPIXELSX);
            _ = DeviceDpiY = PInvoke.GetDeviceCaps(dC, GET_DEVICE_CAPS_INDEX.LOGPIXELSY);
            _ = PInvoke.ReleaseDC(HWND.Null, dC);
        }
        else
        {
            DeviceDpiX = LogicalDpi;
            DeviceDpiY = LogicalDpi;
        }
#pragma warning restore CA1416 // Validate platform compatibility

        Matrix identity = Matrix.Identity;
        Matrix identity2 = Matrix.Identity;
        identity.Scale(DeviceDpiX / LogicalDpi, DeviceDpiY / LogicalDpi);
        identity2.Scale(LogicalDpi / DeviceDpiX, LogicalDpi / DeviceDpiY);
        TransformFromDevice = new MatrixTransform(identity2);
        TransformFromDevice.Freeze();
        TransformToDevice = new MatrixTransform(identity);
        TransformToDevice.Freeze();
    }

    /// <summary>
    /// The current device.
    /// </summary>
    internal MatrixTransform TransformFromDevice { get; }

    /// <summary>
    /// The destination device.
    /// </summary>
    internal MatrixTransform TransformToDevice { get; }

    /// <summary>
    /// The device DPI X value.
    /// </summary>
    internal double DeviceDpiX { get; }

    /// <summary>
    /// The device DPI Y value.
    /// </summary>
    internal double DeviceDpiY { get; }

    /// <summary>
    /// Logical value to Unit Scaling X.
    /// </summary>
    internal double LogicalToDeviceUnitsScalingFactorX => TransformToDevice.Matrix.M11;

    /// <summary>
    /// Logical value to Unit Scaling Y.
    /// </summary>
    internal double LogicalToDeviceUnitsScalingFactorY => TransformToDevice.Matrix.M22;
}
