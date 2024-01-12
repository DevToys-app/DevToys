using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace DevToys.UnitTests;

internal static class TestDataProvider
{
    /// <summary>
    /// Retrieves the content of a file.
    /// </summary>
    /// <param name="filePath">Path to the file relative to <c>DevToys.Tests.Providers.TestData</c></param>
    /// <returns></returns>
    public static async Task<string> GetEmbeddedFileContent(string filePath)
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            using Stream resourceStream = assembly.GetManifestResourceStream(filePath);
            using StreamReader streamReader = new(resourceStream);
            return await streamReader.ReadToEndAsync();
        }
        catch
        {
            throw new FileNotFoundException(filePath);
        }
    }

    /// <summary>
    /// Retrieves the content of a file.
    /// </summary>
    /// <param name="filePath">Path to the file relative to <c>DevToys.Tests.Providers.TestData</c></param>
    /// <returns></returns>
    public static FileInfo GetFile(string filePath)
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            string assemblyDirectory = Path.GetDirectoryName(assembly.Location);
            string resourcePath = Path.Combine(assemblyDirectory, filePath);
            return new(resourcePath);
        }
        catch
        {
            throw new FileNotFoundException(filePath);
        }
    }
}
