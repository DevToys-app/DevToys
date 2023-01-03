using Microsoft.Extensions.Logging;
using Uno.Extensions;
using Windows.Foundation.Metadata;

namespace DevToys.MonacoEditor.WebInterop;

[AllowForWeb]
internal sealed partial class DebugLogger
{
    private readonly ILogger? _debugLogger;

    public DebugLogger()
    {
        ILogger logger = this.Log();
        _debugLogger = logger.IsEnabled(LogLevel.Debug) ? logger : null;
    }

    public void Log(string message)
    {
        _debugLogger?.LogDebug(message);
    }
}
