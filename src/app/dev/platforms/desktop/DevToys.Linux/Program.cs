namespace DevToys.Linux;

internal partial class Program
{
    private static LinuxProgram? linuxProgram;

    private static int Main(string[] args)
    {
        linuxProgram = new LinuxProgram();
        linuxProgram.Application.Run(0, null);

        return 0;
    }
}
