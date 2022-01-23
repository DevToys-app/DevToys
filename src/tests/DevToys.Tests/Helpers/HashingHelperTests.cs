using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using DevToys.Helpers;
using DevToys.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevToys.Tests.Helpers
{
    [TestClass]
    public class HashingHelperTests
    {
        [DataTestMethod]
        [DataRow("", "")]
        [DataRow(" ", "7215EE9C7D9DC229D2921A40E899EC5F")]
        [DataRow("Hello World", "B10A8DB164E0754105B7A99BE72E3FE5")]
        [DataRow("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nam cursus nec metus vel sagittis. Donec auctor nulla tortor. Praesent pretium, nunc vel porttitor tempor, metus eros pharetra orci, vitae lobortis ligula massa eu libero. Maecenas faucibus dolor ante, non ullamcorper urna blandit nec. Maecenas at pulvinar neque, a fermentum sapien. Curabitur mattis massa leo, a hendrerit lectus condimentum eget. Etiam nec scelerisque tellus. Quisque ligula erat, porttitor quis mi id, vehicula feugiat est.", "02868AD2F0787EF5323B221DBD14BAB2")]
        public async Task ComputeHashMD5(string input, string expectedResult)
        {
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(input)))
            {
                byte[] bytesResult = await HashingHelper.ComputeHashAsync(HashAlgorithm.Create("MD5"), stream, new Progress<HashingProgress>((_) => { }));

                string result = FormatResult(BitConverter.ToString(bytesResult));

                Assert.AreEqual(expectedResult, result);
            }
        }

        [DataTestMethod]
        [DataRow("", "")]
        [DataRow(" ", "B858CB282617FB0956D960215C8E84D1CCF909C6")]
        [DataRow("Hello World", "0A4D55A8D778E5022FAB701977C5D840BBC486D0")]
        [DataRow("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nam cursus nec metus vel sagittis. Donec auctor nulla tortor. Praesent pretium, nunc vel porttitor tempor, metus eros pharetra orci, vitae lobortis ligula massa eu libero. Maecenas faucibus dolor ante, non ullamcorper urna blandit nec. Maecenas at pulvinar neque, a fermentum sapien. Curabitur mattis massa leo, a hendrerit lectus condimentum eget. Etiam nec scelerisque tellus. Quisque ligula erat, porttitor quis mi id, vehicula feugiat est.", "DBFE62B3C0817383AC870AACC61DD016BC599689")]
        public async Task ComputeHashSHA1(string input, string expectedResult)
        {
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(input)))
            {
                byte[] bytesResult = await HashingHelper.ComputeHashAsync(HashAlgorithm.Create("SHA1"), stream, new Progress<HashingProgress>((_) => { }));

                string result = FormatResult(BitConverter.ToString(bytesResult));

                Assert.AreEqual(expectedResult, result);
            }
        }

        [DataTestMethod]
        [DataRow("", "")]
        [DataRow(" ", "36A9E7F1C95B82FFB99743E0C5C4CE95D83C9A430AAC59F84EF3CBFAB6145068")]
        [DataRow("Hello World", "A591A6D40BF420404A011733CFB7B190D62C65BF0BCDA32B57B277D9AD9F146E")]
        [DataRow("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nam cursus nec metus vel sagittis. Donec auctor nulla tortor. Praesent pretium, nunc vel porttitor tempor, metus eros pharetra orci, vitae lobortis ligula massa eu libero. Maecenas faucibus dolor ante, non ullamcorper urna blandit nec. Maecenas at pulvinar neque, a fermentum sapien. Curabitur mattis massa leo, a hendrerit lectus condimentum eget. Etiam nec scelerisque tellus. Quisque ligula erat, porttitor quis mi id, vehicula feugiat est.", "FD5800A6B0C7EE959AFE2CC011DFD9E9A48E9E4FB80828412F7AFC77A3A97782")]
        public async Task ComputeHashSHA256(string input, string expectedResult)
        {
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(input)))
            {
                byte[] bytesResult = await HashingHelper.ComputeHashAsync(HashAlgorithm.Create("SHA256"), stream, new Progress<HashingProgress>((_) => { }));

                string result = FormatResult(BitConverter.ToString(bytesResult));

                Assert.AreEqual(expectedResult, result);
            }
        }

        [DataTestMethod]
        [DataRow("", "")]
        [DataRow(" ", "588016EB10045DD85834D67D187D6B97858F38C58C690320C4A64E0C2F92EEBD9F1BD74DE256E8268815905159449566")]
        [DataRow("Hello World", "99514329186B2F6AE4A1329E7EE6C610A729636335174AC6B740F9028396FCC803D0E93863A7C3D90F86BEEE782F4F3F")]
        [DataRow("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nam cursus nec metus vel sagittis. Donec auctor nulla tortor. Praesent pretium, nunc vel porttitor tempor, metus eros pharetra orci, vitae lobortis ligula massa eu libero. Maecenas faucibus dolor ante, non ullamcorper urna blandit nec. Maecenas at pulvinar neque, a fermentum sapien. Curabitur mattis massa leo, a hendrerit lectus condimentum eget. Etiam nec scelerisque tellus. Quisque ligula erat, porttitor quis mi id, vehicula feugiat est.", "D775DFC327D966FE31F96B82A618A3F41CBB5FD9A05106CCD60F433D8079B8EA67546E6BA0501E29A6C9D6355A11A00C")]
        public async Task ComputeHashSHA384(string input, string expectedResult)
        {
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(input)))
            {
                byte[] bytesResult = await HashingHelper.ComputeHashAsync(HashAlgorithm.Create("SHA384"), stream, new Progress<HashingProgress>((_) => { }));

                string result = FormatResult(BitConverter.ToString(bytesResult));

                Assert.AreEqual(expectedResult, result);
            }
        }

        [DataTestMethod]
        [DataRow("", "")]
        [DataRow(" ", "F90DDD77E400DFE6A3FCF479B00B1EE29E7015C5BB8CD70F5F15B4886CC339275FF553FC8A053F8DDC7324F45168CFFAF81F8C3AC93996F6536EEF38E5E40768")]
        [DataRow("Hello World", "2C74FD17EDAFD80E8447B0D46741EE243B7EB74DD2149A0AB1B9246FB30382F27E853D8585719E0E67CBDA0DAA8F51671064615D645AE27ACB15BFB1447F459B")]
        [DataRow("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nam cursus nec metus vel sagittis. Donec auctor nulla tortor. Praesent pretium, nunc vel porttitor tempor, metus eros pharetra orci, vitae lobortis ligula massa eu libero. Maecenas faucibus dolor ante, non ullamcorper urna blandit nec. Maecenas at pulvinar neque, a fermentum sapien. Curabitur mattis massa leo, a hendrerit lectus condimentum eget. Etiam nec scelerisque tellus. Quisque ligula erat, porttitor quis mi id, vehicula feugiat est.", "9BE8DB4FE9A776337D500BAA2E99A23C41CDEA2E0D0F445EB72DC8C6BEFAADF9FB5FA4C14BA81E93716F24C004477F08ABF684B3E50F0365C63AE602136E56B5")]
        public async Task ComputeHashSHA512(string input, string expectedResult)
        {
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(input)))
            {
                byte[] bytesResult = await HashingHelper.ComputeHashAsync(HashAlgorithm.Create("SHA512"), stream, new Progress<HashingProgress>((_) => { }));

                string result = FormatResult(BitConverter.ToString(bytesResult));

                Assert.AreEqual(expectedResult, result);
            }
        }

        private string FormatResult(string input) => input.Replace("-", string.Empty);
    }
}
