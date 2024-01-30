using System.Runtime.CompilerServices;

namespace DevToys.Api;

/// <summary>
/// Represents the root ui definition of a <see cref="IGuiTool"/>.
/// </summary>
[DebuggerDisplay($"RootElement = {{{nameof(RootElement)}}}, IsScrollable = {{{nameof(IsScrollable)}}}")]
public class UIToolView : INotifyPropertyChanged
{
    private UIDialog? _currentOpenedDialog;
    private IUIElement? _rootElement;
    private bool _isScrollable;

    /// <summary>
    /// Creates a new instance of the <see cref="UIToolView"/> class.
    /// </summary>
    public UIToolView()
        : this(true, null)
    {
    }

    /// <summary>
    /// Creates a new instance of the <see cref="UIToolView"/> class.
    /// </summary>
    /// <param name="rootElement">The root element of the tool's UI.</param>
    public UIToolView(IUIElement? rootElement)
        : this(true, rootElement)
    {
    }

    /// <summary>
    /// Creates a new instance of the <see cref="UIToolView"/> class.
    /// </summary>
    /// <param name="isScrollable">Indicates whether the UI of the tool is scrollable or should fits to the window boundaries.</param>
    public UIToolView(bool isScrollable)
        : this(isScrollable, null)
    {
    }

    /// <summary>
    /// Creates a new instance of the <see cref="UIToolView"/> class.
    /// </summary>
    /// <param name="isScrollable">Indicates whether the UI of the tool is scrollable or should fits to the window boundaries.</param>
    /// <param name="rootElement">The root element of the tool's UI.</param>
    public UIToolView(bool isScrollable, IUIElement? rootElement)
    {
        IsScrollable = isScrollable;
        RootElement = rootElement;
    }

    /// <summary>
    /// Gets whether the UI of the tool is scrollable or should fits to the window boundaries. Default is true.
    /// </summary>
    public bool IsScrollable
    {
        get => _isScrollable;
        internal set => SetPropertyValue(ref _isScrollable, value, IsScrollableChanged);
    }

    /// <summary>
    /// Gets the <see cref="UIDialog"/> currently opened. Only one at a time can be opened.
    /// </summary>
    public UIDialog? CurrentOpenedDialog
    {
        get => _currentOpenedDialog;
        internal set => SetPropertyValue(ref _currentOpenedDialog, value, CurrentOpenedDialogChanged);
    }

    /// <summary>
    /// Gets the root element of the tool's UI.
    /// </summary>
    public IUIElement? RootElement
    {
        get => _rootElement;
        internal set => SetPropertyValue(ref _rootElement, value, RootElementChanged);
    }

    /// <summary>
    /// Raised when <see cref="IsScrollable"/> changed.
    /// </summary>
    public event EventHandler? IsScrollableChanged;

    /// <summary>
    /// Raised when <see cref="CurrentOpenedDialog"/> changed.
    /// </summary>
    public event EventHandler? CurrentOpenedDialogChanged;

    /// <summary>
    /// Raised when <see cref="RootElement"/> changed.
    /// </summary>
    public event EventHandler? RootElementChanged;

    /// <summary>
    /// Raised when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Gets the first child element with the specified id.
    /// </summary>
    /// <param name="id">The id of a child element.</param>
    /// <remarks>This method is recursive and navigate through each element in the UI. It can be slow depending on the complexity of the UI. Be cautious when using it.</remarks>
    /// <returns>Returns null if the element could not be found. If many elements have the name id, this method returns the first it finds</returns>
    public IUIElement? GetChildElementById(string id)
    {
        if (RootElement is IUIElementWithChildren uiElementWithChildren)
        {
            return uiElementWithChildren.GetChildElementById(id);
        }

        return null;
    }

    /// <summary>
    /// Shows the dialog box in the UI. Only one dialog can be opened at a time.
    /// </summary>
    /// <param name="dialogContent">The element to display in the dialog.</param>
    /// <param name="isDismissible">Indicates whether the dialog can be closed by clicking outside of the dialog.</param>
    /// <returns>An instance of the opened dialog.</returns>
    public Task<UIDialog> OpenDialogAsync(
        IUIElement dialogContent,
        bool isDismissible = false)
    {
        return OpenDialogAsync(dialogContent, null, isDismissible);
    }

    /// <summary>
    /// Shows the dialog box in the UI. Only one dialog can be opened at a time.
    /// </summary>
    /// <param name="dialogContent">The element to display in the dialog.</param>
    /// <param name="footerContent">The element to display in the dialog footer.</param>
    /// <param name="isDismissible">Indicates whether the dialog can be closed by clicking outside of the dialog.</param>
    /// <returns>An instance of the opened dialog.</returns>
    public async Task<UIDialog> OpenDialogAsync(
        IUIElement dialogContent,
        IUIElement? footerContent,
        bool isDismissible = false)
    {
        await TaskSchedulerAwaiter.SwitchOffMainThreadAsync(CancellationToken.None);

        if (CurrentOpenedDialog is not null)
        {
            ThrowHelper.ThrowInvalidOperationException("A dialog is already opened. Close it before opening another one.");
        }

        var dialog = new UIDialog(dialogContent, footerContent, isDismissible);
        dialog.IsOpenedChanged += Dialog_IsOpenedChanged;
        CurrentOpenedDialog = dialog;
        return dialog;
    }

    private void Dialog_IsOpenedChanged(object? sender, EventArgs e)
    {
        Guard.IsNotNull(CurrentOpenedDialog);
        if (!CurrentOpenedDialog.IsOpened)
        {
            CurrentOpenedDialog.IsOpenedChanged -= Dialog_IsOpenedChanged;
            CurrentOpenedDialog.Dispose();
            CurrentOpenedDialog = null;
        }
    }

    /// <summary>
    /// Sets the property value and raises the property changed event if the value has changed.
    /// </summary>
    /// <typeparam name="T">The type of the property.</typeparam>
    /// <param name="field">The reference to the field storing the property value.</param>
    /// <param name="value">The new value to set.</param>
    /// <param name="propertyChangedEventHandler">The event handler to raise the property changed event.</param>
    /// <param name="propertyName">The name of the property. Automatically inferred if not provided.</param>
    protected void SetPropertyValue<T>(
        ref T field,
        T value,
        EventHandler? propertyChangedEventHandler,
        [CallerMemberName] string? propertyName = null)
    {
        if (!EqualityComparer<T>.Default.Equals(field, value))
        {
            field = value;
            propertyChangedEventHandler?.Invoke(this, EventArgs.Empty);
            OnPropertyChanged(propertyName);
        }
    }

    /// <summary>
    /// Raises the property changed event for the specified property.
    /// </summary>
    /// <param name="propertyName">The name of the property. Automatically inferred if not provided.</param>
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new(propertyName));
    }
}

public static partial class GUI
{
    /// <summary>
    /// Sets the element to be displayed in the view.
    /// </summary>
    public static UIToolView WithRootElement(this UIToolView toolView, IUIElement? rootElement)
    {
        toolView.RootElement = rootElement;
        return toolView;
    }
}
