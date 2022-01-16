#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DevToys.Core.Threading;
using DevToys.Helpers;
using DevToys.Shared.Core;
using DevToys.Shared.Core.Threading;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;

namespace DevToys.ViewModels.Tools.ImageConverter
{
    internal sealed class ImageConversionWorkItem : ObservableRecipient
    {
        private static readonly string[] SizesStrings
            = {
                LanguageManager.Instance.Common.Bytes,
                LanguageManager.Instance.Common.Kilobytes,
                LanguageManager.Instance.Common.Megabytes,
                LanguageManager.Instance.Common.Gigabytes,
                LanguageManager.Instance.Common.Terabytes
            };

        private string _fileSize = string.Empty;

        internal ImageConverterStrings Strings => LanguageManager.Instance.ImageConverter;

        internal string FileName { get; }

        internal string FilePath { get; }

        internal SoftwareBitmap Bitmap { get; }

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

            using (IRandomAccessStream stream = Task.Run(async () => await file.OpenAsync(FileAccessMode.Read)).Result)
            {
                BitmapDecoder decoder = Task.Run(async () => await BitmapDecoder.CreateAsync(stream)).Result;
                Bitmap = Task.Run(async () => await decoder.GetSoftwareBitmapAsync()).Result;
            }

            DeleteCommand = new RelayCommand(ExecuteDeleteCommand);
            SaveCommand = new AsyncRelayCommand<ImageConverterToolViewModel>(ExecuteSaveCommandAsync);
            ShowErrorMessageCommand = new AsyncRelayCommand(ExecuteShowErrorMessageCommandAsync);

            ComputePropertiesAsync(file).Forget();
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
            string? fileExtension = Path.GetExtension(FilePath);

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
                    BitmapEncoder encoder = await BitmapEncoder.CreateAsync(ImageHelper.GetEncoderGuid(viewModel.ConvertedFormat), outputStream);
                    encoder.SetSoftwareBitmap(Bitmap);
                    await encoder.FlushAsync();
                }

                DeleteCommand.Execute(this);
            }
        }

        #endregion

        #region ShowErrorMessageCommand

        public IAsyncRelayCommand ShowErrorMessageCommand { get; }

        private async Task ExecuteShowErrorMessageCommandAsync()
        {
            var dialog = new ContentDialog
            {
                Title = Strings.DetailsTitle,
                CloseButtonText = Strings.OK,
                DefaultButton = ContentDialogButton.Close,
                Content = ErrorMessage!
            };

            await dialog.ShowAsync();
        }

        #endregion

        private async Task ComputePropertiesAsync(StorageFile file)
        {
            BasicProperties fileProperties = await file.GetBasicPropertiesAsync();

            string fileSize = HumanizeFileSize(fileProperties.Size);

            await ThreadHelper.RunOnUIThreadAsync(() =>
            {
                FileSize = fileSize;
            });
        }

        private string HumanizeFileSize(double fileSize)
        {
            int order = 0;
            while (fileSize >= 1024 && order < SizesStrings.Length - 1)
            {
                order++;
                fileSize /= 1024;
            }

            string fileSizeString = string.Format(Strings.FileSizeDisplay, fileSize, SizesStrings[order]);
            return fileSizeString;
        }
    }
}
