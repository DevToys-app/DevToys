using System.Collections.ObjectModel;
using System.Globalization;
using DevToys.Core.Settings;
using Microsoft.AspNetCore.Components.Forms;

namespace DevToys.Blazor.Components;

public partial class TextBox : MefComponentBase, IFocusable
{
    private static readonly ContextMenuItem CutContextMenuItem
        = new()
        {
            IconGlyph = '\uF33A',
            Text = DevToys.Localization.Strings.TextBox.TextBox.Cut,
            KeyboardShortcut = "Ctrl+X"
        };
    private static readonly ContextMenuItem CopyContextMenuItem
        = new()
        {
            IconGlyph = '\uF32B',
            Text = DevToys.Localization.Strings.TextBox.TextBox.Copy,
            KeyboardShortcut = "Ctrl+C"
        };
    private static readonly ContextMenuItem PasteContextMenuItem
        = new()
        {
            IconGlyph = '\uF2D5',
            Text = DevToys.Localization.Strings.TextBox.TextBox.Paste,
            KeyboardShortcut = "Ctrl+V"
        };
    private static readonly ContextMenuItem SelectAllContextMenuItem
        = new()
        {
            Text = DevToys.Localization.Strings.TextBox.TextBox.SelectAll,
            KeyboardShortcut = "Ctrl+A"
        };

    private readonly ObservableCollection<ContextMenuItem> _contextMenuItems = new();
    private InputText? _input;
    private bool _isContextMenuOpened;
    private int _textChangedCount;
    private TextBoxTypes _internalType = TextBoxTypes.Text;

    protected override string JavaScriptFile => "./_content/DevToys.Blazor/Components/Text/TextBox/TextBox.razor.js";

#pragma warning disable IDE0044 // Add readonly modifier
    [Import]
    private IClipboard _clipboard = default!;

    [Import]
    private ISettingsProvider _settingsProvider = default!;
#pragma warning restore IDE0044 // Add readonly modifier

    [Parameter]
    public string? Text { get; set; }

    [Parameter]
    public string? Placeholder { get; set; }

    [Parameter]
    public string? Header { get; set; }

    [Parameter]
    public bool IsReadOnly { get; set; }

    [Parameter]
    public TextBoxTypes Type { get; set; } = TextBoxTypes.Text;

    /// <summary>
    /// Gets or sets the minimum value possible when <see cref="Type"/> is <see cref="TextBoxTypes.Number"/>.
    /// </summary>
    [Parameter]
    public double Min { get; set; } = int.MinValue;

    /// <summary>
    /// Gets or sets the maximum value possible when <see cref="Type"/> is <see cref="TextBoxTypes.Number"/>.
    /// </summary>
    [Parameter]
    public double Max { get; set; } = int.MaxValue;

    /// <summary>
    /// Gets or sets the interval between legal numbers when <see cref="Type"/> is <see cref="TextBoxTypes.Number"/>.
    /// </summary>
    [Parameter]
    public double Step { get; set; } = 1;

    [Parameter]
    public RenderFragment? Buttons { get; set; }

    [Parameter]
    public EventCallback<string?> TextChanged { get; set; }

