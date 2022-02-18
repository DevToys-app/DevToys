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
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;

namespace DevToys.ViewModels.Tools.HashGenerator
{
    [Export(typeof(HashGeneratorToolViewModel))]
    public sealed class HashGeneratorToolViewModel : ObservableRecipient, IToolViewModel
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
        /// The Output Code to generate.
        /// </summary>
        private static readonly SettingDefinition<string> OutType
            = new(
                name: $"{nameof(HashGeneratorToolViewModel)}.{nameof(OutType)}",
                isRoaming: true,
                defaultValue: DefaultOutputType);

        private readonly IMarketingService _marketingService;
        private readonly ISettingsProvider _settingsProvider;
        private readonly Queue<string> _hashCalculationQueue = new();

        private bool _toolSuccessfullyWorked;
        private bool _calculationInProgress;
        private string? _input;
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
            get => _settingsProvider.GetSetting(Uppercase);
            set
            {
                if (_settingsProvider.GetSetting(Uppercase) != value)
                {
                    _settingsProvider.SetSetting(Uppercase, value);
                    OnPropertyChanged();
                    QueueHashCalculation();
                }
            }
        }

        internal string OutputType
        {
            get => _settingsProvider.GetSetting(OutType);
            set
            {
                if (_settingsProvider.GetSetting(OutType) != value)
                {
                    _settingsProvider.SetSetting(OutType, value);
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
            _settingsProvider = settingsProvider;
            _marketingService = marketingService;
        }

        private void QueueHashCalculation()
        {
            _hashCalculationQueue.Enqueue(Input ?? string.Empty);
            TreatQueueAsync().Forget();
        }

        private async Task TreatQueueAsync()
        {
            if (_calculationInProgress)
            {
                return;
            }

            _calculationInProgress = true;

            await TaskScheduler.Default;

            while (_hashCalculationQueue.TryDequeue(out string? text))
            {
                Task<string> md5CalculationTask = CalculateHashAsync(HashAlgorithmNames.Md5, text);
                Task<string> sha1CalculationTask = CalculateHashAsync(HashAlgorithmNames.Sha1, text);
                Task<string> sha256CalculationTask = CalculateHashAsync(HashAlgorithmNames.Sha256, text);
                Task<string> sha512CalculationTask = CalculateHashAsync(HashAlgorithmNames.Sha512, text);

                await Task.WhenAll(md5CalculationTask).ConfigureAwait(false);

                ThreadHelper.RunOnUIThreadAsync(() =>
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
                }).ForgetSafely();
            }

            _calculationInProgress = false;
        }

        private async Task<string> CalculateHashAsync(string alrogithmName, string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            await TaskScheduler.Default;

            try
            {
                var algorithmProvider = HashAlgorithmProvider.OpenAlgorithm(alrogithmName);

                IBuffer buffer = CryptographicBuffer.ConvertStringToBinary(text, BinaryStringEncoding.Utf8);
                buffer = algorithmProvider.HashData(buffer);

                string? hash="";
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
                Logger.LogFault("Hash Generator", ex, $"Alrogithm name: {alrogithmName}");
                return ex.Message;
            }
        }
    }
}
