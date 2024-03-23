using System.Globalization;
using System.IO;
using System.Text;
using DevToys.Tools.Helpers;
using DevToys.Tools.Tools.EncodersDecoders.Certificate;

namespace DevToys.UnitTests.Tools.Helpers;

public class CertificateHelperTests
{
    private static readonly string baseTestDataDirectory = Path.Combine("Tools", "TestData", nameof(CertificateDecoder));

    [Theory]
    [InlineData("PemCertPublic.txt", null, true, "CertDecoded.txt")]
    [InlineData("PemCertWithPrivateKey.txt", null, true, "CertDecoded.txt")]
    [InlineData("PfxNoPassword.pfx", null, true, "CertDecoded.txt")]
    [InlineData("PfxWithPassword.pfx", "test1234", true, "CertDecoded.txt")]
    [InlineData("PemCertPublicWithExtensions.txt", null, true, "CertWithExtensionsDecoded.txt")]
    [InlineData("PemCertWithPrivateKeyWithExtensions.txt", null, true, "CertWithExtensionsDecoded.txt")]
    [InlineData("PfxWithExtensionsNoPassword.pfx", null, true, "CertWithExtensionsDecoded.txt")]
    public void DecodeCertificateSuccess(string input, string password, bool successfullyDecoded, string expectedResult)
            => DecodeCertificate(input, password, successfullyDecoded, expectedResult);

    [Fact]
    public void DecodeCertificateErrors()
    {
        // Cannot populate these vars via DataRow because https://learn.microsoft.com/en-us/dotnet/csharp/misc/cs0182?f1url=%3FappId%3Droslyn%26k%3Dk(CS0182)
        DecodeCertificate("PfxWithPassword.pfx", "wrong password!", false, CertificateDecoder.InvalidPasswordError);
        DecodeCertificate("CertificateRequest.txt", null, false, CertificateDecoder.UnsupportedFormatError);
    }

    private static void DecodeCertificate(string inputFile, string password, bool successfullyDecoded, string expectedResultFile)
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

        string inputFilePath = TestDataProvider.GetFile(Path.Combine(baseTestDataDirectory, inputFile)).FullName;

        string input;
        if (inputFilePath.EndsWith(".pfx", StringComparison.OrdinalIgnoreCase))
        {
            byte[] data = File.ReadAllBytes(inputFilePath);
            input = CertificateHelper.GetRawCertificateString(data);
        }
        else
        {
            input = File.ReadAllText(inputFilePath);
        }

        string expectedResult;
        string expectedResultFilePath = TestDataProvider.GetFile(Path.Combine(baseTestDataDirectory, expectedResultFile)).FullName;
        if (File.Exists(expectedResultFilePath))
        {
            expectedResult = File.ReadAllText(TestDataProvider.GetFile(Path.Combine(baseTestDataDirectory, expectedResultFile)).FullName);
        }
        else
        {
            expectedResult = expectedResultFile;
        }

        bool result = CertificateHelper.TryDecodeCertificate(new MockILogger(), input, password, out string decoded);
        decoded = CleanDateTimes(decoded);
        expectedResult = CleanDateTimes(expectedResult);

        result.Should().Be(successfullyDecoded);
        expectedResult.Should().Be(decoded);
    }

    /// <summary>
    /// Strips times from decoded certificates to avoid issues with testing in different timezones
    /// </summary>
    internal static string CleanDateTimes(string decoded)
    {
        var decodedCleaned = new StringBuilder();
        foreach (string line in decoded.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None))
        {
            if (DateTime.TryParse(line, out DateTime dateTime))
            {
                decodedCleaned.AppendLine($"  {dateTime.Date}");
            }
            else
            {
                decodedCleaned.AppendLine(line);
            }
        }

        return decodedCleaned.ToString();
    }
}
