using DevToys.Tools.Helpers;

namespace DevToys.Tools.SmartDetection;

[Export(typeof(IDataTypeDetector))]
[DataTypeName(InternalName, baseName: PredefinedCommonDataTypeNames.Text)]
internal sealed class CertificateDetector : IDataTypeDetector
{
    internal const string InternalName = "Certificate";

    public ValueTask<DataDetectionResult> TryDetectDataAsync(object rawData, DataDetectionResult? resultFromBaseDetector, CancellationToken cancellationToken)
    {
        if (resultFromBaseDetector is not null
            && resultFromBaseDetector.Data is string dataString
            && !string.IsNullOrWhiteSpace(dataString))
        {
            bool isCertificate = CertificateHelper.TryDecodeCertificate(this.Log(), dataString, password: null, out string? _);

            if (isCertificate)
            {
                return ValueTask.FromResult(new DataDetectionResult(Success: true, Data: dataString));
            }
        }

        return ValueTask.FromResult(DataDetectionResult.Unsuccessful);
    }
}
