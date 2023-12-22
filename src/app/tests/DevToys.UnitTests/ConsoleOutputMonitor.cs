using System.IO;

namespace DevToys.UnitTests;

internal sealed class ConsoleOutputMonitor : IDisposable
{
    private readonly StringWriter _outputWriter = new();
    private readonly StringWriter _errorWriter = new();
    private readonly TextWriter _originalOutput = Console.Out;
    private readonly TextWriter _originalError = Console.Error;

    internal ConsoleOutputMonitor()
    {
        Console.SetOut(_outputWriter);
        Console.SetError(_errorWriter);
    }

    internal string GetOutput()
    {
        string result = _outputWriter.ToString();
        _outputWriter.GetStringBuilder().Clear();
        return result;
    }

    internal string GetError()
    {
        string result = _errorWriter.ToString();
        _errorWriter.GetStringBuilder().Clear();
        return result;
    }

    public void Dispose()
    {
        Console.SetOut(_originalOutput);
        Console.SetError(_originalError);
        _outputWriter.Dispose();
        _errorWriter.Dispose();
    }
}
