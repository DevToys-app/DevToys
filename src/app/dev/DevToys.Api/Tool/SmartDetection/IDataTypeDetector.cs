namespace DevToys.Api;

/// <summary>
/// Represents a way to detect the type of data coming from an external source such as the OS's clipboard.
/// </summary>
/// <remarks>
/// <example>
///     <code>
///         [Export(typeof(IDataTypeDetector))]
///         [DataTypeName("jwt-header", baseName: "json" /* optional */)] // jwt-header type inheriting from json type.
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
    /// Tries to detect whether the given <paramref name="rawData"/> match the expected format known by this
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
    /// <param name="rawData">The data to analyze, often coming from the OS's clipboard.</param>
    /// <param name="resultFromBaseDetector">The result coming from the <see cref="IDataTypeDetector"/> corresponding to the given <see cref="DataTypeNameAttribute.DataTypeBaseName"/>.
    /// Since, the <see cref="DataTypeNameAttribute.DataTypeBaseName"/> is optional, this parameter can be null.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that gets canceled within 2 seconds.</param>
    /// <returns>
    /// Returns a <see cref="DataDetectionResult"/> that indicates whether the data could be analyzed / parsed / read
    /// correctly, along with the parsed data, if any change has been made to it during parsing (for example, string to
    /// integer conversion).
    /// </returns>
    ValueTask<DataDetectionResult> TryDetectDataAsync(object rawData, DataDetectionResult? resultFromBaseDetector, CancellationToken cancellationToken);
}
