using CommunityToolkit.Mvvm.Messaging;
using DevToys.Core.Models;
using DevToys.Core.Tools;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;
using Uno.Extensions;

namespace DevToys.Blazor.Components.UIElements;

public partial class UITextInputWrapper : MefComponentBase
{
    private const int MinimumWidthBeforeShrinkingToolBar = 500;
    private const int MinimumWidthBeforeHidingNonEssentialToolBar = 300;

    private static readonly string[] FileTypes = new[]
    {
        "*.*",
        "*.txt",
        "*.js",
        "*.ts",
        "*.cs",
        "*.java",
        "*.xml",
        "*.xsd",
        "*.json",
        "*.md",
        "*.sql",
        "*.html",
        "*.cshtml",
        "*.razor",
        "*.css"
    };

    private readonly ILogger _logger;
    private readonly DisposableSemaphore _disposableSemaphore = new();

    private CancellationTokenSource? _cancellationTokenSource;
    private bool _isToolBarShrink;
    private bool _hideNonEssentialToolBar;
    private bool _isInFullScreenMode;

    protected override string? JavaScriptFile => "./_content/DevToys.Blazor/Components/UIElements/UITextInputWrapper.razor.js";

    [Import]
    internal SmartDetectionService SmartDetectionService { get; set; } = default!;

    [Import]
    internal IClipboard Clipboard { get; set; } = default!;

    [Import]
    internal IFileStorage FileStorage { get; set; } = default!;

    [Import]
    internal ISettingsProvider SettingsProvider { get; set; } = default!;

    [Parameter]
    public IUISingleLineTextInput UITextInput { get; set; } = default!;

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public bool IsExtendableToFullScreen { get; set; }

    protected bool IsLoadingFile { get; set; }

    protected bool ToolsDetectedBySmartDetection { get; set; }

    protected List<SmartDetectionDropDownListItem> SmartDetectionDropDownItems { get; } = new();

    protected List<DropDownListItem> CollapsedDropDownListItems { get; } = new();

    protected string ExtendedId => UITextInput.Id + "-" + Id;

    [CascadingParameter]
    protected FullScreenContainer? FullScreenContainer { get; set; }

    public UITextInputWrapper()
    {
        _logger = this.Log();
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        UITextInput.TextChanged += UITextInput_TextChanged;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        base.OnAfterRender(firstRender);
        if (firstRender)
        {
            TriggerSmartDetection();

            using (await Semaphore.WaitAsync(CancellationToken.None))
            {
                await (await JSModule).InvokeVoidWithErrorHandlingAsync("registerResizeHandler", ExtendedId, Reference);
                if (!UITextInput.IsReadOnly)
                {
                    await (await JSModule).InvokeVoidWithErrorHandlingAsync("registerDropZone", ExtendedId);
                }
            }
        }
    }

    public override async ValueTask DisposeAsync()
    {
        await (await JSModule).InvokeVoidWithErrorHandlingAsync("dispose", ExtendedId);
        await base.DisposeAsync();
    }

    [JSInvokable]
    public void OnComponentResize(int textInputWrapperWidth)
    {
        bool stateChanged
            = (textInputWrapperWidth < MinimumWidthBeforeShrinkingToolBar && !_isToolBarShrink)
            || (textInputWrapperWidth >= MinimumWidthBeforeShrinkingToolBar && _isToolBarShrink);

        if (stateChanged)
        {
            _isToolBarShrink = !_isToolBarShrink;
            StateHasChanged();
        }

        bool oldNonEssentialToolBarState = _hideNonEssentialToolBar;
        _hideNonEssentialToolBar = _isToolBarShrink && textInputWrapperWidth < MinimumWidthBeforeHidingNonEssentialToolBar;
        if (_hideNonEssentialToolBar != oldNonEssentialToolBarState)
        {
            if (_hideNonEssentialToolBar)
            {
                BuildCollapsedMenu();
            }

            StateHasChanged();
        }
    }

    private async Task OnFileDroppedAsync(InputFileChangeEventArgs args)
    {
        if (string.IsNullOrEmpty(args.File.Name))
        {
            return;
        }

        await InvokeAsync(() =>
        {
            IsLoadingFile = true;
            StateHasChanged();
        });

        using var stream = new MemoryStream();
        using Stream fileStream = args.File.OpenReadStream(maxAllowedSize: long.MaxValue);
        await fileStream.CopyToAsync(stream);
        stream.Seek(0, SeekOrigin.Begin);
        await LoadFileAsync(stream);
    }

    private void UITextInput_TextChanged(object? sender, EventArgs e)
    {
        TriggerSmartDetection();
    }

