namespace DevToys.Blazor.BuiltInDataTypeDetectors;

[Export(typeof(IDataTypeDetector))]
[DataTypeName(PredefinedCommonDataTypeNames.ImageFile, baseName: PredefinedCommonDataTypeNames.File)]
internal sealed partial class ImageFileDataTypeDetector : IDataTypeDetector
{
    public ValueTask<DataDetectionResult> TryDetectDataAsync(object data, DataDetectionResult? resultFromBaseDetector, CancellationToken cancellationToken)
    {
        if (resultFromBaseDetector is not null
            && resultFromBaseDetector.Data is FileInfo dataFile)
        {
            if (PredefinedSupportedImageFormats.ImageFileExtensions.Contains(dataFile.Extension, StringComparer.OrdinalIgnoreCase))
            {
                return ValueTask.FromResult(new DataDetectionResult(Success: true, Data: dataFile));
            }
        }

        return ValueTask.FromResult(DataDetectionResult.Unsuccessful);
    }
}
