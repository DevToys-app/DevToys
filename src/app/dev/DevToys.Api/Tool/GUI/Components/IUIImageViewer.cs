using OneOf;

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
    /// Raised when <see cref="ImageSource"/> is changed.
    /// </summary>
    event EventHandler? ImageSourceChanged;
}

[DebuggerDisplay($"Id = {{{nameof(Id)}}}, FileName = {{{nameof(Title)}}}")]
internal sealed class UIImageViewer : UITitledElement, IUIImageViewer, IDisposable
{
    private OneOf<FileInfo, Image, SandboxedFileReader>? _imageSource = default;

    internal UIImageViewer(string? id)
        : base(id)
    {
    }

    internal bool DisposeAutomatically { get; set; }

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

    public event EventHandler? ImageSourceChanged;

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
    public static IUIImageViewer ImageViewer()
    {
        return ImageViewer(id: null);
    }

    /// <summary>
    /// A component that displays an image and allows the user to perform some read-only actions on it.
    /// By default, image viewer supports BMP, GIF, JPEG, PBM, PNG, TIFF, TGA, WEBP, SVG formats.
    /// </summary>
    /// <param name="id">An optional unique identifier for this UI element.</param>
    public static IUIImageViewer ImageViewer(string? id)
    {
        return new UIImageViewer(id);
    }

    /// <summary>
    /// Sets the <see cref="IUIImageViewer.ImageSource"/> from a <see cref="FileInfo"/>.
    /// </summary>
    public static IUIImageViewer WithFile(this IUIImageViewer element, FileInfo imageFile)
    {
        ValidateFileExtension(imageFile.Name);
        ((UIImageViewer)element).ImageSource = imageFile;
        return element;
    }

    /// <summary>
    /// Sets the <see cref="IUIImageViewer.ImageSource"/> from a <see cref="SandboxedFileReader"/>.
    /// </summary>
    /// <param name="pickedFile">The file to display.</param>
    /// <param name="disposeAutomatically">Indicates whether <paramref name="pickedFile"/> should be disposed when not displayed in the UI anymore.</param>
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
    /// <param name="image">The image to display</param>
    /// <param name="disposeAutomatically">Indicates whether <paramref name="image"/> should be disposed when not displayed in the UI anymore.</param>
    public static IUIImageViewer WithImage(this IUIImageViewer element, Image image, bool disposeAutomatically)
    {
        var imageViewer = (UIImageViewer)element;
        imageViewer.ImageSource = image;
        imageViewer.DisposeAutomatically = disposeAutomatically;
        return element;
    }

    /// <summary>
    /// Clears the value of <see cref="IUIImageViewer.ImageSource"/>.
    /// </summary>
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
