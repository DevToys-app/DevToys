using Microsoft.Extensions.Logging;

namespace DevToys.Api;

public static class LoggingExtensions
{
    private static ILoggerFactory _loggerFactory = default!;

    /// <summary>
    /// Sets the <see cref="ILoggerFactory"/>.
    /// </summary>
    public static ILoggerFactory LoggerFactory
    {
        internal get => _loggerFactory;
        set
        {
            Guard.IsNull(_loggerFactory);
            Guard.IsNotNull(value);
            _loggerFactory = value;
        }
    }

    /// <summary>
    /// Creates an instance of <see cref="ILogger"/> for the specified type.
    /// </summary>
    public static ILogger Log(this Type forType)
        => LoggerFactory.CreateLogger(forType);

    /// <summary>
    /// Creates an instance of <see cref="ILogger"/> for the given object's type.
    /// </summary>
    public static ILogger Log<T>(this T instance)
    {
        Guard.IsNotNull(instance);
        return instance.GetType().Log();
    }
}
