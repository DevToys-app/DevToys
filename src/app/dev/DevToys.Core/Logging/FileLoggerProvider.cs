using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace DevToys.Core.Logging;

[ProviderAlias("File")]
public sealed class FileLoggerProvider : ILoggerProvider
{
    public const string LogFolderName = "Logs";

    private readonly ConcurrentDictionary<string, FileLogger> _loggers = new();
    private readonly BlockingCollection<string> _entryQueue = new(1024);
    private readonly Task _processQueueTask;
    private readonly IFileStorage _fileStorage;
    private readonly Lazy<TextWriter> _textWriter;
    private readonly Lazy<Stream> _fileStream;

    internal FileLoggerProvider(IFileStorage fileStorage)
    {
        Guard.IsNotNull(fileStorage);
        _fileStorage = fileStorage;

        // Delete older log files
        ClearOlderLogFiles();

        // Create log file for the current instance.
        string logFileName = $"log-{DateTime.Now.ToString("s").Replace(":", string.Empty).Replace("-", string.Empty)}.txt";
        _fileStream = new(() => _fileStorage.OpenWriteFile(Path.Combine(LogFolderName, logFileName), replaceIfExist: true));
        _textWriter = new(() => new StreamWriter(_fileStream.Value));

        _processQueueTask
            = Task.Factory.StartNew(
                ProcessQueue,
                this,
                TaskCreationOptions.LongRunning);
    }

    public ILogger CreateLogger(string categoryName)
    {
        return _loggers.GetOrAdd(categoryName, new FileLogger(categoryName, this));
    }

    public void Dispose()
    {
        _entryQueue.CompleteAdding();
        try
        {
            _processQueueTask.Wait(1500); // the same as in ConsoleLogger
        }
        catch (TaskCanceledException) { }
        catch (AggregateException ex) when (ex.InnerExceptions.Count == 1 && ex.InnerExceptions[0] is TaskCanceledException) { }

        _textWriter.Value.Dispose();
        _fileStream.Value.Dispose();
    }

    internal void WriteEntry(string message)
    {
        if (!_entryQueue.IsAddingCompleted)
        {
            try
            {
                _entryQueue.Add(message);
                return;
            }
            catch (InvalidOperationException) { }
        }

        // do nothing
    }

    private static void ProcessQueue(object? state)
    {
        Guard.IsNotNull(state);
        Guard.IsOfType<FileLoggerProvider>(state);
        var fileLogger = (FileLoggerProvider)state;
        fileLogger.ProcessQueue();
    }

    private void ProcessQueue()
    {
        TextWriter textWriter;
        try
        {
            textWriter = _textWriter.Value;
            foreach (string message in _entryQueue.GetConsumingEnumerable())
            {
                WriteMessage(textWriter, message, _entryQueue.Count == 0);
            }
        }
        catch
        {
#if DEBUG
            System.Diagnostics.Debugger.Launch();
#endif
        }
    }

    private static void WriteMessage(TextWriter textWriter, string message, bool flush)
    {
        textWriter.WriteLine(message);
        if (flush)
        {
            try
            {
                textWriter.Flush();
            }
            catch (NotSupportedException) { } // May not work in WASM.
        }
    }

    private void ClearOlderLogFiles()
    {
        try
        {
            string logFolder = Path.Combine(_fileStorage.AppCacheDirectory, LogFolderName);
            if (Directory.Exists(logFolder))
            {
                foreach (string logFilePath in Directory.EnumerateFiles(logFolder, "log-*.txt", SearchOption.TopDirectoryOnly))
                {
                    var fileInfo = new FileInfo(logFilePath);
                    if (DateTime.Now - fileInfo.LastWriteTime > TimeSpan.FromDays(2))
                    {
                        File.Delete(logFilePath);
                    }
                }
            }
        }
        catch
        {
            // Ignore.
        }
    }
}
