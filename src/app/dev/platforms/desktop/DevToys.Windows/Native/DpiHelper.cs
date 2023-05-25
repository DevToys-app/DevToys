using System.Windows.Media;

namespace DevToys.Windows.Native;

/// <summary>
/// An helper class to help with DPI.
/// </summary>
public static class DpiHelper
{
    private const double LogicalDpi = 96.0;

    static DpiHelper()
    {
        nint dC = NativeMethods.GetDC(nint.Zero);
        if (dC != nint.Zero)
        {
            const int logicPixelsX = 88;
            const int logicPixelsY = 90;
            _ = DeviceDpiX = NativeMethods.GetDeviceCaps(dC, logicPixelsX);
            _ = DeviceDpiY = NativeMethods.GetDeviceCaps(dC, logicPixelsY);
            _ = NativeMethods.ReleaseDC(nint.Zero, dC);
        }
        else
        {
            DeviceDpiX = LogicalDpi;
            DeviceDpiY = LogicalDpi;
        }

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
