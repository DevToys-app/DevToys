using DevToys.Core.Threading;
using DevToys.ViewModels.Tools.StringUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace DevToys.Tests.Providers.Tools
{
    [TestClass]
    public class StringUtilitiesTests : MefBaseTest
    {
        private const string Text
= @"UWP is one of many ways to create client applications for Windows. UWP apps use WinRT APIs to provide powerful UI and advanced asynchronous features that are ideal for internet-connected devices.

To download the tools you need to start creating UWP apps, see Get set up, and then write your first app.

Where does UWP fit in the Microsoft development story?
UWP is one choice for creating apps that run on Windows 10 devices, and can be combined with other platforms. UWP apps can make use of Win32 APIs and .NET classes (see API Sets for UWP apps, Dlls for UWP apps, and .NET for UWP apps).

The Microsoft development story continues to evolve, and along with initiatives such as WinUI, MSIX, and Project Reunion, UWP is a powerful tool for creating client apps.

Features of a UWP app
A UWP app is:

Secure: UWP apps declare which device resources and data they access. The user must authorize that access.
Able to use a common API on all devices that run Windows 10.
Able to use device specific capabilities and adapt the UI to different device screen sizes, resolutions, and DPI.
Available from the Microsoft Store on all devices (or only those that you specify) that run on Windows 10. The Microsoft Store provides multiple ways to make money on your app.
Able to be installed and uninstalled without risk to the machine or incurring ""machine rot"".
Engaging: use live tiles, push notifications, and user activities that interact with Windows Timeline and Cortana's Pick Up Where I Left Off, to engage users.
Programmable in C#, C++, Visual Basic, and Javascript. For UI, use WinUI, XAML, HTML, or DirectX.
Let's look at these in more detail.";

        [TestMethod]
        public async Task CalculateSelectionStatisticsAsync()
        {
            var viewModel = ExportProvider.Import<StringUtilitiesToolViewModel>();

            viewModel.Text = Text;
            viewModel.SelectionStart = 0;

            await Task.Delay(100);

            Assert.AreEqual(1, viewModel.Line);
            Assert.AreEqual(0, viewModel.Column);

            viewModel.SelectionStart = 1;

            await Task.Delay(100);

            Assert.AreEqual(1, viewModel.Line);
            Assert.AreEqual(1, viewModel.Column);

            viewModel.SelectionStart = 2;

            await Task.Delay(100);

            Assert.AreEqual(1, viewModel.Line);
            Assert.AreEqual(2, viewModel.Column);

            viewModel.SelectionStart = 810;

            await Task.Delay(100);

            Assert.AreEqual(11, viewModel.Line);
            Assert.AreEqual(13, viewModel.Column);

            viewModel.SelectionStart = 812;

            await Task.Delay(100);

            Assert.AreEqual(12, viewModel.Line);
            Assert.AreEqual(0, viewModel.Column);
        }


        [TestMethod]
        public async Task CalculateTextStatisticsAsync()
        {
            var viewModel = ExportProvider.Import<StringUtilitiesToolViewModel>();

            viewModel.Text = Text;

            await Task.Delay(100);

            Assert.AreEqual(1666, viewModel.Characters);
            Assert.AreEqual(288, viewModel.Words);
            Assert.AreEqual(20, viewModel.Lines);
            Assert.AreEqual(22, viewModel.Sentences);
            Assert.AreEqual(6, viewModel.Paragraphs);
            Assert.AreEqual(1666, viewModel.Bytes);
        }
    }
}
