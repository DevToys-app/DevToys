using Microsoft.Extensions.Logging;

namespace DevToys.UnitTests.Mocks;
internal class MockILogger : ILogger
{
    public IDisposable BeginScope<TState>(TState state) where TState : notnull
    {
        return new MockIDisposable();
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
    }
}
