#nullable enable

using System;
using System.Collections.Generic;
using System.Composition;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DevToys.Helpers;
using DevToys.Models;
using DevToys.Models.JwtDecoderEncoder;
using DevToys.Shared.Core;
using Microsoft.IdentityModel.Tokens;
using Microsoft.UI.Xaml.Controls;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.OpenSsl;

namespace DevToys.ViewModels.Tools.EncodersDecoders.JwtDecoderEncoder
{
    [Export(typeof(JwtEncoder))]
    [Shared]
    public class JwtEncoder
    {
        private const string PrivateKeyStart = "-----BEGIN PRIVATE KEY-----";
        private const string PrivateKeyEnd = "-----END PRIVATE KEY-----";
        private Action<TokenResultErrorEventArgs> _encodingErrorCallBack;
        private JwtDecoderEncoderStrings _localizedStrings => LanguageManager.Instance.JwtDecoderEncoder;

        public TokenResult? GenerateToken(
                EncoderParameters encodeParameters,
                TokenParameters tokenParameters,
                Action<TokenResultErrorEventArgs> encodingErrorCallBack)
        {
            Arguments.NotNull(encodeParameters, nameof(encodeParameters));
            Arguments.NotNull(tokenParameters, nameof(tokenParameters));
            _encodingErrorCallBack = Arguments.NotNull(encodingErrorCallBack, nameof(encodingErrorCallBack));
            Arguments.NotNullOrWhiteSpace(tokenParameters.Payload, nameof(tokenParameters.Payload));

            var tokenResult = new TokenResult();

            try
            {
                var serializeOptions = new JsonSerializerOptions();
                serializeOptions.Converters.Add(new JwtPayloadConverter());
                Dictionary<string, object>? payload = JsonSerializer.Deserialize<Dictionary<string, object>>(tokenParameters.Payload!, serializeOptions);
                SigningCredentials? signingCredentials = GetSigningCredentials(tokenParameters);

                if (signingCredentials is null)
                {
                    return null;
                }

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Claims = payload,
                    SigningCredentials = signingCredentials,
                    Expires = null
                };

                if (encodeParameters.HasExpiration)
                {
                    DateTime expirationDate = DateTime.Now
                        .AddYears(tokenParameters.ExpirationYear)
                        .AddMonths(tokenParameters.ExpirationMonth)
                        .AddDays(tokenParameters.ExpirationDay)
                        .AddHours(tokenParameters.ExpirationHour)
                        .AddMinutes(tokenParameters.ExpirationMinute);
                    tokenDescriptor.Expires = expirationDate;
                }

                if (encodeParameters.HasAudience)
                {
                    tokenDescriptor.Audience = string.Join(',', tokenParameters.ValidAudiences);
                }

                if (encodeParameters.HasIssuer)
                {
                    tokenDescriptor.Issuer = string.Join(',', tokenParameters.ValidIssuers);
                    tokenDescriptor.IssuedAt = DateTime.Now;
                }

                var handler = new JwtSecurityTokenHandler
                {
                    SetDefaultTimesOnTokenCreation = false
                };

                if (encodeParameters.HasDefaultTime)
                {
                    handler.SetDefaultTimesOnTokenCreation = true;
                    tokenDescriptor.Expires = DateTime.Now.AddHours(1);
                }

                SecurityToken? token = handler.CreateToken(tokenDescriptor);
                tokenResult.Token = handler.WriteToken(token);
                tokenResult.Payload = tokenParameters.Payload;
            }
            catch (Exception exception)
            {
                RaiseError(exception.Message);
                return null;
            }

            return tokenResult;
        }

        private SigningCredentials GetSigningCredentials(TokenParameters tokenParameters)
        {
            SigningCredentials? signingCredentials = tokenParameters.TokenAlgorithm switch
            {
                JwtAlgorithm.ES512 => new SigningCredentials(GetECDsaSigningKey(tokenParameters), SecurityAlgorithms.EcdsaSha512Signature),
                JwtAlgorithm.ES384 => new SigningCredentials(GetECDsaSigningKey(tokenParameters), SecurityAlgorithms.EcdsaSha384Signature),
                JwtAlgorithm.ES256 => new SigningCredentials(GetECDsaSigningKey(tokenParameters), SecurityAlgorithms.EcdsaSha256Signature),
                JwtAlgorithm.PS512 => new SigningCredentials(GetRsaShaSigningKey(tokenParameters), SecurityAlgorithms.RsaSsaPssSha512),
                JwtAlgorithm.PS384 => new SigningCredentials(GetRsaShaSigningKey(tokenParameters), SecurityAlgorithms.RsaSsaPssSha384),
                JwtAlgorithm.PS256 => new SigningCredentials(GetRsaShaSigningKey(tokenParameters), SecurityAlgorithms.RsaSsaPssSha256),
                JwtAlgorithm.RS512 => new SigningCredentials(GetRsaShaSigningKey(tokenParameters), SecurityAlgorithms.RsaSha512Signature),
                JwtAlgorithm.RS384 => new SigningCredentials(GetRsaShaSigningKey(tokenParameters), SecurityAlgorithms.RsaSha384Signature),
                JwtAlgorithm.RS256 => new SigningCredentials(GetRsaShaSigningKey(tokenParameters), SecurityAlgorithms.RsaSha256Signature),
                JwtAlgorithm.HS512 => new SigningCredentials(GetHmacShaSigningKey(tokenParameters), SecurityAlgorithms.HmacSha512Signature),
                JwtAlgorithm.HS384 => new SigningCredentials(GetHmacShaSigningKey(tokenParameters), SecurityAlgorithms.HmacSha384Signature),
                _ => new SigningCredentials(GetHmacShaSigningKey(tokenParameters), SecurityAlgorithms.HmacSha256Signature),// HS256
            };

            return signingCredentials;
        }