    [Parameter]
    public string? FontFamily { get; set; }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        FontFamily = _settingsProvider.GetSetting(PredefinedSettings.TextEditorFont);
        _settingsProvider.SettingChanged += SettingsProvider_SettingChanged;
    }

    protected override void OnParametersSet()
    {
        _internalType = Type;

        base.OnParametersSet();
    }

    public ValueTask<bool> FocusAsync()
    {
        Guard.IsNotNull(_input);
        Guard.IsNotNull(_input.Element);
        return JSRuntime.InvokeVoidWithErrorHandlingAsync("devtoys.DOM.setFocus", _input.Element);
    }

    public override async ValueTask DisposeAsync()
    {
        if (_settingsProvider is not null)
        {
            _settingsProvider.SettingChanged -= SettingsProvider_SettingChanged;
        }

        try
        {
            await (await JSModule).InvokeVoidWithErrorHandlingAsync("dispose", Element);
        }
        catch
        {
        }
        finally
        {
            await base.DisposeAsync();
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender)
        {
            using (await Semaphore.WaitAsync(CancellationToken.None))
            {
                await (await JSModule).InvokeVoidWithErrorHandlingAsync("initializeKeyboardTracking", Element);
            }
        }
    }

    internal Task SetTextAsync(string text)
    {
        if (Type == TextBoxTypes.Number)
        {
            if (double.TryParse(text, out double value))
            {
                if (value < Min)
                {
                    value = Min;
                }
                else if (value > Max)
                {
                    value = Max;
                }

                Text = value.ToString(CultureInfo.InvariantCulture);
            }
            else
            {
                Text = Math.Max(Min, Math.Min(0, Max)).ToString(CultureInfo.InvariantCulture);
            }
        }
        else
        {
            Text = text;
        }

        return TextChanged.InvokeAsync(Text);
    }

    private void OnClearClick()
    {
        SetTextAsync(string.Empty).Forget();
        FocusAsync();
    }

    private void OnRevealPasswordMouseDown()
    {
        _internalType = TextBoxTypes.Text;
    }

    private void OnRevealPasswordMouseUp()
    {
        _internalType = Type;
        FocusAsync();
    }

    private async Task OnDecreaseClickAsync()
    {
        using (await Semaphore.WaitAsync(CancellationToken.None))
        {
            Guard.IsNotNull(_input);
            Guard.IsNotNull(_input.Element);
            await SetTextAsync((await (await JSModule).InvokeAsync<double>("decreaseValue", _input.Element)).ToString());
            await FocusAsync();
        }
    }

    private async Task OnIncreaseClickAsync()
    {
        using (await Semaphore.WaitAsync(CancellationToken.None))
        {
            Guard.IsNotNull(_input);
            Guard.IsNotNull(_input.Element);
            await SetTextAsync((await (await JSModule).InvokeAsync<double>("increaseValue", _input.Element)).ToString());
            await FocusAsync();
        }
    }

    private Task OnContextMenuOpening()
    {
        return UpdateContextMenuAsync();
    }

    private void OnContextMenuOpened()
    {
        _isContextMenuOpened = true;
    }

    private async Task OnContextMenuClosedAsync()
    {
        await FocusAsync();
        _isContextMenuOpened = false;
    }

    private Task InputTextChangedAsync(ChangeEventArgs e)
    {
        Interlocked.Increment(ref _textChangedCount);
        return SetTextAsync(e.Value as string ?? string.Empty);
    }

    private async Task UpdateContextMenuAsync()
    {
        using (await Semaphore.WaitAsync(CancellationToken.None))
        {
            Guard.IsNotNull(_input);
            Guard.IsNotNull(_input.Element);
            int selectionLength = await (await JSModule).InvokeAsync<int>("getSelectionLength", _input.Element);

            _contextMenuItems.Clear();

            if (selectionLength > 0)
            {
                if (!IsReadOnly)
                {
                    CutContextMenuItem.OnClick = EventCallback.Factory.Create<DropDownListItem>(this, OnCutAsync);
                    _contextMenuItems.Add(CutContextMenuItem);
                }

                CopyContextMenuItem.OnClick = EventCallback.Factory.Create<DropDownListItem>(this, OnCopyAsync);
                _contextMenuItems.Add(CopyContextMenuItem);
            }

            if (!IsReadOnly)
            {
                PasteContextMenuItem.OnClick = EventCallback.Factory.Create<DropDownListItem>(this, OnPasteAsync);
                _contextMenuItems.Add(PasteContextMenuItem);
            }

            if (Text?.Length > 0 && Type != TextBoxTypes.Number)
            {
                SelectAllContextMenuItem.OnClick = EventCallback.Factory.Create<DropDownListItem>(this, OnSelectAllAsync);
                _contextMenuItems.Add(SelectAllContextMenuItem);
            }
        }
    }

    private async Task OnCutAsync(DropDownListItem dropDownListItem)
    {
        await OnCopyAsync(dropDownListItem);

        if (!string.IsNullOrEmpty(Text))
        {
            Guard.IsNotNull(_input);
            Guard.IsEqualTo(_input.Value!, Text);
            TextSpan selection = await GetSelectionAsync();

            string newText = Text.Remove(selection.StartPosition, selection.Length);
            await SetTextAsync(newText);
        }
    }

    private async Task OnCopyAsync(DropDownListItem _)
    {
        if (!string.IsNullOrEmpty(Text))
        {
            Guard.IsNotNull(_input);
            Guard.IsEqualTo(_input.Value!, Text);
            TextSpan selection = await GetSelectionAsync();

            string textToCopy = Text.Substring(selection.StartPosition, selection.Length);
            await _clipboard.SetClipboardTextAsync(textToCopy);
        }
    }

    private async Task OnPasteAsync(DropDownListItem _)
    {
        string? clipboardDataString = await _clipboard.GetClipboardTextAsync();
        if (clipboardDataString is not null)
        {
            TextSpan selection = await GetSelectionAsync();
            Text ??= string.Empty;
            string newText = string.Concat(Text.AsSpan(0, selection.StartPosition), clipboardDataString, Text.AsSpan(selection.EndPosition));
            await SetTextAsync(newText);
        }
    }

    private async Task OnSelectAllAsync(DropDownListItem _)
    {
        using (await Semaphore.WaitAsync(CancellationToken.None))
        {
            Guard.IsNotNull(_input);
            await (await JSModule).InvokeVoidWithErrorHandlingAsync("selectAll", _input.Element);
        }
    }

    private async Task<TextSpan> GetSelectionAsync()
    {
        using (await Semaphore.WaitAsync(CancellationToken.None))
        {
            Guard.IsNotNull(_input);
            int[] selection = await (await JSModule).InvokeAsync<int[]>("getSelectionSpan", _input.Element);
            return new TextSpan(selection[0], selection[1] - selection[0]);
        }
    }

    private void SettingsProvider_SettingChanged(object? sender, SettingChangedEventArgs e)
    {
        FontFamily = _settingsProvider.GetSetting(PredefinedSettings.TextEditorFont);
    }
}
