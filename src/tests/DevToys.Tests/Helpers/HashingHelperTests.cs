using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DevToys.Helpers;
using DevToys.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevToys.Tests.Helpers
{
    [TestClass]
    public class HashingHelperTests
    {
        private static byte[] _twoMBArray;

        [ClassInitialize]
        public static void Initialize(TestContext _)
        {
            _twoMBArray = Enumerable.Repeat<byte>(1, 1024 * 1024 * 2).ToArray();
        }

        [TestMethod]
        public async Task ComputeHashShouldThrowIfStreamIsNull()
        {
            ArgumentNullException ex = await Assert.ThrowsExceptionAsync<ArgumentNullException>(() =>
                    HashingHelper.ComputeHashAsync(MD5.Create(), null, new Progress<HashingProgress>((_) => { }), new CancellationToken()));
            Assert.AreEqual(ex.ParamName, "stream");
        }

        [TestMethod]
        public async Task ComputeHashShouldThrowIfHashingAlgorithmIsNull()
        {
            using (var stream = new MemoryStream())
            {
                ArgumentNullException ex = await Assert.ThrowsExceptionAsync<ArgumentNullException>(() =>
                   HashingHelper.ComputeHashAsync(null, stream, new Progress<HashingProgress>((_) => { }), new CancellationToken()));
                Assert.AreEqual(ex.ParamName, "hashAlgorithm");
            }
        }

        [DataTestMethod]
        [DataRow("", "D41D8CD98F00B204E9800998ECF8427E")]
        [DataRow(" ", "7215EE9C7D9DC229D2921A40E899EC5F")]
        [DataRow("Hello World", "B10A8DB164E0754105B7A99BE72E3FE5")]
        [DataRow("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nam cursus nec metus vel sagittis. Donec auctor nulla tortor. Praesent pretium, nunc vel porttitor tempor, metus eros pharetra orci, vitae lobortis ligula massa eu libero. Maecenas faucibus dolor ante, non ullamcorper urna blandit nec. Maecenas at pulvinar neque, a fermentum sapien. Curabitur mattis massa leo, a hendrerit lectus condimentum eget. Etiam nec scelerisque tellus. Quisque ligula erat, porttitor quis mi id, vehicula feugiat est.", "02868AD2F0787EF5323B221DBD14BAB2")]
        public async Task ComputeHashMD5(string input, string expectedResult)
        {
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(input)))
            {
                byte[] bytesResult = await HashingHelper.ComputeHashAsync(MD5.Create(), stream, new Progress<HashingProgress>((_) => { }), new CancellationToken());

                string result = FormatResult(BitConverter.ToString(bytesResult));

                Assert.AreEqual(expectedResult, result);
            }
        }

        [TestMethod]
        public async Task ComputeHashMD5ForLargeFile()
        {
            using (var stream = new MemoryStream(_twoMBArray))
            {
                byte[] bytesResult = await HashingHelper.ComputeHashAsync(MD5.Create(), stream, new Progress<HashingProgress>((_) => { }), new CancellationToken());

                string result = FormatResult(BitConverter.ToString(bytesResult));

                Assert.AreEqual("9F05E7ADB91551CA82CF646CA371344A", result);
            }
        }

        [DataTestMethod]
        [DataRow("", "DA39A3EE5E6B4B0D3255BFEF95601890AFD80709")]
        [DataRow(" ", "B858CB282617FB0956D960215C8E84D1CCF909C6")]
        [DataRow("Hello World", "0A4D55A8D778E5022FAB701977C5D840BBC486D0")]
        [DataRow("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nam cursus nec metus vel sagittis. Donec auctor nulla tortor. Praesent pretium, nunc vel porttitor tempor, metus eros pharetra orci, vitae lobortis ligula massa eu libero. Maecenas faucibus dolor ante, non ullamcorper urna blandit nec. Maecenas at pulvinar neque, a fermentum sapien. Curabitur mattis massa leo, a hendrerit lectus condimentum eget. Etiam nec scelerisque tellus. Quisque ligula erat, porttitor quis mi id, vehicula feugiat est.", "DBFE62B3C0817383AC870AACC61DD016BC599689")]
        public async Task ComputeHashSHA1(string input, string expectedResult)
        {
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(input)))
            {
                byte[] bytesResult = await HashingHelper.ComputeHashAsync(SHA1.Create(), stream, new Progress<HashingProgress>((_) => { }), new CancellationToken());

                string result = FormatResult(BitConverter.ToString(bytesResult));

                Assert.AreEqual(expectedResult, result);
            }
        }

        [TestMethod]
        public async Task ComputeHashSHA1ForLargeFile()
        {
            using (var stream = new MemoryStream(_twoMBArray))
            {
                byte[] bytesResult = await HashingHelper.ComputeHashAsync(SHA1.Create(), stream, new Progress<HashingProgress>((_) => { }), new CancellationToken());

                string result = FormatResult(BitConverter.ToString(bytesResult));

                Assert.AreEqual("F613FCBB54EF5E04D50D734FB0457B245833702B", result);
            }
        }

        [DataTestMethod]
        [DataRow("", "E3B0C44298FC1C149AFBF4C8996FB92427AE41E4649B934CA495991B7852B855")]
        [DataRow(" ", "36A9E7F1C95B82FFB99743E0C5C4CE95D83C9A430AAC59F84EF3CBFAB6145068")]
        [DataRow("Hello World", "A591A6D40BF420404A011733CFB7B190D62C65BF0BCDA32B57B277D9AD9F146E")]
        [DataRow("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nam cursus nec metus vel sagittis. Donec auctor nulla tortor. Praesent pretium, nunc vel porttitor tempor, metus eros pharetra orci, vitae lobortis ligula massa eu libero. Maecenas faucibus dolor ante, non ullamcorper urna blandit nec. Maecenas at pulvinar neque, a fermentum sapien. Curabitur mattis massa leo, a hendrerit lectus condimentum eget. Etiam nec scelerisque tellus. Quisque ligula erat, porttitor quis mi id, vehicula feugiat est.", "FD5800A6B0C7EE959AFE2CC011DFD9E9A48E9E4FB80828412F7AFC77A3A97782")]
        public async Task ComputeHashSHA256(string input, string expectedResult)
        {
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(input)))
            {
                byte[] bytesResult = await HashingHelper.ComputeHashAsync(SHA256.Create(), stream, new Progress<HashingProgress>((_) => { }), new CancellationToken());

                string result = FormatResult(BitConverter.ToString(bytesResult));

                Assert.AreEqual(expectedResult, result);
            }
        }

        [TestMethod]
        public async Task ComputeHashSHA256ForLargeFile()
        {
            using (var stream = new MemoryStream(_twoMBArray))
            {
                byte[] bytesResult = await HashingHelper.ComputeHashAsync(SHA256.Create(), stream, new Progress<HashingProgress>((_) => { }), new CancellationToken());

                string result = FormatResult(BitConverter.ToString(bytesResult));

                Assert.AreEqual("6D75695D93DEB1BF5805617CFBA166466CF60DC0A99A8014FA58893F358F33B8", result);
            }
        }

        [DataTestMethod]
        [DataRow("", "38B060A751AC96384CD9327EB1B1E36A21FDB71114BE07434C0CC7BF63F6E1DA274EDEBFE76F65FBD51AD2F14898B95B")]
        [DataRow(" ", "588016EB10045DD85834D67D187D6B97858F38C58C690320C4A64E0C2F92EEBD9F1BD74DE256E8268815905159449566")]
        [DataRow("Hello World", "99514329186B2F6AE4A1329E7EE6C610A729636335174AC6B740F9028396FCC803D0E93863A7C3D90F86BEEE782F4F3F")]
        [DataRow("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nam cursus nec metus vel sagittis. Donec auctor nulla tortor. Praesent pretium, nunc vel porttitor tempor, metus eros pharetra orci, vitae lobortis ligula massa eu libero. Maecenas faucibus dolor ante, non ullamcorper urna blandit nec. Maecenas at pulvinar neque, a fermentum sapien. Curabitur mattis massa leo, a hendrerit lectus condimentum eget. Etiam nec scelerisque tellus. Quisque ligula erat, porttitor quis mi id, vehicula feugiat est.", "D775DFC327D966FE31F96B82A618A3F41CBB5FD9A05106CCD60F433D8079B8EA67546E6BA0501E29A6C9D6355A11A00C")]
        public async Task ComputeHashSHA384(string input, string expectedResult)
        {
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(input)))
            {
                byte[] bytesResult = await HashingHelper.ComputeHashAsync(SHA384.Create(), stream, new Progress<HashingProgress>((_) => { }), new CancellationToken());

                string result = FormatResult(BitConverter.ToString(bytesResult));

                Assert.AreEqual(expectedResult, result);
            }
        }

        [TestMethod]
        public async Task ComputeHashSHA384ForLargeFile()
        {
            using (var stream = new MemoryStream(_twoMBArray))
            {
                byte[] bytesResult = await HashingHelper.ComputeHashAsync(SHA384.Create(), stream, new Progress<HashingProgress>((_) => { }), new CancellationToken());

                string result = FormatResult(BitConverter.ToString(bytesResult));

                Assert.AreEqual("DBC3E43FA05876C686A500F648D815B4CEEE99C1761C58EFB031C8E439884D0C4363C8BC56A9F9AD5343B4E8EFDE6D37", result);
            }
        }

        [DataTestMethod]
        [DataRow("", "CF83E1357EEFB8BDF1542850D66D8007D620E4050B5715DC83F4A921D36CE9CE47D0D13C5D85F2B0FF8318D2877EEC2F63B931BD47417A81A538327AF927DA3E")]
        [DataRow(" ", "F90DDD77E400DFE6A3FCF479B00B1EE29E7015C5BB8CD70F5F15B4886CC339275FF553FC8A053F8DDC7324F45168CFFAF81F8C3AC93996F6536EEF38E5E40768")]
        [DataRow("Hello World", "2C74FD17EDAFD80E8447B0D46741EE243B7EB74DD2149A0AB1B9246FB30382F27E853D8585719E0E67CBDA0DAA8F51671064615D645AE27ACB15BFB1447F459B")]
        [DataRow("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nam cursus nec metus vel sagittis. Donec auctor nulla tortor. Praesent pretium, nunc vel porttitor tempor, metus eros pharetra orci, vitae lobortis ligula massa eu libero. Maecenas faucibus dolor ante, non ullamcorper urna blandit nec. Maecenas at pulvinar neque, a fermentum sapien. Curabitur mattis massa leo, a hendrerit lectus condimentum eget. Etiam nec scelerisque tellus. Quisque ligula erat, porttitor quis mi id, vehicula feugiat est.", "9BE8DB4FE9A776337D500BAA2E99A23C41CDEA2E0D0F445EB72DC8C6BEFAADF9FB5FA4C14BA81E93716F24C004477F08ABF684B3E50F0365C63AE602136E56B5")]
        public async Task ComputeHashSHA512(string input, string expectedResult)
        {
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(input)))
            {
                byte[] bytesResult = await HashingHelper.ComputeHashAsync(SHA512.Create(), stream, new Progress<HashingProgress>((_) => { }), new CancellationToken());

                string result = FormatResult(BitConverter.ToString(bytesResult));

                Assert.AreEqual(expectedResult, result);
            }
        }

        [TestMethod]
        public async Task ComputeHashSHA512ForLargeFile()
        {
            using (var stream = new MemoryStream(_twoMBArray))
            {
                byte[] bytesResult = await HashingHelper.ComputeHashAsync(SHA512.Create(), stream, new Progress<HashingProgress>((_) => { }), new CancellationToken());

                string result = FormatResult(BitConverter.ToString(bytesResult));

                Assert.AreEqual("417E836E280AEF701556B6135CA4B690819F82B4CF82EAD3B493B5BBC41174B9A868A68F13C90FA1A9DCDCA72BFB6268ABC061C702450BD1F838F59E33E9686D", result);
            }
        }

        [DataTestMethod]
        [DataRow(0)]
        [DataRow(-1)]
        [DataRow(-100)]
        [DataRow(int.MinValue)]
        public void ComputeHashIterationsShouldThrowIfBufferSizeIsZeroOrBelow(int bufferSize)
        {
            byte[] byteArray = Array.Empty<byte>();
            using (var stream = new MemoryStream(byteArray))
            {
                ArgumentException ex = Assert.ThrowsException<ArgumentException>(() =>
                    HashingHelper.ComputeHashIterations(stream, bufferSize));
                Assert.AreEqual(ex.ParamName, "bufferSize");
            }
        }

        [DataTestMethod]
        [DataRow(100, 10, 10)]
        [DataRow(1000, 10, 100)]
        [DataRow(10000, 100, 100)]
        [DataRow(0, 10, 0)]
        [DataRow(1, 1, 1)]
        [DataRow(1, 10, 1)]
        public void ComputeHashIterations(int streamSize, int bufferSize, int expectedResult)
        {
            byte[] byteArray = new byte[streamSize];
            using (var stream = new MemoryStream(byteArray))
            {
                long iterations = HashingHelper.ComputeHashIterations(stream, bufferSize);

                Assert.AreEqual(expectedResult, iterations);
            }
        }

        [DataTestMethod]
        [DataRow(1048576, 1)]
        [DataRow(5242880, 5)]
        [DataRow(10485760, 10)]
        [DataRow(104857600, 100)]
        public void ComputeHashingIterationsWithDefaultBufferSize(long streamSize, int expectedResult)
        {
            byte[] byteArray = new byte[streamSize];
            using (var stream = new MemoryStream(byteArray))
            {
                long iterations = HashingHelper.ComputeHashIterations(stream);

                Assert.AreEqual(expectedResult, iterations);
            }
        }

        private string FormatResult(string input) => input.Replace("-", string.Empty);
    }
}
