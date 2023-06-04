using System.Collections.ObjectModel;
using DevToys.Api;
using Microsoft.AspNetCore.Components.Forms;

namespace DevToys.Blazor.Components;

public partial class TextBox : JSStyledComponentBase
{
    private static readonly ContextMenuItem CutContextMenuItem
        = new()
        {
            IconGlyph = '\uF33A',
            Text = "Cut", // TODO: Localize
            KeyboardShortcut = "Ctrl+X"
        };
    private static readonly ContextMenuItem CopyContextMenuItem
        = new()
        {
            IconGlyph = '\uF32B',
            Text = "Copy", // TODO: Localize
            KeyboardShortcut = "Ctrl+C"
        };
    private static readonly ContextMenuItem PasteContextMenuItem
        = new()
        {
            IconGlyph = '\uF2D5',
            Text = "Paste", // TODO: Localize
            KeyboardShortcut = "Ctrl+V"
        };
    private static readonly ContextMenuItem SelectAllContextMenuItem
        = new()
        {
            Text = "Select All", // TODO: Localize
            KeyboardShortcut = "Ctrl+A"
        };

    private readonly string _contextMenuId = NewId();
    private readonly ObservableCollection<ContextMenuItem> _contextMenuItems = new();
    private InputText? _input;
    private bool _isContextMenuOpened;
    private int _textChangedCount;

    protected override string JavaScriptFile => "./_content/DevToys.Blazor/Components/TextBox/TextBox.razor.js";

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

    [Parameter]
    public RenderFragment? Buttons { get; set; }

    [Parameter]
    public EventCallback<string> OnTextChanged { get; set; }

    internal ValueTask<bool> FocusAsync()
    {
        Guard.IsNotNull(_input);
        Guard.IsNotNull(_input.Element);
        return JSRuntime.InvokeVoidWithErrorHandlingAsync("devtoys.DOM.setFocus", _input.Element);
    }

    private void OnClearClick()
    {
        Text = string.Empty;
        OnTextChanged.InvokeAsync(Text).Forget();
        FocusAsync();
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
        Text = e.Value as string;
        return OnTextChanged.InvokeAsync(e.Value as string);
    }

    private async Task UpdateContextMenuAsync()
    {
        Guard.IsNotNull(_input);
        Guard.IsNotNull(_input.Element);
        int selectionLength = await (await JSModule).InvokeAsync<int>("getSelectionLength", _input.Element);

        _contextMenuItems.Clear();

        if (selectionLength > 0)
        {
            if (!IsReadOnly)
            {
                CutContextMenuItem.OnClick = EventCallback.Factory.Create(this, OnCutAsync);
                _contextMenuItems.Add(CutContextMenuItem);
            }

            CopyContextMenuItem.OnClick = EventCallback.Factory.Create(this, OnCopyAsync);
            _contextMenuItems.Add(CopyContextMenuItem);
        }

        if (!IsReadOnly)
        {
            PasteContextMenuItem.OnClick = EventCallback.Factory.Create(this, OnPasteAsync);
            _contextMenuItems.Add(PasteContextMenuItem);
        }

        if (Text?.Length > 0)
        {
            SelectAllContextMenuItem.OnClick = EventCallback.Factory.Create(this, OnSelectAllAsync);
            _contextMenuItems.Add(SelectAllContextMenuItem);
        }
    }

    private async Task OnCutAsync()
    {
        Guard.IsNotNull(_input);
        await (await JSModule).InvokeVoidAsync("cut", _input.Element);
    }

    private async Task OnCopyAsync()
    {
        Guard.IsNotNull(_input);
        await (await JSModule).InvokeVoidAsync("copy", _input.Element);
    }

    private async Task OnPasteAsync()
    {
        Guard.IsNotNull(_input);
        await (await JSModule).InvokeVoidAsync("paste", _input.Element);
    }

    private async Task OnSelectAllAsync()
    {
        Guard.IsNotNull(_input);
        await (await JSModule).InvokeVoidAsync("selectAll", _input.Element);
    }
}
