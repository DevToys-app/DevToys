using Microsoft.Extensions.Logging;

namespace DevToys.Api;

/// <summary>
/// Provides extension methods for logging.
/// </summary>
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
            Guard.IsNotNull(value);
            _loggerFactory = value;
        }
    }

    /// <summary>
    /// Creates an instance of <see cref="ILogger"/> for the specified type.
    /// </summary>
    /// <param name="forType">The type to create the logger for.</param>
    /// <returns>An instance of <see cref="ILogger"/>.</returns>
    public static ILogger Log(this Type forType)
        => LoggerFactory.CreateLogger(forType);

    /// <summary>
    /// Creates an instance of <see cref="ILogger"/> for the given object's type.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <param name="instance">The object instance.</param>
    /// <returns>An instance of <see cref="ILogger"/>.</returns>
    public static ILogger Log<T>(this T instance)
    {
        Guard.IsNotNull(instance);
        return instance.GetType().Log();
    }
}
