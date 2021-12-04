using System.Diagnostics;
using System.IO;

namespace DevToys.OutOfProcService.Core
{
    internal static class Constants
    {
        internal static readonly string AssetsFolderFullPath = Path.Combine(Path.GetDirectoryName(System.Environment.ProcessPath)!, "Assets");

        internal const string EfficientCompressionToolFileName = "ect-0.8.3.exe";
    }
}
