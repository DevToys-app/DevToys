#nullable enable

using System.Diagnostics;
using Windows.Foundation.Metadata;

namespace DevToys.MonacoEditor.Helpers
{
    [AllowForWeb]
    public sealed class DebugLogger
    {
        public void Log(string message)
        {
#if DEBUG
            Debug.WriteLine(message);
#endif
        }
    }
}
