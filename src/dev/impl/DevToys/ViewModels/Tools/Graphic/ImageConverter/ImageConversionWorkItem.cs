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
    internal sealed class ImageConversionWorkItem : ObservableRecipient, IDisposable
    {
        private string _fileSize = string.Empty;

        internal ImageConverterStrings Strings => LanguageManager.Instance.ImageConverter;

        internal string FileName { get; }

        internal string FilePath { get; }

        internal string DisplayName { get; }

        internal SoftwareBitmap? Bitmap { get; private set; }

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

            FileName = file.Name;
            FilePath = file.Path;
            DisplayName = file.DisplayName;

            DeleteCommand = new RelayCommand(ExecuteDeleteCommand);
            SaveCommand = new AsyncRelayCommand<ImageConverterToolViewModel>(ExecuteSaveCommandAsync);

            ComputePropertiesAsync(file).ForgetSafely();
            DecodeImageAsync(file).ForgetSafely();
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

                    encoder.SetSoftwareBitmap(Bitmap);
                    await encoder.FlushAsync();
                }

                Dispose();
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

        private async Task DecodeImageAsync(StorageFile file)
        {
            await TaskScheduler.Default;

            using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.Read))
            {
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);
                var bitmap = await decoder.GetSoftwareBitmapAsync();
                await ThreadHelper.RunOnUIThreadAsync(() => Bitmap = bitmap);
            }
        }

        public void Dispose()
        {
            if (Bitmap is not null)
            {
                Bitmap.Dispose();
            }
        }
    }
}
