#nullable enable

using System;
using System.Composition;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using DevToys.Helpers;
using DevToys.Helpers.JsonYaml;
using DevToys.Models;
using DevToys.Models.JwtDecoderEncoder;
using DevToys.Shared.Core;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.UI.Xaml.Controls;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.X509;

namespace DevToys.ViewModels.Tools.EncodersDecoders.JwtDecoderEncoder
{
    [Export(typeof(JwtDecoder))]
    [Shared]
    public class JwtDecoder
    {
        private const string PublicKeyStart = "-----BEGIN PUBLIC KEY-----";
        private const string PublicKeyEnd = "-----END PUBLIC KEY-----";
        private const string CertificateStart = "-----BEGIN CERTIFICATE-----";
        private const string CertificateEnd = "-----END CERTIFICATE-----";
        private Action<TokenResultErrorEventArgs>? _decodingErrorCallBack;
        private JwtDecoderEncoderStrings _localizedStrings => LanguageManager.Instance.JwtDecoderEncoder;

        public TokenResult? DecodeToken(
                DecoderParameters decodeParameters,
                TokenParameters tokenParameters,
                Action<TokenResultErrorEventArgs> decodingErrorCallBack,
                out JwtAlgorithm? jwtAlgorithm)
        {
            Arguments.NotNull(decodeParameters, nameof(decodeParameters));
            Arguments.NotNull(tokenParameters, nameof(tokenParameters));
            _decodingErrorCallBack = Arguments.NotNull(decodingErrorCallBack, nameof(decodingErrorCallBack));
            Arguments.NotNullOrWhiteSpace(tokenParameters.Token, nameof(tokenParameters.Token));
            jwtAlgorithm = null;

            var tokenResult = new TokenResult();

            try
            {
                IdentityModelEventSource.ShowPII = true;
                var handler = new JwtSecurityTokenHandler();
                JwtSecurityToken jwtSecurityToken = handler.ReadJwtToken(tokenParameters.Token);
                tokenResult.Header = JsonHelper.Format(jwtSecurityToken.Header.SerializeToJson(), Indentation.TwoSpaces, false);
                tokenResult.Payload = JsonHelper.Format(jwtSecurityToken.Payload.SerializeToJson(), Indentation.TwoSpaces, false);
                jwtAlgorithm = tokenResult.TokenAlgorithm = tokenParameters.TokenAlgorithm =
                    Enum.TryParse(jwtSecurityToken.SignatureAlgorithm, out JwtAlgorithm parsedAlgorithm)
                        ? parsedAlgorithm
                        : tokenParameters.TokenAlgorithm;

                if (decodeParameters.ValidateSignature)
                {
                    bool signatureValid = ValidateTokenSignature(handler, decodeParameters, tokenParameters, tokenResult);
                    if (!signatureValid)
                    {
                        return null;
                    }
                }

                tokenResult.Claims = jwtSecurityToken.Claims.Select(c => new JwtClaim(c));
            }
            catch (Exception exception)
            {
                RaiseError(exception.Message);
                return null;
            }

            return tokenResult;
        }

        /// <summary>
        /// Validate the token using the Signing Credentials 
        /// </summary>
        private bool ValidateTokenSignature(
            JwtSecurityTokenHandler handler,
            DecoderParameters decodeParameters,
            TokenParameters tokenParameters,
            TokenResult tokenResult)
        {
            var validationParameters = new TokenValidationParameters
            {
                ValidateActor = decodeParameters.ValidateActor,
                ValidateLifetime = decodeParameters.ValidateLifetime,
                ValidateIssuer = decodeParameters.ValidateIssuer,
                ValidateAudience = decodeParameters.ValidateAudience
            };
            
            if (decodeParameters.ValidateIssuerSigningKey)
            {
                SigningCredentials? signingCredentials = GetValidationCredentials(tokenParameters);
                validationParameters.ValidateIssuerSigningKey = decodeParameters.ValidateIssuerSigningKey;
                validationParameters.IssuerSigningKey = signingCredentials.Key;
                validationParameters.TryAllIssuerSigningKeys = true;
            }
            else
            {
                // Create a custom signature validator that does nothing so it always passes in this mode
                validationParameters.SignatureValidator = (token, _) => new JwtSecurityToken(token);
            }

            /// check if the token issuers are part of the user provided issuers
            if (decodeParameters.ValidateIssuer)
            {
                if (tokenParameters.ValidIssuers.Count == 0)
                {
                    RaiseError(_localizedStrings.ValidIssuersError);
                    return false;
                }
                validationParameters.ValidIssuers = tokenParameters.ValidIssuers;
            }

            /// check if the token audience are part of the user provided audiences
            if (decodeParameters.ValidateAudience)
            {
                if (tokenParameters.ValidAudiences.Count == 0)
                {
                    RaiseError(_localizedStrings.ValidAudiencesError);
                    return false;
                }
                validationParameters.ValidAudiences = tokenParameters.ValidAudiences;
            }

            try
            {
                handler.ValidateToken(tokenParameters.Token, validationParameters, out _);
                tokenResult.Signature = tokenParameters.Signature;
                tokenResult.PublicKey = tokenParameters.PublicKey;
                return true;
            }
            catch (Exception exception)
            {
                RaiseError(exception.Message);
            }
            return false;
        }

