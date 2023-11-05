namespace DevToys.Tools.SmartDetection;

[Export(typeof(IDataTypeDetector))]
[DataTypeName("ColorBlindnessSimulatorImageFile", baseName: PredefinedCommonDataTypeNames.File)]
internal sealed partial class ColorBlindnessSimulatorImageFileDataTypeDetector : IDataTypeDetector
{
    internal static readonly string[] SupportedFileTypes = new[]
    {
        ".bmp",
        ".jpeg",
        ".jpg",
        ".pbm",
        ".png",
        ".tiff",
        ".tga",
        ".webp"
    };

    public ValueTask<DataDetectionResult> TryDetectDataAsync(object data, DataDetectionResult? resultFromBaseDetector, CancellationToken cancellationToken)
    {
        if (resultFromBaseDetector is not null
            && resultFromBaseDetector.Data is FileInfo dataFile)
        {
            if (SupportedFileTypes.Contains(dataFile.Extension, StringComparer.OrdinalIgnoreCase))
            {
                return ValueTask.FromResult(new DataDetectionResult(Success: true, Data: dataFile));
            }
        }

        return ValueTask.FromResult(DataDetectionResult.Unsuccessful);
    }
}
