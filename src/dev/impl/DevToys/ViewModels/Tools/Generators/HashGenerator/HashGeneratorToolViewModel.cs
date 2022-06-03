#nullable enable

using System;
using System.Collections.Generic;
using System.Composition;
using System.Threading.Tasks;
using DevToys.Api.Core;
using DevToys.Api.Core.Settings;
using DevToys.Api.Tools;
using DevToys.Core;
using DevToys.Core.Threading;
using DevToys.Shared.Core.Threading;
using DevToys.Views.Tools.HashGenerator;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;

namespace DevToys.ViewModels.Tools.HashGenerator
{
    [Export(typeof(HashGeneratorToolViewModel))]
    public sealed class HashGeneratorToolViewModel : QueueWorkerViewModelBase<Tuple<string,string>>, IToolViewModel
    {
        /// <summary>
        /// Whether the generated hash should be uppercase or lowercase.
        /// </summary>
        private static readonly SettingDefinition<bool> Uppercase
            = new(
                name: $"{nameof(HashGeneratorToolViewModel)}.{nameof(Uppercase)}",
                isRoaming: true,
                defaultValue: false);

        /// <summary>
        /// Whether the tool should operate and return HMAC string
        /// </summary>
        private static readonly SettingDefinition<bool> IsHMAC
             = new(
                name: $"{nameof(HashGeneratorToolViewModel)}.{nameof(IsHMAC)}",
                isRoaming: true,
                defaultValue: false);

        /// <summary>
        /// The Output Code to generate.
        /// </summary>
        private static readonly SettingDefinition<string> OutType
            = new(
                name: $"{nameof(HashGeneratorToolViewModel)}.{nameof(OutType)}",
                isRoaming: true,
                defaultValue: DefaultOutputType);

        private readonly IMarketingService _marketingService;

        internal ISettingsProvider SettingsProvider;

        private bool _toolSuccessfullyWorked;
        private string? _input;
        private string? _secretKey;
        private string? _md5;
        private string? _sha1;
        private string? _sha256;
        private string? _sha512;
        private const string HexOutput = "Hex";
        private const string Base64Output = "Base64";
        private const string DefaultOutputType = HexOutput;

        public Type View { get; } = typeof(HashGeneratorToolPage);

        internal HashGeneratorStrings Strings => LanguageManager.Instance.HashGenerator;

        internal bool IsUppercase
        {
            get => SettingsProvider.GetSetting(Uppercase);
            set
            {
                if (SettingsProvider.GetSetting(Uppercase) != value)
                {
                    SettingsProvider.SetSetting(Uppercase, value);
                    OnPropertyChanged();
                    QueueHashCalculation();
                }
            }
        }

        internal string OutputType
        {
            get => SettingsProvider.GetSetting(OutType);
            set
            {
                if (SettingsProvider.GetSetting(OutType) != value)
                {
                    SettingsProvider.SetSetting(OutType, value);
                    OnPropertyChanged();
                    QueueHashCalculation();
                }
            }
        }

        internal bool IsHmacMode
        {
            get => _settingsProvider.GetSetting(IsHMAC);
            set
            {
                if(_settingsProvider.GetSetting(IsHMAC) != value)
                {
                    _settingsProvider.SetSetting(IsHMAC, value);
                    OnPropertyChanged();
                    QueueHashCalculation();
                }
            }
        }

        internal string? Input
        {
            get => _input;
            set
            {
                SetProperty(ref _input, value);
                QueueHashCalculation();
            }
        }

        internal string? SecretKey
        {
            get => _secretKey;
            set
            {
                SetProperty(ref _secretKey, value);
                QueueHashCalculation();
            }
        }

        internal string? MD5
        {
            get => _md5;
            private set => SetProperty(ref _md5, value);
        }

        internal string? SHA1
        {
            get => _sha1;
            private set => SetProperty(ref _sha1, value);
        }

        internal string? SHA256
        {
            get => _sha256;
            private set => SetProperty(ref _sha256, value);
        }

        internal string? SHA512
        {
            get => _sha512;
            private set => SetProperty(ref _sha512, value);
        }

        [ImportingConstructor]
        public HashGeneratorToolViewModel(ISettingsProvider settingsProvider, IMarketingService marketingService)
        {
            SettingsProvider = settingsProvider;
            _marketingService = marketingService;
        }