        /// <summary>
        /// Get the Signing Credentials depending on the token Algorithm
        /// </summary>
        private SigningCredentials GetValidationCredentials(
            TokenParameters tokenParameters)
        {
            SigningCredentials? signingCredentials = tokenParameters.TokenAlgorithm switch
            {
                JwtAlgorithm.ES512 => new SigningCredentials(GetECDsaValidationKey(tokenParameters), SecurityAlgorithms.EcdsaSha512Signature),
                JwtAlgorithm.ES384 => new SigningCredentials(GetECDsaValidationKey(tokenParameters), SecurityAlgorithms.EcdsaSha384Signature),
                JwtAlgorithm.ES256 => new SigningCredentials(GetECDsaValidationKey(tokenParameters), SecurityAlgorithms.EcdsaSha256Signature),
                JwtAlgorithm.PS512 => new SigningCredentials(GetRsaShaValidationKey(tokenParameters), SecurityAlgorithms.RsaSsaPssSha512),
                JwtAlgorithm.PS384 => new SigningCredentials(GetRsaShaValidationKey(tokenParameters), SecurityAlgorithms.RsaSsaPssSha384),
                JwtAlgorithm.PS256 => new SigningCredentials(GetRsaShaValidationKey(tokenParameters), SecurityAlgorithms.RsaSsaPssSha256),
                JwtAlgorithm.RS512 => new SigningCredentials(GetRsaShaValidationKey(tokenParameters), SecurityAlgorithms.RsaSha512Signature),
                JwtAlgorithm.RS384 => new SigningCredentials(GetRsaShaValidationKey(tokenParameters), SecurityAlgorithms.RsaSha384Signature),
                JwtAlgorithm.RS256 => new SigningCredentials(GetRsaShaValidationKey(tokenParameters), SecurityAlgorithms.RsaSha256Signature),
                JwtAlgorithm.HS512 => new SigningCredentials(GetHmacShaValidationKey(tokenParameters), SecurityAlgorithms.HmacSha512Signature),
                JwtAlgorithm.HS384 => new SigningCredentials(GetHmacShaValidationKey(tokenParameters), SecurityAlgorithms.HmacSha384Signature),
                _ => new SigningCredentials(GetHmacShaValidationKey(tokenParameters), SecurityAlgorithms.HmacSha256Signature),// HS256
            };
            return signingCredentials;
        }

        /// <summary>
        /// Generate a Symmetric Security Key using the token signature (base 64 or not)
        /// </summary>
        /// <param name="tokenParameters">Token parameters with the token signature</param>
        private SymmetricSecurityKey? GetHmacShaValidationKey(TokenParameters tokenParameters)
        {
            if (string.IsNullOrWhiteSpace(tokenParameters.Signature))
            {
                return null;
            }

            byte[]? signatureByte;
            if (Base64Helper.IsBase64DataStrict(tokenParameters.Signature))
            {
                signatureByte = Convert.FromBase64String(tokenParameters.Signature);
            }
            else
            {
                signatureByte = Encoding.UTF8.GetBytes(tokenParameters.Signature);
            }
            byte[] byteKey = tokenParameters.TokenAlgorithm switch
            {
                JwtAlgorithm.HS512 => new HMACSHA512(signatureByte).Key,
                JwtAlgorithm.HS384 => new HMACSHA384(signatureByte).Key,
                _ => new HMACSHA256(signatureByte).Key,// HS256
            };
            return new SymmetricSecurityKey(byteKey);
        }

