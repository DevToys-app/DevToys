namespace DevToys.Api;

/// <summary>
/// Represents a way to detect the type of data coming from an external source such as the OS's clipboard.
/// </summary>
/// <remarks>
/// <example>
///     <code>
///         [Export(typeof(IDataTypeDetector))]
///         [DataTypeName("jwt-header", baseName: "json")] // jwt-header type inheriting from json type.
///         [TargetPlatform(Platform.Windows)] // Optional
///         [TargetPlatform(Platform.WASM)] // Optional
///         internal sealed class JwtDetector : IDataTypeDetector
///         {
///         }
///     </code>
/// </example>
/// </remarks>
public interface IDataTypeDetector
{
    /// <summary>
    /// Tries to detect whether the given <paramref name="data"/> match the expected format known by this
    /// <see cref="IDataTypeDetector"/>, often by trying to reading and/or parsing it.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When the data successfully got parsed, pass the parsed value to <see cref="DataDetectionResult.Data"/>.
    /// </para>
    /// <para>
    /// Ultimately, when detection is successful, the <see cref="DataDetectionResult.Data"/> will be
    /// passed to <see cref="IGuiTool.OnDataReceived(string, object?)"/>.
    /// </para>
    /// </remarks>
    /// <param name="data">The data to analyze, often coming from the OS's clipboard.</param>
    /// <returns>
    /// Returns a <see cref="DataDetectionResult"/> that indicates whether the data could be analyzed / parsed / read
    /// correctly, along with the parsed data, if any change has been made to it during parsing (for example, string to
    /// integer conversion).
    /// </returns>
    ValueTask<DataDetectionResult> TryDetectDataAsync(object data);
}
