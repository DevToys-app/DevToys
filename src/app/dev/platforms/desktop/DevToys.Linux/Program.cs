namespace DevToys.Linux;

internal partial class Program
{
    private static LinuxProgram? linuxProgram;

    private static int Main(string[] args)
    {
        linuxProgram = new LinuxProgram();
        return linuxProgram.Application.Run(0, args);
    }
}
