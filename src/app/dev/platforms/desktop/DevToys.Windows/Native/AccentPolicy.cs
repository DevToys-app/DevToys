using System.Runtime.InteropServices;

namespace DevToys.Windows.Native;

[StructLayout(LayoutKind.Sequential)]
internal struct AccentPolicy
{
    public AccentState AccentState;
    public AccentFlag AccentFlags;
    public uint GradientColor;
    public int AnimationId;
}