    private async Task OnPasteButtonClickAsync()
    {
        object? clipboardContent = await Clipboard.GetClipboardDataAsync();
        if (clipboardContent is string clipboardString)
        {
            if (SettingsProvider.GetSetting(PredefinedSettings.TextEditorPasteClearsText))
            {
                UITextInput.Text(clipboardString);
            }
            else
            {
                UITextInput.Text(UITextInput.Text + clipboardString);
            }
        }

        await FocusOnTextEditorAsync();
    }

    private async Task OnLoadFileButtonClickAsync()
    {
        PickedFile? pickedFile = null;
        try
        {
            pickedFile = await FileStorage.PickOpenFileAsync(FileTypes);
            if (pickedFile is not null)
            {
                await LoadFileAsync(pickedFile.Stream);
            }
        }
        finally
        {
            pickedFile?.Stream.Dispose();
        }
    }

    private async Task OnClearButtonClickAsync()
    {
        UITextInput.Text(string.Empty);
        await FocusOnTextEditorAsync();
    }

    private async Task OnSaveAsButtonClickAsync()
    {
        string text = UITextInput.Text;

        Stream? fileStream = null;
        try
        {
            fileStream = await FileStorage.PickSaveFileAsync(new[] { "*.*" });
            if (fileStream is not null)
            {
                await TaskSchedulerAwaiter.SwitchOffMainThreadAsync(CancellationToken.None);

                bool failed = false;

                try
                {
                    using var writer = new StreamWriter(fileStream);
                    writer.Write(text);
                }
                catch (Exception ex)
                {
                    LogErrorSavingFile(ex);
                    failed = true;
                }
                finally
                {
                    if (failed)
                    {
                        await InvokeAsync(() =>
                        {
                            // TODO: Display ContentDialog

                            //var dialog = new ContentDialog
                            //{
                            //    // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
                            //    XamlRoot = this.XamlRoot,
                            //    Title = ToolPage.UnableSaveFile,
                            //    Content = string.Format(ToolPage.UnableSaveFileDescription, file.Name),
                            //    CloseButtonText = ToolPage.Ok,
                            //    DefaultButton = ContentDialogButton.Close
                            //};
                            //await dialog.ShowAsync();
                        });
                    }
                }
            }
        }
        finally
        {
            fileStream?.Dispose();
        }
    }

    private void OnCopyButtonClick()
    {
        Clipboard.SetClipboardTextAsync(UITextInput.Text).ForgetSafely();
    }

    private async Task LoadFileAsync(Stream fileStream)
    {
        await InvokeAsync(() =>
        {
            IsLoadingFile = true;
            StateHasChanged();
        });

        try
        {
            await TaskSchedulerAwaiter.SwitchOffMainThreadAsync(CancellationToken.None);

            LogOpeningFile(fileStream.Length);

            // TODO: Detect encoding.
            using var reader = new StreamReader(fileStream);
            string? text = await reader.ReadToEndAsync();

            await InvokeAsync(async () =>
            {
                UITextInput.Text(text);

                IsLoadingFile = false;

                StateHasChanged();

                await FocusOnTextEditorAsync();
            });
        }
        catch (Exception ex)
        {
            LogErrorOpeningFile(ex);

            await InvokeAsync(() =>
            {
                IsLoadingFile = false;
                // TODO: Display ContentDialog

                //var dialog = new ContentDialog
                //{
                //    // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
                //    XamlRoot = this.XamlRoot,
                //    Title = ToolPage.UnableOpenFile,
                //    Content = string.Format(ToolPage.UnableOpenFileDescription, fileStream.Name),
                //    CloseButtonText = ToolPage.Ok,
                //    DefaultButton = ContentDialogButton.Close
                //};
                //await dialog.ShowAsync();
            });
        }
    }

    private async Task OnToggleFullScreenButtonClickAsync()
    {
        Guard.IsNotNull(FullScreenContainer);
        _isInFullScreenMode = await FullScreenContainer.ToggleFullScreenModeAsync(ExtendedId);
        StateHasChanged();
    }

    private async Task FocusOnTextEditorAsync()
    {
        Guard.IsEqualTo(_childrenComponents.Count, 1);
        var focusableChild = _childrenComponents.First() as IFocusable;
        if (focusableChild is not null)
        {
            await focusableChild.FocusAsync();
        }
    }

    private void TriggerSmartDetection()
    {
        CancellationTokenSource? cancellationSourceToken = _cancellationTokenSource;
        cancellationSourceToken?.Cancel();
        cancellationSourceToken?.Dispose();
        Interlocked.Exchange(ref _cancellationTokenSource, new(TimeSpan.FromSeconds(5)));
        SmartDetectToolsAsync(UITextInput.Text, _cancellationTokenSource.Token).ForgetSafely();
    }