        /// <summary>
        /// Generate a RSA Security Key using the token signing public key
        /// </summary>
        /// <param name="tokenParameters">Token parameters with the token signing public key</param>
        private RsaSecurityKey? GetRsaShaValidationKey(TokenParameters tokenParameters)
        {
            try
            {
                AsymmetricKeyParameter? asymmetricKeyParameter = GetPublicAsymmetricKeyParameter(tokenParameters);
                if (asymmetricKeyParameter is null)
                {
                    RaiseError(_localizedStrings.InvalidPublicKeyError);
                    return null;
                }

                var publicKey = (RsaKeyParameters)asymmetricKeyParameter;
                if (publicKey.IsPrivate)
                {
                    RaiseError(_localizedStrings.PublicKeyIsPrivateKeyError);
                    return null;
                }

                RSAParameters rsaParameters = new()
                {
                    Modulus = publicKey.Modulus.ToByteArrayUnsigned(),
                    Exponent = publicKey.Exponent.ToByteArrayUnsigned()
                };
                return new RsaSecurityKey(rsaParameters);
            }
            catch (Exception exception)
            {
                RaiseError($"{_localizedStrings.InvalidPublicKeyError}: '{exception.Message}'");
                return null;
            }
        }

        /// <summary>
        /// Generate a ECDsa Security Key using the token signing public key
        /// </summary>
        /// <param name="tokenParameters">Token parameters with the token signing public key</param>
        private ECDsaSecurityKey? GetECDsaValidationKey(TokenParameters tokenParameters)
        {
            try
            {
                AsymmetricKeyParameter? asymmetricKeyParameter = GetPublicAsymmetricKeyParameter(tokenParameters);
                if (asymmetricKeyParameter is null)
                {
                    RaiseError(_localizedStrings.InvalidPublicKeyError);
                    return null;
                }

                var publicKey = (ECPublicKeyParameters)asymmetricKeyParameter;
                if (publicKey.IsPrivate)
                {
                    RaiseError(_localizedStrings.PublicKeyIsPrivateKeyError);
                    return null;
                }

                ECParameters ecParameters = new()
                {
                    Curve = ECCurve.NamedCurves.nistP256,
                    Q = new()
                    {
                        X = publicKey.Q.AffineXCoord.GetEncoded(),
                        Y = publicKey.Q.AffineYCoord.GetEncoded()
                    }
                };
                var ecdSa = ECDsa.Create(ecParameters);
                return new ECDsaSecurityKey(ecdSa);
            }
            catch (Exception exception)
            {
                RaiseError($"{_localizedStrings.InvalidPublicKeyError}: '{exception.Message}'");
                return null;
            }
        }

        /// <summary>
        /// Generate the Asymmetric Security Key using the token signing public key
        /// </summary>
        /// <param name="tokenParameters">Token parameters with the token signing public key</param>
        private AsymmetricKeyParameter? GetPublicAsymmetricKeyParameter(TokenParameters tokenParameters)
        {
            if (string.IsNullOrWhiteSpace(tokenParameters.PublicKey))
            {
                RaiseError(_localizedStrings.InvalidPublicKeyError);
                return null;
            }
            var publicKeyStringBuilder = new StringBuilder(tokenParameters.PublicKey!.Trim());
            if (!tokenParameters.PublicKey!.StartsWith(PublicKeyStart, StringComparison.OrdinalIgnoreCase) &&
                !tokenParameters.PublicKey!.StartsWith(CertificateStart, StringComparison.OrdinalIgnoreCase))
            {
                publicKeyStringBuilder.Insert(0, PublicKeyStart);
            }
            if (!tokenParameters.PublicKey.EndsWith(PublicKeyEnd, StringComparison.OrdinalIgnoreCase) &&
                !tokenParameters.PublicKey.EndsWith(CertificateEnd, StringComparison.OrdinalIgnoreCase))
            {
                publicKeyStringBuilder.Append(PublicKeyEnd);
            }

            var pemReader = new PemReader(new StringReader(publicKeyStringBuilder.ToString()));
            var pemObject = pemReader.ReadObject();
            AsymmetricKeyParameter asymmetricPublicKey;

            // If it's a cert, extract the public key
            if (pemObject is X509Certificate certificate)
            {
                asymmetricPublicKey = certificate.GetPublicKey();
            }
            else
            {
                asymmetricPublicKey = (AsymmetricKeyParameter)pemObject;
            }

            if (asymmetricPublicKey is null)
            {
                RaiseError(_localizedStrings.InvalidPublicKeyError);
                return null;
            }

            return asymmetricPublicKey;
        }

        private void RaiseError(string message)
        {
            var eventArg = new TokenResultErrorEventArgs
            {
                Message = message,
                Severity = InfoBarSeverity.Error
            };
            _decodingErrorCallBack!.Invoke(eventArg);
        }
    }
}
