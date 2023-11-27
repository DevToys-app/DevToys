namespace DevToys.Linux;

internal partial class Program
{
    private static LinuxProgram? linuxProgram;

    private static int Main(string[] args)
    {
        linuxProgram = new LinuxProgram();
        linuxProgram.Application.Run();

        return 0;
    }
}
