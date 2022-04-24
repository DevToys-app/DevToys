using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace DevToys.Tests.Providers.Tools
{
    public static class TestDataProvider
    {
        public static async Task<string> GetFileContent(string filename)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Stream resourceStream = assembly.GetManifestResourceStream("DevToys.Tests.Providers.TestData." + filename);
            StreamReader streamReader = new StreamReader(resourceStream);

            return await streamReader.ReadToEndAsync();
        }
    }
}
