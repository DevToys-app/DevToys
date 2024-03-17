using System.Collections.ObjectModel;
using OneOf;
using SixLabors.ImageSharp;

namespace DevToys.Api;

/// <summary>
/// A component that displays an image and allows the user to perform some read-only actions on it.
/// By default, image viewer supports BMP, GIF, JPEG, PBM, PNG, TIFF, TGA, WEBP, SVG formats.
/// </summary>
public interface IUIImageViewer : IUITitledElement
{
    /// <summary>
    /// Gets the source the image to display comes from.
    /// </summary>
    OneOf<FileInfo, Image, SandboxedFileReader>? ImageSource { get; }

    /// <summary>
    /// Gets the custom actions to perform when the user saves the image with a specific file extension.
    /// </summary>
    IReadOnlyDictionary<string, Func<FileStream, ValueTask>> CustomActionPerFileExtensionOnSaving { get; }

    /// <summary>
    /// Raised when <see cref="ImageSource"/> is changed.
    /// </summary>
    event EventHandler? ImageSourceChanged;

    /// <summary>
    /// Raised when <see cref="CustomActionPerFileExtensionOnSaving"/> is changed.
    /// </summary>
    event EventHandler? CustomActionPerFileExtensionOnSavingChanged;
}

[DebuggerDisplay($"Id = {{{nameof(Id)}}}, FileName = {{{nameof(Title)}}}")]
internal sealed class UIImageViewer : UITitledElement, IUIImageViewer, IDisposable
{
    private OneOf<FileInfo, Image, SandboxedFileReader>? _imageSource = default;
    private IReadOnlyDictionary<string, Func<FileStream, ValueTask>> _customActionPerFileExtensionOnSaving = ReadOnlyDictionary<string, Func<FileStream, ValueTask>>.Empty;

    internal UIImageViewer(string? id)
        : base(id)
    {
    }

    internal bool DisposeAutomatically { get; set; }

    /// <inheritdoc/>
    public OneOf<FileInfo, Image, SandboxedFileReader>? ImageSource
    {
        get => _imageSource;
        internal set
        {
            if (DisposeAutomatically && ImageSource.HasValue && ImageSource.Value.Value is IDisposable disposable)
            {
                disposable.Dispose();
            }

            SetPropertyValue(ref _imageSource, value, ImageSourceChanged);
        }
    }

    /// <inheritdoc/>
    public IReadOnlyDictionary<string, Func<FileStream, ValueTask>> CustomActionPerFileExtensionOnSaving
    {
        get => _customActionPerFileExtensionOnSaving;
        internal set => SetPropertyValue(ref _customActionPerFileExtensionOnSaving, value, CustomActionPerFileExtensionOnSavingChanged);
    }

    /// <inheritdoc/>
    public event EventHandler? ImageSourceChanged;

    /// <inheritdoc/>
    public event EventHandler? CustomActionPerFileExtensionOnSavingChanged;

    /// <inheritdoc/>
    public void Dispose()
    {
        if (ImageSource.HasValue && ImageSource.Value.Value is IDisposable disposable && DisposeAutomatically)
        {
            disposable.Dispose();
        }
    }
}

public static partial class GUI
{
    /// <summary>
    /// A component that displays an image and allows the user to perform some read-only actions on it.
    /// By default, image viewer supports BMP, GIF, JPEG, PBM, PNG, TIFF, TGA, WEBP, SVG formats.
    /// </summary>
    /// <returns>The created <see cref="IUIImageViewer"/> instance.</returns>
    public static IUIImageViewer ImageViewer()
    {
        return ImageViewer(id: null);
    }

    /// <summary>
    /// A component that displays an image and allows the user to perform some read-only actions on it.
    /// By default, image viewer supports BMP, GIF, JPEG, PBM, PNG, TIFF, TGA, WEBP, SVG formats.
    /// </summary>
    /// <param name="id">An optional unique identifier for this UI element.</param>
    /// <returns>The created <see cref="IUIImageViewer"/> instance.</returns>
    public static IUIImageViewer ImageViewer(string? id)
    {
        return new UIImageViewer(id);
    }

    /// <summary>
    /// Sets the <see cref="IUIImageViewer.ImageSource"/> from a <see cref="FileInfo"/>.
    /// </summary>
    /// <param name="element">The <see cref="IUIImageViewer"/> instance.</param>
    /// <param name="imageFile">The <see cref="FileInfo"/> representing the image file.</param>
    /// <returns>The updated <see cref="IUIImageViewer"/> instance.</returns>
    public static IUIImageViewer WithFile(this IUIImageViewer element, FileInfo imageFile)
    {
        ValidateFileExtension(imageFile.Name);
        ((UIImageViewer)element).ImageSource = imageFile;
        return element;
    }

