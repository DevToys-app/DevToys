namespace DevToys.Blazor.BuiltInDataTypeDetectors;

[Export(typeof(IDataTypeDetector))]
[DataTypeName(PredefinedCommonDataTypeNames.Files)]
internal sealed partial class FilesDataTypeDetector : IDataTypeDetector
{
    public ValueTask<DataDetectionResult> TryDetectDataAsync(object data, DataDetectionResult? resultFromBaseDetector, CancellationToken cancellationToken)
    {
        if (data is FileInfo[] dataFiles)
        {
            return ValueTask.FromResult(new DataDetectionResult(true, dataFiles));
        }

        return ValueTask.FromResult(DataDetectionResult.Unsuccessful);
    }
}
