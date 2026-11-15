using System.IO;
using System.Threading.Tasks;
using DevToys.Core.Logging;
using Microsoft.Extensions.Logging;

namespace DevToys.UnitTests.Core.Logging;

public class FileLoggerProviderTests
{
    [Fact]
    public void Dispose_WhenLogFileCannotBeOpened_DoesNotThrow()
    {
        var provider = new FileLoggerProvider(CreateUnwritableFileStorage());
        provider.CreateLogger("Test").LogInformation("Entry");

        provider.Invoking(p => p.Dispose()).Should().NotThrow();
    }

    [Fact]
    public void Log_WhenLogFileCannotBeOpened_DoesNotBlock()
    {
        var provider = new FileLoggerProvider(CreateUnwritableFileStorage());
        ILogger logger = provider.CreateLogger("Test");

        // More entries than the provider's queue holds (1024).
        var logging = Task.Run(() =>
        {
            for (int i = 0; i < 2000; i++)
            {
                logger.LogInformation("Entry {Index}", i);
            }
        });

        logging.Wait(TimeSpan.FromSeconds(10)).Should().BeTrue();
        provider.Dispose();
    }

    private static IFileStorage CreateUnwritableFileStorage()
    {
        var fileStorage = new Mock<IFileStorage>();
        fileStorage
            .Setup(f => f.AppCacheDirectory)
            .Returns(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")));
        fileStorage
            .Setup(f => f.OpenWriteFile(It.IsAny<string>(), It.IsAny<bool>()))
            .Throws(new UnauthorizedAccessException("Access to the path is denied."));
        return fileStorage.Object;
    }
}
