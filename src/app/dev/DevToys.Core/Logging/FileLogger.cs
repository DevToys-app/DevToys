using System.Text;
using Microsoft.Extensions.Logging;

namespace DevToys.Core.Logging;

internal sealed class FileLogger : ILogger
{
    private readonly string _logName;
    private readonly FileLoggerProvider _fileLoggerProvider;

    internal FileLogger(string logName, FileLoggerProvider fileLoggerProvider)
    {
        Guard.IsNotNull(logName);
        Guard.IsNotNull(fileLoggerProvider);
        _logName = logName;
        _fileLoggerProvider = fileLoggerProvider;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return null;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (formatter is null)
        {
            ThrowHelper.ThrowArgumentNullException(nameof(formatter));
        }

        string message = formatter(state, exception);

        var logBuilder = new StringBuilder();
        if (!string.IsNullOrEmpty(message))
        {
            DateTime timeStamp = DateTime.Now;
            logBuilder.Append(timeStamp.ToString("o"));
            logBuilder.Append('\t');
            logBuilder.Append(logLevel.ToString());
            logBuilder.Append("\t[");
            logBuilder.Append(_logName);
            logBuilder.Append("]");
            logBuilder.Append("\t[");
            logBuilder.Append(eventId);
            logBuilder.Append("]\t");
            logBuilder.Append(message);
        }

        if (exception != null)
        {
            // exception message
            logBuilder.AppendLine();
            logBuilder.AppendLine(exception.Message);
            logBuilder.AppendLine(exception.ToString());
        }

        _fileLoggerProvider.WriteEntry(logBuilder.ToString());
    }
}
