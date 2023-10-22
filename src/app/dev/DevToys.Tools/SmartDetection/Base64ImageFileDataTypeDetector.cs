namespace DevToys.Tools.SmartDetection;

[Export(typeof(IDataTypeDetector))]
[DataTypeName("Base64ImageFile", baseName: PredefinedCommonDataTypeNames.File)]
internal sealed partial class Base64ImageFileDataTypeDetector : IDataTypeDetector
{
    internal static readonly string[] SupportedFileTypes = new[]
    {
        "bmp",
        "gif",
        "ico",
        "jpg",
        "jpeg",
        "png",
        "svg",
        "webp"
    };

    public ValueTask<DataDetectionResult> TryDetectDataAsync(object data, DataDetectionResult? resultFromBaseDetector, CancellationToken cancellationToken)
    {
        if (resultFromBaseDetector is not null
            && resultFromBaseDetector.Data is FileInfo dataFile)
        {
            if (SupportedFileTypes.Contains(dataFile.Extension.Trim('.'), StringComparer.OrdinalIgnoreCase))
            {
                return ValueTask.FromResult(new DataDetectionResult(Success: true, Data: dataFile));
            }
        }

        return ValueTask.FromResult(DataDetectionResult.Unsuccessful);
    }
}
