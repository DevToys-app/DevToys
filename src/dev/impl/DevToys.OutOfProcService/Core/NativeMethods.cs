using System;
using System.Runtime.InteropServices;

namespace DevToys.OutOfProcService.Core
{
    internal static class NativeMethods
    {
        [DllImport("userenv.dll", SetLastError = false, CharSet = CharSet.Unicode)]
        internal static extern int DeriveAppContainerSidFromAppContainerName(string appContainerName, out IntPtr sid);

        [DllImport("advapi32.dll", SetLastError = false)]
        internal static extern IntPtr FreeSid(IntPtr sid);
    }
}
