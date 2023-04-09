using CommunityToolkit.Mvvm.Messaging;
using DevToys.Api;
using DevToys.Core.Models;
using DevToys.Core.Tools;
using DevToys.Localization.Strings.ToolPage;
using DevToys.UI.Framework.Converters;
using DevToys.UI.Framework.Helpers;
using DevToys.UI.Framework.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;
using Uno.Extensions;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;

namespace DevToys.UI.Framework.Controls.GuiTool;

[ContentProperty(Name = nameof(InnerEditor))]
public sealed partial class UITextInputHeader : UserControl
{
    private readonly ILogger _logger;
    private readonly SmartDetectionService _smartDetectionService;
    private readonly DisposableSemaphore _disposableSemaphore = new();
    private readonly StringToStaticResourceConverter _stringToStaticResourceConverter = new();

    private CancellationTokenSource? _cancellationTokenSource;

    public UITextInputHeader()
    {
        this.InitializeComponent();

        _logger = this.Log();
        _smartDetectionService = Parts.MefProvider.Import<SmartDetectionService>();

        Loaded += UITextInputHeader_Loaded;
        Unloaded += UITextInputHeader_Unloaded;
    }

    internal IUISinglelineTextInput UITextInput => (IUISinglelineTextInput)DataContext;

    public static readonly DependencyProperty TitleProperty
       = DependencyProperty.Register(
           nameof(Title),
           typeof(string),
           typeof(UITextInputHeader),
           new PropertyMetadata(
               string.Empty,
               (d, e) =>
               {
                   AutomationProperties.SetName(d, (string)e.NewValue);
               }));

