namespace DevToys.Tools.SmartDetection;

[Export(typeof(IDataTypeDetector))]
[DataTypeName("StaticImageFiles", baseName: PredefinedCommonDataTypeNames.Files)]
internal sealed partial class StaticImageFilesDataTypeDetector : IDataTypeDetector
{
    public ValueTask<DataDetectionResult> TryDetectDataAsync(object data, DataDetectionResult? resultFromBaseDetector, CancellationToken cancellationToken)
    {
        if (resultFromBaseDetector is not null
            && resultFromBaseDetector.Data is FileInfo[] dataFiles)
        {
            var files = new List<FileInfo>();
            for (int i = 0; i < dataFiles.Length; i++)
            {
                if (StaticImageFileDataTypeDetector.SupportedFileTypes.Contains(dataFiles[i].Extension, StringComparer.OrdinalIgnoreCase))
                {
                    files.Add(dataFiles[i]);
                }
            }

            if (files.Count > 0)
            {
                return ValueTask.FromResult(new DataDetectionResult(Success: true, Data: files.ToArray()));
            }
        }

        return ValueTask.FromResult(DataDetectionResult.Unsuccessful);
    }
}
