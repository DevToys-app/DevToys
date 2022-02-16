#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using DevToys.Api.Core;
using DevToys.Api.Core.Settings;
using DevToys.Api.Tools;
using DevToys.Core;
using DevToys.Core.Threading;
using DevToys.Helpers;
using DevToys.Models;
using DevToys.Shared.Core.Threading;
using DevToys.Views.Tools.CheckSumGenerator;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;

namespace DevToys.ViewModels.Tools.CheckSumGenerator
{
    [Export(typeof(CheckSumGeneratorToolViewModel))]
    public sealed class CheckSumGeneratorToolViewModel : ObservableRecipient, IToolViewModel
    {
        /// <summary>
        /// Whether the generated hash should be uppercase or lowercase.
        /// </summary>
        private static readonly SettingDefinition<bool> UppercaseSetting
            = new(
                name: $"{nameof(CheckSumGeneratorToolViewModel)}.{nameof(UppercaseSetting)}",
                isRoaming: true,
                defaultValue: false);

        /// <summary>
        /// The hashing algorithm to be used in the generation.
        /// </summary>
        private static readonly SettingDefinition<HashingAlgorithm> HashingAlgorithmSetting
            = new(
                name: $"{nameof(CheckSumGeneratorToolViewModel)}.{nameof(HashingAlgorithmSetting)}",
                isRoaming: true,
                defaultValue: HashingAlgorithm.MD5);

        private readonly IMarketingService _marketingService;
        private readonly ISettingsProvider _settingsProvider;
        private readonly int _displayAboveIterations = 124;

        private readonly object _lockObject = new();
        private CancellationTokenSource _cancellationTokenSource = new();
        private bool _isCalculationInProgress;
        private IStorageFile? _inputFile;
        private int _progress;
        private string? _output;
        private string? _outputComparer;
        private bool _toolSuccessfullyWorked;
        private bool _hasCancelledCalculation;
        private bool _shouldDisplayProgress;
        private HashingAlgorithmDisplayPair? _outputHashingAlgorithm;
        private readonly Queue<Task> _computationTaskQueue = new();

        internal CheckSumGeneratorStrings Strings => LanguageManager.Instance.CheckSumGenerator;

        public Type View => typeof(CheckSumGeneratorToolPage);

        internal bool IsUppercase
        {
            get => _settingsProvider.GetSetting(UppercaseSetting);
            set
            {
                if (IsUppercase != value)
                {
                    _settingsProvider.SetSetting(UppercaseSetting, value);
                    OnPropertyChanged();
                    UpdateOutputCase();
                }
            }
        }

        internal HashingAlgorithmDisplayPair InputHashingAlgorithm
        {
            get
            {
                HashingAlgorithm settingValue = _settingsProvider.GetSetting(HashingAlgorithmSetting);
                return HashingAlgorithms.FirstOrDefault(x => x.Value == settingValue) ?? HashingAlgorithmDisplayPair.MD5;
            }

            set
            {
                if (InputHashingAlgorithm != value)
                {
                    _settingsProvider.SetSetting(HashingAlgorithmSetting, value.Value);
                    OnPropertyChanged();
                    if (!_hasCancelledCalculation)
                    {
                        Checksum();
                    }
                }
            }
        }

        internal int Progress
        {
            get => _progress;
            set => SetProperty(ref _progress, value);
        }

        internal IStorageFile? InputFile
        {
            get => _inputFile;
            set
            {
                if (InputFile != value)
                {
                    SetProperty(ref _inputFile, value);
                    Checksum();
                }
            }
        }

        internal bool ShouldDisplayProgress
        {
            get => _shouldDisplayProgress;
            set => SetProperty(ref _shouldDisplayProgress, value);
        }

        internal string? Output
        {
            get => _output;
            set => SetProperty(ref _output, value);
        }

        internal string? OutputComparer
        {
            get => _outputComparer;
            set => SetProperty(ref _outputComparer, value);
        }

        /// <summary>
        /// Get a list of supported Hashing Algorithms
        /// </summary>
        internal IReadOnlyList<HashingAlgorithmDisplayPair> HashingAlgorithms = new ObservableCollection<HashingAlgorithmDisplayPair>
        {
            HashingAlgorithmDisplayPair.MD5,
            HashingAlgorithmDisplayPair.SHA1,
            HashingAlgorithmDisplayPair.SHA256,
            HashingAlgorithmDisplayPair.SHA384,
            HashingAlgorithmDisplayPair.SHA512,
        };

        [ImportingConstructor]
        public CheckSumGeneratorToolViewModel(ISettingsProvider settingsProvider, IMarketingService marketingService)
        {
            _settingsProvider = settingsProvider;
            _marketingService = marketingService;
            FilesSelectedCommand = new RelayCommand<StorageFile[]>(ExecuteFilesSelectedCommand);
            CancelCommand = new RelayCommand(() => ExecuteCancelCommand(shouldRevertHashingAlgo: true));
        }

        #region FilesSelectedCommand

        public IRelayCommand<StorageFile[]> FilesSelectedCommand { get; }

        private void ExecuteFilesSelectedCommand(StorageFile[]? files)
        {
            if (files is not null)
            {
                Debug.Assert(files.Length == 1);
                InputFile = files[0];
            }
        }