    public string? Title
    {
        get => (string?)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly DependencyProperty IsReadOnlyProperty
       = DependencyProperty.Register(
           nameof(IsReadOnly),
           typeof(bool),
           typeof(UITextInputHeader),
           new PropertyMetadata(false));

    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    public static readonly DependencyProperty CanCopyWhenEditableProperty
       = DependencyProperty.Register(
           nameof(CanCopyWhenEditable),
           typeof(bool),
           typeof(UITextInputHeader),
           new PropertyMetadata(false));

    public bool CanCopyWhenEditable
    {
        get => (bool)GetValue(CanCopyWhenEditableProperty);
        set => SetValue(CanCopyWhenEditableProperty, value);
    }

    public static readonly DependencyProperty IsReadOnlyOrCanCopyWhenEditableProperty
       = DependencyProperty.Register(
           nameof(IsReadOnlyOrCanCopyWhenEditable),
           typeof(bool),
           typeof(UITextInputHeader),
           new PropertyMetadata(false));

    public bool IsReadOnlyOrCanCopyWhenEditable
    {
        get => (bool)GetValue(IsReadOnlyOrCanCopyWhenEditableProperty);
        set => SetValue(IsReadOnlyOrCanCopyWhenEditableProperty, value);
    }

    public static readonly DependencyProperty InnerEditorProperty
        = DependencyProperty.Register(
            nameof(InnerEditor),
            typeof(object),
            typeof(UITextInputHeader),
            new PropertyMetadata(null));

    public object? InnerEditor
    {
        get => GetValue(InnerEditorProperty);
        set => SetValue(InnerEditorProperty, value);
    }

    public static readonly DependencyProperty IsDraggingHoverProperty
       = DependencyProperty.Register(
           nameof(IsDraggingHover),
           typeof(bool),
           typeof(UITextInputHeader),
           new PropertyMetadata(false));

    public bool IsDraggingHover
    {
        get => (bool)GetValue(IsDraggingHoverProperty);
        set => SetValue(IsDraggingHoverProperty, value);
    }

    public static readonly DependencyProperty IsLoadingFileProperty
       = DependencyProperty.Register(
           nameof(IsLoadingFile),
           typeof(bool),
           typeof(UITextInputHeader),
           new PropertyMetadata(false));

    public bool IsLoadingFile
    {
        get => (bool)GetValue(IsLoadingFileProperty);
        set => SetValue(IsLoadingFileProperty, value);
    }

    public static readonly DependencyProperty ToolsDetectedBySmartDetectionProperty
       = DependencyProperty.Register(
           nameof(ToolsDetectedBySmartDetection),
           typeof(bool),
           typeof(UITextInputHeader),
           new PropertyMetadata(false));

    public bool ToolsDetectedBySmartDetection
    {
        get => (bool)GetValue(ToolsDetectedBySmartDetectionProperty);
        set => SetValue(ToolsDetectedBySmartDetectionProperty, value);
    }

    private void UITextInputHeader_Loaded(object sender, RoutedEventArgs e)
    {
        UITextInput.TitleChanged += UITextInput_TitleChanged;
        UITextInput.IsReadOnlyChanged += UITextInput_IsReadOnlyChanged;
        UITextInput.CanCopyWhenEditableChanged += UITextInput_CanCopyWhenEditableChanged;
        UITextInput.IsEnabledChanged += UITextInput_IsEnabledChanged;
        UITextInput.IsVisibleChanged += UITextInput_IsVisibleChanged;
        UITextInput.TextChanged += UITextInput_TextChanged;
        Title = UITextInput.Title;
        IsReadOnly = UITextInput.IsReadOnly;
        CanCopyWhenEditable = UITextInput.CanCopyWhenEditable;
        IsEnabled = UITextInput.IsEnabled;
        Visibility = UITextInput.IsVisible ? Visibility.Visible : Visibility.Collapsed;
        IsReadOnlyOrCanCopyWhenEditable = IsReadOnly || CanCopyWhenEditable;

        TriggerSmartDetection();
    }

    private void UITextInputHeader_Unloaded(object sender, RoutedEventArgs e)
    {
        UITextInput.TitleChanged -= UITextInput_TitleChanged;
        UITextInput.IsReadOnlyChanged -= UITextInput_IsReadOnlyChanged;
        UITextInput.CanCopyWhenEditableChanged -= UITextInput_CanCopyWhenEditableChanged;
        UITextInput.IsEnabledChanged -= UITextInput_IsEnabledChanged;
        UITextInput.IsVisibleChanged -= UITextInput_IsVisibleChanged;
        UITextInput.TextChanged -= UITextInput_TextChanged;
        Loaded -= UITextInputHeader_Loaded;
        Unloaded -= UITextInputHeader_Unloaded;

        CancellationTokenSource? cancellationSourceToken = _cancellationTokenSource;
        cancellationSourceToken?.Cancel();
        cancellationSourceToken?.Dispose();
        _disposableSemaphore.Dispose();
    }

    private void UITextInput_TitleChanged(object? sender, EventArgs e)
    {
        Title = UITextInput.Title;
    }

    private void UITextInput_IsReadOnlyChanged(object? sender, EventArgs e)
    {
        IsReadOnly = UITextInput.IsReadOnly;
        IsReadOnlyOrCanCopyWhenEditable = IsReadOnly || CanCopyWhenEditable;
    }

    private void UITextInput_CanCopyWhenEditableChanged(object? sender, EventArgs e)
    {
        CanCopyWhenEditable = UITextInput.CanCopyWhenEditable;
        IsReadOnlyOrCanCopyWhenEditable = IsReadOnly || CanCopyWhenEditable;
    }

    private void UITextInput_IsEnabledChanged(object? sender, EventArgs e)
    {
        IsEnabled = UITextInput.IsEnabled;
    }

    private void UITextInput_IsVisibleChanged(object? sender, EventArgs e)
    {
        Visibility = UITextInput.IsVisible ? Visibility.Visible : Visibility.Collapsed;
    }

    private void UITextInput_TextChanged(object? sender, EventArgs e)
    {
        TriggerSmartDetection();
    }

    private void Root_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (e.NewSize.Width == e.PreviousSize.Width)
        {
            return;
        }

        if (e.NewSize.Width < 500)
        {
            if (PasteButtonText != null)
            {
                PasteButtonText.Visibility = Visibility.Collapsed;
            }
            if (CopyButtonText != null)
            {
                CopyButtonText.Visibility = Visibility.Collapsed;
            }
        }
        else
        {
            if (PasteButtonText != null)
            {
                PasteButtonText.Visibility = Visibility.Visible;
            }
            if (CopyButtonText != null)
            {
                CopyButtonText.Visibility = Visibility.Visible;
            }
        }
    }

    private void PasteButton_Click(object sender, RoutedEventArgs e)
    {
        this.DispatcherQueue.RunOnUIThreadAsync(async () =>
        {
            object? clipboardContent = await Parts.Clipboard.GetClipboardDataAsync();
            if (clipboardContent is string clipboardString)
            {
                if (Parts.SettingsProvider.GetSetting(PredefinedSettings.TextEditorPasteClearsText))
                {
                    UITextInput.Text(clipboardString);
                }
                else
                {
                    UITextInput.Text(UITextInput.Text + clipboardString);
                }
            }
        }).ForgetSafely();
    }

