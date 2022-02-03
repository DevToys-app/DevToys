#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using DevToys.Core;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace DevToys.Views.Tools.ColorBlindnessSimulator
{
    public sealed partial class ImagePreview : UserControl
    {
        public static readonly DependencyProperty StringsProperty
          = DependencyProperty.Register(
              nameof(Strings),
              typeof(ColorBlindnessSimulatorStrings),
              typeof(ImagePreview),
              new PropertyMetadata(LanguageManager.Instance.ColorBlindnessSimulator));

        public ColorBlindnessSimulatorStrings Strings
        {
            get => (ColorBlindnessSimulatorStrings)GetValue(StringsProperty);
            private set => SetValue(StringsProperty, value);
        }

        public static readonly DependencyProperty HeaderProperty
          = DependencyProperty.Register(
              nameof(Header),
              typeof(string),
              typeof(ImagePreview),
              new PropertyMetadata(null));

        public string? Header
        {
            get => (string?)GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        public static readonly DependencyProperty SourceProperty
          = DependencyProperty.Register(
              nameof(Source),
              typeof(Uri),
              typeof(ImagePreview),
              new PropertyMetadata(null, OnSourcePropertyChangedCalled));

        public Uri? Source
        {
            get => (Uri?)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        public static readonly DependencyProperty ImageSourceProperty
          = DependencyProperty.Register(
              nameof(ImageSource),
              typeof(BitmapSource),
              typeof(ImagePreview),
              new PropertyMetadata(null));

        public BitmapSource? ImageSource
        {
            get => (BitmapSource?)GetValue(ImageSourceProperty);
            private set => SetValue(ImageSourceProperty, value);
        }

        public ImagePreview()
        {
            InitializeComponent();
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width < CommandsToolBar.ActualWidth + 100)
            {
                CommandsToolBar.Visibility = Visibility.Collapsed;
            }
            else
            {
                CommandsToolBar.Visibility = Visibility.Visible;
            }
        }

        private void OutputFitSize_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            OutputRenderer.Height = OutputFitSize.ActualHeight;
            OutputRenderer.Width = OutputFitSize.ActualWidth;
        }

        private async void ViewButton_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchFileAsync(await StorageFile.GetFileFromPathAsync(Source!.OriginalString));
        }

        private async void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dataPackage = new DataPackage { RequestedOperation = DataPackageOperation.Copy };
                dataPackage.SetBitmap(RandomAccessStreamReference.CreateFromFile(await StorageFile.GetFileFromPathAsync(Source!.OriginalString)));
                Clipboard.SetContentWithOptions(dataPackage, new ClipboardContentOptions() { IsAllowedInHistory = true, IsRoamable = true });
                Clipboard.Flush(); // This method allows the content to remain available after the application shuts down.
            }
            catch (Exception ex)
            {
                Logger.LogFault("Failed to copy from image preview", ex);
            }
        }

        private async void SaveAsButton_Click(object sender, RoutedEventArgs e)
        {
            string? fileExtension = Path.GetExtension(Source!.OriginalString);

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
                StorageFile tempCompressedFile = await StorageFile.GetFileFromPathAsync(Source!.OriginalString);
                await tempCompressedFile.CopyAndReplaceAsync(newFile);
            }
        }

        private static void OnSourcePropertyChangedCalled(DependencyObject sender, DependencyPropertyChangedEventArgs eventArgs)
        {
            var imagePreview = ((ImagePreview)sender);
            imagePreview.ImageSource = new BitmapImage(imagePreview.Source);
        }
    }
}
