using DevTools.Providers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace DevTools.Tests.Providers.Tools
{
    [TestClass]
    public class Base64EncoderDecoderToolProviderTests : MefBaseTest
    {
        [DataTestMethod]
        [DataRow(null, false)]
        [DataRow("", false)]
        [DataRow(" ", false)]
        [DataRow("aGVsbG8gd29ybGQ=", true)]
        [DataRow("aGVsbG8gd2f9ybGQ=", false)]
        [DataRow("SGVsbG8gV29y", true)]
        [DataRow("SGVsbG8gVa29y", false)]
        public void CanBeTreatedByTool(string input, bool expectedResult)
        {
            var result = ExportProvider.Import<IToolProviderFactory>().GetTools(string.Empty);
            var base64Tool = result.First(item => item.Metadata.ProtocolName == "base64");

            Assert.AreEqual(expectedResult, base64Tool.ToolProvider.CanBeTreatedByTool(input));
        }
    }
}
