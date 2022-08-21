#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using DevToys.Api.Core;
using DevToys.Api.Core.Settings;
using DevToys.Helpers;
using DevToys.Helpers.JsonYaml;
using DevToys.Models;
using DevToys.UI.Controls;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Controls;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Windows.UI.Xaml;
using YamlDotNet.Core.Tokens;
using PemReader = Org.BouncyCastle.OpenSsl.PemReader;

namespace DevToys.ViewModels.Tools.EncodersDecoders.JwtDecoderEncoder
{
    public class JwtDecoderEncoderViewModel : ObservableRecipient
    {
        private string? _token;
        private string? _header;
        private string? _payload;
        private string? _signature;
        private string? _publicKey;
        private string? _privateKey;
        private string? _validIssuers;
        private string? _validAudiences;
        private JwtAlgorithmDisplayPair _algorithmSelected;
        private bool _showValidation;
        private bool _requireSignature;
        private InfoBarData? _validationResult;
        private const string PublicKeyStart = "-----BEGIN PUBLIC KEY-----";
        private const string PublicKeyEnd = "-----END PUBLIC KEY-----";
        private const string PrivateKeyStart = "-----BEGIN PRIVATE KEY-----";
        private const string PrivateKeyEnd = "-----END PRIVATE KEY-----";

        protected bool WorkInProgress;
        protected bool ToolSuccessfullyWorked;
        protected ValidationBase JwtValidation = new();
        protected JwtToolJobItem CurrentJobItem = new();
        protected readonly Queue<bool> JobQueue = new();
        protected readonly ISettingsProvider SettingsProvider;
        protected readonly IMarketingService MarketingService;

        internal JwtDecoderEncoderStrings LocalizedStrings => LanguageManager.Instance.JwtDecoderEncoder;

        internal RoutedEventHandler InputFocusChanged { get; }

        internal virtual string? Token
        {
            get => _token;
            set
            {
                if (_token != value)
                {
                    SetProperty(ref _token, value?.Trim());
                    QueueNewTokenJob();
                }
            }
        }

        internal string? Header
        {
            get => _header;
            set
            {
                if (_header != value)
                {
                    SetProperty(ref _header, value);
                    QueueNewTokenJob();
                }
            }
        }

        internal string? Payload
        {
            get => _payload;
            set
            {
                if (_payload != value)
                {
                    SetProperty(ref _payload, value);
                    QueueNewTokenJob();
                }
            }
        }

        internal string? ValidIssuers
        {
            get => _validIssuers;
            set
            {
                if (_validIssuers != value)
                {
                    SetProperty(ref _validIssuers, value);
                    QueueNewTokenJob();
                }
            }
        }

        internal string? ValidAudiences
        {
            get => _validAudiences;
            set
            {
                if (_validAudiences != value)
                {
                    SetProperty(ref _validAudiences, value);
                    QueueNewTokenJob();
                }
            }
        }

        internal string? PublicKey
        {
            get => _publicKey;
            set
            {
                if (_publicKey != value)
                {
                    SetProperty(ref _publicKey, value);
                    QueueNewTokenJob();
                }
            }
        }

        internal string? PrivateKey
        {
            get => _privateKey;
            set
            {
                if (_privateKey != value)
                {
                    SetProperty(ref _privateKey, value);
                    QueueNewTokenJob();
                }
            }
        }

        internal string? Signature
        {
            get => _signature;
            set
            {
                if (_signature != value)
                {
                    SetProperty(ref _signature, value);
                    QueueNewTokenJob();
                }
            }
        }

        internal bool ShowValidation
        {
            get => SettingsProvider.GetSetting(JwtDecoderEncoderSettings.ShowValidation);
            set
            {
                if (_showValidation != value)
                {
                    SettingsProvider.SetSetting(JwtDecoderEncoderSettings.ShowValidation, value);
                    SetProperty(ref _showValidation, value);
                    QueueNewTokenJob();
                }
            }
        }

