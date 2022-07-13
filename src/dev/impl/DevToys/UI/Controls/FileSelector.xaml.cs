#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Toolkit.Mvvm.Input;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace DevToys.UI.Controls
{
    public sealed partial class FileSelector : UserControl
    {
        private readonly List<StorageFile> _cachedFilesToDeleteOnShutdown = new();
        private string[] _allowedFileExtensions = new[] { "*" };

        /// <summary>
        /// Gets of sets the command to invoke when the user selected one or several valid files.
        /// </summary>
        public static readonly DependencyProperty FilesSelectedCommandProperty
            = DependencyProperty.Register(
                nameof(FilesSelectedCommand),
                typeof(IRelayCommand<StorageFile[]>),
                typeof(FileSelector),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets of sets the command to invoke when the user selected one or several valid files.
        /// </summary>
        public IRelayCommand<StorageFile[]>? FilesSelectedCommand
        {
            get => (IRelayCommand<StorageFile[]>?)GetValue(FilesSelectedCommandProperty);
            set => SetValue(FilesSelectedCommandProperty, value);
        }

        /// <summary>
        /// Gets or sets whether the user can select more than 1 file.
        /// </summary>
        public static readonly DependencyProperty AllowMultipleFileSelectionProperty
            = DependencyProperty.Register(
                nameof(AllowMultipleFileSelection),
                typeof(bool),
                typeof(FileSelector),
                new PropertyMetadata(false, AllowedFileExtensionsPropertyChangedCallback));

        /// <summary>
        /// Gets or sets whether the user can select more than 1 file.
        /// </summary>
        public bool AllowMultipleFileSelection
        {
            get => (bool)GetValue(AllowMultipleFileSelectionProperty);
            set => SetValue(AllowMultipleFileSelectionProperty, value);
        }

        /// <summary>
        /// Gets or sets a list of file extension, separated by a semi-colon, that should be accepted by the control.
        /// An empty list means any file is accepted.
        /// </summary>
        public static readonly DependencyProperty AllowedFileExtensionsProperty
            = DependencyProperty.Register(
                nameof(AllowedFileExtensions),
                typeof(string),
                typeof(FileSelector),
                new PropertyMetadata("*", AllowedFileExtensionsPropertyChangedCallback));

        /// <summary>
        /// Gets or sets a list of file extension, separated by a semi-colon, that should be accepted by the control.
        /// An empty list means any file is accepted.
        /// </summary>
        public string AllowedFileExtensions
        {
            get => (string)GetValue(AllowedFileExtensionsProperty);
            set => SetValue(AllowedFileExtensionsProperty, value);
        }

        /// <summary>
        /// Gets or sets whether the user can paste an image from the clipboard in addition to files.
        /// </summary>
        public static readonly DependencyProperty AllowPasteImageProperty
            = DependencyProperty.Register(
                nameof(AllowPasteImage),
                typeof(bool),
                typeof(FileSelector),
                new PropertyMetadata(false));

        /// <summary>
        /// Gets or sets whether the user can paste an image from the clipboard in addition to files.
        /// </summary>
        public bool AllowPasteImage
        {
            get => (bool)GetValue(AllowPasteImageProperty);
            set => SetValue(AllowPasteImageProperty, value);
        }

        public static readonly DependencyProperty IsSelectFilesAreaHighlightedProperty
            = DependencyProperty.Register(
                nameof(IsSelectFilesAreaHighlighted),
                typeof(bool),
                typeof(FileSelector),
                new PropertyMetadata(false));

        public bool IsSelectFilesAreaHighlighted
        {
            get => (bool)GetValue(IsSelectFilesAreaHighlightedProperty);
            private set => SetValue(IsSelectFilesAreaHighlightedProperty, value);
        }

        public static readonly DependencyProperty HasInvalidFilesSelectedProperty
            = DependencyProperty.Register(
                nameof(HasInvalidFilesSelected),
                typeof(bool),
                typeof(FileSelector),
                new PropertyMetadata(false));

        public bool HasInvalidFilesSelected
        {
            get => (bool)GetValue(HasInvalidFilesSelectedProperty);
            private set => SetValue(HasInvalidFilesSelectedProperty, value);
        }

        public static readonly DependencyProperty DragDropInstructionProperty
            = DependencyProperty.Register(
                nameof(DragDropInstruction),
                typeof(string),
                typeof(FileSelector),
                new PropertyMetadata(string.Empty));

        public string DragDropInstruction
        {
            get => (string)GetValue(DragDropInstructionProperty);
            private set => SetValue(DragDropInstructionProperty, value);
        }

        public static readonly DependencyProperty HasInvalidFilesSelectedIndicationProperty
            = DependencyProperty.Register(
                nameof(HasInvalidFilesSelectedIndication),
                typeof(string),
                typeof(FileSelector),
                new PropertyMetadata(string.Empty));

        public string HasInvalidFilesSelectedIndication
        {
            get => (string)GetValue(HasInvalidFilesSelectedIndicationProperty);
            private set => SetValue(HasInvalidFilesSelectedIndicationProperty, value);
        }

        public FileSelector()
        {
            InitializeComponent();
            App.Current.Suspending += OnAppSuspending;

            UpdateInstructionStrings();
        }

        private async void Grid_DragOver(object sender, DragEventArgs e)
        {
            DragOperationDeferral? deferral = e.GetDeferral();
            if (e!.DataView.Contains(StandardDataFormats.StorageItems))
            {
                // This line may cause a hang, but we have no choice since we can't afford to make this method async
                // since the parent caller won't be able to see what changed in DragEventArgs since it won't
                // wait for the execution to end.
                IReadOnlyList<IStorageItem>? storageItems = await e.DataView.GetStorageItemsAsync();
                if (storageItems is not null
                    && ((AllowMultipleFileSelection && storageItems.Count > 0)
                        || (!AllowMultipleFileSelection && storageItems.Count == 1)))
                {
                    e.AcceptedOperation = DataPackageOperation.Copy;
                    e.Handled = false;
                }
            }

            IsSelectFilesAreaHighlighted = true;
            HasInvalidFilesSelected = false;
            deferral?.Complete();
        }

        private void Grid_DragLeave(object sender, DragEventArgs e)
        {
            IsSelectFilesAreaHighlighted = false;
            HasInvalidFilesSelected = false;
        }

        private async void Grid_Drop(object sender, DragEventArgs e)
        {
            DragOperationDeferral? deferral = e.GetDeferral();
            IsSelectFilesAreaHighlighted = false;
            if (!e!.DataView.Contains(StandardDataFormats.StorageItems))
            {
                return;
            }

            IReadOnlyList<IStorageItem>? storageItems = await e.DataView.GetStorageItemsAsync();
            await TreatStorageItemsAsync(storageItems);
            deferral?.Complete();
        }

        private async void BrowseFilesHyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            HasInvalidFilesSelected = false;

            IReadOnlyList<StorageFile>? files = null;
            var filePicker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.List
            };

            for (int i = 0; i < _allowedFileExtensions.Length; i++)
            {
                filePicker.FileTypeFilter.Add(_allowedFileExtensions[i]);
            }

            if (AllowMultipleFileSelection)
            {
                files = await filePicker.PickMultipleFilesAsync();
            }
            else
            {
                StorageFile? file = await filePicker.PickSingleFileAsync();
                if (file is not null)
                {
                    files = new List<StorageFile>() { file };
                }
            }

            if (files is not null && files.Count > 0)
            {
                FilesSelectedCommand?.Execute(files.ToArray());
            }
        }

        private async void BrowseFoldersHyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            HasInvalidFilesSelected = false;

            var files = new List<StorageFile>();
            var folderPicker = new FolderPicker
            {
                ViewMode = PickerViewMode.List
            };

            folderPicker.FileTypeFilter.Add("*");

            StorageFolder? folder = await folderPicker.PickSingleFolderAsync();

            if (folder is not null)
            {
                foreach (StorageFile file in await folder.GetFilesAsync())
                {
                    if (_allowedFileExtensions.Any(ext => string.Equals(ext, "*", StringComparison.Ordinal) || string.Equals(ext, file.FileType, StringComparison.OrdinalIgnoreCase)))
                    {
                        files.Add(file);
                    }
                }
            }

            if (files.Count > 0)
            {
                FilesSelectedCommand?.Execute(files.ToArray());
            }
        }

        private async void PasteHyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            HasInvalidFilesSelected = false;

            DataPackageView? dataPackageView = Clipboard.GetContent();
            if (dataPackageView is not null)
            {
                if (AllowPasteImage && dataPackageView.Contains(StandardDataFormats.Bitmap))
                {
                    IRandomAccessStreamReference? imageReceived = await dataPackageView.GetBitmapAsync();
                    if (imageReceived is not null)
                    {
                        using IRandomAccessStreamWithContentType imageStream = await imageReceived.OpenReadAsync();
                        StorageFolder localCacheFolder = ApplicationData.Current.LocalCacheFolder;
                        StorageFile storageFile = await localCacheFolder.CreateFileAsync($"{Guid.NewGuid()}.jpeg", CreationCollisionOption.ReplaceExisting);

                        using (IRandomAccessStream? stream = await storageFile.OpenAsync(FileAccessMode.ReadWrite))
                        {
                            await imageStream.AsStreamForRead().CopyToAsync(stream.AsStreamForWrite());
                        }

                        _cachedFilesToDeleteOnShutdown.Add(storageFile);
                        FilesSelectedCommand?.Execute(new[] { storageFile });
                    }
                }
                else if (dataPackageView.Contains(StandardDataFormats.StorageItems))
                {
                    IReadOnlyList<IStorageItem>? storageItems = await dataPackageView.GetStorageItemsAsync();
                    await TreatStorageItemsAsync(storageItems);
                }
            }
        }

        private async void OnAppSuspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            foreach (StorageFile cacheFile in _cachedFilesToDeleteOnShutdown)
            {
                if (File.Exists(cacheFile.Path))
                {
                    await cacheFile.DeleteAsync(StorageDeleteOption.PermanentDelete);
                }
            }
        }

        private async Task TreatStorageItemsAsync(IReadOnlyList<IStorageItem>? storageItems)
        {
            if (storageItems is null || storageItems.Count == 0)
            {
                return;
            }

            var files = new HashSet<StorageFile>();

            if (!AllowMultipleFileSelection)
            {
                if (storageItems.Count == 1)
                {
                    IStorageItem storageItem = storageItems[0];
                    if (storageItem is StorageFile file)
                    {
                        if (_allowedFileExtensions.Any(ext => string.Equals(ext, "*", StringComparison.Ordinal) || string.Equals(ext, file.FileType, StringComparison.OrdinalIgnoreCase)))
                        {
                            files.Add(file);
                            FilesSelectedCommand?.Execute(files.ToArray());
                            return;
                        }
                    }
                }

                HasInvalidFilesSelected = true;
                return;
            }
            else
            {
                for (int i = 0; i < storageItems.Count; i++)
                {
                    IStorageItem storageItem = storageItems[i];
                    if (storageItem is StorageFile file)
                    {
                        if (_allowedFileExtensions.Any(ext => string.Equals(ext, "*", StringComparison.Ordinal) || string.Equals(ext, file.FileType, StringComparison.OrdinalIgnoreCase)))
                        {
                            files.Add(file);
                        }
                        else
                        {
                            HasInvalidFilesSelected = true;
                        }
                    }
                    else if (storageItem is StorageFolder folder)
                    {
                        foreach (StorageFile innerFile in await folder.GetFilesAsync())
                        {
                            if (_allowedFileExtensions.Any(ext => string.Equals(ext, "*", StringComparison.Ordinal) || string.Equals(ext, innerFile.FileType, StringComparison.OrdinalIgnoreCase)))
                            {
                                files.Add(innerFile);
                            }
                        }
                    }
                    else
                    {
                        HasInvalidFilesSelected = true;
                    }
                }

                if (files.Count > 0)
                {
                    FilesSelectedCommand?.Execute(files.ToArray());
                }
                else
                {
                    HasInvalidFilesSelected = true;
                }
            }
        }

        private void UpdateInstructionStrings()
        {
            string? allowedFileExtensions = AllowedFileExtensions;

            if (string.IsNullOrWhiteSpace(allowedFileExtensions))
            {
                _allowedFileExtensions = new[] { "*" };
            }
            else
            {
                var newListOfExtensions = new HashSet<string>();
                string[] extensions = allowedFileExtensions!.Split(';', System.StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < extensions.Length; i++)
                {
                    string extension = extensions[i];
                    extension = extension.Trim().ToLowerInvariant();
                    if (!string.Equals("*", extension, StringComparison.OrdinalIgnoreCase)
                        && !extension.StartsWith("."))
                    {
                        extension = "." + extension;
                    }

                    newListOfExtensions.Add(extension);
                }

                _allowedFileExtensions = newListOfExtensions.ToArray();
            }


            if (_allowedFileExtensions is null
                || _allowedFileExtensions.Length == 0
                || (_allowedFileExtensions.Length == 1 && string.Equals(_allowedFileExtensions[0], "*", StringComparison.Ordinal)))
            {
                if (AllowMultipleFileSelection)
                {
                    DragDropInstruction = LanguageManager.Instance.Common.FileSelectorDragDropAnyFiles;
                }
                else
                {
                    DragDropInstruction = LanguageManager.Instance.Common.FileSelectorDragDropAnyFile;
                }
            }
            else if (_allowedFileExtensions.Length == 1)
            {
                string extensionsString
                    = _allowedFileExtensions[0]
                    .Replace(".", string.Empty)
                    .ToUpperInvariant();

                if (AllowMultipleFileSelection)
                {
                    DragDropInstruction
                        = string.Format(
                            LanguageManager.Instance.Common.FileSelectorDragDropAnySpecificFiles,
                            extensionsString);
                }
                else
                {
                    DragDropInstruction
                        = string.Format(
                            LanguageManager.Instance.Common.FileSelectorDragDropAnySpecificFile,
                            extensionsString);
                }

                HasInvalidFilesSelectedIndication
                        = string.Format(
                            LanguageManager.Instance.Common.FileSelectorInvalidSelectedFiles,
                            extensionsString);
            }
            else
            {
                string extensionsString
                    = string.Join(", ", _allowedFileExtensions)
                    .Replace(".", string.Empty)
                    .ToUpperInvariant();

                if (AllowMultipleFileSelection)
                {
                    DragDropInstruction
                        = string.Format(
                            LanguageManager.Instance.Common.FileSelectorDragDropAnySpecificFiles,
                            extensionsString);
                }
                else
                {
                    DragDropInstruction
                        = string.Format(
                            LanguageManager.Instance.Common.FileSelectorDragDropAnySpecificFile,
                            extensionsString);
                }

                HasInvalidFilesSelectedIndication
                        = string.Format(
                            LanguageManager.Instance.Common.FileSelectorInvalidSelectedFiles,
                            extensionsString);
            }
        }

        private static void AllowedFileExtensionsPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (FileSelector)d;
            control.UpdateInstructionStrings();
        }
    }
}