    private async Task SmartDetectToolsAsync(string text, CancellationToken cancellationToken)
    {
        await TaskSchedulerAwaiter.SwitchOffMainThreadAsync(cancellationToken);

        IReadOnlyList<SmartDetectedTool> detectedTools;
        using (await _disposableSemaphore.WaitAsync(cancellationToken))
        {
            // Detect tools to recommend.
            detectedTools
                = await SmartDetectionService.DetectAsync(text, strict: false, cancellationToken)
                .ConfigureAwait(true);

            await InvokeAsync(() =>
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    SmartDetectionDropDownItems.Clear();

                    if (detectedTools.Count > 0)
                    {
                        ToolsDetectedBySmartDetection = true;

                        for (int i = 0; i < detectedTools.Count; i++)
                        {
                            SmartDetectedTool detectedTool = detectedTools[i];
                            EventCallback<DropDownListItem> onClickEventCallback = EventCallback.Factory.Create<DropDownListItem>(this, SmartDetectionMenuItem_Click);
                            var menuItem = new SmartDetectionDropDownListItem
                            {
                                IconGlyph = detectedTool.ToolInstance.IconGlyph,
                                IconFontFamily = detectedTool.ToolInstance.IconFontName,
                                Text = detectedTool.ToolInstance.LongOrShortDisplayTitle,
                                SmartDetectedTool = detectedTool,
                                OnClick = onClickEventCallback
                            };

                            SmartDetectionDropDownItems.Add(menuItem);
                        }
                    }
                    else
                    {
                        ToolsDetectedBySmartDetection = false;

                        var noToolDetectedMenuItem = new SmartDetectionDropDownListItem
                        {
                            Text = "No tool to suggest",
                            IsEnabled = false
                        };

                        SmartDetectionDropDownItems.Add(noToolDetectedMenuItem);
                    }

                    StateHasChanged();
                }
            });
        }
    }

    private void SmartDetectionMenuItem_Click(DropDownListItem menuItem)
    {
        if (menuItem is SmartDetectionDropDownListItem smartDetectionDropDownListItem)
        {
            WeakReferenceMessenger.Default.Send(new ChangeSelectedMenuItemMessage(smartDetectionDropDownListItem.SmartDetectedTool));
        }
    }

    private void BuildCollapsedMenu()
    {
        // TODO: Localize
        CollapsedDropDownListItems.Clear();

        if (!UITextInput.IsReadOnly)
        {
            CollapsedDropDownListItems.Add(new DropDownListItem
            {
                IconGlyph = '\uF2D5',
                Text = "Paste",
                OnClick = EventCallback.Factory.Create<DropDownListItem>(this, OnPasteButtonClickAsync)
            });
            CollapsedDropDownListItems.Add(new DropDownListItem
            {
                IconGlyph = '\uF378',
                Text = "Load a file",
                OnClick = EventCallback.Factory.Create<DropDownListItem>(this, OnLoadFileButtonClickAsync)
            });
            CollapsedDropDownListItems.Add(new DropDownListItem
            {
                IconGlyph = '\uF369',
                Text = "Clear",
                OnClick = EventCallback.Factory.Create<DropDownListItem>(this, OnClearButtonClickAsync)
            });
        }

        if (UITextInput.IsReadOnly || UITextInput.CanCopyWhenEditable)
        {
            CollapsedDropDownListItems.Add(new DropDownListItem
            {
                IconGlyph = '\uF67F',
                Text = "Save as...",
                OnClick = EventCallback.Factory.Create<DropDownListItem>(this, OnSaveAsButtonClickAsync)
            });
            CollapsedDropDownListItems.Add(new DropDownListItem
            {
                IconGlyph = '\uF32B',
                Text = "Copy",
                OnClick = EventCallback.Factory.Create<DropDownListItem>(this, OnCopyButtonClick)
            });
        }
    }

    [LoggerMessage(1, LogLevel.Information, "Loading a file into a text input control. The file size is {fileSize} byte(s).")]
    partial void LogOpeningFile(long fileSize);

    [LoggerMessage(2, LogLevel.Warning, "Unable to open a file in a text input control.")]
    partial void LogErrorOpeningFile(Exception ex);

    [LoggerMessage(3, LogLevel.Warning, "Unable to save a file from a text input control.")]
    partial void LogErrorSavingFile(Exception ex);

    public class SmartDetectionDropDownListItem : DropDownListItem
    {
        internal SmartDetectedTool SmartDetectedTool { get; set; } = default!;
    }
}