        internal bool RequireSignature
        {
            get => _requireSignature;
            set
            {
                if (_requireSignature != value)
                {
                    SetProperty(ref _requireSignature, value);
                    OnPropertyChanged();
                }
            }
        }

        internal InfoBarData? ValidationResult
        {
            get => _validationResult;
            private set => SetProperty(ref _validationResult, value);
        }

        internal JwtAlgorithmDisplayPair AlgorithmMode
        {
            get
            {
                JwtAlgorithm settingsValue = SettingsProvider.GetSetting(JwtDecoderEncoderSettings.JwtAlgorithm);
                JwtAlgorithmDisplayPair? algorithm = Algorithms.FirstOrDefault(x => x.Value == settingsValue);
                Header = JsonHelper.Format(@"{""alg"": """ + algorithm.DisplayName + @""", ""typ"": ""JWT""}", Indentation.TwoSpaces, false);
                IsSignatureRequired(algorithm);
                return _algorithmSelected ?? JwtAlgorithmDisplayPair.HS256;
            }
            set
            {
                if (_algorithmSelected != value)
                {
                    SettingsProvider.SetSetting(JwtDecoderEncoderSettings.JwtAlgorithm, value.Value);
                    IsSignatureRequired(value);
                    SetProperty(ref _algorithmSelected, value);
                    OnPropertyChanged();
                }
            }
        }

        internal IReadOnlyList<JwtAlgorithmDisplayPair> Algorithms = new ObservableCollection<JwtAlgorithmDisplayPair> {
            JwtAlgorithmDisplayPair.HS256, JwtAlgorithmDisplayPair.HS384, JwtAlgorithmDisplayPair.HS512,
            JwtAlgorithmDisplayPair.RS256, JwtAlgorithmDisplayPair.RS384, JwtAlgorithmDisplayPair.RS512,
            JwtAlgorithmDisplayPair.ES256, JwtAlgorithmDisplayPair.ES384, JwtAlgorithmDisplayPair.ES512,
            JwtAlgorithmDisplayPair.PS256, JwtAlgorithmDisplayPair.PS384, JwtAlgorithmDisplayPair.PS512,
        };

        public JwtDecoderEncoderViewModel(
            ISettingsProvider settingsProvider,
            IMarketingService marketingService)
        {
            SettingsProvider = settingsProvider;
            MarketingService = marketingService;
            InputFocusChanged = ControlFocusChanged;
            IsSignatureRequired(AlgorithmMode);
        }

        internal void QueueNewTokenJob()
        {
            JobQueue.Enqueue(true);
            Messenger.Send(new JwtJobAddedMessage());
        }

        protected void DisplayValidationInfoBar()
        {
            InfoBarSeverity infoBarSeverity;
            string message;
            if (!JwtValidation.IsValid)
            {
                infoBarSeverity = InfoBarSeverity.Error;
                message = JwtValidation.ErrorMessage ?? LocalizedStrings.JwtInValidMessage;
            }
            else
            {
                infoBarSeverity = InfoBarSeverity.Success;
                message = LocalizedStrings.JwtIsValidMessage;
            }

            ValidationResult = new InfoBarData(infoBarSeverity, message);
        }

        protected void ControlFocusChanged(object source, RoutedEventArgs args)
        {
            var input = (CustomTextBox)source;

            if (input.Text.Length == 0)
            {
                return;
            }

            QueueNewTokenJob();
        }

        #region Decoding

        protected SigningCredentials GetValidationCredentials(JwtAlgorithm algorithmMode)
        {
            SigningCredentials? signingCredentials = algorithmMode switch
            {
                JwtAlgorithm.ES512 => new SigningCredentials(GetECDsaValidationKey(), SecurityAlgorithms.EcdsaSha512Signature),
                JwtAlgorithm.ES384 => new SigningCredentials(GetECDsaValidationKey(), SecurityAlgorithms.EcdsaSha384Signature),
                JwtAlgorithm.ES256 => new SigningCredentials(GetECDsaValidationKey(), SecurityAlgorithms.EcdsaSha256Signature),
                JwtAlgorithm.PS512 => new SigningCredentials(GetRsaShaValidationKey(), SecurityAlgorithms.RsaSsaPssSha512),
                JwtAlgorithm.PS384 => new SigningCredentials(GetRsaShaValidationKey(), SecurityAlgorithms.RsaSsaPssSha384),
                JwtAlgorithm.PS256 => new SigningCredentials(GetRsaShaValidationKey(), SecurityAlgorithms.RsaSsaPssSha256),
                JwtAlgorithm.RS512 => new SigningCredentials(GetRsaShaValidationKey(), SecurityAlgorithms.RsaSha512Signature),
                JwtAlgorithm.RS384 => new SigningCredentials(GetRsaShaValidationKey(), SecurityAlgorithms.RsaSha384Signature),
                JwtAlgorithm.RS256 => new SigningCredentials(GetRsaShaValidationKey(), SecurityAlgorithms.RsaSha256Signature),
                JwtAlgorithm.HS512 => new SigningCredentials(GetHmacShaValidationKey(algorithmMode), SecurityAlgorithms.HmacSha512Signature),
                JwtAlgorithm.HS384 => new SigningCredentials(GetHmacShaValidationKey(algorithmMode), SecurityAlgorithms.HmacSha384Signature),
                _ => new SigningCredentials(GetHmacShaValidationKey(algorithmMode), SecurityAlgorithms.HmacSha256Signature),// HS256
            };

            return signingCredentials;
        }

        private SymmetricSecurityKey? GetHmacShaValidationKey(JwtAlgorithm algorithmMode)
        {
            if (string.IsNullOrWhiteSpace(Signature))
            {
                return null;
            }

            byte[]? signatureByte;
            if (Base64Helper.IsBase64DataStrict(Signature))
            {
                signatureByte = Convert.FromBase64String(Signature);
            }
            else
            {
                signatureByte = Encoding.UTF8.GetBytes(Signature);
            }
            byte[] byteKey = algorithmMode switch
            {
                JwtAlgorithm.HS512 => new HMACSHA512(signatureByte).Key,
                JwtAlgorithm.HS384 => new HMACSHA384(signatureByte).Key,
                _ => new HMACSHA256(signatureByte).Key,// HS256
            };
            return new SymmetricSecurityKey(byteKey);
        }

        private RsaSecurityKey? GetRsaShaValidationKey()
        {
            try
            {
                AsymmetricKeyParameter? asymmetricKeyParameter = GetPublicAsymmetricKeyParameter();
                if (asymmetricKeyParameter is null)
                {
                    JwtValidation.IsValid = false;
                    JwtValidation.ErrorMessage = LocalizedStrings.InvalidPublicKeyError;
                    return null;
                }

                var publicKey = (RsaKeyParameters)asymmetricKeyParameter;
                if (publicKey.IsPrivate)
                {
                    JwtValidation.IsValid = false;
                    JwtValidation.ErrorMessage = LocalizedStrings.PublicKeyIsPrivateKeyError;
                    return null;
                }

                RSAParameters rsaParameters = new();
                rsaParameters.Modulus = publicKey.Modulus.ToByteArrayUnsigned();
                rsaParameters.Exponent = publicKey.Exponent.ToByteArrayUnsigned();
                return new RsaSecurityKey(rsaParameters);
            }
            catch (Exception exception)
            {
                JwtValidation.IsValid = false;
                JwtValidation.ErrorMessage = $"{LocalizedStrings.InvalidPublicKeyError}: '{exception.Message}'";
                return null;
            }
        }

        private ECDsaSecurityKey? GetECDsaValidationKey()
        {
            try
            {
                AsymmetricKeyParameter? asymmetricKeyParameter = GetPublicAsymmetricKeyParameter();
                if (asymmetricKeyParameter is null)
                {
                    JwtValidation.IsValid = false;
                    JwtValidation.ErrorMessage = LocalizedStrings.InvalidPublicKeyError;
                    return null;
                }

                var publicKey = (ECPublicKeyParameters)asymmetricKeyParameter;
                if (publicKey.IsPrivate)
                {
                    JwtValidation.IsValid = false;
                    JwtValidation.ErrorMessage = LocalizedStrings.PublicKeyIsPrivateKeyError;
                    return null;
                }

                ECParameters ecParameters = new()
                {
                    Curve = ECCurve.NamedCurves.nistP521,
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
                JwtValidation.IsValid = false;
                JwtValidation.ErrorMessage = $"{LocalizedStrings.InvalidPublicKeyError}: '{exception.Message}'";
                return null;
            }
        }

        private AsymmetricKeyParameter? GetPublicAsymmetricKeyParameter()
        {
            if (string.IsNullOrWhiteSpace(PublicKey))
            {
                JwtValidation.IsValid = false;
                JwtValidation.ErrorMessage = LocalizedStrings.InvalidPublicKeyError;
                return null;
            }
            var publicKeyStringBuilder = new StringBuilder(PublicKey!.Trim());
            if (!PublicKey!.StartsWith(PublicKeyStart))
            {
                publicKeyStringBuilder.Insert(0, PublicKeyStart);
            }
            if (!PublicKey.EndsWith(PublicKeyEnd))
            {
                publicKeyStringBuilder.Append(PublicKeyEnd);
            }

            var pemReader = new PemReader(new StringReader(publicKeyStringBuilder.ToString()));
            var asymetricPublicKey = (AsymmetricKeyParameter)pemReader.ReadObject();
            if (asymetricPublicKey is null)
            {
                JwtValidation.IsValid = false;
                JwtValidation.ErrorMessage = LocalizedStrings.InvalidPrivateKeyError;
                return null;
            }

            return asymetricPublicKey;
        }

        #endregion

        #region Encoding

        protected SigningCredentials GetSigningCredentials(JwtAlgorithm algorithmMode)
        {
            SigningCredentials? signingCredentials = algorithmMode switch
            {
                JwtAlgorithm.ES512 => new SigningCredentials(GetECDsaSigningKey(), SecurityAlgorithms.EcdsaSha512Signature),
                JwtAlgorithm.ES384 => new SigningCredentials(GetECDsaSigningKey(), SecurityAlgorithms.EcdsaSha384Signature),
                JwtAlgorithm.ES256 => new SigningCredentials(GetECDsaSigningKey(), SecurityAlgorithms.EcdsaSha256Signature),
                JwtAlgorithm.PS512 => new SigningCredentials(GetRsaShaSigningKey(), SecurityAlgorithms.RsaSsaPssSha512),
                JwtAlgorithm.PS384 => new SigningCredentials(GetRsaShaSigningKey(), SecurityAlgorithms.RsaSsaPssSha384),
                JwtAlgorithm.PS256 => new SigningCredentials(GetRsaShaSigningKey(), SecurityAlgorithms.RsaSsaPssSha256),
                JwtAlgorithm.RS512 => new SigningCredentials(GetRsaShaSigningKey(), SecurityAlgorithms.RsaSha512Signature),
                JwtAlgorithm.RS384 => new SigningCredentials(GetRsaShaSigningKey(), SecurityAlgorithms.RsaSha384Signature),
                JwtAlgorithm.RS256 => new SigningCredentials(GetRsaShaSigningKey(), SecurityAlgorithms.RsaSha256Signature),
                JwtAlgorithm.HS512 => new SigningCredentials(GetHmacShaSigningKey(), SecurityAlgorithms.HmacSha512Signature),
                JwtAlgorithm.HS384 => new SigningCredentials(GetHmacShaSigningKey(), SecurityAlgorithms.HmacSha384Signature),
                _ => new SigningCredentials(GetHmacShaSigningKey(), SecurityAlgorithms.HmacSha256Signature),// HS256
            };

            return signingCredentials;
        }

        private SymmetricSecurityKey? GetHmacShaSigningKey()
        {
            if (string.IsNullOrWhiteSpace(Signature))
            {
                return null;
            }
            return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Signature));
        }

        private RsaSecurityKey? GetRsaShaSigningKey()
        {
            AsymmetricKeyParameter? asymmetricKeyParameter = GetPrivateAsymmetricKeyParameter();
            if (asymmetricKeyParameter is null)
            {
                JwtValidation.IsValid = false;
                JwtValidation.ErrorMessage = LocalizedStrings.InvalidPrivateKeyError;
                return null;
            }

            var rsaPrivateKeyParameters = (RsaPrivateCrtKeyParameters)asymmetricKeyParameter;
            if (!rsaPrivateKeyParameters.IsPrivate)
            {
                JwtValidation.IsValid = false;
                JwtValidation.ErrorMessage = LocalizedStrings.InvalidPrivateKeyError;
                return null;
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

        private ECDsaSecurityKey? GetECDsaSigningKey()
        {
            AsymmetricKeyParameter? asymmetricKeyParameter = GetPrivateAsymmetricKeyParameter();
            if (asymmetricKeyParameter is null)
            {
                JwtValidation.IsValid = false;
                JwtValidation.ErrorMessage = LocalizedStrings.InvalidPrivateKeyError;
                return null;
            }

            var ecPrivateKeyParameters = (ECPrivateKeyParameters)asymmetricKeyParameter!;
            if (!ecPrivateKeyParameters.IsPrivate)
            {
                JwtValidation.IsValid = false;
                JwtValidation.ErrorMessage = LocalizedStrings.InvalidPrivateKeyError;
                return null;
            }

            ECPoint ecPoint = new()
            {
                X = ecPrivateKeyParameters.Parameters.G.AffineXCoord.GetEncoded(),
                Y = ecPrivateKeyParameters.Parameters.G.AffineYCoord.GetEncoded()
            };
            ECParameters ecParameters = new();
            ecParameters.Curve = ECCurve.NamedCurves.nistP521;
            ecParameters.Q = ecPoint;
            ecParameters.D = ecPrivateKeyParameters.D.ToByteArrayUnsigned();

            var ecdSa = ECDsa.Create(ecParameters);
            return new ECDsaSecurityKey(ecdSa);
        }

        private AsymmetricKeyParameter? GetPrivateAsymmetricKeyParameter()
        {
            if (string.IsNullOrWhiteSpace(PrivateKey))
            {
                JwtValidation.IsValid = false;
                JwtValidation.ErrorMessage = LocalizedStrings.InvalidPrivateKeyError;
                return null;
            }
            var privateKeyStringBuilder = new StringBuilder(PrivateKey!.Trim());
            if (!PrivateKey!.StartsWith(PrivateKeyStart))
            {
                privateKeyStringBuilder.Insert(0, PrivateKeyStart);
            }
            if (!PrivateKey.EndsWith(PrivateKeyEnd))
            {
                privateKeyStringBuilder.Append(PrivateKeyEnd);
            }

            var pemReader = new PemReader(new StringReader(privateKeyStringBuilder.ToString()));
            object? pemObject = pemReader.ReadObject();
            if (pemObject is null)
            {
                JwtValidation.IsValid = false;
                JwtValidation.ErrorMessage = LocalizedStrings.InvalidPrivateKeyError;
                return null;
            }

            if (pemObject is AsymmetricKeyParameter)
            {
                return pemObject as AsymmetricKeyParameter;
            }
            else if (pemObject is AsymmetricCipherKeyPair)
            {
                var pair = pemObject as AsymmetricCipherKeyPair;
                return pair!.Private;
            }

            JwtValidation.IsValid = false;
            JwtValidation.ErrorMessage = LocalizedStrings.InvalidPrivateKeyError;
            return null;
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

        #endregion

        private void IsSignatureRequired(JwtAlgorithmDisplayPair value)
        {
            if (value.Value is JwtAlgorithm.HS256 ||
                value.Value is JwtAlgorithm.HS384 ||
                value.Value is JwtAlgorithm.HS512)
            {
                RequireSignature = true;
            }
            else
            {
                RequireSignature = false;
            }
        }
    }
}