    /// <summary>
    /// Sets the <see cref="IUIImageViewer.ImageSource"/> from a <see cref="SandboxedFileReader"/>.
    /// </summary>
    /// <param name="element">The <see cref="IUIImageViewer"/> instance.</param>
    /// <param name="pickedFile">The <see cref="SandboxedFileReader"/> representing the picked file.</param>
    /// <param name="disposeAutomatically">Indicates whether the <paramref name="pickedFile"/> should be disposed when not displayed in the UI anymore.</param>
    /// <returns>The updated <see cref="IUIImageViewer"/> instance.</returns>
    public static IUIImageViewer WithPickedFile(this IUIImageViewer element, SandboxedFileReader pickedFile, bool disposeAutomatically)
    {
        ValidateFileExtension(pickedFile.FileName);
        var imageViewer = (UIImageViewer)element;
        imageViewer.ImageSource = pickedFile;
        imageViewer.DisposeAutomatically = disposeAutomatically;
        return element;
    }

    /// <summary>
    /// Sets the <see cref="IUIImageViewer.ImageSource"/> from an <see cref="Image"/>.
    /// </summary>
    /// <param name="element">The <see cref="IUIImageViewer"/> instance.</param>
    /// <param name="image">The <see cref="Image"/> to display.</param>
    /// <param name="disposeAutomatically">Indicates whether the <paramref name="image"/> should be disposed when not displayed in the UI anymore.</param>
    /// <returns>The updated <see cref="IUIImageViewer"/> instance.</returns>
    public static IUIImageViewer WithImage(this IUIImageViewer element, Image image, bool disposeAutomatically)
    {
        var imageViewer = (UIImageViewer)element;
        imageViewer.ImageSource = image;
        imageViewer.DisposeAutomatically = disposeAutomatically;
        return element;
    }

    /// <summary>
    /// Adds a custom action to perform when the user saves the image with a specific file extension.
    /// </summary>
    /// <remarks>
    /// As a side effect of this method, the specified file extension will be available in the Save File Dialog when the user decides to save the image.
    /// When the user saves the image with the specified <paramref name="fileExtension"/>, the specified <paramref name="action"/> will be invoked
    /// with a <see cref="FileStream"/> pointing to the file to save. The <paramref name="action"/> is responsible for writing the image to the file.
    /// </remarks>
    /// <param name="element">The <see cref="IUIImageViewer"/> instance.</param>
    /// <param name="fileExtension">The file extension to handle.</param>
    /// <param name="action">The action to perform when the user wishes to save the image with the given <paramref name="fileExtension"/>.</param>
    /// <returns>The updated <see cref="IUIImageViewer"/> instance.</returns>
    public static IUIImageViewer ManuallyHandleSaveAs(this IUIImageViewer element, string fileExtension, Func<FileStream, ValueTask> action)
    {
        fileExtension = "." + fileExtension.ToLowerInvariant().Trim().TrimStart('.');

        var imageViewer = (UIImageViewer)element;
        var customActions = new Dictionary<string, Func<FileStream, ValueTask>>(imageViewer.CustomActionPerFileExtensionOnSaving)
        {
            [fileExtension] = action
        };
        imageViewer.CustomActionPerFileExtensionOnSaving = new ReadOnlyDictionary<string, Func<FileStream, ValueTask>>(customActions);
        return element;
    }

    /// <summary>
    /// Removes the custom action to perform when the user saves the image with a specific file extension.
    /// </summary>
    /// <param name="element">The <see cref="IUIImageViewer"/> instance.</param>
    /// <param name="fileExtension">The file extension to remove.</param>
    /// <returns>The updated <see cref="IUIImageViewer"/> instance.</returns>
    public static IUIImageViewer RemoveManuallyHandleSaveAs(this IUIImageViewer element, string fileExtension)
    {
        fileExtension = "." + fileExtension.ToLowerInvariant().Trim().TrimStart('.');

        var imageViewer = (UIImageViewer)element;
        var customActions = imageViewer.CustomActionPerFileExtensionOnSaving.ToDictionary();
        customActions.Remove(fileExtension);
        imageViewer.CustomActionPerFileExtensionOnSaving = new ReadOnlyDictionary<string, Func<FileStream, ValueTask>>(customActions);
        return element;
    }

    /// <summary>
    /// Clears the value of <see cref="IUIImageViewer.ImageSource"/>.
    /// </summary>
    /// <param name="element">The <see cref="IUIImageViewer"/> instance.</param>
    /// <returns>The updated <see cref="IUIImageViewer"/> instance.</returns>
    public static IUIImageViewer Clear(this IUIImageViewer element)
    {
        ((UIImageViewer)element).ImageSource = default;
        return element;
    }

    private static void ValidateFileExtension(string fileName)
    {
        string extension = Path.GetExtension(fileName).ToLowerInvariant();
        switch (extension)
        {
            case ".bmp":
            case ".gif":
            case ".jpeg":
            case ".jpg":
            case ".pbm":
            case ".png":
            case ".tiff":
            case ".tga":
            case ".webp":
            case ".svg":
                return;

            default:
                ThrowHelper.ThrowArgumentException(nameof(fileName), $"Unsupported file extension: {fileName}");
                break;
        }
    }
}
