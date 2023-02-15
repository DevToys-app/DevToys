using Microsoft.Extensions.Logging;
using Uno.Extensions;
using Windows.Foundation.Metadata;

namespace DevToys.MonacoEditor.WebInterop;

[AllowForWeb]
internal sealed partial class DebugLogger
{
    private readonly ILogger? _logger;

    public DebugLogger()
    {
        _logger = this.Log();
    }

    public void Log(string message)
    {
        LogMessage(message);
    }

    [LoggerMessage(0, LogLevel.Information, "{message}")]
    partial void LogMessage(string message);
}