        private void QueueHashCalculation()
        {
            EnqueueComputation(new Tuple<string, string>(Input ?? string.Empty, SecretKey ?? string.Empty));
        }

        protected override async Task TreatComputationQueueAsync(Tuple<string, string> inputSecretKeyPair)
        {
            Task<string> md5CalculationTask = CalculateHashAsync(HashAlgorithmNames.Md5, inputSecretKeyPair.Item1, inputSecretKeyPair.Item2);
            Task<string> sha1CalculationTask = CalculateHashAsync(HashAlgorithmNames.Sha1, inputSecretKeyPair.Item1, inputSecretKeyPair.Item2);
            Task<string> sha256CalculationTask = CalculateHashAsync(HashAlgorithmNames.Sha256, inputSecretKeyPair.Item1, inputSecretKeyPair.Item2);
            Task<string> sha512CalculationTask = CalculateHashAsync(HashAlgorithmNames.Sha512, inputSecretKeyPair.Item1, inputSecretKeyPair.Item2);

            await Task.WhenAll(md5CalculationTask).ConfigureAwait(false);

            await ThreadHelper.RunOnUIThreadAsync(() =>
            {
                MD5 = md5CalculationTask.Result;
                SHA1 = sha1CalculationTask.Result;
                SHA256 = sha256CalculationTask.Result;
                SHA512 = sha512CalculationTask.Result;

                if (!_toolSuccessfullyWorked)
                {
                    _toolSuccessfullyWorked = true;
                    _marketingService.NotifyToolSuccessfullyWorked();
                }
            });
        }

        private async Task<string> CalculateHashAsync(string algorithmName, string text, string secretKey)
        {
            if (string.IsNullOrEmpty(text) || (IsHmacMode && secretKey.Length == 0))
            {
                return string.Empty;
            }

            await TaskScheduler.Default;

            try
            {
                string? hash = "";
                IBuffer? buffer = null;
                if (IsHmacMode)
                {
                    var macAlgorithmProvider = MacAlgorithmProvider.OpenAlgorithm(GetHmacAlgorithmName(algorithmName));
                    IBuffer textBuffer = CryptographicBuffer.ConvertStringToBinary(text, BinaryStringEncoding.Utf8);
                    IBuffer secretKeyBuffer = CryptographicBuffer.ConvertStringToBinary(secretKey, BinaryStringEncoding.Utf8);
                    CryptographicKey hmacKey = macAlgorithmProvider.CreateKey(secretKeyBuffer);
                    buffer = CryptographicEngine.Sign(hmacKey, textBuffer);
                }
                else
                {
                    var algorithmProvider = HashAlgorithmProvider.OpenAlgorithm(algorithmName);
                    buffer = CryptographicBuffer.ConvertStringToBinary(text, BinaryStringEncoding.Utf8);
                    buffer = algorithmProvider.HashData(buffer);
                }         

                if (string.Equals(OutputType, HexOutput))
                {
                    hash = IsUppercase
                        ? CryptographicBuffer.EncodeToHexString(buffer).ToUpperInvariant()
                        : CryptographicBuffer.EncodeToHexString(buffer).ToLowerInvariant();
                }
                else if (string.Equals(OutputType, Base64Output))
                {
                    hash = CryptographicBuffer.EncodeToBase64String(buffer);
                }
                return hash;
            }
            catch (Exception ex)
            {
                Logger.LogFault("Hash Generator", ex, $"Alrogithm name: {algorithmName}");
                return ex.Message;
            }
        }

        private string GetHmacAlgorithmName(string algorithmName)
        {
            if (algorithmName == HashAlgorithmNames.Md5)
            {
                return MacAlgorithmNames.HmacMd5;
            }
            else if (algorithmName == HashAlgorithmNames.Sha1)
            {
                return MacAlgorithmNames.HmacSha1;
            }
            else if (algorithmName == HashAlgorithmNames.Sha256)
            {
                return MacAlgorithmNames.HmacSha256;
            }
            else if (algorithmName == HashAlgorithmNames.Sha512)
            {
                return MacAlgorithmNames.HmacSha512;
            }

            throw new Exception("Unsupported algorithm: " + algorithmName);
        }
    }
}