        private SymmetricSecurityKey? GetHmacShaSigningKey(TokenParameters tokenParameters)
        {
            if (string.IsNullOrWhiteSpace(tokenParameters.Signature))
            {
                throw new InvalidOperationException(_localizedStrings.InvalidSignatureError);
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

        private RsaSecurityKey? GetRsaShaSigningKey(TokenParameters tokenParameters)
        {
            AsymmetricKeyParameter asymmetricKeyParameter = GetPrivateAsymmetricKeyParameter(tokenParameters);

            var rsaPrivateKeyParameters = (RsaPrivateCrtKeyParameters)asymmetricKeyParameter;
            if (!rsaPrivateKeyParameters.IsPrivate)
            {
                throw new InvalidOperationException(_localizedStrings.InvalidPrivateKeyError);
            }

            RSAParameters rsaParameters = new();
            rsaParameters.Modulus = rsaPrivateKeyParameters.Modulus.ToByteArrayUnsigned();
            rsaParameters.Exponent = rsaPrivateKeyParameters.PublicExponent.ToByteArrayUnsigned();
            rsaParameters.P = rsaPrivateKeyParameters.P.ToByteArrayUnsigned();
            rsaParameters.Q = rsaPrivateKeyParameters.Q.ToByteArrayUnsigned();
            rsaParameters.D = ConvertRSAParametersField(rsaPrivateKeyParameters.Exponent, rsaParameters.Modulus.Length);
            rsaParameters.DP = ConvertRSAParametersField(rsaPrivateKeyParameters.DP, rsaParameters.P.Length);
            rsaParameters.DQ = ConvertRSAParametersField(rsaPrivateKeyParameters.DQ, rsaParameters.Q.Length);
            rsaParameters.InverseQ = ConvertRSAParametersField(rsaPrivateKeyParameters.QInv, rsaParameters.Q.Length);

            return new RsaSecurityKey(rsaParameters);
        }

        private ECDsaSecurityKey? GetECDsaSigningKey(TokenParameters tokenParameters)
        {
            AsymmetricKeyParameter? asymmetricKeyParameter = GetPrivateAsymmetricKeyParameter(tokenParameters);
            var ecPrivateKeyParameters = (ECPrivateKeyParameters)asymmetricKeyParameter!;
            if (!ecPrivateKeyParameters.IsPrivate)
            {
                throw new InvalidOperationException(_localizedStrings.InvalidPrivateKeyError);
            }

            ECPoint ecPoint = new()
            {
                X = ecPrivateKeyParameters.Parameters.G.AffineXCoord.GetEncoded(),
                Y = ecPrivateKeyParameters.Parameters.G.AffineYCoord.GetEncoded()
            };
            ECParameters ecParameters = new()
            {
                Curve = ECCurve.NamedCurves.nistP256,
                Q = ecPoint,
                D = ecPrivateKeyParameters.D.ToByteArrayUnsigned()
            };

            var ecdSa = ECDsa.Create(ecParameters);
            return new ECDsaSecurityKey(ecdSa);
        }

        private AsymmetricKeyParameter GetPrivateAsymmetricKeyParameter(TokenParameters tokenParameters)
        {
            if (string.IsNullOrWhiteSpace(tokenParameters.PrivateKey))
            {
                throw new InvalidOperationException(_localizedStrings.InvalidPrivateKeyError);
            }
            var privateKeyStringBuilder = new StringBuilder(tokenParameters.PrivateKey!.Trim());
            if (!tokenParameters.PrivateKey!.StartsWith(PrivateKeyStart, StringComparison.OrdinalIgnoreCase))
            {
                privateKeyStringBuilder.Insert(0, PrivateKeyStart);
            }
            if (!tokenParameters.PrivateKey.EndsWith(PrivateKeyEnd, StringComparison.OrdinalIgnoreCase))
            {
                privateKeyStringBuilder.Append(PrivateKeyEnd);
            }

            var pemReader = new PemReader(new StringReader(privateKeyStringBuilder.ToString()));
            object? pemObject = pemReader.ReadObject();
            if (pemObject is null)
            {
                throw new InvalidOperationException(_localizedStrings.InvalidPrivateKeyError);
            }

            if (pemObject is AsymmetricKeyParameter parameter)
            {
                return parameter;
            }
            else if (pemObject is AsymmetricCipherKeyPair)
            {
                var pair = pemObject as AsymmetricCipherKeyPair;
                return pair!.Private;
            }

            throw new InvalidOperationException(_localizedStrings.InvalidPrivateKeyError);
        }

        /// <summary>
        /// Source (https://stackoverflow.com/questions/28370414/import-rsa-key-from-bouncycastle-sometimes-throws-bad-data)
        /// </summary>
        private static byte[] ConvertRSAParametersField(BigInteger n, int size)
        {
            byte[] bs = n.ToByteArrayUnsigned();
            if (bs.Length == size)
            {
                return bs;
            }
            if (bs.Length > size)
            {
                throw new ArgumentException("Specified size too small", "size");
            }
            byte[] padded = new byte[size];
            Array.Copy(bs, 0, padded, size - bs.Length, bs.Length);
            return padded;
        }

        private void RaiseError(string message)
        {
            var eventArg = new TokenResultErrorEventArgs
            {
                Message = message,
                Severity = InfoBarSeverity.Error
            };
            _encodingErrorCallBack!.Invoke(eventArg);
        }
    }
}
