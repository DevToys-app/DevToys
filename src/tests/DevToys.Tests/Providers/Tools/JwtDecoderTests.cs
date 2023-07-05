using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using DevToys.Core.Threading;
using DevToys.Models.JwtDecoderEncoder;
using DevToys.Models;
using DevToys.Shared.Api.Core;
using DevToys.Shared.Core;
using DevToys.ViewModels;
using DevToys.ViewModels.Tools.EncodersDecoders.JwtDecoderEncoder;
using Microsoft.UI.Xaml.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Windows.Storage;
using System.Collections.Generic;

namespace DevToys.Tests.Providers.Tools
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class JwtDecoderTest
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
        public void JwtDecoder_DecodeToken_With_Null_DecoderParameters_Should_Throw_ArgumentNullException()
        {
            var jwtDecoder = new JwtDecoder();
            jwtDecoder.DecodeToken(null, null, DecodingErrorCallBack, out _);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void JwtDecoder_DecodeToken_With_Null_TokenParameters_Should_Throw_ArgumentNullException()
        {
            var jwtDecoder = new JwtDecoder();
            jwtDecoder.DecodeToken(new DecoderParameters(), null, DecodingErrorCallBack, out _);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void JwtDecoder_DecodeToken_With_Null_TokenResultErrorEventArgs_Should_Throw_ArgumentNullException()
        {
            var jwtDecoder = new JwtDecoder();
            jwtDecoder.DecodeToken(new DecoderParameters(), new TokenParameters(), null, out _);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void JwtDecoder_DecodeToken_With_Null_Token_Should_Throw_ArgumentNullException()
        {
            var decodeParameters = new DecoderParameters();
            var tokenParameters = new TokenParameters()
            {
                Token = null
            };


            var jwtDecoder = new JwtDecoder();
            jwtDecoder.DecodeToken(decodeParameters, tokenParameters, DecodingErrorCallBack, out _);
        }

        [TestMethod]
        public void JwtDecoder_DecodeToken_With_Invalid_Token_Should_Fail_With_Error()
        {
            var decodeParameters = new DecoderParameters();
            var tokenParameters = new TokenParameters()
            {
                Token = "eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ"
            };


            var jwtDecoder = new JwtDecoder();
            TokenResult result = jwtDecoder.DecodeToken(decodeParameters, tokenParameters, DecodingErrorCallBack, out _);

            Assert.IsNull(result);
            Assert.IsNotNull(_validationResult);
            Assert.AreEqual(_validationResult.Severity, InfoBarSeverity.Error);
            Assert.IsNotNull(_validationResult.Message);
        }

        [TestMethod]
        public async Task JwtDecoder_DecodeToken_Valid_Token_With_Signature_Validation_And_Without_Signature_Should_Fail_With_ErrorAsync()
        {
            string token = await TestDataProvider.GetFileContent("Jwt.HS.BasicToken.txt");
            string signature = await TestDataProvider.GetFileContent("Jwt.HS.Signature.txt");
            string header = await TestDataProvider.GetFileContent("Jwt.HS.Header.json");
            string payload = await TestDataProvider.GetFileContent("Jwt.BasicPayload.json");
            var decodeParameters = new DecoderParameters()
            {
                ValidateSignature = true,
                ValidateAudience = true
            };
            var tokenParameters = new TokenParameters()
            {
                Token = await TestDataProvider.GetFileContent("Jwt.HS.BasicToken.txt"),
            };

            var jwtDecoder = new JwtDecoder();
            TokenResult result = jwtDecoder.DecodeToken(decodeParameters, tokenParameters, DecodingErrorCallBack, out _);

            Assert.IsNull(result);
            Assert.IsNotNull(_validationResult);
            Assert.AreEqual(_validationResult.Severity, InfoBarSeverity.Error);
            Assert.IsNotNull(_validationResult.Message);
        }

        [TestMethod]
        public async Task JwtDecoder_DecodeToken_Valid_Token_With_Signature_And_Invalid_Issuers_Should_Fail_With_ErrorAsync()
        {
            string token = await TestDataProvider.GetFileContent("Jwt.HS.BasicToken.txt");
            string signature = await TestDataProvider.GetFileContent("Jwt.HS.Signature.txt");
            string header = await TestDataProvider.GetFileContent("Jwt.HS.Header.json");
            string payload = await TestDataProvider.GetFileContent("Jwt.BasicPayload.json");
            var decodeParameters = new DecoderParameters()
            {
                ValidateSignature = true,
                ValidateIssuer = true
            };
            var tokenParameters = new TokenParameters()
            {
                Token = await TestDataProvider.GetFileContent("Jwt.HS.BasicToken.txt"),
                Signature = signature
            };

            var jwtDecoder = new JwtDecoder();
            TokenResult result = jwtDecoder.DecodeToken(decodeParameters, tokenParameters, DecodingErrorCallBack, out _);

            Assert.IsNull(result);
            Assert.IsNotNull(_validationResult);
            Assert.AreEqual(_validationResult.Severity, InfoBarSeverity.Error);
            Assert.AreEqual(_validationResult.Message, _localizedStrings.ValidIssuersError);
        }

        [TestMethod]
        public async Task JwtDecoder_DecodeToken_Valid_Token_With_Signature_And_Invalid_Audiences_Should_Fail_With_ErrorAsync()
        {
            string token = await TestDataProvider.GetFileContent("Jwt.HS.BasicToken.txt");
            string signature = await TestDataProvider.GetFileContent("Jwt.HS.Signature.txt");
            string header = await TestDataProvider.GetFileContent("Jwt.HS.Header.json");
            string payload = await TestDataProvider.GetFileContent("Jwt.BasicPayload.json");
            var decodeParameters = new DecoderParameters()
            {
                ValidateSignature = true,
                ValidateAudience = true
            };
            var tokenParameters = new TokenParameters()
            {
                Token = await TestDataProvider.GetFileContent("Jwt.HS.BasicToken.txt"),
                Signature = signature
            };

            var jwtDecoder = new JwtDecoder();
            TokenResult result = jwtDecoder.DecodeToken(decodeParameters, tokenParameters, DecodingErrorCallBack, out _);

            Assert.IsNull(result);
            Assert.IsNotNull(_validationResult);
            Assert.AreEqual(_validationResult.Severity, InfoBarSeverity.Error);
            Assert.AreEqual(_validationResult.Message, _localizedStrings.ValidAudiencesError);
        }

        [TestMethod]
        public async Task JwtDecoder_DecodeToken_Valid_Token_With_Signature_And_Invalid_LifeTime_Should_Fail_With_ErrorAsync()
        {
            string token = await TestDataProvider.GetFileContent("Jwt.HS.BasicToken.txt");
            string signature = await TestDataProvider.GetFileContent("Jwt.HS.Signature.txt");
            string header = await TestDataProvider.GetFileContent("Jwt.HS.Header.json");
            string payload = await TestDataProvider.GetFileContent("Jwt.ComplexPayload.json");
            var decodeParameters = new DecoderParameters()
            {
                ValidateSignature = true,
                ValidateActor = true,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true
            };
            var tokenParameters = new TokenParameters()
            {
                Token = await TestDataProvider.GetFileContent("Jwt.HS.ComplexToken.txt"),
                Signature = signature,
                ValidIssuers = new HashSet<string> { "devtoys" },
                ValidAudiences = new HashSet<string> { "devtoys" },
            };
            var jwtDecoder = new JwtDecoder();
            TokenResult result = jwtDecoder.DecodeToken(decodeParameters, tokenParameters, DecodingErrorCallBack, out _);

            Assert.IsNull(result);
            Assert.IsNotNull(_validationResult);
        }

        [TestMethod]
        public async Task JwtDecoder_DecodeToken_Valid_Token_With_Signature_And_Invalid_PublicKey_Should_Fail_With_ErrorAsync()
        {
            string token = await TestDataProvider.GetFileContent("Jwt.RS.BasicToken.txt");
            string publicKey = await TestDataProvider.GetFileContent("Jwt.HS.Signature.txt");
            string header = await TestDataProvider.GetFileContent("Jwt.RS.Header.json");
            string payload = await TestDataProvider.GetFileContent("Jwt.BasicPayload.json");
            var decodeParameters = new DecoderParameters()
            {
                ValidateSignature = true,
                ValidateActor = true,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true
            };
            var tokenParameters = new TokenParameters()
            {
                Token = await TestDataProvider.GetFileContent("Jwt.RS.BasicToken.txt"),
                PublicKey = publicKey,
                ValidIssuers = new HashSet<string> { "devtoys" },
                ValidAudiences = new HashSet<string> { "devtoys" },
            };
            var jwtDecoder = new JwtDecoder();
            TokenResult result = jwtDecoder.DecodeToken(decodeParameters, tokenParameters, DecodingErrorCallBack, out _);

            Assert.IsNull(result);
            Assert.IsNotNull(_validationResult);
        }

        #region DecodeHS

        [TestMethod]
        public async Task JwtDecoder_Decode_Basic_HS_Token_Without_Signature_Validation()
        {
            string header = await TestDataProvider.GetFileContent("Jwt.HS.Header.json");
            string payload = await TestDataProvider.GetFileContent("Jwt.BasicPayload.json");
            var decodeParameters = new DecoderParameters();
            var tokenParameters = new TokenParameters()
            {
                Token = await TestDataProvider.GetFileContent("Jwt.HS.BasicToken.txt")
            };


            var jwtDecoder = new JwtDecoder();
            TokenResult result = jwtDecoder.DecodeToken(decodeParameters, tokenParameters, DecodingErrorCallBack, out JwtAlgorithm? algorithm);

            Assert.IsNotNull(result);
            Assert.IsNull(_validationResult);
            Assert.AreEqual(result.Header, header);
            Assert.AreEqual(result.Payload, payload);
            Assert.AreEqual(algorithm, JwtAlgorithm.HS256);
        }

        [TestMethod]
        public async Task JwtDecoder_Decode_Basic_HS_Token_With_Signature_Validation()
        {
            string token = await TestDataProvider.GetFileContent("Jwt.HS.BasicToken.txt");
            string signature = await TestDataProvider.GetFileContent("Jwt.HS.Signature.txt");
            string payload = await TestDataProvider.GetFileContent("Jwt.BasicPayload.json");
            var decodeParameters = new DecoderParameters()
            {
                ValidateSignature = true
            };
            var tokenParameters = new TokenParameters()
            {
                Token = await TestDataProvider.GetFileContent("Jwt.HS.BasicToken.txt"),
                Signature = signature
            };
            var jwtDecoder = new JwtDecoder();
            TokenResult result = jwtDecoder.DecodeToken(decodeParameters, tokenParameters, DecodingErrorCallBack, out JwtAlgorithm? jwtAlgorithm);

            Assert.IsNotNull(result);
            Assert.IsNull(_validationResult);
            Assert.AreEqual(result.Payload, payload);
            Assert.AreEqual(result.Signature, signature);
            Assert.AreEqual(jwtAlgorithm, JwtAlgorithm.HS256);
        }

        [TestMethod]
        public async Task JwtDecoder_Decode_Complex_HS_Token_Without_Signature_Validation()
        {
            string header = await TestDataProvider.GetFileContent("Jwt.HS.Header.json");
            string payload = await TestDataProvider.GetFileContent("Jwt.ComplexPayload.json");
            var decodeParameters = new DecoderParameters();
            var tokenParameters = new TokenParameters()
            {
                Token = await TestDataProvider.GetFileContent("Jwt.HS.ComplexToken.txt")
            };


            var jwtDecoder = new JwtDecoder();
            TokenResult result = jwtDecoder.DecodeToken(decodeParameters, tokenParameters, DecodingErrorCallBack, out JwtAlgorithm? algorithm);

            Assert.IsNotNull(result);
            Assert.IsNull(_validationResult);
            Assert.AreEqual(result.Header, header);
            Assert.AreEqual(result.Payload, payload);
            Assert.AreEqual(algorithm, JwtAlgorithm.HS256);
        }

        [TestMethod]
        public async Task JwtDecoder_Decode_Complex_HS_Token_With_Signature_Validation()
        {
            string token = await TestDataProvider.GetFileContent("Jwt.HS.BasicToken.txt");
            string signature = await TestDataProvider.GetFileContent("Jwt.HS.Signature.txt");
            string header = await TestDataProvider.GetFileContent("Jwt.HS.Header.json");
            string payload = await TestDataProvider.GetFileContent("Jwt.ComplexPayload.json");
            var decodeParameters = new DecoderParameters()
            {
                ValidateSignature = true,
                ValidateActor = true,
                ValidateIssuer = true,
                ValidateAudience = true
            };
            var tokenParameters = new TokenParameters()
            {
                Token = await TestDataProvider.GetFileContent("Jwt.HS.ComplexToken.txt"),
                Signature = signature,
                ValidIssuers = new HashSet<string> { "devtoys" },
                ValidAudiences = new HashSet<string> { "devtoys" },
            };
            var jwtDecoder = new JwtDecoder();
            TokenResult result = jwtDecoder.DecodeToken(decodeParameters, tokenParameters, DecodingErrorCallBack, out _);

            Assert.IsNotNull(result);
            Assert.IsNull(_validationResult);
            Assert.AreEqual(result.Header, header);
            Assert.AreEqual(result.Payload, payload);
            Assert.AreEqual(result.Signature, signature);
        }

        [TestMethod]
        public async Task JwtDecoder_Decode_Complex_HS_Token_Validate_Actor_Issuer_Audience()
        {
            string header = await TestDataProvider.GetFileContent("Jwt.HS.Header.json");
            string payload = await TestDataProvider.GetFileContent("Jwt.ComplexPayload.json");
            var decodeParameters = new DecoderParameters
            {
                ValidateSignature = true,
                ValidateIssuerSigningKey = false,
                ValidateActor = true,
                ValidateIssuer = true,
                ValidateAudience = true
            };
            var tokenParameters = new TokenParameters
            {
                Token = await TestDataProvider.GetFileContent("Jwt.HS.ComplexToken.txt"),
                ValidIssuers = new HashSet<string> { "devtoys" },
                ValidAudiences = new HashSet<string> { "devtoys" },
            };
            var jwtDecoder = new JwtDecoder();
            TokenResult result = jwtDecoder.DecodeToken(decodeParameters, tokenParameters, DecodingErrorCallBack, out JwtAlgorithm? algorithm);

            Assert.IsNotNull(result);
            Assert.IsNull(_validationResult);
            Assert.AreEqual(result.Header, header);
            Assert.AreEqual(result.Payload, payload);
            Assert.AreEqual(algorithm, JwtAlgorithm.HS256);
        }

        #endregion

        #region DecodeRS

        [TestMethod]
        public async Task JwtDecoder_Decode_Basic_RS_Token_Without_Signature_Validation()
        {
            string header = await TestDataProvider.GetFileContent("Jwt.RS.Header.json");
            string payload = await TestDataProvider.GetFileContent("Jwt.BasicPayload.json");
            var decodeParameters = new DecoderParameters();
            var tokenParameters = new TokenParameters()
            {
                Token = await TestDataProvider.GetFileContent("Jwt.RS.BasicToken.txt")
            };


            var jwtDecoder = new JwtDecoder();
            TokenResult result = jwtDecoder.DecodeToken(decodeParameters, tokenParameters, DecodingErrorCallBack, out JwtAlgorithm? algorithm);

            Assert.IsNotNull(result);
            Assert.IsNull(_validationResult);
            Assert.AreEqual(result.Header, header);
            Assert.AreEqual(result.Payload, payload);
            Assert.AreEqual(algorithm, JwtAlgorithm.RS256);
        }

        [TestMethod]
        public async Task JwtDecoder_Decode_Basic_RS_Token_With_Signature_Validation()
        {
            string token = await TestDataProvider.GetFileContent("Jwt.RS.BasicToken.txt");
            string publicKey = await TestDataProvider.GetFileContent("Jwt.RS.PublicKey.txt");
            string payload = await TestDataProvider.GetFileContent("Jwt.BasicPayload.json");
            var decodeParameters = new DecoderParameters()
            {
                ValidateSignature = true
            };
            var tokenParameters = new TokenParameters()
            {
                Token = await TestDataProvider.GetFileContent("Jwt.RS.BasicToken.txt"),
                PublicKey = publicKey,
            };
            var jwtDecoder = new JwtDecoder();
            TokenResult result = jwtDecoder.DecodeToken(decodeParameters, tokenParameters, DecodingErrorCallBack, out JwtAlgorithm? algorithm);

            Assert.IsNotNull(result);
            Assert.IsNull(_validationResult);
            Assert.AreEqual(result.Payload, payload);
            Assert.AreEqual(result.PublicKey, publicKey);
            Assert.AreEqual(algorithm, JwtAlgorithm.RS256);
        }

        [TestMethod]
        public async Task JwtDecoder_Decode_Complex_RS_Token_Without_Signature_Validation()
        {
            string payload = await TestDataProvider.GetFileContent("Jwt.ComplexPayload.json");
            var decodeParameters = new DecoderParameters();
            var tokenParameters = new TokenParameters()
            {
                Token = await TestDataProvider.GetFileContent("Jwt.RS.ComplexToken.txt")
            };


            var jwtDecoder = new JwtDecoder();
            TokenResult result = jwtDecoder.DecodeToken(decodeParameters, tokenParameters, DecodingErrorCallBack, out JwtAlgorithm? algorithm);

            Assert.IsNotNull(result);
            Assert.IsNull(_validationResult);
            Assert.AreEqual(result.Payload, payload);
            Assert.AreEqual(algorithm, JwtAlgorithm.RS256);
        }

        [TestMethod]
        public async Task JwtDecoder_Decode_Complex_RS_Token_With_Signature_Validation()
        {
            string token = await TestDataProvider.GetFileContent("Jwt.RS.BasicToken.txt");
            string publicKey = await TestDataProvider.GetFileContent("Jwt.RS.PublicKey.txt");
            string header = await TestDataProvider.GetFileContent("Jwt.RS.Header.json");
            string payload = await TestDataProvider.GetFileContent("Jwt.ComplexPayload.json");
            var decodeParameters = new DecoderParameters()
            {
                ValidateSignature = true,
                ValidateActor = true,
                ValidateIssuer = true,
                ValidateAudience = true
            };
            var tokenParameters = new TokenParameters()
            {
                Token = await TestDataProvider.GetFileContent("Jwt.RS.ComplexToken.txt"),
                PublicKey = publicKey,
                ValidIssuers = new HashSet<string> { "devtoys" },
                ValidAudiences = new HashSet<string> { "devtoys" },
            };
            var jwtDecoder = new JwtDecoder();
            TokenResult result = jwtDecoder.DecodeToken(decodeParameters, tokenParameters, DecodingErrorCallBack, out JwtAlgorithm? algorithm);

            Assert.IsNotNull(result);
            Assert.IsNull(_validationResult);
            Assert.AreEqual(result.Header, header);
            Assert.AreEqual(result.Payload, payload);
            Assert.AreEqual(result.PublicKey, publicKey);
            Assert.AreEqual(algorithm, JwtAlgorithm.RS256);
        }

        [TestMethod]
        public async Task JwtDecoder_Decode_Complex_RS_Token_Validate_Actor_Issuer_Audience()
        {
            string header = await TestDataProvider.GetFileContent("Jwt.RS.Header.json");
            string payload = await TestDataProvider.GetFileContent("Jwt.ComplexPayload.json");
            var decodeParameters = new DecoderParameters
            {
                ValidateSignature = true,
                ValidateIssuerSigningKey = false,
                ValidateActor = true,
                ValidateIssuer = true,
                ValidateAudience = true
            };
            var tokenParameters = new TokenParameters
            {
                Token = await TestDataProvider.GetFileContent("Jwt.RS.ComplexToken.txt"),
                ValidIssuers = new HashSet<string> { "devtoys" },
                ValidAudiences = new HashSet<string> { "devtoys" },
            };
            var jwtDecoder = new JwtDecoder();
            TokenResult result = jwtDecoder.DecodeToken(decodeParameters, tokenParameters, DecodingErrorCallBack, out JwtAlgorithm? algorithm);

            Assert.IsNotNull(result);
            Assert.IsNull(_validationResult);
            Assert.AreEqual(result.Header, header);
            Assert.AreEqual(result.Payload, payload);
            Assert.AreEqual(algorithm, JwtAlgorithm.RS256);
        }

        [TestMethod]
        public async Task JwtDecoder_Decode_Basic_RS_Token_With_Certificate_Signature_Validation()
        {
            // Generated with:  openssl req -x509 -key test-devtoys.key -out test-devtoys.pem -sha256 -days 3650 -nodes
            string publicKey = await TestDataProvider.GetFileContent("Jwt.RS.Certificate.txt");
            string payload = await TestDataProvider.GetFileContent("Jwt.BasicPayload.json");
            var decodeParameters = new DecoderParameters
            {
                ValidateSignature = true
            };
            var tokenParameters = new TokenParameters
            {
                TokenAlgorithm = JwtAlgorithm.RS384,
                // Generated using BasicPayload.json and RsaPrivateKey.txt
                Token = await TestDataProvider.GetFileContent("Jwt.RS.CertBasicToken.txt"),
                PublicKey = publicKey,
            };
            var jwtDecoder = new JwtDecoder();
            TokenResult result = jwtDecoder.DecodeToken(decodeParameters, tokenParameters, DecodingErrorCallBack, out _);

            Assert.IsNotNull(result);
            Assert.IsNull(_validationResult);
            Assert.AreEqual(result.Payload, payload);
            Assert.AreEqual(result.PublicKey, publicKey);
        }

        #endregion

        #region DecodePS

        [TestMethod]
        public async Task JwtDecoder_Decode_Basic_PS_Token_Without_Signature_Validation()
        {
            string header = await TestDataProvider.GetFileContent("Jwt.PS.Header.json");
            string payload = await TestDataProvider.GetFileContent("Jwt.BasicPayload.json");
            var decodeParameters = new DecoderParameters();
            var tokenParameters = new TokenParameters()
            {
                Token = await TestDataProvider.GetFileContent("Jwt.PS.BasicToken.txt")
            };


            var jwtDecoder = new JwtDecoder();
            TokenResult result = jwtDecoder.DecodeToken(decodeParameters, tokenParameters, DecodingErrorCallBack, out JwtAlgorithm? algorithm);

            Assert.IsNotNull(result);
            Assert.IsNull(_validationResult);
            Assert.AreEqual(result.Header, header);
            Assert.AreEqual(result.Payload, payload);
            Assert.AreEqual(algorithm, JwtAlgorithm.PS256);
        }

        [TestMethod]
        public async Task JwtDecoder_Decode_Basic_PS_Token_With_Signature_Validation()
        {
            string token = await TestDataProvider.GetFileContent("Jwt.PS.BasicToken.txt");
            string publicKey = await TestDataProvider.GetFileContent("Jwt.PS.PublicKey.txt");
            string payload = await TestDataProvider.GetFileContent("Jwt.BasicPayload.json");
            var decodeParameters = new DecoderParameters()
            {
                ValidateSignature = true
            };
            var tokenParameters = new TokenParameters()
            {
                Token = await TestDataProvider.GetFileContent("Jwt.PS.BasicToken.txt"),
                PublicKey = publicKey,
            };
            var jwtDecoder = new JwtDecoder();
            TokenResult result = jwtDecoder.DecodeToken(decodeParameters, tokenParameters, DecodingErrorCallBack, out JwtAlgorithm? algorithm);

            Assert.IsNotNull(result);
            Assert.IsNull(_validationResult);
            Assert.AreEqual(result.Payload, payload);
            Assert.AreEqual(result.PublicKey, publicKey);
            Assert.AreEqual(algorithm, JwtAlgorithm.PS256);
        }

        [TestMethod]
        public async Task JwtDecoder_Decode_Complex_PS_Token_Without_Signature_Validation()
        {
            string payload = await TestDataProvider.GetFileContent("Jwt.ComplexPayload.json");
            var decodeParameters = new DecoderParameters();
            var tokenParameters = new TokenParameters()
            {
                Token = await TestDataProvider.GetFileContent("Jwt.PS.ComplexToken.txt")
            };


            var jwtDecoder = new JwtDecoder();
            TokenResult result = jwtDecoder.DecodeToken(decodeParameters, tokenParameters, DecodingErrorCallBack, out JwtAlgorithm? algorithm);

            Assert.IsNotNull(result);
            Assert.IsNull(_validationResult);
            Assert.AreEqual(result.Payload, payload);
            Assert.AreEqual(algorithm, JwtAlgorithm.PS256);
        }

        [TestMethod]
        public async Task JwtDecoder_Decode_Complex_PS_Token_With_Signature_Validation()
        {
            string token = await TestDataProvider.GetFileContent("Jwt.PS.BasicToken.txt");
            string publicKey = await TestDataProvider.GetFileContent("Jwt.PS.PublicKey.txt");
            string header = await TestDataProvider.GetFileContent("Jwt.PS.Header.json");
            string payload = await TestDataProvider.GetFileContent("Jwt.ComplexPayload.json");
            var decodeParameters = new DecoderParameters()
            {
                ValidateSignature = true,
                ValidateActor = true,
                ValidateIssuer = true,
                ValidateAudience = true
            };
            var tokenParameters = new TokenParameters()
            {
                Token = await TestDataProvider.GetFileContent("Jwt.PS.ComplexToken.txt"),
                PublicKey = publicKey,
                ValidIssuers = new HashSet<string> { "devtoys" },
                ValidAudiences = new HashSet<string> { "devtoys" },
            };
            var jwtDecoder = new JwtDecoder();
            TokenResult result = jwtDecoder.DecodeToken(decodeParameters, tokenParameters, DecodingErrorCallBack, out JwtAlgorithm? algorithm);

            Assert.IsNotNull(result);
            Assert.IsNull(_validationResult);
            Assert.AreEqual(result.Header, header);
            Assert.AreEqual(result.Payload, payload);
            Assert.AreEqual(result.PublicKey, publicKey);
            Assert.AreEqual(algorithm, JwtAlgorithm.PS256);
        }

        [TestMethod]
        public async Task JwtDecoder_Decode_Complex_PS_Token_Validate_Actor_Issuer_Audience()
        {
            string header = await TestDataProvider.GetFileContent("Jwt.PS.Header.json");
            string payload = await TestDataProvider.GetFileContent("Jwt.ComplexPayload.json");
            var decodeParameters = new DecoderParameters
            {
                ValidateSignature = true,
                ValidateIssuerSigningKey = false,
                ValidateActor = true,
                ValidateIssuer = true,
                ValidateAudience = true
            };
            var tokenParameters = new TokenParameters
            {
                Token = await TestDataProvider.GetFileContent("Jwt.PS.ComplexToken.txt"),
                ValidIssuers = new HashSet<string> { "devtoys" },
                ValidAudiences = new HashSet<string> { "devtoys" },
            };
            var jwtDecoder = new JwtDecoder();
            TokenResult result = jwtDecoder.DecodeToken(decodeParameters, tokenParameters, DecodingErrorCallBack, out JwtAlgorithm? algorithm);

            Assert.IsNotNull(result);
            Assert.IsNull(_validationResult);
            Assert.AreEqual(result.Header, header);
            Assert.AreEqual(result.Payload, payload);
            Assert.AreEqual(algorithm, JwtAlgorithm.PS256);
        }

        #endregion

        #region DecodeES

        [TestMethod]
        public async Task JwtDecoder_Decode_Basic_ES_Token_Without_Signature_Validation()
        {
            string header = await TestDataProvider.GetFileContent("Jwt.ES.Header.json");
            string payload = await TestDataProvider.GetFileContent("Jwt.BasicPayload.json");
            var decodeParameters = new DecoderParameters();
            var tokenParameters = new TokenParameters()
            {
                Token = await TestDataProvider.GetFileContent("Jwt.ES.BasicToken.txt")
            };


            var jwtDecoder = new JwtDecoder();
            TokenResult result = jwtDecoder.DecodeToken(decodeParameters, tokenParameters, DecodingErrorCallBack, out JwtAlgorithm? algorithm);

            Assert.IsNotNull(result);
            Assert.IsNull(_validationResult);
            Assert.AreEqual(result.Header, header);
            Assert.AreEqual(result.Payload, payload);
            Assert.AreEqual(algorithm, JwtAlgorithm.ES256);
        }

        [TestMethod]
        public async Task JwtDecoder_Decode_Basic_ES_Token_With_Signature_Validation()
        {
            string token = await TestDataProvider.GetFileContent("Jwt.ES.BasicToken.txt");
            string publicKey = await TestDataProvider.GetFileContent("Jwt.ES.PublicKey.txt");
            string payload = await TestDataProvider.GetFileContent("Jwt.BasicPayload.json");
            var decodeParameters = new DecoderParameters()
            {
                ValidateSignature = true
            };
            var tokenParameters = new TokenParameters()
            {
                Token = await TestDataProvider.GetFileContent("Jwt.ES.BasicToken.txt"),
                PublicKey = publicKey,
            };
            var jwtDecoder = new JwtDecoder();
            TokenResult result = jwtDecoder.DecodeToken(decodeParameters, tokenParameters, DecodingErrorCallBack, out JwtAlgorithm? algorithm);

            Assert.IsNotNull(result);
            Assert.IsNull(_validationResult);
            Assert.AreEqual(result.Payload, payload);
            Assert.AreEqual(result.PublicKey, publicKey);
            Assert.AreEqual(algorithm, JwtAlgorithm.ES256);
        }

        [TestMethod]
        public async Task JwtDecoder_Decode_Complex_ES_Token_Without_Signature_Validation()
        {
            string payload = await TestDataProvider.GetFileContent("Jwt.ComplexPayload.json");
            var decodeParameters = new DecoderParameters();
            var tokenParameters = new TokenParameters()
            {
                Token = await TestDataProvider.GetFileContent("Jwt.ES.ComplexToken.txt")
            };


            var jwtDecoder = new JwtDecoder();
            TokenResult result = jwtDecoder.DecodeToken(decodeParameters, tokenParameters, DecodingErrorCallBack, out JwtAlgorithm? algorithm);

            Assert.IsNotNull(result);
            Assert.IsNull(_validationResult);
            Assert.AreEqual(result.Payload, payload);
            Assert.AreEqual(algorithm, JwtAlgorithm.ES256);
        }

        [TestMethod]
        public async Task JwtDecoder_Decode_Complex_ES_Token_With_Signature_Validation()
        {
            string token = await TestDataProvider.GetFileContent("Jwt.ES.BasicToken.txt");
            string publicKey = await TestDataProvider.GetFileContent("Jwt.ES.PublicKey.txt");
            string header = await TestDataProvider.GetFileContent("Jwt.ES.Header.json");
            string payload = await TestDataProvider.GetFileContent("Jwt.ComplexPayload.json");
            var decodeParameters = new DecoderParameters()
            {
                ValidateSignature = true,
                ValidateActor = true,
                ValidateIssuer = true,
                ValidateAudience = true
            };
            var tokenParameters = new TokenParameters()
            {
                Token = await TestDataProvider.GetFileContent("Jwt.ES.ComplexToken.txt"),
                PublicKey = publicKey,
                ValidIssuers = new HashSet<string> { "devtoys" },
                ValidAudiences = new HashSet<string> { "devtoys" },
            };
            var jwtDecoder = new JwtDecoder();
            TokenResult result = jwtDecoder.DecodeToken(decodeParameters, tokenParameters, DecodingErrorCallBack, out JwtAlgorithm? algorithm);

            Assert.IsNotNull(result);
            Assert.IsNull(_validationResult);
            Assert.AreEqual(result.Header, header);
            Assert.AreEqual(result.Payload, payload);
            Assert.AreEqual(result.PublicKey, publicKey);
            Assert.AreEqual(algorithm, JwtAlgorithm.ES256);
        }

        [TestMethod]
        public async Task JwtDecoder_Decode_Complex_ES_Token_Validate_Actor_Issuer_Audience()
        {
            string header = await TestDataProvider.GetFileContent("Jwt.ES.Header.json");
            string payload = await TestDataProvider.GetFileContent("Jwt.ComplexPayload.json");
            var decodeParameters = new DecoderParameters
            {
                ValidateSignature = true,
                ValidateIssuerSigningKey = false,
                ValidateActor = true,
                ValidateIssuer = true,
                ValidateAudience = true
            };
            var tokenParameters = new TokenParameters
            {
                Token = await TestDataProvider.GetFileContent("Jwt.ES.ComplexToken.txt"),
                ValidIssuers = new HashSet<string> { "devtoys" },
                ValidAudiences = new HashSet<string> { "devtoys" },
            };
            var jwtDecoder = new JwtDecoder();
            TokenResult result = jwtDecoder.DecodeToken(decodeParameters, tokenParameters, DecodingErrorCallBack, out JwtAlgorithm? algorithm);

            Assert.IsNotNull(result);
            Assert.IsNull(_validationResult);
            Assert.AreEqual(result.Header, header);
            Assert.AreEqual(result.Payload, payload);
            Assert.AreEqual(algorithm, JwtAlgorithm.ES256);
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