    private void OpenFileButton_Click(object sender, RoutedEventArgs e)
    {
        this.DispatcherQueue.RunOnUIThreadAsync(async () =>
        {
            var filePicker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.List,
                SuggestedStartLocation = PickerLocationId.ComputerFolder
            };

            filePicker.FileTypeFilter.Add("*");
            filePicker.FileTypeFilter.Add(".txt");
            filePicker.FileTypeFilter.Add(".js");
            filePicker.FileTypeFilter.Add(".ts");
            filePicker.FileTypeFilter.Add(".cs");
            filePicker.FileTypeFilter.Add(".java");
            filePicker.FileTypeFilter.Add(".xml");
            filePicker.FileTypeFilter.Add(".xsd");
            filePicker.FileTypeFilter.Add(".json");
            filePicker.FileTypeFilter.Add(".md");
            filePicker.FileTypeFilter.Add(".sql");
            filePicker.FileTypeFilter.Add(".html");
            filePicker.FileTypeFilter.Add(".cshtml");
            filePicker.FileTypeFilter.Add(".razor");
            filePicker.FileTypeFilter.Add(".css");

#if __WINDOWS__
            // Initialize the file picker with the window handle (HWND).
            WinRT.Interop.InitializeWithWindow.Initialize(filePicker, BackdropWindow.MainWindowHandle);
#endif
            StorageFile file = await filePicker.PickSingleFileAsync();
            if (file is not null)
            {
                await LoadFileAsync(file);
            }
        }).ForgetSafely();
    }

    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
        UITextInput.Text(string.Empty);
    }

    private void SaveAsButton_Click(object sender, RoutedEventArgs e)
    {
        this.DispatcherQueue.RunOnUIThreadAsync(async () =>
        {
            string text = UITextInput.Text; // make sure to get this value on the UI thread.

            var filePicker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.ComputerFolder
            };

            filePicker.FileTypeChoices.Add(ToolPage.AllFileType, new List<string> { "." });

#if __WINDOWS__
            // Initialize the file picker with the window handle (HWND).
            WinRT.Interop.InitializeWithWindow.Initialize(filePicker, BackdropWindow.MainWindowHandle);
#endif
            StorageFile file = await filePicker.PickSaveFileAsync();
            if (file is not null)
            {
                await TaskSchedulerAwaiter.SwitchOffMainThreadAsync(CancellationToken.None);

                // Prevent updates to the remote version of the file until we finish making changes and call CompleteUpdatesAsync.
                CachedFileManager.DeferUpdates(file);
                bool failed = false;

                try
                {
                    await FileIO.WriteTextAsync(file, text);
                }
                catch (Exception ex)
                {
                    LogErrorSavingFile(ex);
                    failed = true;
                }
                finally
                {
                    // Let Windows know that we're finished changing the file so the other app can update the remote version of the file.
                    // Completing updates may require Windows to ask for user input.
                    FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
                    if (status != FileUpdateStatus.Complete && status != FileUpdateStatus.CompleteAndRenamed)
                    {
                        failed = true;
                    }
                }

                if (failed)
                {
                    await this.DispatcherQueue.RunOnUIThreadAsync(async () =>
                    {
                        var dialog = new ContentDialog
                        {
                            // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
                            XamlRoot = this.XamlRoot,
                            Title = ToolPage.UnableSaveFile,
                            Content = string.Format(ToolPage.UnableSaveFileDescription, file.Name),
                            CloseButtonText = ToolPage.Ok,
                            DefaultButton = ContentDialogButton.Close
                        };

                        await dialog.ShowAsync();
                    });
                }
            }
        }).ForgetSafely();
    }

    private void CopyButton_Click(object sender, RoutedEventArgs e)
    {
        Parts.Clipboard.SetClipboardTextAsync(UITextInput.Text).ForgetSafely();
    }

    private async void MainPanel_DragOver(object sender, DragEventArgs e)
    {
        DragOperationDeferral? deferral = e.GetDeferral();
        if (e!.DataView.Contains(StandardDataFormats.StorageItems))
        {
            // This line may cause a hang, but we have no choice since we can't afford to make this method async
            // since the parent caller won't be able to see what changed in DragEventArgs since it won't
            // wait for the execution to end.
            IReadOnlyList<IStorageItem>? storageItems = await e.DataView.GetStorageItemsAsync();
            if (storageItems is not null && storageItems.Count == 1 && storageItems[0] is StorageFile)
            {
                e.AcceptedOperation = DataPackageOperation.Copy;
                e.Handled = false;
            }
        }

        IsDraggingHover = true;
        deferral?.Complete();
    }

    private void MainPanel_DragLeave(object sender, DragEventArgs e)
    {
        IsDraggingHover = false;
    }

    private async void MainPanel_Drop(object sender, DragEventArgs e)
    {
        DragOperationDeferral? deferral = e.GetDeferral();
        IsDraggingHover = false;
        if (!e!.DataView.Contains(StandardDataFormats.StorageItems))
        {
            return;
        }

        IReadOnlyList<IStorageItem>? storageItems = await e.DataView.GetStorageItemsAsync();
        if (storageItems is not null && storageItems.Count == 1 && storageItems[0] is StorageFile file)
        {
            LoadFileAsync(file).Forget();
        }
        deferral?.Complete();
    }

    private void SmartDetectionMenuItem_Click(object sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is MenuFlyoutItem menuItem && menuItem.Tag is SmartDetectedTool detectedTool)
        {
            WeakReferenceMessenger.Default.Send(new ChangeSelectedMenuItemMessage(detectedTool));
        }
    }

    private async Task LoadFileAsync(StorageFile file)
    {
        IsLoadingFile = true;

        try
        {
            await TaskSchedulerAwaiter.SwitchOffMainThreadAsync(CancellationToken.None);

            LogOpeningFile(new FileInfo(file.Path).Length);

            string? text = await FileIO.ReadTextAsync(file);

            await this.DispatcherQueue.RunOnUIThreadAsync(() =>
            {
                UITextInput.Text(text);

                IsLoadingFile = false;

                if (EditorControl.Content is Control control)
                {
                    control.Focus(FocusState.Programmatic);
                }
            });
        }
        catch (Exception ex)
        {
            LogErrorOpeningFile(ex);

            await this.DispatcherQueue.RunOnUIThreadAsync(async () =>
            {
                IsLoadingFile = false;
                var dialog = new ContentDialog
                {
                    // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
                    XamlRoot = this.XamlRoot,
                    Title = ToolPage.UnableOpenFile,
                    Content = string.Format(ToolPage.UnableOpenFileDescription, file.Name),
                    CloseButtonText = ToolPage.Ok,
                    DefaultButton = ContentDialogButton.Close
                };

                await dialog.ShowAsync();
            });
        }
    }

    private void TriggerSmartDetection()
    {
        if (IsLoaded)
        {
            CancellationTokenSource? cancellationSourceToken = _cancellationTokenSource;
            cancellationSourceToken?.Cancel();
            cancellationSourceToken?.Dispose();
            Interlocked.Exchange(ref _cancellationTokenSource, new(TimeSpan.FromSeconds(5)));
            SmartDetectToolsAsync(UITextInput.Text, _cancellationTokenSource.Token).ForgetSafely();
        }
    }

    private async Task SmartDetectToolsAsync(string text, CancellationToken cancellationToken)
    {
        await TaskSchedulerAwaiter.SwitchOffMainThreadAsync(cancellationToken);

        IReadOnlyList<SmartDetectedTool> detectedTools;
        using (await _disposableSemaphore.WaitAsync(cancellationToken))
        {
            // Detect tools to recommend.
            detectedTools
                = await _smartDetectionService.DetectAsync(text, strict: false, cancellationToken)
                .ConfigureAwait(true);

            await DispatcherQueue.RunOnUIThreadAsync(() =>
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    for (int i = 0; i < SmartDetectionMenuFlyout.Items.Count; i++)
                    {
                        if (SmartDetectionMenuFlyout.Items[i] is MenuFlyoutItem menuItem)
                        {
                            menuItem.Click -= SmartDetectionMenuItem_Click;
                        }
                    }

                    SmartDetectionMenuFlyout.Items.Clear();

                    if (detectedTools.Count > 0)
                    {
                        ToolsDetectedBySmartDetection = true;

                        for (int i = 0; i < detectedTools.Count; i++)
                        {
                            SmartDetectedTool detectedTool = detectedTools[i];
                            var menuItem = new MenuFlyoutItem
                            {
                                Icon = new FontIcon
                                {
                                    FontFamily = _stringToStaticResourceConverter.Convert(detectedTool.ToolInstance.IconFontName, typeof(FontFamily), null!, null!) as FontFamily,
                                    Glyph = detectedTool.ToolInstance.IconGlyph
                                },
                                Text = detectedTool.ToolInstance.LongOrShortDisplayTitle,
                                Tag = detectedTool
                            };

                            menuItem.Click += SmartDetectionMenuItem_Click;

                            SmartDetectionMenuFlyout.Items.Add(menuItem);
                        }
                    }
                    else
                    {
                        ToolsDetectedBySmartDetection = false;

                        var noToolDetectedMenuItem = new MenuFlyoutItem { Text = ToolPage.NoSmartDetectedTool };
                        SmartDetectionMenuFlyout.Items.Add(noToolDetectedMenuItem);
                    }
                }
            });
        }
    }

    [LoggerMessage(1, LogLevel.Information, "Loading a file into a text input control. The file size is {fileSize} byte(s).")]
    partial void LogOpeningFile(long fileSize);

    [LoggerMessage(2, LogLevel.Warning, "Unable to open a file in a text input control.")]
    partial void LogErrorOpeningFile(Exception ex);

    [LoggerMessage(3, LogLevel.Warning, "Unable to save a file from a text input control.")]
    partial void LogErrorSavingFile(Exception ex);
}
