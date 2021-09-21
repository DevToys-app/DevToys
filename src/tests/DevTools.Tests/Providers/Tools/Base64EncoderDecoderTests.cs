using DevTools.Core.Threading;
using DevTools.Providers;
using DevTools.Providers.Impl.Tools.Base64EncoderDecoder;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;

namespace DevTools.Tests.Providers.Tools
{
    [TestClass]
    public class Base64EncoderDecoderTests : MefBaseTest
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

        [DataTestMethod]
        [DataRow(null, "")]
        [DataRow("", "")]
        [DataRow(" ", "")]
        [DataRow("Hello There", "SGVsbG8gVGhlcmU=")]
        public async Task EncoderAsync(string input, string expectedResult)
        {
            var viewModel = ExportProvider.Import<Base64EncoderDecoderToolViewModel>();

            await ExportProvider.Import<IThread>().RunOnUIThreadAsync(() =>
            {
                viewModel.ConversionMode = "Encode";
                viewModel.InputValue = input;
            });

            await Task.Delay(100);

            Assert.AreEqual(expectedResult, viewModel.OutputValue);
        }

        [DataTestMethod]
        [DataRow(null, "")]
        [DataRow("", "")]
        [DataRow(" ", "")]
        [DataRow("SGVsbG8gVGhlcmU=", "Hello There")]
        public async Task DecodeAsync(string input, string expectedResult)
        {
            var viewModel = ExportProvider.Import<Base64EncoderDecoderToolViewModel>();

            await ExportProvider.Import<IThread>().RunOnUIThreadAsync(() =>
            {
                viewModel.ConversionMode = "Decode";
                viewModel.InputValue = input;
            });

            await Task.Delay(100);

            Assert.AreEqual(expectedResult, viewModel.OutputValue);
        }
    }
}
