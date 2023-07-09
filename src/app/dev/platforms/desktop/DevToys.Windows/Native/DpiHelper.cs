using System.Windows.Media;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;

namespace DevToys.Windows.Native;

/// <summary>
/// An helper class to help with DPI.
/// </summary>
public static class DpiHelper
{
    private const double LogicalDpi = 96.0;

    static DpiHelper()
    {
#pragma warning disable CA1416 // Validate platform compatibility
        HDC dC = PInvoke.GetDC(HWND.Null);
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
    public static MatrixTransform TransformFromDevice { get; }

    /// <summary>
    /// The destination device.
    /// </summary>
    public static MatrixTransform TransformToDevice { get; }

    /// <summary>
    /// The device DPI X value.
    /// </summary>
    public static double DeviceDpiX { get; }

    /// <summary>
    /// The device DPI Y value.
    /// </summary>
    public static double DeviceDpiY { get; }

    /// <summary>
    /// Logical value to Unit Scaling X.
    /// </summary>
    public static double LogicalToDeviceUnitsScalingFactorX => TransformToDevice.Matrix.M11;

    /// <summary>
    /// Logical value to Unit Scaling Y.
    /// </summary>
    public static double LogicalToDeviceUnitsScalingFactorY => TransformToDevice.Matrix.M22;
}
