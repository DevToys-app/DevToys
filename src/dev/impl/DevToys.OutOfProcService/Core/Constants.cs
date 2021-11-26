using System.Diagnostics;
using System.IO;

namespace DevToys.OutOfProcService.Core
{
    internal static class Constants
    {
        internal static readonly string AssetsFolderFullPath = Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule!.FileName)!, "Assets");

        internal const string EfficientCompressionToolFileName = "ect-0.8.3.exe";
    }
}
