using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DevToys.Core.Logging;

public static class LoggingBuilderExtensions
{
    /// <summary>
    /// Adds a file logger.
    /// </summary>
    public static ILoggingBuilder AddFile(this ILoggingBuilder builder, IFileStorage fileStorage)
    {
        builder.Services.Add(ServiceDescriptor.Singleton<ILoggerProvider, FileLoggerProvider>(
            (srvPrv) =>
            {
                return new FileLoggerProvider(fileStorage);
            }
        ));
        return builder;
    }
}