        #endregion

        #region CancelCommand

        public IRelayCommand CancelCommand { get; }

        private void ExecuteCancelCommand(bool shouldRevertHashingAlgo)
        {
            lock (_lockObject)
            {
                _hasCancelledCalculation = true;

                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = new CancellationTokenSource();

                if (shouldRevertHashingAlgo && _outputHashingAlgorithm != null)
                {
                    InputHashingAlgorithm = _outputHashingAlgorithm;
                }

                _isCalculationInProgress = false;
                _hasCancelledCalculation = false;
                ShouldDisplayProgress = false;

                ResetProgress();
            }
        }

        #endregion

        private void UpdateOutputCase()
        {
            if (string.IsNullOrWhiteSpace(Output))
            {
                return;
            }

            if (IsUppercase)
            {
                Output = Output!.ToUpperInvariant();
            }
            else
            {
                Output = Output!.ToLowerInvariant();
            }
        }

        private void Checksum()
        {
            lock (_lockObject)
            {
                ExecuteCancelCommand(shouldRevertHashingAlgo: false);

                if (_computationTaskQueue.Count > 0 && _computationTaskQueue.Peek().IsCompleted)
                {
                    _computationTaskQueue.Dequeue();
                }

                _computationTaskQueue.Enqueue(CheckSumAsync());
            }
        }

        private async Task CheckSumAsync()
        {
            if (_isCalculationInProgress || InputFile is null)
            {
                return;
            }

            _isCalculationInProgress = true;

            await TaskScheduler.Default;

            if (_computationTaskQueue.Count > 1)
            {
                await _computationTaskQueue.Dequeue();
            }

            using Stream? fileStream = await InputFile.OpenStreamForReadAsync();

            await ThreadHelper.RunOnUIThreadAsync(() => ShouldDisplayProgress = HashingHelper.ComputeHashIterations(fileStream) > _displayAboveIterations);

            string? hashOutput = await CalculateFileHash(InputHashingAlgorithm.Value, fileStream);

            await ThreadHelper.RunOnUIThreadAsync(() =>
            {
                _isCalculationInProgress = false;
                ShouldDisplayProgress = false;

                if (hashOutput != null)
                {
                    Output = hashOutput;
                    _outputHashingAlgorithm = InputHashingAlgorithm;
                }

                if (!_toolSuccessfullyWorked)
                {
                    _toolSuccessfullyWorked = true;
                    _marketingService.NotifyToolSuccessfullyWorked();
                }
            });
        }

        private async Task<string?> CalculateFileHash(HashingAlgorithm inputHashingAlgorithm, Stream fileStream)
        {
            try
            {
                using HashAlgorithm? hashAlgo = CreateHashAlgorithm(inputHashingAlgorithm);

                long fileSize = fileStream.Length;
                int bufferSize;
                if (fileSize > int.MaxValue)
                {
                    string message = LanguageManager.Instance.Common.UnableOpenFile;
                    Exception exception = new FileLoadException(message);
                    throw new FileLoadException(message);
                }
                else
                {
                    bufferSize = (int)fileSize;
                }

                byte[]? fileHash = await HashingHelper.ComputeHashAsync(
                        hashAlgo,
                        fileStream,
                        new Progress<HashingProgress>(UpdateProgress),
                        _cancellationTokenSource.Token,
                        bufferSize);

                string? fileHashString = BitConverter
                    .ToString(fileHash)
                    .Replace("-", string.Empty);

                if (!IsUppercase)
                {
                    return fileHashString.ToLowerInvariant();
                }
                return fileHashString;
            }
            catch (OperationCanceledException)
            {
                return null;
            }
            catch (Exception ex)
            {
                Logger.LogFault("Checksum Generator", ex, $"Failed to calculate FileHash, algorithm used: {inputHashingAlgorithm}");
                return ex.Message;
            }
        }

        private void UpdateProgress(HashingProgress hashingProgress) =>
            ThreadHelper.RunOnUIThreadAsync(() => Progress = hashingProgress.GetPercentage()).ForgetSafely();

        private void ResetProgress() => ThreadHelper.RunOnUIThreadAsync(() => Progress = 0).ForgetSafely();

        private async Task<IStorageItem?> ExtractStorageItem(DataPackageView data)
        {
            if (data?.Contains(StandardDataFormats.StorageItems) != true)
            {
                return null;
            }

            IReadOnlyList<IStorageItem>? copiedFile = await data.GetStorageItemsAsync();
            if (copiedFile is null || copiedFile.Count != 1)
            {
                return null;
            }

            return copiedFile[0];
        }

        private HashAlgorithm CreateHashAlgorithm(HashingAlgorithm hashingAlgorithm) =>
            hashingAlgorithm switch
            {
                HashingAlgorithm.MD5 => MD5.Create(),
                HashingAlgorithm.SHA1 => SHA1.Create(),
                HashingAlgorithm.SHA256 => SHA256.Create(),
                HashingAlgorithm.SHA384 => SHA384.Create(),
                HashingAlgorithm.SHA512 => SHA512.Create(),
                _ => throw new ArgumentException("Hash Algorithm not supported", nameof(HashingAlgorithm))
            };
    }
}
