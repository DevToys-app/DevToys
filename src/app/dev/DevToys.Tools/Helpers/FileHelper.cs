namespace DevToys.Tools.Helpers;
internal static class FileHelper
{
    /// <summary>
    /// Writes the output to the output file path if provided, otherwise to the console
    /// </summary>
    internal static async Task WriteOutputAsync(string? output, FileInfo? outputFile, CancellationToken cancellationToken)
    {
        if (outputFile is null)
        {
            Console.WriteLine(output);
        }
        else
        {
            await File.WriteAllTextAsync(outputFile.FullName, output, cancellationToken);
        }
    }
}
