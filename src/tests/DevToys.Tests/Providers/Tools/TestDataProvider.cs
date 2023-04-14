using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace DevToys.Tests.Providers.Tools
{
    public static class TestDataProvider
    {
        /// <summary>
        /// Retrieves the content of a file.
        /// </summary>
        /// <param name="filePath">Path to the file relative to <c>DevToys.Tests.Providers.TestData</c></param>
        /// <returns></returns>
        public static async Task<string> GetFileContent(string filePath)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Stream resourceStream = assembly.GetManifestResourceStream("DevToys.Tests.Providers.TestData." + filePath);
            StreamReader streamReader = new StreamReader(resourceStream);

            return await streamReader.ReadToEndAsync();
        }
    }
}
