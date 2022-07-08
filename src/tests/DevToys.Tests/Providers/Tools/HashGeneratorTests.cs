using System.Threading.Tasks;
using DevToys.Core.Threading;
using DevToys.ViewModels.Tools.HashGenerator;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevToys.Tests.Providers.Tools
{
    [TestClass]
    public class HashGeneratorTests : MefBaseTest
    {
        [DataTestMethod]
        [DataRow(null, "")]
        [DataRow("", "")]
        [DataRow(" ", "f90ddd77e400dfe6a3fcf479b00b1ee29e7015c5bb8cd70f5f15b4886cc339275ff553fc8a053f8ddc7324f45168cffaf81f8c3ac93996f6536eef38e5e40768")]
        [DataRow("Hello There", "4313da7166cfbabe16be21f43d5ba8f32ad7e7508a5754843ce6be3be00a24b0962e4b585ec9b8736774fbda649c8671440391705646254b79618b385395be13")]
        public async Task LowerCaseHashingAsync(string input, string expectedResult)
        {
            HashGeneratorToolViewModel viewModel = ExportProvider.Import<HashGeneratorToolViewModel>();

            await ThreadHelper.RunOnUIThreadAsync(() =>
            {
                viewModel.IsHmacMode = false;
                viewModel.IsUppercase = false;
                viewModel.Input = input;
            });

            await viewModel.ComputationTask;

            Assert.AreEqual(expectedResult, viewModel.SHA512);
        }

        [DataTestMethod]
        [DataRow(null, "")]
        [DataRow("", "")]
        [DataRow(" ", "F90DDD77E400DFE6A3FCF479B00B1EE29E7015C5BB8CD70F5F15B4886CC339275FF553FC8A053F8DDC7324F45168CFFAF81F8C3AC93996F6536EEF38E5E40768")]
        [DataRow("Hello There", "4313DA7166CFBABE16BE21F43D5BA8F32AD7E7508A5754843CE6BE3BE00A24B0962E4B585EC9B8736774FBDA649C8671440391705646254B79618B385395BE13")]
        public async Task UpperCaseHashingAsync(string input, string expectedResult)
        {
            HashGeneratorToolViewModel viewModel = ExportProvider.Import<HashGeneratorToolViewModel>();

            await ThreadHelper.RunOnUIThreadAsync(() =>
            {
                viewModel.IsHmacMode = false;
                viewModel.IsUppercase = true;
                viewModel.Input = input;
            });

            await viewModel.ComputationTask;

            Assert.AreEqual(expectedResult, viewModel.SHA512);
        }

        [DataTestMethod]
        [DataRow(null, "", "")]
        [DataRow("", "", "")]
        [DataRow(" ", " ", "0b8a72163b925bbb61ffa98e90339e57f0ed5c8956665af83691aebbdebb87e7eb6090a877b62fdcfca2e29768159d0066e7ef875a87d6a8b2ff9d286a98ff56")]
        [DataRow("Hello There", "World", "43bb6d1170cbf1be61ebf85a434ea1acb4caced09b8f0d40125b21804c524a4be66ba0af02617076505973edf819563e4d8eb68a5c2c4dc5d1a0a661bcce5d44")]
        public async Task LowercaseHmacModeHashingAsync(string input, string secretKey, string expectedResult)
        {
            HashGeneratorToolViewModel viewModel = ExportProvider.Import<HashGeneratorToolViewModel>();

            await ThreadHelper.RunOnUIThreadAsync(() =>
            {
                viewModel.IsUppercase = false;
                viewModel.Input = input;
                viewModel.IsHmacMode = true;
                viewModel.SecretKey = secretKey;
            });

            await viewModel.ComputationTask;

            Assert.AreEqual(expectedResult, viewModel.SHA512);
        }

        [DataTestMethod]
        [DataRow(null, "", "")]
        [DataRow("", "", "")]
        [DataRow(" ", " ", "0B8A72163B925BBB61FFA98E90339E57F0ED5C8956665AF83691AEBBDEBB87E7EB6090A877B62FDCFCA2E29768159D0066E7EF875A87D6A8B2FF9D286A98FF56")]
        [DataRow("Hello There", "World", "43BB6D1170CBF1BE61EBF85A434EA1ACB4CACED09B8F0D40125B21804C524A4BE66BA0AF02617076505973EDF819563E4D8EB68A5C2C4DC5D1A0A661BCCE5D44")]
        public async Task UppercaseHmacModeHashingAsync(string input, string secretKey, string expectedResult)
        {
            HashGeneratorToolViewModel viewModel = ExportProvider.Import<HashGeneratorToolViewModel>();

            await ThreadHelper.RunOnUIThreadAsync(() =>
            {
                viewModel.IsUppercase = true;
                viewModel.Input = input;
                viewModel.IsHmacMode = true;
                viewModel.SecretKey = secretKey;
            });

            await viewModel.ComputationTask;

            Assert.AreEqual(expectedResult, viewModel.SHA512);
        }
    }
}
