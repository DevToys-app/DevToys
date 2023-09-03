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
    public static async Task<string> GetFileContent(string filePath)
    {
        var assembly = Assembly.GetExecutingAssembly();
        Stream resourceStream = assembly.GetManifestResourceStream(filePath);
        //Stream resourceStream = assembly.GetManifestResourceStream("DevToys.Tests.Providers.TestData." + filePath);
        var streamReader = new StreamReader(resourceStream);

        return await streamReader.ReadToEndAsync();
    }

    /// <summary>
    /// Retrieves the content of a file.
    /// </summary>
    /// <param name="filePath">Path to the file relative to <c>DevToys.Tests.Providers.TestData</c></param>
    /// <returns></returns>
    public static FileInfo GetFile(string filePath)
    {
        var assembly = Assembly.GetExecutingAssembly();
        string assemblyDirectory = Path.GetDirectoryName(assembly.Location);
        string resourcePath = Path.Combine(assemblyDirectory, filePath);
        return new(resourcePath);
    }
}
