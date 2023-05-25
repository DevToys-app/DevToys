using System.Runtime.InteropServices;

namespace DevToys.Windows.Native;

[StructLayout(LayoutKind.Sequential)]
internal struct OSVERSIONINFOEX
{
    internal int OSVersionInfoSize;
    internal int MajorVersion;
    internal int MinorVersion;
    internal int BuildNumber;
    internal int Revision;
    internal int PlatformId;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
    internal string CSDVersion;
    internal ushort ServicePackMajor;
    internal ushort ServicePackMinor;
    internal short SuiteMask;
    internal byte ProductType;
    internal byte Reserved;
}
