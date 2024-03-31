using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Messaging;
using DevToys.Core.Models;
using DevToys.Core.Settings;
using DevToys.Core.Tools;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;

namespace DevToys.Blazor.Components.UIElements;

public partial class UITextInputWrapper : MefComponentBase
{
    private const int MinimumWidthBeforeShrinkingToolBar = 500;
    private const int MinimumWidthBeforeHidingNonEssentialToolBar = 300;

    private static readonly string[] fileTypes = new[]
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
    private readonly DisposableSemaphore _semaphore = new();

    private CancellationTokenSource? _cancellationTokenSource;
    private bool _isToolBarShrink;
    private bool _hideNonEssentialToolBar;
    private bool _isInFullScreenMode;
    private Button? _toggleFullScreenButton;

    protected override string? JavaScriptFile => "./_content/DevToys.Blazor/Components/UIElements/UITextInputWrapper.razor.js";

    [Import]
#pragma warning disable IDE0044 // Add readonly modifier
    private SmartDetectionService _smartDetectionService = default!;

    [Import]
    private IClipboard _clipboard = default!;

    [Import]
    private IFileStorage _fileStorage = default!;

    [Import]
    private ISettingsProvider _settingsProvider = default!;
#pragma warning restore IDE0044 // Add readonly modifier

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
        UITextInput.IsVisibleChanged += UITextInput_IsVisibleChanged;
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
        UITextInput.TextChanged -= UITextInput_TextChanged;
        UITextInput.IsVisibleChanged -= UITextInput_IsVisibleChanged;
        await (await JSModule).InvokeVoidWithErrorHandlingAsync("dispose", ExtendedId);
        _semaphore.Dispose();
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

    private void UITextInput_IsVisibleChanged(object? sender, EventArgs e)
    {
        if (_isInFullScreenMode && !UITextInput.IsVisible)
        {
            // If the element is not visible anymore, we need to exit the full screen mode.
            OnToggleFullScreenButtonClickAsync().Forget();
        }
    }

    private async Task OnPasteButtonClickAsync()
    {
        string? clipboardString = await _clipboard.GetClipboardTextAsync();
        if (clipboardString is not null)
        {
            if (_settingsProvider.GetSetting(PredefinedSettings.TextEditorPasteClearsText))
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
        SandboxedFileReader? pickedFile = null;
        try
        {
            pickedFile = await _fileStorage.PickOpenFileAsync(fileTypes);
            if (pickedFile is not null)
            {
                using Stream stream = await pickedFile.GetNewAccessToFileContentAsync(CancellationToken.None);
                await LoadFileAsync(stream);
            }
        }
        finally
        {
            pickedFile?.Dispose();
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
            fileStream = await _fileStorage.PickSaveFileAsync(new[] { "*.*" });
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
        _clipboard.SetClipboardTextAsync(UITextInput.Text).ForgetSafely();
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
            string? text = await reader.ReadToEndAsync(); // TODO: Perf risk. Read in chunks.

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
        Guard.IsNotNull(_toggleFullScreenButton);
        _isInFullScreenMode = await FullScreenContainer.ToggleFullScreenModeAsync(ExtendedId, _toggleFullScreenButton);
        StateHasChanged();
    }

    private async Task FocusOnTextEditorAsync()
    {
        Guard.IsEqualTo(ChildrenComponents.Count, 1);
        var focusableChild = ChildrenComponents.First() as IFocusable;
        if (focusableChild is not null)
        {
            await focusableChild.FocusAsync();
        }
    }

    private void TriggerSmartDetection()
    {
        if (UITextInput.IsReadOnly)
        {
            CancellationTokenSource? cancellationSourceToken = _cancellationTokenSource;
            cancellationSourceToken?.Cancel();
            cancellationSourceToken?.Dispose();
            Interlocked.Exchange(ref _cancellationTokenSource, new(TimeSpan.FromSeconds(2)));
            SmartDetectToolsAsync(UITextInput.Text, _cancellationTokenSource.Token).ForgetSafely();
        }
    }

    private async Task SmartDetectToolsAsync(string text, CancellationToken cancellationToken)
    {
        await TaskSchedulerAwaiter.SwitchOffMainThreadAsync(cancellationToken);

        IReadOnlyList<SmartDetectedTool> detectedTools;
        using (await _semaphore.WaitAsync(cancellationToken))
        {
            // Detect tools to recommend.
            detectedTools
                = await _smartDetectionService.DetectAsync(text, strict: false, cancellationToken)
                .ConfigureAwait(true);

            await InvokeAsync(() =>
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    SmartDetectionDropDownItems.Clear();

                    if (detectedTools.Count > 0)
                    {
                        ToolsDetectedBySmartDetection = true;

                        EventCallback<DropDownListItem> onClickEventCallback = EventCallback.Factory.Create<DropDownListItem>(this, SmartDetectionMenuItem_Click);
                        for (int i = 0; i < detectedTools.Count; i++)
                        {
                            SmartDetectedTool detectedTool = detectedTools[i];
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
                            Text = DevToys.Localization.Strings.UITextInputWrapper.UITextInputWrapper.NoToolSuggested,
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
        CollapsedDropDownListItems.Clear();

        if (!UITextInput.IsReadOnly)
        {
            CollapsedDropDownListItems.Add(new DropDownListItem
            {
                IconGlyph = '\uF2D5',
                Text = DevToys.Localization.Strings.UITextInputWrapper.UITextInputWrapper.PasteButtonText,
                OnClick = EventCallback.Factory.Create<DropDownListItem>(this, OnPasteButtonClickAsync)
            });
            CollapsedDropDownListItems.Add(new DropDownListItem
            {
                IconGlyph = '\uF378',
                Text = DevToys.Localization.Strings.UITextInputWrapper.UITextInputWrapper.LoadFileButtonText,
                OnClick = EventCallback.Factory.Create<DropDownListItem>(this, OnLoadFileButtonClickAsync)
            });
            CollapsedDropDownListItems.Add(new DropDownListItem
            {
                IconGlyph = '\uF369',
                Text = DevToys.Localization.Strings.UITextInputWrapper.UITextInputWrapper.ClearButtonText,
                OnClick = EventCallback.Factory.Create<DropDownListItem>(this, OnClearButtonClickAsync)
            });
        }

        if (UITextInput.IsReadOnly || UITextInput.CanCopyWhenEditable)
        {
            CollapsedDropDownListItems.Add(new DropDownListItem
            {
                IconGlyph = '\uF67F',
                Text = DevToys.Localization.Strings.UITextInputWrapper.UITextInputWrapper.SaveAsButtonText,
                OnClick = EventCallback.Factory.Create<DropDownListItem>(this, OnSaveAsButtonClickAsync)
            });
            CollapsedDropDownListItems.Add(new DropDownListItem
            {
                IconGlyph = '\uF32B',
                Text = DevToys.Localization.Strings.UITextInputWrapper.UITextInputWrapper.CopyButtonText,
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
