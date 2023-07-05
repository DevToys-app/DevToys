using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DevToys.Api.Tools;
using DevToys.Core.Threading;
using DevToys.Models;
using DevToys.Models.JwtDecoderEncoder;
using DevToys.ViewModels.Tools.EncodersDecoders.JwtDecoderEncoder;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevToys.Tests.Providers.Tools
{
    [TestClass]
    public class JwtDecoderEncoderTests : MefBaseTest
    {
        private static InfoBarData _validationResult;

        [DataTestMethod]
        [DataRow(null, false)]
        [DataRow("", false)]
        [DataRow(" ", false)]
        [DataRow("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c", true)]
        [DataRow("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ", false)]
        [DataRow("Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c", true)]
        [DataRow("Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ", false)]
        [DataRow("Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c", true)]
        [DataRow("Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ", false)]
        public async Task CanBeTreatedByTool(string input, bool expectedResult)
        {
            await ThreadHelper.RunOnUIThreadAsync(() =>
            {
                System.Collections.Generic.IEnumerable<ToolProviderViewItem> result = ExportProvider.Import<IToolProviderFactory>().GetAllTools();
                ToolProviderViewItem jwtTool = result.First(item => item.Metadata.ProtocolName == "jwt");

                Assert.AreEqual(expectedResult, jwtTool.ToolProvider.CanBeTreatedByTool(input));
            });
        }

        #region GenerateAndDecodeHS

        [TestMethod]
        public async Task JwtDecoderEncoder_Generate_And_Decode_Basic_HS_Token_Without_Signature_Validation()
        {
            string signature = await TestDataProvider.GetFileContent("Jwt.HS.Signature.txt");
            var encoderParameters = new EncoderParameters();
            var tokenParameters = new TokenParameters()
            {
                TokenAlgorithm = JwtAlgorithm.HS384,
                Payload = await TestDataProvider.GetFileContent("Jwt.BasicPayload.json"),
                Signature = signature
            };
            var jwtEncoder = new JwtEncoder();
            TokenResult result = jwtEncoder.GenerateToken(encoderParameters, tokenParameters, DecodingErrorCallBack);

            Assert.IsNotNull(result);
            Assert.IsNull(_validationResult);
            Assert.IsNotNull(result.Token);

            // Decode
            string header = await TestDataProvider.GetFileContent("Jwt.HS.Header.json");
            string payload = await TestDataProvider.GetFileContent("Jwt.BasicPayload.json");
            var decodeParameters = new DecoderParameters();
            tokenParameters = new TokenParameters()
            {
                TokenAlgorithm = JwtAlgorithm.HS384,
                Token = result.Token
            };

            var jwtDecoder = new JwtDecoder();
            result = jwtDecoder.DecodeToken(decodeParameters, tokenParameters, DecodingErrorCallBack, out _);

            Assert.IsNotNull(result);
            Assert.IsNull(_validationResult);
            Assert.AreEqual(result.Payload, payload);
        }

        [TestMethod]
        public async Task JwtDecoderEncoder_Generate_And_Decode_Complex_HS_Token_With_Signature_Validation()
        {
            string signature = await TestDataProvider.GetFileContent("Jwt.HS.Signature.txt");
            string header = await TestDataProvider.GetFileContent("Jwt.HS.Header.json");
            string payload = await TestDataProvider.GetFileContent("Jwt.ComplexPayload.json");

            var encoderParameters = new EncoderParameters()
            {
                HasAudience = true,
                HasExpiration = true,
                HasIssuer = true,
                HasDefaultTime = true
            };
            var tokenParameters = new TokenParameters()
            {
                TokenAlgorithm = JwtAlgorithm.HS512,
                Payload = payload,
                Signature = signature,
                ValidIssuers = new HashSet<string> { "devtoys" },
                ValidAudiences = new HashSet<string> { "devtoys" },
                ExpirationYear = 1,
                ExpirationMonth = 1,
                ExpirationDay = 1,
                ExpirationHour = 0,
                ExpirationMinute = 0
            };

            var jwtEncoder = new JwtEncoder();
            TokenResult result = jwtEncoder.GenerateToken(encoderParameters, tokenParameters, DecodingErrorCallBack);

            Assert.IsNotNull(result);
            Assert.IsNull(_validationResult);
            Assert.IsNotNull(result.Token);

            // Decode
            var decodeParameters = new DecoderParameters()
            {
                ValidateSignature = true,
                ValidateActor = true,
                ValidateIssuer = true,
                ValidateAudience = true
            };
            tokenParameters = new TokenParameters()
            {
                Token = result.Token,
                Signature = signature,
                TokenAlgorithm = JwtAlgorithm.HS512,
                ValidIssuers = new HashSet<string> { "devtoys" },
                ValidAudiences = new HashSet<string> { "devtoys" },
            };
            var jwtDecoder = new JwtDecoder();
            result = jwtDecoder.DecodeToken(decodeParameters, tokenParameters, DecodingErrorCallBack, out _);

            Assert.IsNotNull(result);
            Assert.IsNull(_validationResult);
        }

        #endregion

        #region GenerateRS

        [TestMethod]
        public async Task JwtDecoderEncoder_Generate_And_Decode_Basic_RS_Token_Without_Signature_Validation()
        {
            string header = await TestDataProvider.GetFileContent("Jwt.RS.Header.json");
            string payload = await TestDataProvider.GetFileContent("Jwt.BasicPayload.json");

            var encoderParameters = new EncoderParameters();
            var tokenParameters = new TokenParameters()
            {
                TokenAlgorithm = JwtAlgorithm.RS384,
                Payload = payload,
                PrivateKey = await TestDataProvider.GetFileContent("Jwt.RS.PrivateKey.txt")
            };
            var jwtEncoder = new JwtEncoder();
            TokenResult result = jwtEncoder.GenerateToken(encoderParameters, tokenParameters, DecodingErrorCallBack);

            Assert.IsNotNull(result);
            Assert.IsNull(_validationResult);
            Assert.IsNotNull(result.Token);

            // Decode
            var decodeParameters = new DecoderParameters();
            tokenParameters = new TokenParameters()
            {
                TokenAlgorithm = JwtAlgorithm.RS384,
                Token = result.Token
            };

            var jwtDecoder = new JwtDecoder();
            result = jwtDecoder.DecodeToken(decodeParameters, tokenParameters, DecodingErrorCallBack, out _);

            Assert.IsNotNull(result);
            Assert.IsNull(_validationResult);
        }

        [TestMethod]
        public async Task JwtDecoderEncoder_Generate_And_Decode_Complex_RS_Token_With_Signature_Validation()
        {
            string payload = await TestDataProvider.GetFileContent("Jwt.BasicPayload.json");
            string privateKey = await TestDataProvider.GetFileContent("Jwt.RS.PrivateKey.txt");
            var encoderParameters = new EncoderParameters()
            {
                HasAudience = true,
                HasExpiration = true,
                HasIssuer = true,
                HasDefaultTime = true
            };
            var tokenParameters = new TokenParameters()
            {
                TokenAlgorithm = JwtAlgorithm.RS512,
                Payload = payload,
                PrivateKey = privateKey,
                ValidIssuers = new HashSet<string> { "devtoys" },
                ValidAudiences = new HashSet<string> { "devtoys" },
                ExpirationYear = 1,
                ExpirationMonth = 1,
                ExpirationDay = 1,
                ExpirationHour = 0,
                ExpirationMinute = 0
            };

            var jwtEncoder = new JwtEncoder();
            TokenResult result = jwtEncoder.GenerateToken(encoderParameters, tokenParameters, DecodingErrorCallBack);

            Assert.IsNotNull(result);
            Assert.IsNull(_validationResult);
            Assert.IsNotNull(result.Token);

            // decode
            string publicKey = await TestDataProvider.GetFileContent("Jwt.RS.PublicKey.txt");
            var decodeParameters = new DecoderParameters()
            {
                ValidateSignature = true,
                ValidateActor = true,
                ValidateIssuer = true,
                ValidateAudience = true
            };
            tokenParameters = new TokenParameters()
            {
                TokenAlgorithm = JwtAlgorithm.RS512,
                Token = result.Token,
                PublicKey = publicKey,
                ValidIssuers = new HashSet<string> { "devtoys" },
                ValidAudiences = new HashSet<string> { "devtoys" },
            };
            var jwtDecoder = new JwtDecoder();
            result = jwtDecoder.DecodeToken(decodeParameters, tokenParameters, DecodingErrorCallBack, out _);

            Assert.IsNotNull(result);
            Assert.IsNull(_validationResult);
        }

        #endregion

        #region GeneratePS

        [TestMethod]
        public async Task JwtDecoderEncoder_Generate_And_Decode_Basic_PS_Token_Without_Signature_Validation()
        {
            string header = await TestDataProvider.GetFileContent("Jwt.PS.Header.json");
            string payload = await TestDataProvider.GetFileContent("Jwt.BasicPayload.json");

            var encoderParameters = new EncoderParameters();
            var tokenParameters = new TokenParameters()
            {
                TokenAlgorithm = JwtAlgorithm.PS384,
                Payload = payload,
                PrivateKey = await TestDataProvider.GetFileContent("Jwt.PS.PrivateKey.txt")
            };
            var jwtEncoder = new JwtEncoder();
            TokenResult result = jwtEncoder.GenerateToken(encoderParameters, tokenParameters, DecodingErrorCallBack);

            Assert.IsNotNull(result);
            Assert.IsNull(_validationResult);
            Assert.IsNotNull(result.Token);

            // Decode
            var decodeParameters = new DecoderParameters();
            tokenParameters = new TokenParameters()
            {
                TokenAlgorithm = JwtAlgorithm.PS384,
                Token = result.Token
            };

            var jwtDecoder = new JwtDecoder();
            result = jwtDecoder.DecodeToken(decodeParameters, tokenParameters, DecodingErrorCallBack, out _);

            Assert.IsNotNull(result);
            Assert.IsNull(_validationResult);
        }

        [TestMethod]
        public async Task JwtDecoderEncoder_Generate_And_Decode_Complex_PS_Token_With_Signature_Validation()
        {
            string payload = await TestDataProvider.GetFileContent("Jwt.BasicPayload.json");
            string privateKey = await TestDataProvider.GetFileContent("Jwt.PS.PrivateKey.txt");
            var encoderParameters = new EncoderParameters()
            {
                HasAudience = true,
                HasExpiration = true,
                HasIssuer = true,
                HasDefaultTime = true
            };
            var tokenParameters = new TokenParameters()
            {
                TokenAlgorithm = JwtAlgorithm.PS512,
                Payload = payload,
                PrivateKey = privateKey,
                ValidIssuers = new HashSet<string> { "devtoys" },
                ValidAudiences = new HashSet<string> { "devtoys" },
                ExpirationYear = 1,
                ExpirationMonth = 1,
                ExpirationDay = 1,
                ExpirationHour = 0,
                ExpirationMinute = 0
            };

            var jwtEncoder = new JwtEncoder();
            TokenResult result = jwtEncoder.GenerateToken(encoderParameters, tokenParameters, DecodingErrorCallBack);

            Assert.IsNotNull(result);
            Assert.IsNull(_validationResult);
            Assert.IsNotNull(result.Token);

            // decode
            string publicKey = await TestDataProvider.GetFileContent("Jwt.RS.PublicKey.txt");
            var decodeParameters = new DecoderParameters()
            {
                ValidateSignature = true,
                ValidateActor = true,
                ValidateIssuer = true,
                ValidateAudience = true
            };
            tokenParameters = new TokenParameters()
            {
                TokenAlgorithm = JwtAlgorithm.PS512,
                Token = result.Token,
                PublicKey = publicKey,
                ValidIssuers = new HashSet<string> { "devtoys" },
                ValidAudiences = new HashSet<string> { "devtoys" },
            };
            var jwtDecoder = new JwtDecoder();
            result = jwtDecoder.DecodeToken(decodeParameters, tokenParameters, DecodingErrorCallBack, out _);

            Assert.IsNotNull(result);
            Assert.IsNull(_validationResult);
        }

        #endregion

        #region GenerateES

        [TestMethod]
        public async Task JwtDecoderEncoder_Generate_And_Decode_Basic_ES_Token_Without_Signature_Validation()
        {
            string header = await TestDataProvider.GetFileContent("Jwt.ES.Header.json");
            string payload = await TestDataProvider.GetFileContent("Jwt.BasicPayload.json");

            var encoderParameters = new EncoderParameters();
            var tokenParameters = new TokenParameters()
            {
                TokenAlgorithm = JwtAlgorithm.ES384,
                Payload = payload,
                PrivateKey = await TestDataProvider.GetFileContent("Jwt.ES.PrivateKey.txt")
            };
            var jwtEncoder = new JwtEncoder();
            TokenResult result = jwtEncoder.GenerateToken(encoderParameters, tokenParameters, DecodingErrorCallBack);

            Assert.IsNotNull(result);
            Assert.IsNull(_validationResult);
            Assert.IsNotNull(result.Token);

            // Decode
            var decodeParameters = new DecoderParameters();
            tokenParameters = new TokenParameters()
            {
                TokenAlgorithm = JwtAlgorithm.ES384,
                Token = result.Token
            };

            var jwtDecoder = new JwtDecoder();
            result = jwtDecoder.DecodeToken(decodeParameters, tokenParameters, DecodingErrorCallBack, out _);

            Assert.IsNotNull(result);
            Assert.IsNull(_validationResult);
        }

        [TestMethod]
        public async Task JwtDecoderEncoder_Generate_And_Decode_Complex_ES_Token_With_Signature_Validation()
        {
            string payload = await TestDataProvider.GetFileContent("Jwt.BasicPayload.json");
            string privateKey = await TestDataProvider.GetFileContent("Jwt.ES.PrivateKey.txt");
            var encoderParameters = new EncoderParameters()
            {
                HasAudience = true,
                HasExpiration = true,
                HasIssuer = true,
                HasDefaultTime = true
            };
            var tokenParameters = new TokenParameters()
            {
                TokenAlgorithm = JwtAlgorithm.ES512,
                Payload = payload,
                PrivateKey = privateKey,
                ValidIssuers = new HashSet<string> { "devtoys" },
                ValidAudiences = new HashSet<string> { "devtoys" },
                ExpirationYear = 1,
                ExpirationMonth = 1,
                ExpirationDay = 1,
                ExpirationHour = 0,
                ExpirationMinute = 0
            };

            var jwtEncoder = new JwtEncoder();
            TokenResult result = jwtEncoder.GenerateToken(encoderParameters, tokenParameters, DecodingErrorCallBack);

            Assert.IsNotNull(result);
            Assert.IsNull(_validationResult);
            Assert.IsNotNull(result.Token);

            // decode
            string publicKey = await TestDataProvider.GetFileContent("Jwt.ES.PublicKey.txt");
            var decodeParameters = new DecoderParameters()
            {
                ValidateSignature = true,
                ValidateActor = true,
                ValidateIssuer = true,
                ValidateAudience = true
            };
            tokenParameters = new TokenParameters()
            {
                TokenAlgorithm = JwtAlgorithm.ES512,
                Token = result.Token,
                PublicKey = publicKey,
                ValidIssuers = new HashSet<string> { "devtoys" },
                ValidAudiences = new HashSet<string> { "devtoys" },
            };
            var jwtDecoder = new JwtDecoder();
            result = jwtDecoder.DecodeToken(decodeParameters, tokenParameters, DecodingErrorCallBack, out _);

            Assert.IsNotNull(result);
            Assert.IsNull(_validationResult);
        }

        #endregion

        internal static void DecodingErrorCallBack(TokenResultErrorEventArgs e)
             => _validationResult = new InfoBarData(e.Severity, e.Message);

        [TestCleanup]
        public void TestCleanup()
        {
            _validationResult = null;
        }
    }
}
