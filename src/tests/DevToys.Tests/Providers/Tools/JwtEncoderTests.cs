using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading.Tasks;
using DevToys.Models;
using DevToys.Models.JwtDecoderEncoder;
using DevToys.ViewModels.Tools.EncodersDecoders.JwtDecoderEncoder;
using Microsoft.UI.Xaml.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevToys.Tests.Providers.Tools
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class JwtEncoderTests
    {
        private static InfoBarData _validationResult;
        private static JwtDecoderEncoderStrings _localizedStrings;

        [TestInitialize]
        public void TestInitialize()
        {
            _localizedStrings = new JwtDecoderEncoderStrings();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void JwtEncoder_Generate_With_Null_DecoderParameters_Should_Throw_ArgumentNullException()
        {
            var jwtDecoder = new JwtEncoder();
            jwtDecoder.GenerateToken(null, null, DecodingErrorCallBack);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void JwtEncoder_Generate_With_Null_TokenParameters_Should_Throw_ArgumentNullException()
        {
            var jwtDecoder = new JwtEncoder();
            jwtDecoder.GenerateToken(new EncoderParameters(), null, DecodingErrorCallBack);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void JwtEncoder_Generate_With_Null_TokenResultErrorEventArgs_Should_Throw_ArgumentNullException()
        {
            var jwtDecoder = new JwtEncoder();
            jwtDecoder.GenerateToken(new EncoderParameters(), new TokenParameters(), null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void JwtEncoder_Generate_With_Null_Payload_Should_Throw_ArgumentNullException()
        {
            var encoderParameters = new EncoderParameters();
            var tokenParameters = new TokenParameters()
            {
                Payload = null
            };


            var jwtDecoder = new JwtEncoder();
            jwtDecoder.GenerateToken(encoderParameters, tokenParameters, DecodingErrorCallBack);
        }

        [TestMethod]
        public void JwtEncoder_Generate_With_Invalid_Token_Should_Fail_With_Error()
        {
            var encoderParameters = new EncoderParameters();
            var tokenParameters = new TokenParameters()
            {
                Payload = "xxx"
            };

            var jwtDecoder = new JwtEncoder();
            TokenResult result = jwtDecoder.GenerateToken(encoderParameters, tokenParameters, DecodingErrorCallBack);

            Assert.IsNull(result);
            Assert.IsNotNull(_validationResult);
            Assert.AreEqual(_validationResult.Severity, InfoBarSeverity.Error);
            Assert.IsNotNull(_validationResult.Message);
        }

        #region GenerateHS

        [TestMethod]
        public async Task JwtEncoder_Generate_Basic_HS_Token_Without_Signature_Should_Fail()
        {
            string token = await TestDataProvider.GetFileContent("Jwt.HS.BasicToken.txt");
            var encoderParameters = new EncoderParameters();
            var tokenParameters = new TokenParameters()
            {
                TokenAlgorithm = JwtAlgorithm.HS256,
                Payload = await TestDataProvider.GetFileContent("Jwt.BasicPayload.json"),
            };
            var jwtDecoder = new JwtEncoder();
            TokenResult result = jwtDecoder.GenerateToken(encoderParameters, tokenParameters, DecodingErrorCallBack);

            Assert.IsNull(result);
            Assert.IsNotNull(_validationResult);
            Assert.AreEqual(_validationResult.Message, _localizedStrings.InvalidSignatureError);
        }

        [TestMethod]
        public async Task JwtEncoder_Generate_Basic_HS_Token()
        {
            var encoderParameters = new EncoderParameters();
            var tokenParameters = new TokenParameters()
            {
                TokenAlgorithm = JwtAlgorithm.HS256,
                Payload = await TestDataProvider.GetFileContent("Jwt.BasicPayload.json"),
                Signature = await TestDataProvider.GetFileContent("Jwt.HS.Signature.txt")
            };
            var jwtDecoder = new JwtEncoder();
            TokenResult result = jwtDecoder.GenerateToken(encoderParameters, tokenParameters, DecodingErrorCallBack);

            Assert.IsNotNull(result);
            Assert.IsNull(_validationResult);
            Assert.IsNotNull(result.Token);
        }

        [TestMethod]
        public async Task JwtEncoder_Generate_Basic_HS_Token_With_Base64_Signature()
        {
            string signature = await TestDataProvider.GetFileContent("Jwt.HS.Signature.txt");

            var encoderParameters = new EncoderParameters();
            var tokenParameters = new TokenParameters()
            {
                TokenAlgorithm = JwtAlgorithm.HS256,
                Payload = await TestDataProvider.GetFileContent("Jwt.BasicPayload.json"),
                Signature = Convert.ToBase64String(Encoding.UTF8.GetBytes(signature))
            };
            var jwtDecoder = new JwtEncoder();
            TokenResult result = jwtDecoder.GenerateToken(encoderParameters, tokenParameters, DecodingErrorCallBack);

            Assert.IsNotNull(result);
            Assert.IsNull(_validationResult);
            Assert.IsNotNull(result.Token);
        }

        [TestMethod]
        public async Task JwtEncoder_Generate_Complex_HS_Token()
        {
            var encoderParameters = new EncoderParameters()
            {
                HasAudience = true,
                HasExpiration = true,
                HasIssuer = true,
                HasDefaultTime = true
            };
            var tokenParameters = new TokenParameters()
            {
                TokenAlgorithm = JwtAlgorithm.HS256,
                Payload = await TestDataProvider.GetFileContent("Jwt.BasicPayload.json"),
                Signature = await TestDataProvider.GetFileContent("Jwt.HS.Signature.txt"),
                ValidIssuers = new HashSet<string> { "devtoys" },
                ValidAudiences = new HashSet<string> { "devtoys" },
                ExpirationYear = 1,
                ExpirationMonth = 1,
                ExpirationDay = 1,
                ExpirationHour = 0,
                ExpirationMinute = 0
            };

            var jwtDecoder = new JwtEncoder();
            TokenResult result = jwtDecoder.GenerateToken(encoderParameters, tokenParameters, DecodingErrorCallBack);

            Assert.IsNotNull(result);
            Assert.IsNull(_validationResult);
            Assert.IsNotNull(result.Token);
        }

        #endregion

        #region GenerateRS

        [TestMethod]
        public async Task JwtEncoder_Generate_Basic_RS_Token_Without_Signature_Should_Fail()
        {
            string token = await TestDataProvider.GetFileContent("Jwt.RS.BasicToken.txt");
            var encoderParameters = new EncoderParameters();
            var tokenParameters = new TokenParameters()
            {
                TokenAlgorithm = JwtAlgorithm.RS256,
                Payload = await TestDataProvider.GetFileContent("Jwt.BasicPayload.json"),
            };
            var jwtDecoder = new JwtEncoder();
            TokenResult result = jwtDecoder.GenerateToken(encoderParameters, tokenParameters, DecodingErrorCallBack);

            Assert.IsNull(result);
            Assert.IsNotNull(_validationResult);
            Assert.AreEqual(_validationResult.Message, _localizedStrings.InvalidPrivateKeyError);
        }

        [TestMethod]
        public async Task JwtEncoder_Generate_Basic_RS_Token()
        {
            var encoderParameters = new EncoderParameters();
            var tokenParameters = new TokenParameters()
            {
                TokenAlgorithm = JwtAlgorithm.RS256,
                Payload = await TestDataProvider.GetFileContent("Jwt.BasicPayload.json"),
                PrivateKey = await TestDataProvider.GetFileContent("Jwt.RS.PrivateKey.txt")
            };
            var jwtDecoder = new JwtEncoder();
            TokenResult result = jwtDecoder.GenerateToken(encoderParameters, tokenParameters, DecodingErrorCallBack);

            Assert.IsNotNull(result);
            Assert.IsNull(_validationResult);
            Assert.IsNotNull(result.Token);
        }

        [TestMethod]
        public async Task JwtEncoder_Generate_Complex_RS_Token()
        {
            var encoderParameters = new EncoderParameters()
            {
                HasAudience = true,
                HasExpiration = true,
                HasIssuer = true,
                HasDefaultTime = true
            };
            var tokenParameters = new TokenParameters()
            {
                TokenAlgorithm = JwtAlgorithm.RS256,
                Payload = await TestDataProvider.GetFileContent("Jwt.BasicPayload.json"),
                PrivateKey = await TestDataProvider.GetFileContent("Jwt.RS.PrivateKey.txt"),
                ValidIssuers = new HashSet<string> { "devtoys" },
                ValidAudiences = new HashSet<string> { "devtoys" },
                ExpirationYear = 1,
                ExpirationMonth = 1,
                ExpirationDay = 1,
                ExpirationHour = 0,
                ExpirationMinute = 0
            };

            var jwtDecoder = new JwtEncoder();
            TokenResult result = jwtDecoder.GenerateToken(encoderParameters, tokenParameters, DecodingErrorCallBack);

            Assert.IsNotNull(result);
            Assert.IsNull(_validationResult);
            Assert.IsNotNull(result.Token);
        }

        [TestMethod]
        public async Task JwtEncoder_Generate_Basic_RS_Token_With_Partial_Private_Key()
        {
            string privateKey = await TestDataProvider.GetFileContent("Jwt.RS.PrivateKey.txt");
            privateKey = privateKey.Remove(0, "-----BEGIN PRIVATE KEY-----".Length).Trim();
            privateKey = privateKey.Remove(privateKey.Length - "-----END PRIVATE KEY-----".Length, "-----END PRIVATE KEY-----".Length).Trim();

            var encoderParameters = new EncoderParameters();
            var tokenParameters = new TokenParameters()
            {
                TokenAlgorithm = JwtAlgorithm.PS256,
                Payload = await TestDataProvider.GetFileContent("Jwt.BasicPayload.json"),
                PrivateKey = privateKey
            };
            var jwtDecoder = new JwtEncoder();
            TokenResult result = jwtDecoder.GenerateToken(encoderParameters, tokenParameters, DecodingErrorCallBack);

            Assert.IsNotNull(result);
            Assert.IsNull(_validationResult);
            Assert.IsNotNull(result.Token);
        }

        [TestMethod]
        public async Task JwtEncoder_Generate_Basic_RS_Token_With_Public_Key_Should_Faild()
        {
            var encoderParameters = new EncoderParameters();
            var tokenParameters = new TokenParameters()
            {
                TokenAlgorithm = JwtAlgorithm.PS256,
                Payload = await TestDataProvider.GetFileContent("Jwt.BasicPayload.json"),
                PrivateKey = await TestDataProvider.GetFileContent("Jwt.RS.PublicKey.txt")
            };
            var jwtDecoder = new JwtEncoder();
            TokenResult result = jwtDecoder.GenerateToken(encoderParameters, tokenParameters, DecodingErrorCallBack);

            Assert.IsNull(result);
            Assert.IsNotNull(_validationResult);
            Assert.AreEqual(_validationResult.Severity, InfoBarSeverity.Error);
        }

        #endregion

        #region GeneratePS

        [TestMethod]
        public async Task JwtEncoder_Generate_Basic_PS_Token_Without_Signature_Should_Fail()
        {
            string token = await TestDataProvider.GetFileContent("Jwt.PS.BasicToken.txt");
            var encoderParameters = new EncoderParameters();
            var tokenParameters = new TokenParameters()
            {
                TokenAlgorithm = JwtAlgorithm.PS256,
                Payload = await TestDataProvider.GetFileContent("Jwt.BasicPayload.json"),
            };
            var jwtDecoder = new JwtEncoder();
            TokenResult result = jwtDecoder.GenerateToken(encoderParameters, tokenParameters, DecodingErrorCallBack);

            Assert.IsNull(result);
            Assert.IsNotNull(_validationResult);
            Assert.AreEqual(_validationResult.Message, _localizedStrings.InvalidPrivateKeyError);
        }

        [TestMethod]
        public async Task JwtEncoder_Generate_Basic_PS_Token()
        {
            var encoderParameters = new EncoderParameters();
            var tokenParameters = new TokenParameters()
            {
                TokenAlgorithm = JwtAlgorithm.PS256,
                Payload = await TestDataProvider.GetFileContent("Jwt.BasicPayload.json"),
                PrivateKey = await TestDataProvider.GetFileContent("Jwt.PS.PrivateKey.txt")
            };
            var jwtDecoder = new JwtEncoder();
            TokenResult result = jwtDecoder.GenerateToken(encoderParameters, tokenParameters, DecodingErrorCallBack);

            Assert.IsNotNull(result);
            Assert.IsNull(_validationResult);
            Assert.IsNotNull(result.Token);
        }

        [TestMethod]
        public async Task JwtEncoder_Generate_Complex_PS_Token()
        {
            var encoderParameters = new EncoderParameters()
            {
                HasAudience = true,
                HasExpiration = true,
                HasIssuer = true,
                HasDefaultTime = true
            };
            var tokenParameters = new TokenParameters()
            {
                TokenAlgorithm = JwtAlgorithm.PS256,
                Payload = await TestDataProvider.GetFileContent("Jwt.BasicPayload.json"),
                PrivateKey = await TestDataProvider.GetFileContent("Jwt.PS.PrivateKey.txt"),
                ValidIssuers = new HashSet<string> { "devtoys" },
                ValidAudiences = new HashSet<string> { "devtoys" },
                ExpirationYear = 1,
                ExpirationMonth = 1,
                ExpirationDay = 1,
                ExpirationHour = 0,
                ExpirationMinute = 0
            };

            var jwtDecoder = new JwtEncoder();
            TokenResult result = jwtDecoder.GenerateToken(encoderParameters, tokenParameters, DecodingErrorCallBack);

            Assert.IsNotNull(result);
            Assert.IsNull(_validationResult);
            Assert.IsNotNull(result.Token);
        }

        [TestMethod]
        public async Task JwtEncoder_Generate_Basic_PS_Token_With_Partial_Private_Key()
        {
            string privateKey = await TestDataProvider.GetFileContent("Jwt.PS.PrivateKey.txt");
            privateKey = privateKey.Remove(0, "-----BEGIN PRIVATE KEY-----".Length).Trim();
            privateKey = privateKey.Remove(privateKey.Length - "-----END PRIVATE KEY-----".Length, "-----END PRIVATE KEY-----".Length).Trim();

            var encoderParameters = new EncoderParameters();
            var tokenParameters = new TokenParameters()
            {
                TokenAlgorithm = JwtAlgorithm.PS256,
                Payload = await TestDataProvider.GetFileContent("Jwt.BasicPayload.json"),
                PrivateKey = privateKey
            };
            var jwtDecoder = new JwtEncoder();
            TokenResult result = jwtDecoder.GenerateToken(encoderParameters, tokenParameters, DecodingErrorCallBack);

            Assert.IsNotNull(result);
            Assert.IsNull(_validationResult);
            Assert.IsNotNull(result.Token);
        }

        [TestMethod]
        public async Task JwtEncoder_Generate_Basic_PS_Token_With_Public_Key_Should_Faild()
        {
            var encoderParameters = new EncoderParameters();
            var tokenParameters = new TokenParameters()
            {
                TokenAlgorithm = JwtAlgorithm.PS256,
                Payload = await TestDataProvider.GetFileContent("Jwt.BasicPayload.json"),
                PrivateKey = await TestDataProvider.GetFileContent("Jwt.PS.PublicKey.txt")
            };
            var jwtDecoder = new JwtEncoder();
            TokenResult result = jwtDecoder.GenerateToken(encoderParameters, tokenParameters, DecodingErrorCallBack);

            Assert.IsNull(result);
            Assert.IsNotNull(_validationResult);
            Assert.AreEqual(_validationResult.Severity, InfoBarSeverity.Error);
        }

        #endregion

        #region GenerateES

        [TestMethod]
        public async Task JwtEncoder_Generate_Basic_ES_Token_Without_Signature_Should_Fail()
        {
            string token = await TestDataProvider.GetFileContent("Jwt.ES.BasicToken.txt");
            var encoderParameters = new EncoderParameters();
            var tokenParameters = new TokenParameters()
            {
                TokenAlgorithm = JwtAlgorithm.ES256,
                Payload = await TestDataProvider.GetFileContent("Jwt.BasicPayload.json"),
            };
            var jwtDecoder = new JwtEncoder();
            TokenResult result = jwtDecoder.GenerateToken(encoderParameters, tokenParameters, DecodingErrorCallBack);

            Assert.IsNull(result);
            Assert.IsNotNull(_validationResult);
            Assert.AreEqual(_validationResult.Message, _localizedStrings.InvalidPrivateKeyError);
        }

        [TestMethod]
        public async Task JwtEncoder_Generate_Basic_ES_Token()
        {
            var encoderParameters = new EncoderParameters();
            var tokenParameters = new TokenParameters()
            {
                TokenAlgorithm = JwtAlgorithm.ES256,
                Payload = await TestDataProvider.GetFileContent("Jwt.BasicPayload.json"),
                PrivateKey = await TestDataProvider.GetFileContent("Jwt.ES.PrivateKey.txt")
            };
            var jwtDecoder = new JwtEncoder();
            TokenResult result = jwtDecoder.GenerateToken(encoderParameters, tokenParameters, DecodingErrorCallBack);

            Assert.IsNotNull(result);
            Assert.IsNull(_validationResult);
            Assert.IsNotNull(result.Token);
        }

        [TestMethod]
        public async Task JwtEncoder_Generate_Basic_ES_Token_With_Partial_Private_Key()
        {
            string privateKey = await TestDataProvider.GetFileContent("Jwt.ES.PrivateKey.txt");
            privateKey = privateKey.Remove(0, "-----BEGIN PRIVATE KEY-----".Length).Trim();
            privateKey = privateKey.Remove(privateKey.Length - "-----END PRIVATE KEY-----".Length, "-----END PRIVATE KEY-----".Length).Trim();

            var encoderParameters = new EncoderParameters();
            var tokenParameters = new TokenParameters()
            {
                TokenAlgorithm = JwtAlgorithm.ES256,
                Payload = await TestDataProvider.GetFileContent("Jwt.BasicPayload.json"),
                PrivateKey = privateKey
            };
            var jwtDecoder = new JwtEncoder();
            TokenResult result = jwtDecoder.GenerateToken(encoderParameters, tokenParameters, DecodingErrorCallBack);

            Assert.IsNotNull(result);
            Assert.IsNull(_validationResult);
            Assert.IsNotNull(result.Token);
        }

        [TestMethod]
        public async Task JwtEncoder_Generate_Complex_ES_Token()
        {
            var encoderParameters = new EncoderParameters()
            {
                HasAudience = true,
                HasExpiration = true,
                HasIssuer = true,
                HasDefaultTime = true
            };
            var tokenParameters = new TokenParameters()
            {
                TokenAlgorithm = JwtAlgorithm.ES256,
                Payload = await TestDataProvider.GetFileContent("Jwt.BasicPayload.json"),
                PrivateKey = await TestDataProvider.GetFileContent("Jwt.ES.PrivateKey.txt"),
                ValidIssuers = new HashSet<string> { "devtoys" },
                ValidAudiences = new HashSet<string> { "devtoys" },
                ExpirationYear = 1,
                ExpirationMonth = 1,
                ExpirationDay = 1,
                ExpirationHour = 0,
                ExpirationMinute = 0
            };

            var jwtDecoder = new JwtEncoder();
            TokenResult result = jwtDecoder.GenerateToken(encoderParameters, tokenParameters, DecodingErrorCallBack);

            Assert.IsNotNull(result);
            Assert.IsNull(_validationResult);
            Assert.IsNotNull(result.Token);
        }

        [TestMethod]
        public async Task JwtEncoder_Generate_Basic_ES_Token_With_Public_Key_Should_Faild()
        {
            var encoderParameters = new EncoderParameters();
            var tokenParameters = new TokenParameters()
            {
                TokenAlgorithm = JwtAlgorithm.ES256,
                Payload = await TestDataProvider.GetFileContent("Jwt.BasicPayload.json"),
                PrivateKey = await TestDataProvider.GetFileContent("Jwt.ES.PublicKey.txt")
            };
            var jwtDecoder = new JwtEncoder();
            TokenResult result = jwtDecoder.GenerateToken(encoderParameters, tokenParameters, DecodingErrorCallBack);

            Assert.IsNull(result);
            Assert.IsNotNull(_validationResult);
            Assert.AreEqual(_validationResult.Severity, InfoBarSeverity.Error);
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
