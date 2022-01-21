#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Composition;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using DevToys.Api.Core;
using DevToys.Api.Core.Settings;
using DevToys.Api.Tools;
using DevToys.Core.Threading;
using DevToys.Models;
using DevToys.Shared.Core;
using DevToys.Views.Tools.CheckSumGenerator;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.FileProperties;
using DevToys.Shared.Core.Threading;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using System.Linq;
using DevToys.Core;
using Clipboard = Windows.ApplicationModel.DataTransfer.Clipboard;
using DevToys.Helpers;
using System.Threading;

namespace DevToys.ViewModels.Tools.CheckSumGenerator
{
    [Export(typeof(CheckSumGeneratorToolViewModel))]
    public sealed class CheckSumGeneratorToolViewModel : ObservableRecipient, IToolViewModel
    {
        //TODO: We could use some basic caching

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

        private readonly object _lockObject = new();
        private CancellationTokenSource _cancellationTokenSource = new();
        private bool _isSelectFilesAreaHighlithed;
        private bool _isCalculationInProgress;
        private IStorageFile? _inputFile;
        private int _progress;
        private string? _output;
        private string? _outputComparer;
        private bool _toolSuccessfullyWorked;
        private bool _hasCancelledCalculation;
        private HashingAlgorithmDisplayPair? _outputHashingAlgorithm;

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
                    if(!_hasCancelledCalculation)
                    {
                        CheckSum();
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
                    CheckSum();
                }
            }
        }

        internal bool IsSelectFilesAreaHighlighted
        {
            get => _isSelectFilesAreaHighlithed;
            set => SetProperty(ref _isSelectFilesAreaHighlithed, value);
        }

        internal bool IsCalculationInProgress
        {
            get => _isCalculationInProgress;
            set => SetProperty(ref _isCalculationInProgress, value);
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
            SelectFilesAreaDragOverCommand = new RelayCommand<DragEventArgs>(ExecuteSelectFilesAreaDragOverCommand);
            SelectFilesAreaDragLeaveCommand = new RelayCommand<DragEventArgs>(ExecuteSelectFilesAreaDragLeaveCommand);
            SelectFilesAreaDragDropCommand = new AsyncRelayCommand<DragEventArgs>(ExecuteSelectFilesAreaDragDropCommandAsync);
            SelectFilesBrowseCommand = new AsyncRelayCommand(ExecuteSelectFilesBrowseCommandAsync);
            SelectFilesPasteCommand = new AsyncRelayCommand(ExecuteSelectFilesPasteCommandAsync);
            CancelCommand = new RelayCommand(ExecuteCancelCommand);
        }

        #region SelectFilesAreaDragOverCommand

        public IRelayCommand<DragEventArgs> SelectFilesAreaDragOverCommand { get; }

        private void ExecuteSelectFilesAreaDragOverCommand(DragEventArgs? parameters)
        {
            Arguments.NotNull(parameters, nameof(parameters));

            if (parameters!.DataView.Contains(StandardDataFormats.StorageItems))
            {
                parameters.AcceptedOperation = DataPackageOperation.Copy;
                parameters.Handled = false;
            }

            IsSelectFilesAreaHighlighted = true;
        }

        #endregion

        #region SelectFilesAreaDragLeaveCommand

        public IRelayCommand<DragEventArgs> SelectFilesAreaDragLeaveCommand { get; }

        private void ExecuteSelectFilesAreaDragLeaveCommand(DragEventArgs? parameters)
        {
            IsSelectFilesAreaHighlighted = false;
        }

        #endregion

        #region SelectFilesAreaDragDropCommand

        public IAsyncRelayCommand<DragEventArgs> SelectFilesAreaDragDropCommand { get; }

        private async Task ExecuteSelectFilesAreaDragDropCommandAsync(DragEventArgs? parameters)
        {
            Arguments.NotNull(parameters, nameof(parameters));

            await ThreadHelper.RunOnUIThreadAsync(async () =>
            {
                IsSelectFilesAreaHighlighted = false;

                IStorageItem? storageItem = await ExtractStorageItem(parameters!.DataView);
                if (storageItem is StorageFile file)
                {
                    InputFile = file;
                }
            }).ConfigureAwait(false);
        }

        #endregion

        #region SelectFilesBrowseCommand

        public IAsyncRelayCommand SelectFilesBrowseCommand { get; }

        private async Task ExecuteSelectFilesBrowseCommandAsync()
        {
            var filePicker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.List,
                SuggestedStartLocation = PickerLocationId.ComputerFolder,
            };
            filePicker.FileTypeFilter.Add("*");

            await ThreadHelper.RunOnUIThreadAsync(async () =>
            {
                StorageFile file = await filePicker.PickSingleFileAsync();
                if (file != null)
                {
                    InputFile = file;
                }
            });
        }

        #endregion

        #region SelectFilesPasteCommand

        public IAsyncRelayCommand SelectFilesPasteCommand { get; }

        private async Task ExecuteSelectFilesPasteCommandAsync()
        {
            await ThreadHelper.RunOnUIThreadAsync(async () =>
            {
                DataPackageView? dataPackageView = Clipboard.GetContent();

                IStorageItem? storageItem = await ExtractStorageItem(dataPackageView);
                if (storageItem is StorageFile file)
                {
                    InputFile = file;
                }
            }).ConfigureAwait(false);
        }

        #endregion

        #region CancelCommand

        public IRelayCommand CancelCommand { get; }

        private void ExecuteCancelCommand()
        {
            lock (_lockObject)
            {
                _hasCancelledCalculation = true;
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();

                _cancellationTokenSource = new CancellationTokenSource();
                
                if(_outputHashingAlgorithm != null)
                {
                    InputHashingAlgorithm = _outputHashingAlgorithm;
                }

                IsCalculationInProgress = false;
                ResetProgress();
                _hasCancelledCalculation = false;
            }
        }

        #endregion

        private void UpdateOutputCase()
        {
            if(string.IsNullOrWhiteSpace(Output))
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

        private void CheckSum()
        {
            lock (_lockObject)
            {
                ResetProgress();
                CheckSumAsync().Forget();
            }
        }

        private async Task CheckSumAsync()
        {
            if (IsCalculationInProgress)
            {
                return;
            }

            IsCalculationInProgress = true;

            string? hashOutput = await CalculateFileHash(InputHashingAlgorithm.Value, InputFile);

            ThreadHelper.RunOnUIThreadAsync(() =>
            {
                if(!string.IsNullOrWhiteSpace(hashOutput))
                {
                    Output = hashOutput;
                    _outputHashingAlgorithm = InputHashingAlgorithm;
                }
                
                IsCalculationInProgress = false;

                if (!_toolSuccessfullyWorked)
                {
                    _toolSuccessfullyWorked = true;
                    _marketingService.NotifyToolSuccessfullyWorked();
                }
            }).ForgetSafely();
        }

        private async Task<string?> CalculateFileHash(HashingAlgorithm inputHashingAlgorithm, IStorageFile? inputFile)
        {
            try
            {
                if(inputFile is null)
                {
                    return string.Empty;
                }

                BasicProperties? fileProps = await inputFile.GetBasicPropertiesAsync();
                if (fileProps is null || fileProps.Size == 0)
                {
                    return string.Empty;
                }

                using var hashAlgo = HashAlgorithm.Create(inputHashingAlgorithm.ToString());
                Stream? fileStream = await inputFile.OpenStreamForReadAsync();
                
                byte[]? fileHash = await HashingHelper.ComputeHashAsync(
                    hashAlgo,
                    fileStream,
                    new Progress<HashingProgress>(UpdateProgress),
                    _cancellationTokenSource.Token);

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
                Logger.LogFault("CheckSum Generator", ex, $"Failed to calculate FileHash, algorithm used: {inputHashingAlgorithm}");
                return ex.Message;
            }
        }

        private void UpdateProgress(HashingProgress hashingProgress) =>
            ThreadHelper.RunOnUIThreadAsync(() => Progress = hashingProgress.GetPercentage())
                .ForgetSafely();

        private void ResetProgress() =>
            ThreadHelper.RunOnUIThreadAsync(() => Progress = 0)
                .ForgetSafely();

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
    }
}
