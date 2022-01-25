#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DevToys.Core.Threading;
using DevToys.Helpers;
using DevToys.Shared.Core;
using DevToys.Shared.Core.Threading;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;

namespace DevToys.ViewModels.Tools.ImageConverter
{
    internal sealed class ImageConversionWorkItem : ObservableRecipient
    {
        private string _fileSize = string.Empty;

        internal ImageConverterStrings Strings => LanguageManager.Instance.ImageConverter;

        internal StorageFile File { get; }

        internal string FileSize
        {
            get => _fileSize;
            private set
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                SetProperty(ref _fileSize, value);
            }
        }

        internal string? ErrorMessage { get; private set; }

        internal event EventHandler? DeleteItemRequested;

        internal ImageConversionWorkItem(StorageFile file)
        {
            Arguments.NotNull(file, nameof(file));

            File = file;

            DeleteCommand = new RelayCommand(ExecuteDeleteCommand);
            SaveCommand = new AsyncRelayCommand<ImageConverterToolViewModel>(ExecuteSaveCommandAsync);

            ComputePropertiesAsync(file).ForgetSafely();
        }

        #region DeleteCommand

        public IRelayCommand DeleteCommand { get; }

        private void ExecuteDeleteCommand()
        {
            DeleteItemRequested?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region SaveCommand

        public IAsyncRelayCommand SaveCommand { get; }

        private async Task ExecuteSaveCommandAsync(ImageConverterToolViewModel viewModel)
        {
            string? fileExtension = ImageHelper.GetExtension(viewModel.ConvertedFormat);

            var savePicker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.ComputerFolder
            };

            savePicker.FileTypeChoices.Add(
                fileExtension.Replace(".", string.Empty).ToUpperInvariant(),
                new List<string>() { fileExtension!.ToLowerInvariant() });

            StorageFile? newFile = await savePicker.PickSaveFileAsync();

            if (newFile is not null)
            {
                using (IRandomAccessStream outputStream = await newFile.OpenAsync(FileAccessMode.ReadWrite))
                {
                    BitmapEncoder? encoder = await ImageHelper.GetEncoderAsync(viewModel.ConvertedFormat, outputStream);

                    var bitmap = await DecodeImageAsync();
                    encoder.SetSoftwareBitmap(bitmap);
                    await encoder.FlushAsync();
                }

                DeleteCommand.Execute(this);
            }
        }

        #endregion

        private async Task ComputePropertiesAsync(StorageFile file)
        {
            await TaskScheduler.Default;

            var storageFileSize = (await file.GetBasicPropertiesAsync()).Size;
            var fileSize = StorageFileHelper.HumanizeFileSize(storageFileSize, Strings.FileSizeDisplay);
            await ThreadHelper.RunOnUIThreadAsync(() => FileSize = fileSize);
        }

        public async Task<SoftwareBitmap> DecodeImageAsync()
        {
            await TaskScheduler.Default;

            using (IRandomAccessStream stream = await File.OpenAsync(FileAccessMode.Read))
            {
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);
                return await decoder.GetSoftwareBitmapAsync();
            }
        }
    }
}
