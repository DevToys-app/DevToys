#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DevToys.Core.Threading;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace DevToys.UI.Controls
{
    public sealed partial class ImagePreview : UserControl
    {
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
              typeof(StorageFile),
              typeof(ImagePreview),
              new PropertyMetadata(null, OnSourcePropertyChangedCalled));

        public StorageFile? Source
        {
            get => (StorageFile?)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        public static readonly DependencyProperty ImageSourceProperty
          = DependencyProperty.Register(
              nameof(ImageSource),
              typeof(ImageSource),
              typeof(ImagePreview),
              new PropertyMetadata(null));

        public ImageSource? ImageSource
        {
            get => (ImageSource?)GetValue(ImageSourceProperty);
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
            await Launcher.LaunchFileAsync(Source);
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dataPackage = new DataPackage { RequestedOperation = DataPackageOperation.Copy };
                dataPackage.SetBitmap(RandomAccessStreamReference.CreateFromFile(Source));

                Clipboard.SetContentWithOptions(dataPackage, new ClipboardContentOptions() { IsAllowedInHistory = true, IsRoamable = true });
                Clipboard.Flush(); // This method allows the content to remain available after the application shuts down.
            }
            catch (Exception ex)
            {
                Core.Logger.LogFault("Failed to copy from image preview", ex);
            }
        }

        private async void SaveAsButton_Click(object sender, RoutedEventArgs e)
        {
            string? fileExtension = Path.GetExtension(Source!.Path);

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
                StorageFile tempCompressedFile = Source;
                await tempCompressedFile.CopyAndReplaceAsync(newFile);
            }
        }

        private static void OnSourcePropertyChangedCalled(DependencyObject sender, DependencyPropertyChangedEventArgs eventArgs)
        {
            var imagePreview = ((ImagePreview)sender);
            if (imagePreview.Source is null)
            {
                imagePreview.ImageSource = null;
            }
            else
            {
                ThreadHelper.RunOnUIThreadAsync(async () =>
                {
                    using IRandomAccessStream fileStream = await imagePreview.Source.OpenAsync(FileAccessMode.Read);
                    if (imagePreview.Source.FileType.ToLowerInvariant() == ".svg")
                    {
                        var svgImage = new SvgImageSource();
                        imagePreview.ImageSource = svgImage;
                        svgImage.RasterizePixelHeight = imagePreview.ActualHeight * 2;
                        svgImage.RasterizePixelWidth = imagePreview.ActualWidth * 2;
                        _ = svgImage.SetSourceAsync(fileStream);
                    }
                    else
                    {
                        var bitmapImage = new BitmapImage();
                        imagePreview.ImageSource = bitmapImage;
                        _ = bitmapImage.SetSourceAsync(fileStream);
                    }
                });
            }
        }
    }
}
