#nullable enable

using DevToys.Api.Tools;
using DevToys.Core;
using DevToys.Core.Threading;
using DevToys.Views.Tools.HashGenerator;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;

namespace DevToys.ViewModels.Tools.HashGenerator
{
    [Export(typeof(HashGeneratorToolViewModel))]
    public sealed class HashGeneratorToolViewModel : ObservableRecipient, IToolViewModel
    {
        private readonly Queue<string> _hashCalculationQueue = new();

        private bool _calculationInProgress;
        private bool _isUppercase;
        private string? _input;
        private string? _md5;
        private string? _sha1;
        private string? _sha256;
        private string? _sha512;

        public Type View { get; } = typeof(HashGeneratorToolPage);

        internal HashGeneratorStrings Strings => LanguageManager.Instance.HashGenerator;

        internal bool IsUppercase
        {
            get => _isUppercase;
            set
            {
                SetProperty(ref _isUppercase, value);
                QueueHashCalculation();
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

            while (_hashCalculationQueue.TryDequeue(out string text))
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
                HashAlgorithmProvider algorithmProvider = HashAlgorithmProvider.OpenAlgorithm(alrogithmName);

                IBuffer buffer = CryptographicBuffer.ConvertStringToBinary(text, BinaryStringEncoding.Utf8);
                buffer = algorithmProvider.HashData(buffer);

                string hash = CryptographicBuffer.EncodeToHexString(buffer);

                if (IsUppercase)
                {
                    return hash.ToUpperInvariant();
                }
                else
                {
                    return hash.ToLowerInvariant();
                }
            }
            catch (Exception ex)
            {
                Logger.LogFault("Hash Generator", ex, $"Alrogithm name: {alrogithmName}");
                return ex.Message;
            }
        }
    }
}
