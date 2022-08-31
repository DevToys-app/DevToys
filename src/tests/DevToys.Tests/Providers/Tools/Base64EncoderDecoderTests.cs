using System.Linq;
using System.Threading.Tasks;
using DevToys.Api.Tools;
using DevToys.Core.Threading;
using DevToys.Models;
using DevToys.ViewModels.Tools.Base64EncoderDecoder;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevToys.Tests.Providers.Tools
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
        public async Task CanBeTreatedByTool(string input, bool expectedResult)
        {
            await ThreadHelper.RunOnUIThreadAsync(() =>
            {
                System.Collections.Generic.IEnumerable<ToolProviderViewItem> result = ExportProvider.Import<IToolProviderFactory>().GetAllTools();
                ToolProviderViewItem base64Tool = result.First(item => item.Metadata.ProtocolName == "base64");

                Assert.AreEqual(expectedResult, base64Tool.ToolProvider.CanBeTreatedByTool(input));
            });
        }

        [DataTestMethod]
        [DataRow(null, "", NewlineSeparator.CRLF)]
        [DataRow(null, "", NewlineSeparator.LF)]
        [DataRow("", "", NewlineSeparator.CRLF)]
        [DataRow("", "", NewlineSeparator.LF)]
        [DataRow(" ", "", NewlineSeparator.CRLF)]
        [DataRow(" ", "", NewlineSeparator.LF)]
        [DataRow("Hello There", "SGVsbG8gVGhlcmU=", NewlineSeparator.CRLF)]
        [DataRow("Hello There", "SGVsbG8gVGhlcmU=", NewlineSeparator.LF)]
        [DataRow("Hello\r\nThere", "SGVsbG8NClRoZXJl", NewlineSeparator.CRLF)]
        [DataRow("Hello\r\nThere", "SGVsbG8KVGhlcmU=", NewlineSeparator.LF)]
        [DataRow("Hello\rThere", "SGVsbG8NClRoZXJl", NewlineSeparator.CRLF)]
        [DataRow("Hello\rThere", "SGVsbG8KVGhlcmU=", NewlineSeparator.LF)]
        [DataRow("Hello\nThere", "SGVsbG8NClRoZXJl", NewlineSeparator.CRLF)]
        [DataRow("Hello\nThere", "SGVsbG8KVGhlcmU=", NewlineSeparator.LF)]
        public async Task EncoderAsync(string input, string expectedResult, NewlineSeparator newlineSeparator)
        {
            Base64EncoderDecoderToolViewModel viewModel = ExportProvider.Import<Base64EncoderDecoderToolViewModel>();

            await ThreadHelper.RunOnUIThreadAsync(() =>
            {
                viewModel.IsEncodeMode = true;
                viewModel.NewlineSeparatorMode = NewlineSeparatorDisplayPair.Values.Single(v => v.Value == newlineSeparator);
                viewModel.InputValue = input;
            });

            await viewModel.ComputationTask;

            Assert.AreEqual(expectedResult, viewModel.OutputValue);
        }

        [DataTestMethod]
        [DataRow(null, "")]
        [DataRow("", "")]
        [DataRow(" ", "")]
        [DataRow("SGVsbG8gVGhlcmU=", "Hello There")]
        [DataRow("SGVsbG8gVGhlcmU", "Hello There")]
        public async Task DecodeAsync(string input, string expectedResult)
        {
            Base64EncoderDecoderToolViewModel viewModel = ExportProvider.Import<Base64EncoderDecoderToolViewModel>();

            await ThreadHelper.RunOnUIThreadAsync(() =>
            {
                viewModel.IsEncodeMode = false;
                viewModel.InputValue = input;
            });

            await viewModel.ComputationTask;

            Assert.AreEqual(expectedResult, viewModel.OutputValue);
        }
    }
}
