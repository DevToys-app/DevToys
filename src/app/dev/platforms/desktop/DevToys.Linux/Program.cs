using Microsoft.Extensions.Logging;

namespace DevToys.Linux;

internal partial class Program
{
    private static LinuxProgram? linuxProgram;

    private static int Main(string[] args)
    {
        try
        {
            linuxProgram = new LinuxProgram();
            linuxProgram.Application.Run();
        }
        catch (Exception ex)
        {
            LogUnhandledException(ex);
        }

        return 0;
    }

    private static void LogUnhandledException(Exception exception)
    {
        LinuxProgram.Logger?.LogCritical(0, exception, "Unhandled exception !!!    (╯°□°）╯︵ ┻━┻");
    }
}
