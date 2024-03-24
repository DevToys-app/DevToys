namespace DevToys.Blazor.BuiltInDataTypeDetectors;

[Export(typeof(IDataTypeDetector))]
[DataTypeName(PredefinedCommonDataTypeNames.File, baseName: PredefinedCommonDataTypeNames.Files)]
internal sealed partial class FileDataTypeDetector : IDataTypeDetector
{
    public ValueTask<DataDetectionResult> TryDetectDataAsync(object data, DataDetectionResult? resultFromBaseDetector, CancellationToken cancellationToken)
    {
        if (resultFromBaseDetector is not null
            && resultFromBaseDetector.Data is FileInfo[] dataFiles
            && dataFiles.Length == 1)
        {
            return ValueTask.FromResult(new DataDetectionResult(Success: true, Data: dataFiles[0]));
        }

        return ValueTask.FromResult(DataDetectionResult.Unsuccessful);
    }
}
