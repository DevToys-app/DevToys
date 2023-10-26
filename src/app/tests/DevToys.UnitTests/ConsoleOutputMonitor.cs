using System.IO;

namespace DevToys.UnitTests;

internal sealed class ConsoleOutputMonitor : IDisposable
{
    private readonly StringWriter _stringWriter = new();
    private readonly TextWriter _originalOutput = Console.Out;

    internal ConsoleOutputMonitor()
    {
        Console.SetOut(_stringWriter);
    }

    internal string GetOutput()
    {
        string result = _stringWriter.ToString();
        _stringWriter.GetStringBuilder().Clear();
        return result;
    }

    public void Dispose()
    {
        Console.SetOut(_originalOutput);
        _stringWriter.Dispose();
    }
}
