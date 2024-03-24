using SixLabors.ImageSharp;

namespace DevToys.Blazor.BuiltInDataTypeDetectors;

[Export(typeof(IDataTypeDetector))]
[DataTypeName(PredefinedCommonDataTypeNames.Image)]
internal sealed partial class ImageDataTypeDetector : IDataTypeDetector
{
    public ValueTask<DataDetectionResult> TryDetectDataAsync(object data, DataDetectionResult? resultFromBaseDetector, CancellationToken cancellationToken)
    {
        if (data is Image<SixLabors.ImageSharp.PixelFormats.Rgba32> dataImage)
        {
            return ValueTask.FromResult(new DataDetectionResult(true, dataImage));
        }

        return ValueTask.FromResult(DataDetectionResult.Unsuccessful);
    }
}
